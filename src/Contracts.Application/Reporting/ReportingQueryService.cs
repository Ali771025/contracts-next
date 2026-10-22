namespace Contracts.Application.Reporting;

using System.Globalization;
using System.Text.Json;
using global::Contracts.Application.Finance;
using global::Contracts.Application.Presentation;
using global::Contracts.Application.Security;
using global::Contracts.Domain.Common;
using global::Contracts.Domain.Contracts;

public sealed class ReportingQueryService(FinanceService finance,IOperationalReportSource operations,
    IPersianDateService dates,INumberDisplayService numbers,IBusinessDateService businessDate,ISystemClock clock)
{
    public async Task<ReportDocument> ExecuteAsync(ReportFilter filter,CancellationToken ct)
    {
        filter.Validate();CanonicalFinancialReportSnapshot financial=NeedsFinancial(filter.Kind)?await finance.ReportingSnapshotAsync(filter.ToCanonical(),ct):EmptyFinancial();
        OperationalReportSnapshot operational=NeedsOperational(filter.Kind)?await operations.LoadAsync(filter,businessDate.Today,ct):EmptyOperational();
        return Build(filter,financial,operational,Guid.NewGuid(),clock.UtcNow,businessDate.Today);
    }

    internal ReportDocument Build(ReportFilter filter,CanonicalFinancialReportSnapshot finance,OperationalReportSnapshot operations,
        Guid snapshotId,DateTimeOffset generatedAt,DateOnly today)
    {
        (string title,IReadOnlyList<ReportColumn> columns,List<ReportRow> rows)=filter.Kind switch
        {
            ReportKind.GeneralContracts=>General(finance),ReportKind.PurchaseBySupplier=>Summary(finance,"PURCHASE","خرید به تفکیک تأمین‌کننده",false),
            ReportKind.SalesByCustomer=>Summary(finance,"SALE","فروش به تفکیک مشتری",true),ReportKind.OutsourceByContractor=>Summary(finance,"OUTSOURCE","برون‌سپاری به تفکیک پیمانکار",false),
            ReportKind.Payments=>Settlements(finance,SettlementKind.Payment),ReportKind.Receipts=>Settlements(finance,SettlementKind.Receipt),
            ReportKind.Guarantees=>Guarantees(operations),ReportKind.ExpiringContracts=>Expiring(filter,operations),ReportKind.ProjectFinance=>Projects(operations),
            ReportKind.ContractorEvaluation=>Evaluations(operations),ReportKind.Invoices=>Invoices(operations),ReportKind.Amendments=>Amendments(operations),
            ReportKind.Documents=>Documents(operations),ReportKind.Alerts=>Alerts(operations),_=>throw new ArgumentOutOfRangeException(nameof(filter))
        };
        if(rows.Count>filter.MaxRows)rows=rows.Take(filter.MaxRows).ToList();
        return new(snapshotId,generatedAt,today,filter.Kind,title,Parameters(filter,today),columns,rows,Footer(columns,rows));
    }

    private (string,IReadOnlyList<ReportColumn>,List<ReportRow>) General(CanonicalFinancialReportSnapshot s)
    {
        ReportColumn[] c=[C("no","شماره قرارداد"),C("title","عنوان"),C("type","نوع"),C("party","طرف"),C("project","پروژه"),N("final","مبلغ نهایی"),N("paid","پرداخت"),N("received","دریافت"),N("receivable","مانده طلب"),N("payable","مانده بدهی"),N("context","مانده مرتبط"),C("status","وضعیت")];
        Dictionary<long,Totals> totals=TotalsByContract(s);List<ReportRow> r=s.Contracts.Select(x=>{Totals t=totals.GetValueOrDefault(x.Id,Totals.Zero);long? context=x.TypeCode=="SALE"?t.Receivable:x.TypeCode is"PURCHASE"or"OUTSOURCE"?t.Payable:null;return R(T(x.Number),T(x.Title),T(x.TypeTitle),T(x.PartyName),T(x.ProjectTitle),M(x.FinalAmountRial),M(t.Paid),M(t.Received),M(t.Receivable),M(t.Payable),context.HasValue?M(context.Value):T("—"),T(x.StatusTitle));}).ToList();return("گزارش عمومی قراردادها",c,r);
    }
    private (string,IReadOnlyList<ReportColumn>,List<ReportRow>) Summary(CanonicalFinancialReportSnapshot s,string type,string title,bool claim)
    {
        ReportColumn[] c=[C("party",claim?"مشتری":type=="PURCHASE"?"تأمین‌کننده":"پیمانکار"),N("count","تعداد"),N("obligation",claim?"طلب":"بدهی"),N("settled",claim?"دریافت":"پرداخت"),N("balance","مانده")];
        Dictionary<long,Totals> totals=TotalsByContract(s);List<ReportRow> r=s.Contracts.Where(x=>x.TypeCode==type).GroupBy(x=>new{x.PartyId,x.PartyName}).OrderBy(x=>x.Key.PartyName).Select(g=>{Totals[] values=g.Select(x=>totals.GetValueOrDefault(x.Id,Totals.Zero)).ToArray();long obligation=Sum(values,x=>claim?x.Claims:x.Debts),settled=Sum(values,x=>claim?x.Received:x.Paid),balance=Sum(values,x=>claim?x.Receivable:x.Payable);return R(T(g.Key.PartyName),M(g.Count()),M(obligation),M(settled),M(balance));}).ToList();return(title,c,r);
    }
    private (string,IReadOnlyList<ReportColumn>,List<ReportRow>) Settlements(CanonicalFinancialReportSnapshot s,SettlementKind kind)
    {ReportColumn[] c=[C("contract","قرارداد"),C("party","طرف"),C("number","شماره"),D("date","تاریخ"),N("amount","مبلغ"),N("allocated","تخصیص‌یافته"),N("unallocated","تخصیص‌نیافته"),C("method","روش"),C("bank","بانک"),C("reference","پیگیری")];Dictionary<long,CanonicalReportContract> contracts=s.Contracts.ToDictionary(x=>x.Id);List<ReportRow> r=s.Settlements.Where(x=>x.Kind==kind).Select(x=>{CanonicalReportContract h=contracts[x.ContractId];return R(T(h.Number),T(h.PartyName),T(x.Number),Date(x.Date),M(x.AmountRial),M(x.AllocatedRial),M(x.UnallocatedRial),T(x.PaymentType),T(x.Bank),T(x.ReferenceNo));}).ToList();return(kind==SettlementKind.Payment?"گزارش پرداخت‌ها":"گزارش دریافت‌ها",c,r);}
    private (string,IReadOnlyList<ReportColumn>,List<ReportRow>) Guarantees(OperationalReportSnapshot s)=>Simple("گزارش ضمانت‌نامه‌ها",[C("contract","قرارداد"),C("party","طرف"),C("type","نوع"),C("number","شماره"),C("bank","بانک"),D("issue","صدور"),D("expiry","انقضا"),N("amount","مبلغ"),C("status","وضعیت")],s.Guarantees.Select(x=>R(T(x.ContractNo),T(x.PartyName),T(x.Type),T(x.Number),T(x.Bank),Date(x.IssueDate),Date(x.ExpiryDate),M(x.AmountRial),T(x.Status))));
    private (string,IReadOnlyList<ReportColumn>,List<ReportRow>) Expiring(ReportFilter filter,OperationalReportSnapshot s)=>Simple("قراردادهای در آستانه انقضا",[C("contract","قرارداد"),C("title","عنوان"),C("party","طرف"),C("type","نوع"),D("base","پایان پایه"),D("effective","پایان مؤثر"),C("status","وضعیت")],s.ExpiringContracts.Select(x=>new{x,Effective=ContractTimelinePolicy.EffectiveEndDate(x.BaseEndDate,x.ExtensionDays)}).Where(x=>(!filter.DateFrom.HasValue||x.Effective>=filter.DateFrom)&&(!filter.DateTo.HasValue||x.Effective<=filter.DateTo)).Select(x=>R(T(x.x.ContractNo),T(x.x.Title),T(x.x.PartyName),T(x.x.Type),Date(x.x.BaseEndDate),Date(x.Effective),T(x.x.Status))));
    private (string,IReadOnlyList<ReportColumn>,List<ReportRow>) Projects(OperationalReportSnapshot s)=>Simple("گزارش مالی پروژه‌ها",[C("code","کد"),C("project","پروژه"),C("year","سال مالی"),C("currency","ارز"),N("approved","مصوب"),N("consumed","مصرف"),N("remaining","باقی‌مانده")],s.ProjectFinance.Select(x=>R(T(x.ProjectCode),T(x.ProjectTitle),T(x.FiscalYear),T(x.Currency),M(x.ApprovedMinor),M(x.ConsumedMinor),M(checked(x.ApprovedMinor-x.ConsumedMinor)))));
    private (string,IReadOnlyList<ReportColumn>,List<ReportRow>) Evaluations(OperationalReportSnapshot s)=>Simple("ارزیابی پیمانکاران",[C("contract","قرارداد"),C("party","پیمانکار"),D("date","تاریخ"),N("quality","کیفیت"),N("time","زمان"),N("financial","مالی"),N("hse","HSE"),C("total","امتیاز کل")],s.Evaluations.Select(x=>R(T(x.ContractNo),T(x.PartyName),Date(x.Date),M(x.Quality),M(x.Time),M(x.Financial),M(x.Hse),T(EvaluationScorePolicy.Total(x.Quality,x.Time,x.Financial,x.Hse).ToString("0.0",CultureInfo.InvariantCulture)))));
    private (string,IReadOnlyList<ReportColumn>,List<ReportRow>) Invoices(OperationalReportSnapshot s)=>Simple("گزارش صورتحساب‌ها",[C("contract","قرارداد"),C("party","طرف"),C("number","شماره"),D("date","تاریخ"),C("type","نوع"),N("amount","مبلغ"),N("vat","ارزش افزوده"),N("total","جمع"),C("status","پرداخت")],s.Invoices.Select(x=>R(T(x.ContractNo),T(x.PartyName),T(x.Number),Date(x.Date),T(x.Type),M(x.AmountRial),M(x.VatRial),M(InvoiceCalculationPolicy.Total(x.AmountRial,x.VatRial)),T(x.PaymentStatus))));
    private (string,IReadOnlyList<ReportColumn>,List<ReportRow>) Amendments(OperationalReportSnapshot s)=>Simple("گزارش الحاقیه‌ها",[C("contract","قرارداد"),C("number","شماره"),D("date","تاریخ"),C("subject","موضوع"),N("amount","تغییر مبلغ"),N("extension","تمدید روز")],s.Amendments.Select(x=>R(T(x.ContractNo),T(x.Number),Date(x.Date),T(x.Subject),M(x.AmountChangeRial),M(x.ExtensionDays))));
    private (string,IReadOnlyList<ReportColumn>,List<ReportRow>) Documents(OperationalReportSnapshot s)=>Simple("گزارش اسناد",[C("contract","قرارداد"),C("type","نوع"),C("name","نام اصلی"),N("size","اندازه بایت"),C("hash","SHA-256"),C("missing","مفقود"),C("uploaded","زمان بارگذاری UTC")],s.Documents.Select(x=>R(T(x.ContractNo),T(x.Type),T(x.OriginalName),M(x.Size),T(x.Sha256),T(x.IsMissing?"بله":"خیر"),T(x.UploadedAtUtc.ToString("u",CultureInfo.InvariantCulture)))));
    private (string,IReadOnlyList<ReportColumn>,List<ReportRow>) Alerts(OperationalReportSnapshot s)=>Simple("گزارش هشدارها",[C("contract","قرارداد"),C("type","نوع"),C("title","عنوان"),D("due","سررسید"),C("priority","اولویت"),C("status","وضعیت"),C("key","کلید یکتا")],s.Alerts.Select(x=>R(T(x.ContractNo),T(x.Type),T(x.Title),Date(x.DueDate),T(x.Priority),T(x.Status),T(x.DeduplicationKey))));
    private static (string,IReadOnlyList<ReportColumn>,List<ReportRow>) Simple(string title,IReadOnlyList<ReportColumn> columns,IEnumerable<ReportRow> rows)=>(title,columns,rows.ToList());
    private static Dictionary<long,Totals> TotalsByContract(CanonicalFinancialReportSnapshot s)
    {Dictionary<long,Totals> result=[];foreach(CanonicalReportObligation x in s.Obligations){Totals t=result.GetValueOrDefault(x.ContractId,Totals.Zero);result[x.ContractId]=x.TypeId switch{1=>t with{Claims=checked(t.Claims+x.AmountRial),Receivable=checked(t.Receivable+x.RemainingRial)},2=>t with{Debts=checked(t.Debts+x.AmountRial),Payable=checked(t.Payable+x.RemainingRial)},_=>t};}foreach(CanonicalReportSettlement x in s.Settlements){Totals t=result.GetValueOrDefault(x.ContractId,Totals.Zero);result[x.ContractId]=x.Kind==SettlementKind.Payment?t with{Paid=checked(t.Paid+x.AmountRial)}:t with{Received=checked(t.Received+x.AmountRial)};}return result;}
    private static long Sum(IEnumerable<Totals> values,Func<Totals,long> selector)=>values.Aggregate(0L,(total,item)=>checked(total+selector(item)));
    private ReportFooter Footer(IReadOnlyList<ReportColumn> columns,List<ReportRow> rows)=>new(columns.Select((c,i)=>c.IsTotaled?M(rows.Sum(r=>r.Cells[i].IntegerValue??0)):i==0?T($"جمع / {numbers.FormatMoney(rows.Count)} ردیف"):T("")).ToList());
    private string Parameters(ReportFilter f,DateOnly today)=>$"بازه: {(f.DateFrom.HasValue?dates.Format(f.DateFrom.Value):"ابتدا")} تا {(f.DateTo.HasValue?dates.Format(f.DateTo.Value):"انتهای داده")} | نوع: {f.ContractTypeId?.ToString(CultureInfo.InvariantCulture)??"همه"} | وضعیت: {f.ContractStatusId?.ToString(CultureInfo.InvariantCulture)??"همه"} | طرف: {f.PartyId?.ToString(CultureInfo.InvariantCulture)??"همه"} | پروژه: {f.ProjectId?.ToString(CultureInfo.InvariantCulture)??"همه"} | تاریخ کسب‌وکار: {dates.Format(today)} | زمان audit/export: UTC";
    private static bool NeedsOperational(ReportKind k)=>k>=ReportKind.Guarantees;
    private static bool NeedsFinancial(ReportKind k)=>k<=ReportKind.Receipts;
    private static CanonicalFinancialReportSnapshot EmptyFinancial()=>new([],[],[]);
    private static OperationalReportSnapshot EmptyOperational()=>new([],[],[],[],[],[],[],[]);
    private static ReportColumn C(string key,string title)=>new(key,title,ReportValueKind.Text);private static ReportColumn D(string key,string title)=>new(key,title,ReportValueKind.Date);private static ReportColumn N(string key,string title)=>new(key,title,ReportValueKind.Numeric,true);
    private static ReportCell T(string? value)=>new(value??"");private ReportCell M(long value)=>new(numbers.FormatMoney(value),value);private ReportCell Date(DateOnly value)=>new(dates.Format(value));private static ReportRow R(params ReportCell[] cells)=>new(cells);
    private sealed record Totals(long Claims,long Debts,long Paid,long Received,long Receivable,long Payable){public static Totals Zero{get;}=new(0,0,0,0,0,0);}
}

public sealed class ReportingExportService(IEnumerable<IReportExporter> exporters,IAuthorizationService authorization,
    ICurrentUserService currentUser,IAuditWriter audit,ICorrelationIdProvider correlation)
{
    public async Task ExportAsync(ReportExportRequest request,CancellationToken ct)
    {
        await authorization.RequireAsync(SecuritySection.Reports,SecurityAction.Export,ct);IReportExporter provider=exporters.SingleOrDefault(x=>x.Format==request.Format)??throw new NotSupportedException($"Exporter {request.Format} is not registered.");long user=currentUser.RequireAuthenticated().UserId;string id=correlation.Create();
        try{await provider.ExportAsync(request.Report,request.DestinationPath,ct);await audit.WriteAsync(new(user,"SENSITIVE_EXPORT","REPORT",request.Report.SnapshotId.ToString("N"),id,"SUCCESS",null,JsonSerializer.Serialize(new{format=request.Format.ToString(),rows=request.Report.Rows.Count,kind=request.Report.Kind.ToString()})),ct);}
        catch{await audit.WriteAsync(new(user,"SENSITIVE_EXPORT","REPORT",request.Report.SnapshotId.ToString("N"),id,"FAILURE",null,JsonSerializer.Serialize(new{format=request.Format.ToString(),kind=request.Report.Kind.ToString()})),CancellationToken.None);throw;}
    }
}
