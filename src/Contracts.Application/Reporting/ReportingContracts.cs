namespace Contracts.Application.Reporting;

using global::Contracts.Application.Finance;

public enum ReportKind{GeneralContracts,PurchaseBySupplier,SalesByCustomer,OutsourceByContractor,Payments,Receipts,
    Guarantees,ExpiringContracts,ProjectFinance,ContractorEvaluation,Invoices,Amendments,Documents,Alerts}
public enum ReportValueKind{Text,Numeric,Date,Boolean}
public enum ReportExportFormat{Csv,Xlsx,Pdf}

public sealed record ReportFilter(ReportKind Kind,DateOnly? DateFrom=null,DateOnly? DateTo=null,long? ContractTypeId=null,
    long? ContractStatusId=null,long? PartyId=null,long? ProjectId=null,int MaxRows=5000)
{
    public void Validate(){if(DateFrom.HasValue&&DateTo.HasValue&&DateTo<DateFrom)throw new ArgumentException("Invalid report date range.");if(MaxRows is<1 or>20_000)throw new ArgumentOutOfRangeException(nameof(MaxRows));}
    public CanonicalReportFilter ToCanonical()=>new(DateFrom,DateTo,ContractTypeId,ContractStatusId,PartyId,ProjectId,MaxRows,
        Kind==ReportKind.Payments?CanonicalReportPurpose.Payments:Kind==ReportKind.Receipts?CanonicalReportPurpose.Receipts:CanonicalReportPurpose.Summary);
}
public sealed record ReportColumn(string Key,string Title,ReportValueKind Kind,bool IsTotaled=false);
public sealed record ReportCell(string Text,long? IntegerValue=null);
public sealed record ReportRow(IReadOnlyList<ReportCell> Cells);
public sealed record ReportFooter(IReadOnlyList<ReportCell> Cells);
public sealed record ReportDocument(Guid SnapshotId,DateTimeOffset GeneratedAtUtc,DateOnly BusinessDate,ReportKind Kind,
    string Title,string ParameterSummary,IReadOnlyList<ReportColumn> Columns,IReadOnlyList<ReportRow> Rows,ReportFooter Footer);

public sealed record GuaranteeReportSource(long Id,long ContractId,string ContractNo,string PartyName,string Type,string Number,
    string? Bank,DateOnly IssueDate,DateOnly ExpiryDate,long AmountRial,string Status);
public sealed record ExpiringContractReportSource(long ContractId,string ContractNo,string Title,string PartyName,string Type,
    DateOnly BaseEndDate,int ExtensionDays,string Status);
public sealed record ProjectFinanceReportSource(long ProjectId,string ProjectCode,string ProjectTitle,string FiscalYear,string Currency,
    long ApprovedMinor,long ConsumedMinor);
public sealed record EvaluationReportSource(long Id,long ContractId,string ContractNo,string PartyName,DateOnly Date,int Quality,int Time,int Financial,int Hse);
public sealed record InvoiceReportSource(long Id,long ContractId,string ContractNo,string PartyName,string Number,DateOnly Date,string Type,long AmountRial,long VatRial,string PaymentStatus);
public sealed record AmendmentReportSource(long Id,long ContractId,string ContractNo,string Number,DateOnly Date,string? Subject,long AmountChangeRial,int ExtensionDays);
public sealed record DocumentReportSource(long Id,long ContractId,string ContractNo,string Type,string OriginalName,long Size,string? Sha256,bool IsMissing,DateTimeOffset UploadedAtUtc);
public sealed record AlertReportSource(long Id,long ContractId,string ContractNo,string Type,string Title,DateOnly DueDate,string Priority,string Status,string DeduplicationKey);
public sealed record OperationalReportSnapshot(IReadOnlyList<GuaranteeReportSource> Guarantees,IReadOnlyList<ExpiringContractReportSource> ExpiringContracts,
    IReadOnlyList<ProjectFinanceReportSource> ProjectFinance,IReadOnlyList<EvaluationReportSource> Evaluations,IReadOnlyList<InvoiceReportSource> Invoices,
    IReadOnlyList<AmendmentReportSource> Amendments,IReadOnlyList<DocumentReportSource> Documents,IReadOnlyList<AlertReportSource> Alerts);

public interface IOperationalReportSource{public Task<OperationalReportSnapshot> LoadAsync(ReportFilter filter,DateOnly businessDate,CancellationToken ct);}
public interface IReportExporter{public ReportExportFormat Format{get;}public Task ExportAsync(ReportDocument report,string destinationPath,CancellationToken ct);}
public sealed record ReportExportRequest(ReportDocument Report,ReportExportFormat Format,string DestinationPath);
