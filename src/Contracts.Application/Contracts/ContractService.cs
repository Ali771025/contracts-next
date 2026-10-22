using Contracts.Application.MasterData;
using Contracts.Application.Presentation;
using Contracts.Application.Security;
using Contracts.Domain.Common;
using Contracts.Domain.Contracts;
using Contracts.Application.Finance;

namespace Contracts.Application.Contracts;

public sealed class ContractService(IContractStore store, IAuthorizationService authorization,
    ICurrentUserService currentUser, ISystemClock clock, IFinanceStore financeStore)
{
    public async Task<PageResult<ContractListItem>> SearchAsync(ContractFilter filter, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Contracts, SecurityAction.View, ct); filter.Validate(); return await store.SearchAsync(filter, ct); }
    public async Task<ContractDetail?> GetAsync(long id, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Contracts, SecurityAction.View, ct); return await store.GetAsync(id, ct); }
    public async Task<ContractReferenceData> ReferencesAsync(CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Contracts, SecurityAction.View, ct); return await store.ReferencesAsync(ct); }
    public async Task<ContractRelatedData> RelatedAsync(long id, CancellationToken ct)
    {
        await authorization.RequireAsync(SecuritySection.Contracts, SecurityAction.View, ct);
        ContractRelatedData related = await store.RelatedAsync(id, ct);
        return related with { FinancialSummary = await financeStore.SummaryAsync(id, ct) };
    }
    public async Task<ContractDetail> CreateAsync(ContractInput input, CancellationToken ct)
    {
        await authorization.RequireAsync(SecuritySection.Contracts, SecurityAction.Add, ct);
        (int scale, long rate) = await ValidateAsync(input, ct);
        return await store.CreateAsync(input, scale, rate, Stamp(), ct);
    }
    public async Task<ContractDetail> UpdateAsync(ContractUpdate update, CancellationToken ct)
    {
        await authorization.RequireAsync(SecuritySection.Contracts, SecurityAction.Edit, ct);
        ContractDetail current = await store.GetAsync(update.Id, ct) ?? throw new KeyNotFoundException("Contract was not found.");
        (int scale, long rate) = await ValidateAsync(update.Values, ct);
        if (current.CurrencyId != update.Values.CurrencyId && await store.HasFinancialDependenciesAsync(update.Id, ct))
            throw new MasterDataDependencyException("ارز قرارداد دارای رکورد مالی قابل تغییر نیست.");
        return await store.UpdateAsync(update, scale, rate, Stamp(), ct);
    }
    public async Task DeleteAsync(long id, long version, CancellationToken ct)
    {
        await authorization.RequireAsync(SecuritySection.Contracts, SecurityAction.Delete, ct);
        await store.DeleteAsync(id, version, Stamp(), ct);
    }
    public async Task<RecordNeighbor<long>> AdjacentAsync(long id, RecordDirection direction, ContractFilter filter, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Contracts, SecurityAction.View, ct); filter.Validate(); return await store.AdjacentAsync(id, direction, filter, "NormalizedContractNo,Id", ct); }

    private AuditStamp Stamp() => new(currentUser.RequireAuthenticated().UserId, clock.UtcNow);
    private async Task<(int Scale, long Rate)> ValidateAsync(ContractInput input, CancellationToken ct)
    {
        List<string> errors = [];
        if (string.IsNullOrWhiteSpace(input.ContractNo)) errors.Add("شماره قرارداد الزامی است.");
        if (string.IsNullOrWhiteSpace(input.Title)) errors.Add("عنوان قرارداد الزامی است.");
        if (input.EndDate < input.StartDate) errors.Add("پایان قرارداد نمی‌تواند پیش از شروع باشد.");
        if (input.OriginalAmountMinor < 0) errors.Add("مبلغ قرارداد نمی‌تواند منفی باشد.");
        ContractReferenceData refs = await store.ReferencesAsync(ct);
        CurrencyOption? currency = refs.Currencies.SingleOrDefault(item => item.Id == input.CurrencyId);
        if (currency is null) errors.Add("ارز فعال معتبر نیست.");
        if (!refs.Types.Any(item => item.Id == input.ContractTypeId)) errors.Add("نوع قرارداد معتبر نیست.");
        if (!refs.Statuses.Any(item => item.Id == input.ContractStatusId)) errors.Add("وضعیت قرارداد معتبر نیست.");
        if (!refs.Parties.Any(item => item.Id == input.PartyId)) errors.Add("طرف قرارداد معتبر نیست.");
        if (input.ProjectId.HasValue && !refs.Projects.Any(item => item.Id == input.ProjectId)) errors.Add("پروژه معتبر نیست.");
        if (input.ContractCategoryId.HasValue && !refs.Categories.Any(item => item.Id == input.ContractCategoryId)) errors.Add("دسته‌بندی معتبر نیست.");
        if (errors.Count > 0) throw new MasterDataValidationException(errors);

        int scale = currency!.Scale;
        long rate = currency.Code == "IRR" ? 1 : input.ExchangeRateRialPerUnit ?? 0;
        if (currency.Code != "IRR" && (input.OriginalAmountMinor <= 0 || rate <= 0))
            throw new MasterDataValidationException(["برای ارز خارجی، مبلغ جزء ارز و نرخ ریالی باید بزرگ‌تر از صفر باشد."]);
        _ = MoneyConverter.ToRial(input.OriginalAmountMinor, scale, rate);
        return (scale, rate);
    }
}
