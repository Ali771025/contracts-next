using Contracts.Application.MasterData;
using Contracts.Application.Presentation;
using Contracts.Domain.Common;

namespace Contracts.Application.Contracts;

public sealed record ContractFilter(int Page = 1, int PageSize = 25, string? Search = null, long? TypeId = null,
    long? StatusId = null, long? PartyId = null, long? ProjectId = null, DateOnly? EndDateFrom = null,
    DateOnly? EndDateTo = null, string Sort = "number", bool Descending = false)
{
    public int Skip => checked((Page - 1) * PageSize);
    public void Validate()
    {
        if (Page < 1) throw new ArgumentOutOfRangeException(nameof(Page));
        if (PageSize is < 1 or > 200) throw new ArgumentOutOfRangeException(nameof(PageSize));
        if (EndDateFrom.HasValue && EndDateTo.HasValue && EndDateTo < EndDateFrom)
            throw new MasterDataValidationException(["بازه تاریخ فیلتر نامعتبر است."]);
    }
}

public sealed record ContractInput(string ContractNo, string Title, long ContractTypeId, long? ContractCategoryId,
    long? ProjectId, long PartyId, long ContractStatusId, long CurrencyId, long OriginalAmountMinor,
    long? ExchangeRateRialPerUnit, DateOnly StartDate, DateOnly EndDate, long? ManagerUserId, string? Description);
public sealed record ContractUpdate(long Id, long ConcurrencyVersion, ContractInput Values);

public sealed record ContractListItem(long Id, long ConcurrencyVersion, string ContractNo, string Title,
    string TypeTitle, string? CategoryTitle, string PartyName, string? ProjectName, string CurrencyCode,
    int CurrencyScale, long BaseAmountRial, long FinalAmountRial, string StatusTitle, DateOnly EndDate);

public sealed record ContractDetail(long Id, long ConcurrencyVersion, string ContractNo, string Title,
    long ContractTypeId, long? ContractCategoryId, long? ProjectId, long PartyId, long ContractStatusId,
    long CurrencyId, string CurrencyCode, int CurrencyScale, long OriginalAmountMinor,
    long ExchangeRateRialPerUnit, long BaseAmountRial, long FinalAmountRial, DateOnly StartDate,
    DateOnly EndDate, DateOnly EffectiveEndDate, int DurationDays, long? ManagerUserId, string? Description,
    long CreatedByUserId, string CreatedByDisplayName, DateTimeOffset CreatedAtUtc);

public sealed record ContractOption(long Id, string Title);
public sealed record CurrencyOption(long Id, string Code, string Title, int Scale);
public sealed record ContractReferenceData(IReadOnlyList<SimpleLookupDto> Types,
    IReadOnlyList<SimpleLookupDto> Categories, IReadOnlyList<SimpleLookupDto> Statuses,
    IReadOnlyList<CurrencyOption> Currencies, IReadOnlyList<ContractOption> Parties,
    IReadOnlyList<ContractOption> Projects);

public enum FinancialSummaryState { Unavailable, Available }
public sealed record FinancialSummaryDto(FinancialSummaryState State, long? TotalClaims, long? TotalDebts,
    long? TotalPaid, long? TotalReceived, long? ReceivableBalance, long? PayableBalance, long? ContextualBalance)
{
    public static FinancialSummaryDto Unavailable { get; } = new(FinancialSummaryState.Unavailable,
        null, null, null, null, null, null, null);
}

public sealed record AmendmentRow(string Number, DateOnly Date, long AmountChangeRial, int TimeExtensionDays);
public sealed record PaymentRow(DateOnly Date, long AmountRial, string ReferenceNo);
public sealed record ReceiptRow(DateOnly Date, long AmountRial, string ReferenceNo);
public sealed record InvoiceRow(string Number, DateOnly Date, long TotalAmountRial, DateOnly? DueDate);
public sealed record GuaranteeRow(string Number, long AmountRial, DateOnly ExpiryDate);
public sealed record DocumentRow(string FileName, string StoragePath);
public sealed record ContractHistoryRow(DateTimeOffset TimestampUtc, string Action, string ActorName,
    string Outcome, string? ChangedFieldsJson);
public sealed record ContractRelatedData(IReadOnlyList<AmendmentRow> Amendments,
    IReadOnlyList<PaymentRow> Payments, IReadOnlyList<ReceiptRow> Receipts,
    IReadOnlyList<InvoiceRow> Invoices, IReadOnlyList<GuaranteeRow> Guarantees,
    IReadOnlyList<DocumentRow> Documents, IReadOnlyList<ContractHistoryRow> History,
    FinancialSummaryDto FinancialSummary);

public interface IContractStore
{
    public Task<PageResult<ContractListItem>> SearchAsync(ContractFilter filter, CancellationToken ct);
    public Task<ContractDetail?> GetAsync(long id, CancellationToken ct);
    public Task<ContractReferenceData> ReferencesAsync(CancellationToken ct);
    public Task<ContractRelatedData> RelatedAsync(long id, CancellationToken ct);
    public Task<ContractDetail> CreateAsync(ContractInput input, int currencyScale, long effectiveRate, AuditStamp stamp, CancellationToken ct);
    public Task<ContractDetail> UpdateAsync(ContractUpdate update, int currencyScale, long effectiveRate, AuditStamp stamp, CancellationToken ct);
    public Task DeleteAsync(long id, long version, AuditStamp stamp, CancellationToken ct);
    public Task<bool> HasFinancialDependenciesAsync(long id, CancellationToken ct);
    public Task<RecordNeighbor<long>> AdjacentAsync(long id, RecordDirection direction, ContractFilter filter, string stableSort, CancellationToken ct);
}
