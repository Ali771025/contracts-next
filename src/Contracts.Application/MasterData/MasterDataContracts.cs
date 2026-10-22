using Contracts.Application.Presentation;
using Contracts.Domain.Common;

namespace Contracts.Application.MasterData;

public sealed record PageRequest(int Page = 1, int PageSize = 25, string? Search = null, string Sort = "name", bool Descending = false)
{
    public int Skip => checked((Page - 1) * PageSize);
    public void Validate()
    {
        if (Page < 1) throw new ArgumentOutOfRangeException(nameof(Page));
        if (PageSize is < 1 or > 200) throw new ArgumentOutOfRangeException(nameof(PageSize));
    }
}
public sealed record PageResult<T>(IReadOnlyList<T> Items, int TotalCount, int Page, int PageSize);

public sealed record PartyInput(string Name, long PartyTypeId, string? NationalId, string? EconomicCode,
    string? Mobile, string? Phone, string? Email, string? Address, string? ContactPerson);
public sealed record PartyUpdate(long Id, long ConcurrencyVersion, PartyInput Values);
public sealed record PartyDto(long Id, long ConcurrencyVersion, string Name, long PartyTypeId, string PartyTypeTitle,
    string? NationalId, string? EconomicCode, string? Mobile, string? Phone, string? Email, string? Address, string? ContactPerson);

public sealed record ProjectInput(string Code, string Name, string? Employer, string? Department, long? ManagerUserId,
    DateOnly? StartDate, DateOnly? EndDate, long StatusId, string? Description);
public sealed record ProjectUpdate(long Id, long ConcurrencyVersion, ProjectInput Values);
public sealed record ProjectDto(long Id, long ConcurrencyVersion, string Code, string Name, string? Employer,
    string? Department, long? ManagerUserId, DateOnly? StartDate, DateOnly? EndDate, long StatusId,
    string StatusTitle, string? Description);

public sealed record ProjectBudgetInput(string FiscalYear, long CurrencyId, long ApprovedAmountMinor, long ConsumedAmountMinor);
public sealed record ProjectBudgetUpdate(long Id, long ConcurrencyVersion, ProjectBudgetInput Values);
public sealed record ProjectBudgetDto(long Id, long ConcurrencyVersion, long ProjectId, string FiscalYear, long CurrencyId,
    string CurrencyCode, int CurrencyScale, long ApprovedAmountMinor, long ConsumedAmountMinor, long RemainingAmountMinor);

public enum AdminLookupKind { Bank, PaymentType }
public sealed record LookupCreate(string Code, string TitleFa, int SortOrder, bool IsActive = true);
public sealed record LookupUpdate(long Id, long ConcurrencyVersion, string TitleFa, int SortOrder, bool IsActive);
public sealed record LookupDto(long Id, long ConcurrencyVersion, string Code, string TitleFa, int SortOrder, bool IsActive);
public sealed record SimpleLookupDto(long Id, string Code, string TitleFa, int SortOrder);

public sealed class MasterDataValidationException(IReadOnlyList<string> errors) : Exception(string.Join(" ", errors))
{
    public IReadOnlyList<string> Errors { get; } = errors;
}
public sealed class MasterDataConflictException(string message) : Exception(message);
public sealed class MasterDataDependencyException(string message) : Exception(message);
public sealed class MasterDataDuplicateException(string message) : Exception(message);

public interface IMasterDataStore
{
    public Task<PageResult<PartyDto>> SearchPartiesAsync(PageRequest request, CancellationToken ct);
    public Task<PartyDto?> GetPartyAsync(long id, CancellationToken ct);
    public Task<PartyDto> CreatePartyAsync(PartyInput input, AuditStamp stamp, CancellationToken ct);
    public Task<PartyDto> UpdatePartyAsync(PartyUpdate update, AuditStamp stamp, CancellationToken ct);
    public Task DeletePartyAsync(long id, long version, AuditStamp stamp, CancellationToken ct);
    public Task<RecordNeighbor<long>> GetAdjacentPartyAsync(long id, RecordDirection direction, string search, string stableSort, CancellationToken ct);
    public Task<string?> GetPartyTypeCodeAsync(long id, CancellationToken ct);

    public Task<PageResult<ProjectDto>> SearchProjectsAsync(PageRequest request, CancellationToken ct);
    public Task<ProjectDto?> GetProjectAsync(long id, CancellationToken ct);
    public Task<ProjectDto> CreateProjectAsync(ProjectInput input, AuditStamp stamp, CancellationToken ct);
    public Task<ProjectDto> UpdateProjectAsync(ProjectUpdate update, AuditStamp stamp, CancellationToken ct);
    public Task DeleteProjectAsync(long id, long version, AuditStamp stamp, CancellationToken ct);
    public Task<RecordNeighbor<long>> GetAdjacentProjectAsync(long id, RecordDirection direction, string search, string stableSort, CancellationToken ct);
    public Task<IReadOnlyList<ProjectBudgetDto>> GetBudgetsAsync(long projectId, CancellationToken ct);
    public Task<ProjectBudgetDto> CreateBudgetAsync(long projectId, ProjectBudgetInput input, AuditStamp stamp, CancellationToken ct);
    public Task<ProjectBudgetDto> UpdateBudgetAsync(ProjectBudgetUpdate update, AuditStamp stamp, CancellationToken ct);
    public Task DeleteBudgetAsync(long id, long version, AuditStamp stamp, CancellationToken ct);

    public Task<PageResult<LookupDto>> SearchLookupsAsync(AdminLookupKind kind, PageRequest request, CancellationToken ct);
    public Task<LookupDto> CreateLookupAsync(AdminLookupKind kind, LookupCreate input, AuditStamp stamp, CancellationToken ct);
    public Task<LookupDto> UpdateLookupAsync(AdminLookupKind kind, LookupUpdate update, AuditStamp stamp, CancellationToken ct);
    public Task DeactivateLookupAsync(AdminLookupKind kind, long id, long version, AuditStamp stamp, CancellationToken ct);
    public Task<IReadOnlyList<SimpleLookupDto>> GetActiveLookupsAsync(string kind, CancellationToken ct);
}
