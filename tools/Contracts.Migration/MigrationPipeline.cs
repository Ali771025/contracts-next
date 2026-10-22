using Contracts.Domain.Common;
using Contracts.Domain.Contracts;
using Contracts.Domain.Finance;
using Contracts.Domain.Parties;
using Contracts.Domain.Projects;
using Contracts.Domain.ReferenceData;
using Contracts.Domain.Security;
using Contracts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Contracts.Migration;

public sealed class MigrationPipeline(MappingFile mapping, DateTimeOffset runAtUtc)
{
    private readonly Dictionary<(string Category, string Value, bool Required), List<long>> unresolved = [];
    private readonly List<string> warnings = [];
    private readonly List<string> errors = [];
    private readonly Dictionary<string, Dictionary<long, long>> ids = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<StageCounter> stages = [];

    public async Task<MigrationOutcome> RunAsync(LegacySnapshot source, ContractsDbContext? db, string sourcePath, CancellationToken token)
    {
        bool execute = db is not null;
        Dictionary<string, long> refs = execute ? await LoadReferencesAsync(db!, token) : DefaultReferenceIds();
        long actor = 0;

        await StageAsync("Reference", source.Table("tblContractCategory"), db, async rows =>
        {
            foreach (LegacyRow row in rows)
            {
                string title = row.Text("Title") ?? $"Legacy category {row.Id}";
                long targetId = 1000 + row.Id;
                if (execute) db!.ContractCategories.Add(new ContractCategory(targetId, $"LEGACY_{row.Id}", title, checked((int)row.Id * 10), true, Stamp(null)));
                MapId("Category", row.Id, targetId);
            }
            if (execute) await db!.SaveChangesAsync(token);
            return (rows.Count, 0, 0, 0);
        }, token);

        IReadOnlyList<LegacyRow> users = source.Table("tblUsers");
        await StageAsync("Users/Roles", users, db, async rows =>
        {
            int imported = 0, skipped = 0, failed = 0;
            foreach (LegacyRow row in rows)
            {
                if (!TryRef(row, "Role", "Role", true, refs, out long roleId)) { skipped++; continue; }
                string? username = row.Text("UserName");
                string? display = row.Text("FullName");
                if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(display)) { RequiredFailure("User", row.Id); skipped++; continue; }
                if (execute)
                {
                    User user = new(username, display, roleId, row.Bool("IsActive", true), Stamp(null));
                    db!.Users.Add(user);
                    await db.SaveChangesAsync(token);
                    MapId("User", row.Id, user.Id);
                    actor = actor == 0 ? user.Id : actor;
                }
                else { MapId("User", row.Id, row.Id); actor = actor == 0 ? row.Id : actor; }
                imported++;
            }
            int permissions = source.Table("tblRolePermissions").Count;
            if (permissions > 0) warnings.Add($"RBAC_DIMENSIONS_UNKNOWN:{permissions}");
            return (imported, skipped, permissions > 0 ? 1 : 0, failed);
        }, token);

        await StageAsync("Parties", source.Table("tblParties"), db, async rows =>
        {
            int imported = 0, skipped = 0;
            foreach (LegacyRow row in rows)
            {
                if (!TryRef(row, "PartyType", "PartyType", true, refs, out long typeId) || string.IsNullOrWhiteSpace(row.Text("PartyName"))) { skipped++; continue; }
                if (execute)
                {
                    Party item = new(row.Text("PartyName")!, typeId, row.Text("NationalID"), row.Text("EconomicCode"), null,
                        row.Text("Phone") ?? row.Text("Mobile"), row.Text("Address"), Stamp(actor));
                    db!.Parties.Add(item); await db.SaveChangesAsync(token); MapId("Party", row.Id, item.Id);
                }
                else MapId("Party", row.Id, row.Id);
                imported++;
            }
            return (imported, skipped, 0, 0);
        }, token);

        await StageAsync("Projects", source.Table("tblProjects"), db, async rows =>
        {
            int imported = 0, skipped = 0;
            List<long> invalidDateOrderIds = [];
            foreach (LegacyRow row in rows)
            {
                if (!TryRef(row, "Status", "ProjectStatus", true, refs, out long statusId) || string.IsNullOrWhiteSpace(row.Text("ProjectName"))) { skipped++; continue; }
                string projectNo = row.Text("ProjectCode") ?? $"LEGACY-PROJECT-{row.Id}";
                DateOnly? startDate = DateOnlyValue(row, "StartDate");
                DateOnly? endDate = DateOnlyValue(row, "EndDate");
                if (startDate is not null && endDate is not null && endDate < startDate)
                {
                    invalidDateOrderIds.Add(row.Id);
                    endDate = null;
                }
                if (execute)
                {
                    Project item = new(projectNo, row.Text("ProjectName")!, statusId, startDate, endDate,
                        null, row.Text("Description"), Stamp(actor));
                    db!.Projects.Add(item); await db.SaveChangesAsync(token); MapId("Project", row.Id, item.Id);
                }
                else MapId("Project", row.Id, row.Id);
                imported++;
            }
            if (invalidDateOrderIds.Count > 0)
                warnings.Add($"LEGACY_PROJECT_END_BEFORE_START:CLEARED_END_DATE:COUNT={invalidDateOrderIds.Count}:IDS={string.Join(',', invalidDateOrderIds.Order().Take(25))}");
            return (imported, skipped, invalidDateOrderIds.Count, 0);
        }, token);

        await StageAsync("Contracts", source.Table("tblContracts"), db, async rows =>
        {
            int imported = 0, skipped = 0, localWarnings = 0;
            foreach (LegacyRow row in rows)
            {
                if (!TryRef(row, "ContractType", "ContractType", true, refs, out long typeId) ||
                    !TryRef(row, "Status", "ContractStatus", true, refs, out long statusId) ||
                    !TryTarget("Party", row.KeyValue("PartyID"), row.Id, out long partyId) ||
                    string.IsNullOrWhiteSpace(row.Text("ContractNo")) || string.IsNullOrWhiteSpace(row.Text("ContractTitle")) ||
                    DateOnlyValue(row, "StartDate") is not DateOnly start || DateOnlyValue(row, "EndDate") is not DateOnly end)
                { RequiredFailure("Contract", row.Id); skipped++; continue; }

                string? currencyText = row.Text("CurrencyType");
                string currencyCode;
                long original;
                long rate;
                int scale;
                if (string.IsNullOrWhiteSpace(currencyText) || PersianText.Normalize(currencyText) == PersianText.Normalize("ریال"))
                {
                    currencyCode = "IRR"; scale = 0; rate = 1; original = Money(row, "ContractAmount", true, ref skipped); if (original < 0) continue;
                }
                else if (mapping.TryMap("Currency", currencyText, out currencyCode) && currencyCode is "USD" or "EUR")
                {
                    scale = 2; original = Money(row, "ForeignAmount", true, ref skipped); rate = Money(row, "ExchangeRate", true, ref skipped);
                    if (original < 0 || rate <= 0) continue;
                }
                else { AddUnresolved("Currency", currencyText, true, row.Id); skipped++; continue; }
                long? category = TryTarget("Category", row.KeyValue("ContractCategoryID"), row.Id, out long categoryId) ? categoryId : null;
                long? project = TryTarget("Project", row.KeyValue("ProjectID"), row.Id, out long projectId) ? projectId : null;
                if (execute)
                {
                    Contract item = new(row.Text("ContractNo")!, row.Text("ContractTitle")!, typeId, category, project, partyId, statusId,
                        refs[$"Currency:{currencyCode}"], original, scale, rate, start, end, null, row.Text("Description"), Stamp(actor));
                    db!.Contracts.Add(item); await db.SaveChangesAsync(token); MapId("Contract", row.Id, item.Id);
                }
                else MapId("Contract", row.Id, row.Id);
                imported++;
            }
            return (imported, skipped, localWarnings, 0);
        }, token);

        await StageAsync("Amendments", source.Table("tblAmendments"), db, async rows =>
        {
            int imported = 0, skipped = 0;
            foreach (LegacyRow row in rows)
            {
                if (!TryTarget("Contract", row.KeyValue("ContractID"), row.Id, out long contractId) || DateOnlyValue(row, "AmendmentDate") is not DateOnly date) { skipped++; continue; }
                long amount = Money(row, "AmountChange", false, ref skipped); if (amount == long.MinValue) continue;
                string number = row.Text("AmendmentNo") ?? $"LEGACY-{row.Id}";
                if (execute)
                {
                    Contract contract = await db!.Contracts.Include(item => item.Amendments).SingleAsync(item => item.Id == contractId, token);
                    ContractAmendment item = contract.AddAmendment(number, date, amount, checked((int)(row.KeyValue("TimeExtension") ?? 0)), row.Text("Description"), Stamp(actor));
                    await db.SaveChangesAsync(token); MapId("Amendment", row.Id, item.Id);
                }
                else MapId("Amendment", row.Id, row.Id);
                imported++;
            }
            return (imported, skipped, 0, 0);
        }, token);

        await StageAsync("Payments/Receipts", source.Table("tblPayments").Concat(source.Table("tblReceipts")).ToArray(), db, async _ =>
        {
            int imported = 0, skipped = 0, localWarnings = 0;
            foreach (LegacyRow row in source.Table("tblPayments"))
            {
                if (!TryTarget("Contract", row.KeyValue("ContractID"), row.Id, out long contractId) || !TryRef(row, "PaymentType", "PaymentType", true, refs, out long typeId) || DateOnlyValue(row, "PaymentDate") is not DateOnly date) { skipped++; continue; }
                long amount = Money(row, "Amount", true, ref skipped); if (amount <= 0) continue;
                if (!string.IsNullOrWhiteSpace(row.Text("Bank"))) localWarnings++;
                if (execute) { string number = row.Text("PaymentNo") ?? row.Text("ChequeNo") ?? $"LEGACY-PAY-{row.Id}"; Payment item = new(contractId, number, amount, date, typeId, null, number, null, row.Text("ChequeNo"), null, null, row.Text("Description"), Stamp(actor)); db!.Payments.Add(item); await db.SaveChangesAsync(token); MapId("Payment", row.Id, item.Id); }
                else MapId("Payment", row.Id, row.Id);
                imported++;
            }
            foreach (LegacyRow row in source.Table("tblReceipts"))
            {
                if (!TryTarget("Contract", row.KeyValue("ContractID"), row.Id, out long contractId) || DateOnlyValue(row, "ReceiptDate") is not DateOnly date) { skipped++; continue; }
                long amount = Money(row, "Amount", true, ref skipped); if (amount <= 0) continue;
                if (execute) { string number = row.Text("ReceiptNo") ?? $"LEGACY-REC-{row.Id}"; Receipt item = new(contractId, number, amount, date, refs["PaymentType:UNSPECIFIED_LEGACY"], null, number, null, null, null, null, row.Text("Description"), Stamp(actor)); db!.Receipts.Add(item); await db.SaveChangesAsync(token); MapId("Receipt", row.Id, item.Id); }
                else MapId("Receipt", row.Id, row.Id);
                imported++;
            }
            if (localWarnings > 0) warnings.Add($"BANK_OR_RECEIPT_TYPE_REVIEW:{localWarnings}");
            return (imported, skipped, localWarnings, 0);
        }, token);

        await ImportGuarantees(source, db, refs, actor, token);
        await ImportInvoices(source, db, refs, actor, token);
        await ImportDocuments(source, db, sourcePath, refs, actor, token);
        await ImportAlerts(source, db, refs, actor, token);
        await ImportBudgets(source, db, refs, actor, token);
        await ImportEvaluations(source, db, actor, token);

        IReadOnlyList<UnresolvedMapping> unresolvedList = unresolved.Select(item => new UnresolvedMapping(item.Key.Category,
            item.Key.Value, item.Key.Required, item.Value.Count, item.Value.Distinct().Order().Take(25).ToArray()))
            .OrderBy(item => item.Category).ThenBy(item => item.NormalizedValue).ToArray();
        List<ReconciliationMetric> reconciliation = await ReconcileAsync(source, db, token);
        bool reconciled = reconciliation.All(item => item.SourceCount == item.TargetCount + item.SkippedCount + item.FailedCount &&
            item.SourceTotalRial == item.TargetTotalRial);
        return new(stages, unresolvedList, warnings, errors, reconciliation, reconciled);
    }

    private async Task ImportGuarantees(LegacySnapshot source, ContractsDbContext? db, Dictionary<string, long> refs, long actor, CancellationToken token) =>
        await StageAsync("Guarantees", source.Table("tblGuarantees"), db, async rows => { int imported=0, skipped=0, warn=0; foreach(LegacyRow row in rows) { if(!TryTarget("Contract",row.Integer("ContractID"),row.Id,out long contractId)||!TryRef(row,"GuaranteeType","GuaranteeType",true,refs,out long type)||!TryRef(row,"Status","GuaranteeStatus",true,refs,out long status)||DateOnlyValue(row,"IssueDate") is not DateOnly issue||DateOnlyValue(row,"ExpireDate") is not DateOnly expiry){skipped++;continue;} long amount=Money(row,"Amount",true,ref skipped);if(amount<=0)continue;if(!string.IsNullOrWhiteSpace(row.Text("BankName")))warn++; string no=row.Text("GuaranteeNo")??$"LEGACY-{row.Id}"; if(db is not null){Guarantee item=new(contractId,no,type,status,amount,null,issue,expiry,null,Stamp(actor));db.Guarantees.Add(item);await db.SaveChangesAsync(token);MapId("Guarantee",row.Id,item.Id);}else MapId("Guarantee",row.Id,row.Id);imported++;}if(warn>0)warnings.Add($"BANK_MAPPING_REVIEW:{warn}");return(imported,skipped,warn,0);},token);

    private async Task ImportInvoices(LegacySnapshot source, ContractsDbContext? db, Dictionary<string, long> refs, long actor, CancellationToken token) =>
        await StageAsync("Invoices", source.Table("tblInvoices"), db, async rows =>
        {
            int imported = 0, skipped = 0;
            List<long> reportedTotalDriftIds = [];
            long reportedTotalDriftRial = 0;
            foreach (LegacyRow row in rows)
            {
                if (!TryTarget("Contract", row.Integer("ContractID"), row.Id, out long contractId) ||
                    !TryRef(row, "InvoiceType", "InvoiceType", true, refs, out long type) ||
                    !TryRef(row, "PaymentStatus", "PaymentStatus", true, refs, out long status) ||
                    DateOnlyValue(row, "InvoiceDate") is not DateOnly date)
                {
                    skipped++;
                    continue;
                }

                long amount = Money(row, "Amount", false, ref skipped);
                long vat = Money(row, "VAT", false, ref skipped);
                if (amount == long.MinValue || vat == long.MinValue) continue;
                long canonicalTotal = checked(amount + vat);
                long reportedTotal = checked(decimal.ToInt64(row.Number("TotalAmount") ?? canonicalTotal));
                if (reportedTotal != canonicalTotal)
                {
                    reportedTotalDriftIds.Add(row.Id);
                    reportedTotalDriftRial = checked(reportedTotalDriftRial + reportedTotal - canonicalTotal);
                }

                string no = row.Text("InvoiceNo") ?? $"LEGACY-{row.Id}";
                if (db is not null)
                {
                    Invoice item = new(contractId, no, date, type, status, amount, vat, null, null, Stamp(actor));
                    db.Invoices.Add(item);
                    await db.SaveChangesAsync(token);
                    MapId("Invoice", row.Id, item.Id);
                }
                else MapId("Invoice", row.Id, row.Id);
                imported++;
            }

            if (reportedTotalDriftIds.Count > 0)
            {
                string idsText = string.Join(',', reportedTotalDriftIds.Order().Take(25));
                warnings.Add($"LEGACY_INVOICE_REPORTED_TOTAL_DRIFT:COUNT={reportedTotalDriftIds.Count}:DELTA_RIAL={reportedTotalDriftRial}:IDS={idsText}");
            }
            return (imported, skipped, reportedTotalDriftIds.Count, 0);
        }, token);

    private async Task ImportDocuments(LegacySnapshot source, ContractsDbContext? db, string sourcePath, Dictionary<string,long> refs,long actor,CancellationToken token) =>
        await StageAsync("Documents",source.Table("tblDocuments"),db,async rows=>{int imported=0,skipped=0,warn=0;foreach(LegacyRow row in rows){if(!TryTarget("Contract",row.Integer("ContractID"),row.Id,out long contractId)||!TryRef(row,"DocumentType","DocumentType",true,refs,out long type)){skipped++;continue;}string? path=row.Text("FilePath");string? name=row.Text("FileName");if(string.IsNullOrWhiteSpace(path)){RequiredFailure("DocumentPath",row.Id);skipped++;continue;}string resolved=Path.IsPathRooted(path)?path:Path.GetFullPath(path,Path.GetDirectoryName(sourcePath)!);bool exists=File.Exists(resolved);if(!exists)warn++;string original=Path.GetFileName(name??path);string stored=$"legacy-{row.Id}{Path.GetExtension(original)}";string relative=$"legacy/{row.Id}/{stored}";long size=exists?new FileInfo(resolved).Length:0;string? hash=null;if(exists){await using FileStream stream=File.OpenRead(resolved);hash=Convert.ToHexString(await System.Security.Cryptography.SHA256.HashDataAsync(stream,token));}if(db is not null){ContractDocument item=new(contractId,type,original,stored,relative,size,hash,Stamp(actor));db.ContractDocuments.Add(item);await db.SaveChangesAsync(token);MapId("Document",row.Id,item.Id);}else MapId("Document",row.Id,row.Id);imported++;}if(warn>0)warnings.Add($"MISSING_ATTACHMENTS:{warn}");return(imported,skipped,warn,0);},token);

    private async Task ImportAlerts(LegacySnapshot source,ContractsDbContext? db,Dictionary<string,long> refs,long actor,CancellationToken token)=>
        await StageAsync("Alerts",source.Table("tblAlerts"),db,async rows=>{int imported=0,skipped=0;foreach(LegacyRow row in rows){if(!TryTarget("Contract",row.Integer("ContractID"),row.Id,out long contract)||!TryRef(row,"AlertType","AlertType",true,refs,out long type)||!TryRef(row,"Status","AlertStatus",true,refs,out long status)||!TryRef(row,"Priority","Priority",true,refs,out long priority)||DateOnlyValue(row,"DueDate") is not DateOnly due){skipped++;continue;}long? guarantee=TryTarget("Guarantee",row.Integer("GuaranteeID"),row.Id,out long gid)?gid:null;if(db is not null){Alert item=new(contract,guarantee,null,type,status,priority,row.Text("Description")??row.Text("AlertType")??$"Legacy alert {row.Id}",$"LEGACY_ALERT:{row.Id}",due,Stamp(actor));db.Alerts.Add(item);await db.SaveChangesAsync(token);MapId("Alert",row.Id,item.Id);}else MapId("Alert",row.Id,row.Id);imported++;}return(imported,skipped,0,0);},token);

    private async Task ImportBudgets(LegacySnapshot source,ContractsDbContext? db,Dictionary<string,long> refs,long actor,CancellationToken token)=>
        await StageAsync("Budgets",source.Table("tblProjectBudget"),db,async rows=>{int imported=0,skipped=0;foreach(LegacyRow row in rows){if(!TryTarget("Project",row.Integer("ProjectID"),row.Id,out long project)){skipped++;continue;}long approved=Money(row,"ApprovedBudget",false,ref skipped),consumed=Money(row,"ConsumedBudget",false,ref skipped);if(approved==long.MinValue||consumed==long.MinValue)continue;string year=(row.Integer("FiscalYear")??0).ToString(System.Globalization.CultureInfo.InvariantCulture);if(db is not null){ProjectBudget item=new(project,year,refs["Currency:IRR"],0,approved,consumed,Stamp(actor));db.ProjectBudgets.Add(item);await db.SaveChangesAsync(token);MapId("Budget",row.Id,item.Id);}else MapId("Budget",row.Id,row.Id);imported++;}return(imported,skipped,0,0);},token);

    private async Task ImportEvaluations(LegacySnapshot source,ContractsDbContext? db,long actor,CancellationToken token)=>
        await StageAsync("Evaluations",source.Table("tblContractorEvaluation"),db,async rows=>{int imported=0,skipped=0;foreach(LegacyRow row in rows){if(!TryTarget("Contract",row.Integer("ContractID"),row.Id,out long contract)||!TryTarget("Party",row.Integer("PartyID"),row.Id,out long party)||DateOnlyValue(row,"EvaluationDate") is not DateOnly date){skipped++;continue;}int Score(string field)=>checked((int)Math.Round(row.Number(field)??0,0,MidpointRounding.AwayFromZero));try{if(db is not null){ContractorEvaluation item=new(contract,party,date,Score("QualityScore"),Score("TimeScore"),Score("FinancialScore"),Score("HSEScore"),row.Text("Comments"),Stamp(actor));db.ContractorEvaluations.Add(item);await db.SaveChangesAsync(token);MapId("Evaluation",row.Id,item.Id);}else MapId("Evaluation",row.Id,row.Id);imported++;}catch(ArgumentOutOfRangeException){RequiredFailure("EvaluationScore",row.Id);skipped++;}}return(imported,skipped,0,0);},token);

    private async Task StageAsync(string name,IReadOnlyList<LegacyRow> rows,ContractsDbContext? db,Func<IReadOnlyList<LegacyRow>,Task<(int Imported,int Skipped,int Warnings,int Failed)>> action,CancellationToken token)
    {
        IDbContextTransaction? transaction=db is null?null:await db.Database.BeginTransactionAsync(token);
        try{(int imported,int skipped,int warning,int failed)=await action(rows);if(transaction is not null)await transaction.CommitAsync(token);stages.Add(new(name,rows.Count,imported,skipped,warning,failed));}
        catch(Exception exception){if(transaction is not null)await transaction.RollbackAsync(token);db?.ChangeTracker.Clear();errors.Add($"{name}:{exception.GetType().Name}");stages.Add(new(name,rows.Count,0,0,0,rows.Count));}
        finally{if(transaction is not null)await transaction.DisposeAsync();}
    }

    private bool TryRef(LegacyRow row,string column,string category,bool required,Dictionary<string,long> refs,out long id)
    {id=0;string? value=row.Text(column);if(mapping.TryMap(category,value,out string code)&&refs.TryGetValue($"{category}:{code}",out id))return true;AddUnresolved(category,value,required,row.Id);return false;}
    private void AddUnresolved(string category,string? value,bool required,long id){string normalized=PersianText.Normalize(value??"<NULL>");if(!unresolved.TryGetValue((category,normalized,required),out List<long>? list))unresolved[(category,normalized,required)]=list=[];list.Add(id);warnings.Add($"UNRESOLVED_{category.ToUpperInvariant()}");}
    private void RequiredFailure(string kind,long id){warnings.Add($"REQUIRED_{kind.ToUpperInvariant()}_SKIPPED");}
    private bool TryTarget(string type,long? legacyId,long rowId,out long target){target=0;if(legacyId is null)return false;if(ids.TryGetValue(type,out Dictionary<long,long>? map)&&map.TryGetValue(legacyId.Value,out target))return true;RequiredFailure($"{type}Reference",rowId);return false;}
    private void MapId(string type,long source,long target){if(!ids.TryGetValue(type,out Dictionary<long,long>? map))ids[type]=map=[];map[source]=target;}
    private AuditStamp Stamp(long? actor)=>new(actor,runAtUtc.ToUniversalTime());
    private static DateOnly? DateOnlyValue(LegacyRow row,string column)=>row.Date(column) is DateTime value?DateOnly.FromDateTime(value):null;
    private long Money(LegacyRow row,string column,bool positive,ref int skipped){decimal value=row.Number(column)??0;if(value!=decimal.Truncate(value)||value>long.MaxValue||value<long.MinValue||(positive&&value<=0)){RequiredFailure($"Money{column}",row.Id);skipped++;return long.MinValue;}return decimal.ToInt64(value);}

    private static async Task<Dictionary<string,long>> LoadReferencesAsync(ContractsDbContext db,CancellationToken token)
    {Dictionary<string,long> result=DefaultReferenceIds();async Task Add<T>(DbSet<T> set,string category) where T:ReferenceEntity{foreach(T item in await set.AsNoTracking().ToListAsync(token))result[$"{category}:{item.Code}"]=item.Id;}await Add(db.Roles,"Role");await Add(db.Set<ContractType>(),"ContractType");await Add(db.Set<ContractStatus>(),"ContractStatus");await Add(db.Set<PartyType>(),"PartyType");await Add(db.Set<ProjectStatus>(),"ProjectStatus");await Add(db.Set<Currency>(),"Currency");await Add(db.Set<PaymentType>(),"PaymentType");await Add(db.Set<GuaranteeType>(),"GuaranteeType");await Add(db.Set<GuaranteeStatus>(),"GuaranteeStatus");await Add(db.Set<InvoiceType>(),"InvoiceType");await Add(db.Set<PaymentStatus>(),"PaymentStatus");await Add(db.Set<DocumentType>(),"DocumentType");await Add(db.Set<AlertType>(),"AlertType");await Add(db.Set<AlertStatus>(),"AlertStatus");await Add(db.Set<Priority>(),"Priority");return result;}
    private static Dictionary<string,long> DefaultReferenceIds()=>new(StringComparer.OrdinalIgnoreCase)
    { ["Role:SYSTEM_ADMIN"]=1,["Role:CONTRACT_MANAGER"]=2,["Role:CHIEF_EXECUTIVE"]=3,["Role:VIEWER"]=4,["Role:LEGAL_EXPERT"]=5,["Role:FINANCE_EXPERT"]=6,
      ["ContractType:PURCHASE"]=1,["ContractType:SALE"]=2,["ContractType:OUTSOURCE"]=3,["ContractType:CONTRACTING"]=4,["ContractType:CONSULTING"]=5,
      ["ContractStatus:DRAFT"]=1,["ContractStatus:ACTIVE"]=2,["ContractStatus:COMPLETED"]=3,["ContractStatus:CANCELLED"]=4,["ContractStatus:SUSPENDED"]=5,["ContractStatus:TERMINATED"]=6,
      ["PartyType:CUSTOMER"]=1,["PartyType:SUPPLIER"]=2,["PartyType:CONTRACTOR"]=3,["PartyType:CONSULTANT"]=4,["PartyType:LEGAL_ENTITY"]=5,["PartyType:NATURAL_PERSON"]=6,
      ["ProjectStatus:PLANNED"]=1,["ProjectStatus:ACTIVE"]=2,["ProjectStatus:COMPLETED"]=3,["ProjectStatus:CANCELLED"]=4,["ProjectStatus:SUSPENDED"]=5,
      ["Currency:IRR"]=1,["Currency:USD"]=2,["Currency:EUR"]=3,["PaymentType:CASH"]=1,["PaymentType:CHEQUE"]=2,["PaymentType:BANK_TRANSFER"]=3,["PaymentType:ADVANCE"]=4,["PaymentType:ON_ACCOUNT"]=5,["PaymentType:RAHSA"]=6,["PaymentType:UNSPECIFIED_LEGACY"]=7,
      ["GuaranteeType:TENDER"]=1,["GuaranteeType:ADVANCE_PAYMENT"]=2,["GuaranteeType:PERFORMANCE"]=3,["GuaranteeType:RETENTION"]=4,["GuaranteeStatus:ACTIVE"]=1,["GuaranteeStatus:RELEASED"]=2,["GuaranteeStatus:EXPIRED"]=3,["GuaranteeStatus:CLAIMED"]=4,
      ["InvoiceType:PURCHASE"]=1,["InvoiceType:SALE"]=2,["InvoiceType:ADJUSTMENT"]=3,["InvoiceType:PROGRESS"]=4,["PaymentStatus:UNPAID"]=1,["PaymentStatus:PARTIAL"]=2,["PaymentStatus:PAID"]=3,["PaymentStatus:CANCELLED"]=4,
      ["DocumentType:CONTRACT"]=1,["DocumentType:AMENDMENT"]=2,["DocumentType:INVOICE"]=3,["DocumentType:GUARANTEE"]=4,["DocumentType:OTHER"]=5,["AlertType:CONTRACT_EXPIRY"]=1,["AlertType:GUARANTEE_EXPIRY"]=2,["AlertType:PAYMENT_DUE"]=3,["AlertType:RECEIPT_DUE"]=4,["AlertStatus:OPEN"]=1,["AlertStatus:COMPLETED"]=2,["AlertStatus:DISMISSED"]=3,["Priority:LOW"]=1,["Priority:MEDIUM"]=2,["Priority:HIGH"]=3,["Priority:CRITICAL"]=4 };

    private async Task<List<ReconciliationMetric>> ReconcileAsync(LegacySnapshot source, ContractsDbContext? db, CancellationToken token)
    {
        string[] names = ["Contracts", "Payments", "Receipts", "Amendments", "Invoices", "Guarantees"];
        Dictionary<string, string> tables = new() { { "Contracts", "tblContracts" }, { "Payments", "tblPayments" }, { "Receipts", "tblReceipts" }, { "Amendments", "tblAmendments" }, { "Invoices", "tblInvoices" }, { "Guarantees", "tblGuarantees" } };
        Dictionary<string, string> amounts = new() { { "Contracts", "ContractAmount" }, { "Payments", "Amount" }, { "Receipts", "Amount" }, { "Amendments", "AmountChange" }, { "Guarantees", "Amount" } };
        Dictionary<string, string> idTypes = new() { { "Contracts", "Contract" }, { "Payments", "Payment" }, { "Receipts", "Receipt" }, { "Amendments", "Amendment" }, { "Invoices", "Invoice" }, { "Guarantees", "Guarantee" } };
        List<ReconciliationMetric> result = [];
        foreach (string name in names)
        {
            IReadOnlyList<LegacyRow> rows = source.Table(tables[name]);
            long SourceAmount(LegacyRow row) => name == "Invoices"
                ? checked(decimal.ToInt64(row.Number("Amount") ?? 0) + decimal.ToInt64(row.Number("VAT") ?? 0))
                : decimal.ToInt64(row.Number(amounts[name]) ?? 0);
            long sourceTotal = rows.Sum(SourceAmount);
            Dictionary<long, long>? importedIds = ids.GetValueOrDefault(idTypes[name]);
            long targetCount = importedIds?.Count ?? 0;
            long targetTotal = rows.Where(row => importedIds?.ContainsKey(row.Id) == true).Sum(SourceAmount);
            if (db is not null)
            {
                (targetCount, targetTotal) = name switch
                {
                    "Contracts" => (await db.Contracts.CountAsync(token), await db.Contracts.SumAsync(x => x.BaseAmountRial, token)),
                    "Payments" => (await db.Payments.CountAsync(token), await db.Payments.SumAsync(x => x.AmountRial, token)),
                    "Receipts" => (await db.Receipts.CountAsync(token), await db.Receipts.SumAsync(x => x.AmountRial, token)),
                    "Amendments" => (await db.ContractAmendments.CountAsync(token), await db.ContractAmendments.SumAsync(x => x.AmountChangeRial, token)),
                    "Invoices" => (await db.Invoices.CountAsync(token), await db.Invoices.SumAsync(x => x.AmountRial + x.VatAmountRial, token)),
                    "Guarantees" => (await db.Guarantees.CountAsync(token), await db.Guarantees.SumAsync(x => x.AmountRial, token)),
                    _ => (0, 0),
                };
            }
            long skipped = rows.Count - targetCount;
            result.Add(new(name, rows.Count, targetCount, skipped, 0, sourceTotal, targetTotal));
        }
        return result;
    }
}
