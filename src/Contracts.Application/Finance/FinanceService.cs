using Contracts.Application.MasterData;
using Contracts.Application.Security;
using Contracts.Domain.Common;
using Contracts.Domain.ReferenceData;

namespace Contracts.Application.Finance;

public sealed class FinanceService(IFinanceStore store, IAuthorizationService authorization,
    ICurrentUserService currentUser, ISystemClock clock, IBusinessDateService businessDate)
{
    public async Task<CanonicalFinancialReportSnapshot> ReportingSnapshotAsync(CanonicalReportFilter filter,CancellationToken ct)
    {await authorization.RequireAsync(SecuritySection.Reports,SecurityAction.View,ct);filter.Validate();return await store.ReportingSnapshotAsync(filter,businessDate.Today,ct);}
    public async Task<FinanceReferenceData> ReferencesAsync(CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.FinancialLedger, SecurityAction.View, ct); return await store.ReferencesAsync(ct); }

    public async Task<PageResult<ObligationRow>> SearchObligationsAsync(LedgerFilter filter, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.FinancialLedger, SecurityAction.View, ct); filter.Validate(); return await store.SearchObligationsAsync(filter, businessDate.Today, ct); }

    public async Task<ObligationRow?> GetObligationAsync(long id, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.FinancialLedger, SecurityAction.View, ct); return await store.GetObligationAsync(id, businessDate.Today, ct); }

    public async Task<ObligationRow> CreateObligationAsync(ObligationInput input, CancellationToken ct)
    {
        await authorization.RequireAsync(SecuritySection.FinancialLedger, SecurityAction.Add, ct);
        ValidateObligation(input.TransactionDate, input.DueDate, input.AmountRial);
        string contractType = await store.ContractTypeCodeAsync(input.ContractId, ct) ?? throw new KeyNotFoundException("Contract was not found.");
        long? suggested = SuggestedType(contractType);
        if (suggested.HasValue && input.TransactionTypeId != suggested)
        {
            if (!input.ExplicitTypeOverride) throw new MasterDataValidationException(["Changing the suggested transaction type requires explicit confirmation."]);
            await authorization.RequireAsync(SecuritySection.FinancialLedger, SecurityAction.Admin, ct);
        }
        return await store.CreateObligationAsync(input, Stamp(), businessDate.Today, ct);
    }

    public async Task<ObligationRow> UpdateObligationAsync(ObligationUpdate input, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.FinancialLedger, SecurityAction.Edit, ct); ValidateObligation(input.TransactionDate, input.DueDate, input.AmountRial); return await store.UpdateObligationAsync(input, Stamp(), businessDate.Today, ct); }

    public async Task DeleteObligationAsync(long id, long version, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.FinancialLedger, SecurityAction.Delete, ct); await store.DeleteObligationAsync(id, version, Stamp(), ct); }

    public async Task<IReadOnlyList<ObligationRow>> OpenObligationsAsync(long contractId, SettlementKind kind, CancellationToken ct)
    { await authorization.RequireAsync(Section(kind), SecurityAction.View, ct); return await store.OpenObligationsAsync(contractId, kind, businessDate.Today, ct); }

    public async Task<PageResult<SettlementRow>> SearchSettlementsAsync(SettlementKind kind, long? contractId, int page, int pageSize, CancellationToken ct)
    { await authorization.RequireAsync(Section(kind), SecurityAction.View, ct); ValidatePage(page, pageSize); return await store.SearchSettlementsAsync(kind, contractId, page, pageSize, ct); }

    public async Task<SettlementRow?> GetSettlementAsync(SettlementKind kind, long id, CancellationToken ct)
    { await authorization.RequireAsync(Section(kind), SecurityAction.View, ct); return await store.GetSettlementAsync(kind, id, ct); }

    public async Task<SettlementRow> CreateSettlementAsync(SettlementKind kind, SettlementInput input, CancellationToken ct)
    { await authorization.RequireAsync(Section(kind), SecurityAction.Add, ct); ValidateSettlement(input); return await store.CreateSettlementAsync(kind, input, Stamp(), ct); }

    public async Task<SettlementRow> UpdateSettlementAsync(SettlementKind kind, SettlementUpdate input, CancellationToken ct)
    { await authorization.RequireAsync(Section(kind), SecurityAction.Edit, ct); ValidateSettlement(input.Values); return await store.UpdateSettlementAsync(kind, input, Stamp(), ct); }

    public async Task DeleteSettlementAsync(SettlementKind kind, long id, long version, CancellationToken ct)
    { await authorization.RequireAsync(Section(kind), SecurityAction.Delete, ct); await store.DeleteSettlementAsync(kind, id, version, Stamp(), ct); }

    public async Task<global::Contracts.Application.Contracts.FinancialSummaryDto> SummaryAsync(long contractId, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Contracts, SecurityAction.View, ct); return await store.SummaryAsync(contractId, ct); }

    public static long? SuggestedType(string contractTypeCode) => contractTypeCode switch
    { "SALE" => ReferenceIds.TransactionTypes.Claim, "PURCHASE" or "OUTSOURCE" => ReferenceIds.TransactionTypes.Debt, _ => null };

    private static SecuritySection Section(SettlementKind kind) => kind == SettlementKind.Payment ? SecuritySection.Payments : SecuritySection.Receipts;
    private AuditStamp Stamp() => new(currentUser.RequireAuthenticated().UserId, clock.UtcNow);
    private static void ValidateObligation(DateOnly date, DateOnly due, long amount)
    { if (due < date) throw new MasterDataValidationException(["Due date cannot be before transaction date."]); if (amount <= 0) throw new MasterDataValidationException(["Amount must be positive."]); }
    private static void ValidatePage(int page, int size) { if (page < 1 || size is < 1 or > 200) throw new ArgumentOutOfRangeException(nameof(page)); }
    private static void ValidateSettlement(SettlementInput input)
    {
        List<string> errors = [];
        if (string.IsNullOrWhiteSpace(input.Number)) errors.Add("Settlement number is required.");
        if (input.AmountRial <= 0) errors.Add("Amount must be positive.");
        if (input.Allocations.Any(item => item.AmountRial <= 0)) errors.Add("Allocation amounts must be positive.");
        long allocated;
        try { allocated = input.Allocations.Aggregate(0L, (sum, item) => checked(sum + item.AmountRial)); }
        catch (OverflowException) { errors.Add("Allocation total is outside the supported range."); allocated = long.MaxValue; }
        if (allocated > input.AmountRial) errors.Add("Allocated amount cannot exceed settlement amount.");
        if (input.Allocations.Select(item => item.ObligationId).Distinct().Count() != input.Allocations.Count) errors.Add("An obligation can appear only once.");
        if (errors.Count > 0) throw new MasterDataValidationException(errors);
    }
}
