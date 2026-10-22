using Contracts.Application.Contracts;
using Contracts.Application.MasterData;
using Contracts.Application.Presentation;
using Contracts.Domain.Common;

namespace Contracts.Application.Finance;

public enum SettlementKind { Payment, Receipt }

public sealed record LedgerFilter(int Page = 1, int PageSize = 25, string? Search = null,
    long? ContractId = null, long? TransactionTypeId = null, long? StatusId = null,
    DateOnly? DueFrom = null, DateOnly? DueTo = null, bool? IsOverdue = null,
    string Sort = "due", bool Descending = false)
{
    public int Skip => checked((Page - 1) * PageSize);
    public void Validate()
    {
        if (Page < 1 || PageSize is < 1 or > 200) throw new ArgumentOutOfRangeException(nameof(Page));
        if (DueFrom.HasValue && DueTo.HasValue && DueTo < DueFrom) throw new ArgumentException("Invalid due-date range.");
    }
}

public sealed record ObligationInput(long ContractId, long TransactionTypeId, DateOnly TransactionDate,
    DateOnly DueDate, long AmountRial, string? Description, bool ExplicitTypeOverride = false);
public sealed record ObligationUpdate(long Id, long ConcurrencyVersion, DateOnly TransactionDate,
    DateOnly DueDate, long AmountRial, string? Description);
public sealed record ObligationRow(long Id, long ConcurrencyVersion, string TransactionNo, long ContractId,
    string ContractNo, string ContractTitle, string ContractTypeTitle, string PartyName,
    long TransactionTypeId, string TransactionTypeCode, string TransactionTypeTitle,
    DateOnly TransactionDate, DateOnly DueDate, long AmountRial, long AllocatedAmountRial,
    long RemainingAmountRial, string StatusCode, string StatusTitle, bool IsOverdue, int DaysOverdue,
    string? Description);

public sealed record AllocationInput(long ObligationId, long AmountRial);
public sealed record SettlementInput(long ContractId, string Number, DateOnly Date, long AmountRial,
    long PaymentTypeId, long? BankId, string? ReferenceNo, string? InvoiceNo, string? ChequeNo,
    string? PayerDescription, string? Description, string? AttachmentPath,
    IReadOnlyList<AllocationInput> Allocations);
public sealed record SettlementUpdate(long Id, long ConcurrencyVersion, SettlementInput Values);
public sealed record SettlementRow(long Id, long ConcurrencyVersion, SettlementKind Kind, long ContractId,
    string ContractNo, string Number, DateOnly Date, long AmountRial, long AllocatedAmountRial,
    long UnallocatedAmountRial, string PaymentTypeTitle, string? BankTitle, string? ReferenceNo, string? InvoiceNo,
    string? ChequeNo, string? PayerDescription, string? Description, string? AttachmentPath,
    long RecordedByUserId, string RecordedByDisplayName, DateTimeOffset RecordedAtUtc,
    IReadOnlyList<SettlementAllocationRow> Allocations);
public sealed record SettlementAllocationRow(long ObligationId, string TransactionNo, DateOnly DueDate,
    long ObligationAmountRial, long AmountRial, long ObligationRemainingAfterRial);

public sealed record FinanceContractOption(long Id, string ContractNo, string Title, string TypeCode,
    string TypeTitle, string PartyName);
public sealed record FinanceReferenceData(IReadOnlyList<FinanceContractOption> Contracts,
    IReadOnlyList<SimpleLookupDto> TransactionTypes, IReadOnlyList<SimpleLookupDto> Statuses,
    IReadOnlyList<SimpleLookupDto> PaymentTypes, IReadOnlyList<SimpleLookupDto> Banks);

public interface IBusinessDateService { public DateOnly Today { get; } }

public enum CanonicalReportPurpose{Summary,Payments,Receipts}
public sealed record CanonicalReportFilter(DateOnly? DateFrom=null,DateOnly? DateTo=null,long? ContractTypeId=null,
    long? ContractStatusId=null,long? PartyId=null,long? ProjectId=null,int MaxRows=5000,CanonicalReportPurpose Purpose=CanonicalReportPurpose.Summary)
{
    public void Validate(){if(DateFrom.HasValue&&DateTo.HasValue&&DateTo<DateFrom)throw new ArgumentException("Invalid report date range.");if(MaxRows is<1 or>20_000)throw new ArgumentOutOfRangeException(nameof(MaxRows));}
}
public sealed record CanonicalReportContract(long Id,string Number,string Title,long TypeId,string TypeCode,string TypeTitle,
    long StatusId,string StatusTitle,long PartyId,string PartyName,long? ProjectId,string? ProjectTitle,long BaseAmountRial,long FinalAmountRial);
public sealed record CanonicalReportObligation(long Id,long ContractId,long TypeId,DateOnly Date,DateOnly DueDate,long AmountRial,long AllocatedRial,long RemainingRial);
public sealed record CanonicalReportSettlement(long Id,SettlementKind Kind,long ContractId,string Number,DateOnly Date,long AmountRial,long AllocatedRial,long UnallocatedRial,string PaymentType,string? Bank,string? ReferenceNo);
public sealed record CanonicalFinancialReportSnapshot(IReadOnlyList<CanonicalReportContract> Contracts,
    IReadOnlyList<CanonicalReportObligation> Obligations,IReadOnlyList<CanonicalReportSettlement> Settlements);

public static class FinancialProjectionPolicy
{
    public static long Difference(long amountRial, long allocatedRial) => checked(amountRial - allocatedRial);

    public static long Remaining(long amountRial, long allocatedRial)
    {
        long remaining = Difference(amountRial, allocatedRial);
        if (remaining < 0) throw new InvalidOperationException("Allocated amount exceeds source amount.");
        return remaining;
    }
}

public interface IFinanceStore
{
    public Task<CanonicalFinancialReportSnapshot> ReportingSnapshotAsync(CanonicalReportFilter filter,DateOnly today,CancellationToken ct);
    public Task<FinanceReferenceData> ReferencesAsync(CancellationToken ct);
    public Task<string?> ContractTypeCodeAsync(long contractId, CancellationToken ct);
    public Task<PageResult<ObligationRow>> SearchObligationsAsync(LedgerFilter filter, DateOnly today, CancellationToken ct);
    public Task<ObligationRow?> GetObligationAsync(long id, DateOnly today, CancellationToken ct);
    public Task<ObligationRow> CreateObligationAsync(ObligationInput input, AuditStamp stamp, DateOnly today, CancellationToken ct);
    public Task<ObligationRow> UpdateObligationAsync(ObligationUpdate input, AuditStamp stamp, DateOnly today, CancellationToken ct);
    public Task DeleteObligationAsync(long id, long version, AuditStamp stamp, CancellationToken ct);
    public Task<IReadOnlyList<ObligationRow>> OpenObligationsAsync(long contractId, SettlementKind kind, DateOnly today, CancellationToken ct);
    public Task<PageResult<SettlementRow>> SearchSettlementsAsync(SettlementKind kind, long? contractId, int page, int pageSize, CancellationToken ct);
    public Task<SettlementRow?> GetSettlementAsync(SettlementKind kind, long id, CancellationToken ct);
    public Task<SettlementRow> CreateSettlementAsync(SettlementKind kind, SettlementInput input, AuditStamp stamp, CancellationToken ct);
    public Task<SettlementRow> UpdateSettlementAsync(SettlementKind kind, SettlementUpdate input, AuditStamp stamp, CancellationToken ct);
    public Task DeleteSettlementAsync(SettlementKind kind, long id, long version, AuditStamp stamp, CancellationToken ct);
    public Task<FinancialSummaryDto> SummaryAsync(long contractId, CancellationToken ct);
}
