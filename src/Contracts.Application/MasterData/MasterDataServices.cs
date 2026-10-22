using System.Net.Mail;
using Contracts.Application.Presentation;
using Contracts.Application.Security;
using Contracts.Domain.Common;

namespace Contracts.Application.MasterData;

public sealed class PartyCrudService(IMasterDataStore store, IAuthorizationService authorization,
    ICurrentUserService currentUser, ISystemClock clock)
{
    public async Task<PageResult<PartyDto>> SearchAsync(PageRequest request, CancellationToken cancellationToken)
    { await authorization.RequireAsync(SecuritySection.Parties, SecurityAction.View, cancellationToken); request.Validate(); return await store.SearchPartiesAsync(request, cancellationToken); }
    public async Task<PartyDto?> GetAsync(long id, CancellationToken cancellationToken)
    { await authorization.RequireAsync(SecuritySection.Parties, SecurityAction.View, cancellationToken); return await store.GetPartyAsync(id, cancellationToken); }
    public async Task<PartyDto> CreateAsync(PartyInput input, CancellationToken cancellationToken)
    { await authorization.RequireAsync(SecuritySection.Parties, SecurityAction.Add, cancellationToken); await ValidateAsync(input, cancellationToken); return await store.CreatePartyAsync(Normalize(input), Stamp(), cancellationToken); }
    public async Task<PartyDto> UpdateAsync(PartyUpdate update, CancellationToken cancellationToken)
    { await authorization.RequireAsync(SecuritySection.Parties, SecurityAction.Edit, cancellationToken); await ValidateAsync(update.Values, cancellationToken); return await store.UpdatePartyAsync(update with { Values = Normalize(update.Values) }, Stamp(), cancellationToken); }
    public async Task DeleteAsync(long id, long version, CancellationToken cancellationToken)
    { await authorization.RequireAsync(SecuritySection.Parties, SecurityAction.Delete, cancellationToken); await store.DeletePartyAsync(id, version, Stamp(), cancellationToken); }
    public async Task<RecordNeighbor<long>> AdjacentAsync(long id, RecordDirection direction, string search, CancellationToken cancellationToken)
    { await authorization.RequireAsync(SecuritySection.Parties, SecurityAction.View, cancellationToken); return await store.GetAdjacentPartyAsync(id, direction, TextNormalizer.SearchKey(search ?? string.Empty), "NormalizedName,Id", cancellationToken); }
    public async Task<IReadOnlyList<SimpleLookupDto>> TypesAsync(CancellationToken cancellationToken)
    { await authorization.RequireAsync(SecuritySection.Parties, SecurityAction.View, cancellationToken); return await store.GetActiveLookupsAsync("PartyType", cancellationToken); }

    private AuditStamp Stamp() => new(currentUser.RequireAuthenticated().UserId, clock.UtcNow);
    private async Task ValidateAsync(PartyInput input, CancellationToken cancellationToken)
    {
        List<string> errors = [];
        if (string.IsNullOrWhiteSpace(input.Name)) errors.Add("نام الزامی است.");
        string type = await store.GetPartyTypeCodeAsync(input.PartyTypeId, cancellationToken) ?? string.Empty;
        if (type.Length == 0) errors.Add("نوع طرف قرارداد معتبر نیست.");
        string national = TextNormalizer.Digits(input.NationalId);
        if (national.Length > 0 && type == "NATURAL_PERSON" && !IsValidIranianNationalCode(national)) errors.Add("کد ملی شخص حقیقی معتبر نیست.");
        if (national.Length > 0 && type == "LEGAL_ENTITY" && (national.Length != 11 || national.Any(character => !char.IsAsciiDigit(character)))) errors.Add("شناسه ملی شخص حقوقی باید ۱۱ رقم باشد.");
        if (!string.IsNullOrWhiteSpace(input.Mobile) && !IsMobile(TextNormalizer.Digits(input.Mobile))) errors.Add("شماره همراه باید با 09 شروع شود و ۱۱ رقم باشد.");
        if (!string.IsNullOrWhiteSpace(input.Email) && !MailAddress.TryCreate(input.Email.Trim(), out _)) errors.Add("نشانی ایمیل معتبر نیست.");
        if (errors.Count > 0) throw new MasterDataValidationException(errors);
    }
    private static PartyInput Normalize(PartyInput input) => input with
    {
        Name = TextNormalizer.SearchKey(input.Name).Length == 0 ? input.Name : input.Name.Trim(),
        NationalId = NullIfEmpty(TextNormalizer.Digits(input.NationalId)), EconomicCode = NullIfEmpty(TextNormalizer.Digits(input.EconomicCode)),
        Mobile = NullIfEmpty(TextNormalizer.Digits(input.Mobile)), Email = NullIfEmpty(input.Email?.Trim().ToLowerInvariant()),
    };
    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
    private static bool IsMobile(string value) => value.Length == 11 && value.StartsWith("09", StringComparison.Ordinal) && value.All(char.IsAsciiDigit);
    private static bool IsValidIranianNationalCode(string value)
    {
        if (value.Length != 10 || value.Any(character => !char.IsAsciiDigit(character)) || value.Distinct().Count() == 1) return false;
        int sum = Enumerable.Range(0, 9).Sum(index => (value[index] - '0') * (10 - index));
        int remainder = sum % 11; int check = value[9] - '0';
        return remainder < 2 ? check == remainder : check == 11 - remainder;
    }
}

public sealed class ProjectCrudService(IMasterDataStore store, IAuthorizationService authorization,
    ICurrentUserService currentUser, ISystemClock clock)
{
    public async Task<PageResult<ProjectDto>> SearchAsync(PageRequest request, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Projects, SecurityAction.View, ct); request.Validate(); return await store.SearchProjectsAsync(request, ct); }
    public async Task<ProjectDto?> GetAsync(long id, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Projects, SecurityAction.View, ct); return await store.GetProjectAsync(id, ct); }
    public async Task<ProjectDto> CreateAsync(ProjectInput input, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Projects, SecurityAction.Add, ct); await ValidateAsync(input, ct); return await store.CreateProjectAsync(input, Stamp(), ct); }
    public async Task<ProjectDto> UpdateAsync(ProjectUpdate update, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Projects, SecurityAction.Edit, ct); await ValidateAsync(update.Values, ct); return await store.UpdateProjectAsync(update, Stamp(), ct); }
    public async Task DeleteAsync(long id, long version, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Projects, SecurityAction.Delete, ct); await store.DeleteProjectAsync(id, version, Stamp(), ct); }
    public async Task<RecordNeighbor<long>> AdjacentAsync(long id, RecordDirection direction, string search, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Projects, SecurityAction.View, ct); return await store.GetAdjacentProjectAsync(id, direction, TextNormalizer.SearchKey(search ?? string.Empty), "NormalizedProjectNo,Id", ct); }
    public async Task<IReadOnlyList<ProjectBudgetDto>> BudgetsAsync(long projectId, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Budget, SecurityAction.View, ct); return await store.GetBudgetsAsync(projectId, ct); }
    public async Task<ProjectBudgetDto> CreateBudgetAsync(long projectId, ProjectBudgetInput input, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Budget, SecurityAction.Add, ct); ValidateBudget(input); return await store.CreateBudgetAsync(projectId, input, Stamp(), ct); }
    public async Task<ProjectBudgetDto> UpdateBudgetAsync(ProjectBudgetUpdate input, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Budget, SecurityAction.Edit, ct); ValidateBudget(input.Values); return await store.UpdateBudgetAsync(input, Stamp(), ct); }
    public async Task DeleteBudgetAsync(long id, long version, CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Budget, SecurityAction.Delete, ct); await store.DeleteBudgetAsync(id, version, Stamp(), ct); }
    public async Task<IReadOnlyList<SimpleLookupDto>> StatusesAsync(CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Projects, SecurityAction.View, ct); return await store.GetActiveLookupsAsync("ProjectStatus", ct); }
    public async Task<IReadOnlyList<SimpleLookupDto>> CurrenciesAsync(CancellationToken ct)
    { await authorization.RequireAsync(SecuritySection.Budget, SecurityAction.View, ct); return await store.GetActiveLookupsAsync("Currency", ct); }
    private AuditStamp Stamp() => new(currentUser.RequireAuthenticated().UserId, clock.UtcNow);
    private async Task ValidateAsync(ProjectInput input, CancellationToken ct)
    {
        List<string> errors = [];
        if (string.IsNullOrWhiteSpace(input.Code)) errors.Add("کد پروژه الزامی است.");
        if (string.IsNullOrWhiteSpace(input.Name)) errors.Add("نام پروژه الزامی است.");
        if (input.StartDate.HasValue && input.EndDate.HasValue && input.EndDate < input.StartDate) errors.Add("پایان پروژه نمی‌تواند پیش از شروع باشد.");
        if (!(await store.GetActiveLookupsAsync("ProjectStatus", ct)).Any(item => item.Id == input.StatusId)) errors.Add("وضعیت پروژه معتبر نیست.");
        if (errors.Count > 0) throw new MasterDataValidationException(errors);
    }
    private static void ValidateBudget(ProjectBudgetInput input)
    {
        if (string.IsNullOrWhiteSpace(input.FiscalYear) || input.ApprovedAmountMinor < 0 || input.ConsumedAmountMinor < 0 || input.ConsumedAmountMinor > input.ApprovedAmountMinor)
            throw new MasterDataValidationException(["بودجه نامعتبر است؛ مصرف نمی‌تواند منفی یا بیش از مبلغ مصوب باشد."]);
        _ = checked(input.ApprovedAmountMinor - input.ConsumedAmountMinor);
    }
}

public sealed class LookupAdministrationService(IMasterDataStore store, IAuthorizationService authorization,
    ICurrentUserService currentUser, ISystemClock clock)
{
    public async Task<PageResult<LookupDto>> SearchAsync(AdminLookupKind kind, PageRequest request, CancellationToken ct)
    { await DemandAsync(ct); request.Validate(); return await store.SearchLookupsAsync(kind, request, ct); }
    public async Task<LookupDto> CreateAsync(AdminLookupKind kind, LookupCreate input, CancellationToken ct)
    { await DemandAsync(ct); Validate(input.Code, input.TitleFa, input.SortOrder); return await store.CreateLookupAsync(kind, input, Stamp(), ct); }
    public async Task<LookupDto> UpdateAsync(AdminLookupKind kind, LookupUpdate input, CancellationToken ct)
    { await DemandAsync(ct); Validate(null, input.TitleFa, input.SortOrder); return await store.UpdateLookupAsync(kind, input, Stamp(), ct); }
    public async Task DeactivateAsync(AdminLookupKind kind, long id, long version, CancellationToken ct)
    { await DemandAsync(ct); await store.DeactivateLookupAsync(kind, id, version, Stamp(), ct); }
    private Task DemandAsync(CancellationToken ct) => authorization.RequireAsync(SecuritySection.Settings, SecurityAction.Admin, ct);
    private AuditStamp Stamp() => new(currentUser.RequireAuthenticated().UserId, clock.UtcNow);
    private static void Validate(string? code, string title, int sortOrder)
    {
        List<string> errors = [];
        string normalizedCode = code?.Trim() ?? string.Empty;
        if (code is not null && (normalizedCode.Length is 0 or > 64 || !char.IsAsciiLetter(normalizedCode[0]) ||
            normalizedCode.Any(character => !(char.IsAsciiLetterOrDigit(character) || character == '_'))))
            errors.Add("کد پایدار باید انگلیسی و فقط شامل حرف، رقم یا _ باشد.");
        if (string.IsNullOrWhiteSpace(title) || title.Trim().Length > 150) errors.Add("عنوان فارسی الزامی و حداکثر ۱۵۰ نویسه است.");
        if (sortOrder < 0) errors.Add("ترتیب نمی‌تواند منفی باشد.");
        if (errors.Count > 0) throw new MasterDataValidationException(errors);
    }
}
