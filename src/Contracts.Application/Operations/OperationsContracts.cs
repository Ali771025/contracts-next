using Contracts.Application.MasterData;
using Contracts.Domain.Common;

namespace Contracts.Application.Operations;

public sealed record OperationContractOption(long Id,string ContractNo,string Title);
public sealed record OperationsReferenceData(IReadOnlyList<OperationContractOption> Contracts,IReadOnlyList<SimpleLookupDto> GuaranteeTypes,
    IReadOnlyList<SimpleLookupDto> GuaranteeStatuses,IReadOnlyList<SimpleLookupDto> InvoiceTypes,
    IReadOnlyList<SimpleLookupDto> PaymentStatuses,IReadOnlyList<SimpleLookupDto> DocumentTypes,
    IReadOnlyList<SimpleLookupDto> Banks);

public sealed record AmendmentInput(long ContractId,string Number,DateOnly Date,string? Subject,long AmountChangeRial,
    int TimeExtensionDays,string? Description,string? AttachmentPath);
public sealed record AmendmentUpdate(long Id,long ConcurrencyVersion,AmendmentInput Values);
public sealed record AmendmentDto(long Id,long ConcurrencyVersion,long ContractId,string ContractNo,string Number,
    DateOnly Date,string? Subject,long AmountChangeRial,int TimeExtensionDays,string? Description,string? AttachmentPath,
    long FinalAmountRial,DateOnly BaseEndDate,DateOnly EffectiveEndDate);

public sealed record GuaranteeInput(long ContractId,long TypeId,string Number,long? BankId,DateOnly IssueDate,
    DateOnly ExpiryDate,long AmountRial,long StatusId,string? Description);
public sealed record GuaranteeUpdate(long Id,long ConcurrencyVersion,GuaranteeInput Values);
public sealed record GuaranteeDto(long Id,long ConcurrencyVersion,long ContractId,string ContractNo,long TypeId,
    string TypeTitle,string Number,long? BankId,string? BankTitle,DateOnly IssueDate,DateOnly ExpiryDate,long AmountRial,
    long StatusId,string StatusTitle,bool IsExpired,string? Description);

public sealed record InvoiceInput(long ContractId,string Number,DateOnly Date,long TypeId,long AmountRial,long VatAmountRial,
    DateOnly? DueDate,long PaymentStatusId,string? Description);
public sealed record InvoiceUpdate(long Id,long ConcurrencyVersion,InvoiceInput Values);
public sealed record InvoiceDto(long Id,long ConcurrencyVersion,long ContractId,string ContractNo,string Number,
    DateOnly Date,long TypeId,string TypeTitle,long AmountRial,long VatAmountRial,long TotalAmountRial,DateOnly? DueDate,
    long PaymentStatusId,string PaymentStatusTitle,string? Description);

public sealed record DocumentUploadInput(long ContractId,long DocumentTypeId,string SourcePath);
public sealed record DocumentReplaceInput(long Id,long ConcurrencyVersion,long DocumentTypeId,string SourcePath);
public sealed record DocumentDto(long Id,long ConcurrencyVersion,long ContractId,string ContractNo,long DocumentTypeId,
    string DocumentTypeTitle,string OriginalFileName,string StoredFileName,string RelativePath,long Size,string? Sha256,
    DateTimeOffset UploadedAtUtc,long UploadedByUserId,string UploadedByDisplayName,bool IsMissing);

public sealed record AlertDto(long Id,long ConcurrencyVersion,long ContractId,string ContractNo,string TypeTitle,
    string StatusTitle,string PriorityTitle,string Title,DateOnly DueDate,string DeduplicationKey);
public sealed record AlertGenerationResult(int Created,int Reopened,int Refreshed,int Resolved);
public sealed class AlertRuleOptions { public int ContractWarningDays{get;set;}=30; public int GuaranteeWarningDays{get;set;}=30; public int FinancialWarningDays{get;set;} }

public sealed record EvaluationInput(long ContractId,DateOnly Date,int QualityScore,int TimeScore,int FinancialScore,int HseScore,string? Notes);
public sealed record EvaluationUpdate(long Id,long ConcurrencyVersion,EvaluationInput Values);
public sealed record EvaluationDto(long Id,long ConcurrencyVersion,long ContractId,string ContractNo,long PartyId,string PartyName,
    DateOnly Date,int QualityScore,int TimeScore,int FinancialScore,int HseScore,decimal TotalScore,string? Notes);

public interface IOperationsStore
{
    public Task<OperationsReferenceData> ReferencesAsync(CancellationToken ct);
    public Task<IReadOnlyList<AmendmentDto>> AmendmentsAsync(long? contractId,CancellationToken ct);
    public Task<AmendmentDto> CreateAmendmentAsync(AmendmentInput x,AuditStamp stamp,CancellationToken ct);
    public Task<AmendmentDto> UpdateAmendmentAsync(AmendmentUpdate x,AuditStamp stamp,CancellationToken ct);
    public Task DeleteAmendmentAsync(long id,long version,AuditStamp stamp,CancellationToken ct);
    public Task<IReadOnlyList<GuaranteeDto>> GuaranteesAsync(long? contractId,DateOnly today,CancellationToken ct);
    public Task<GuaranteeDto> CreateGuaranteeAsync(GuaranteeInput x,AuditStamp stamp,DateOnly today,CancellationToken ct);
    public Task<GuaranteeDto> UpdateGuaranteeAsync(GuaranteeUpdate x,AuditStamp stamp,DateOnly today,CancellationToken ct);
    public Task DeleteGuaranteeAsync(long id,long version,AuditStamp stamp,CancellationToken ct);
    public Task<IReadOnlyList<InvoiceDto>> InvoicesAsync(long? contractId,CancellationToken ct);
    public Task<InvoiceDto> CreateInvoiceAsync(InvoiceInput x,AuditStamp stamp,CancellationToken ct);
    public Task<InvoiceDto> UpdateInvoiceAsync(InvoiceUpdate x,AuditStamp stamp,CancellationToken ct);
    public Task DeleteInvoiceAsync(long id,long version,AuditStamp stamp,CancellationToken ct);
    public Task<IReadOnlyList<DocumentDto>> DocumentsAsync(long? contractId,CancellationToken ct);
    public Task<DocumentDto> UploadDocumentAsync(DocumentUploadInput x,AuditStamp stamp,CancellationToken ct);
    public Task<DocumentDto> ReplaceDocumentAsync(DocumentReplaceInput x,AuditStamp stamp,CancellationToken ct);
    public Task DeleteDocumentAsync(long id,long version,AuditStamp stamp,CancellationToken ct);
    public Task<IReadOnlyList<AlertDto>> AlertsAsync(long? contractId,CancellationToken ct);
    public Task<AlertGenerationResult> GenerateAlertsAsync(DateOnly today,AlertRuleOptions options,AuditStamp stamp,CancellationToken ct);
    public Task ResolveAlertAsync(long id,long version,AuditStamp stamp,CancellationToken ct);
    public Task ReopenAlertAsync(long id,long version,AuditStamp stamp,CancellationToken ct);
    public Task<IReadOnlyList<EvaluationDto>> EvaluationsAsync(long? contractId,CancellationToken ct);
    public Task<EvaluationDto> CreateEvaluationAsync(EvaluationInput x,AuditStamp stamp,CancellationToken ct);
    public Task<EvaluationDto> UpdateEvaluationAsync(EvaluationUpdate x,AuditStamp stamp,CancellationToken ct);
    public Task DeleteEvaluationAsync(long id,long version,AuditStamp stamp,CancellationToken ct);
}
