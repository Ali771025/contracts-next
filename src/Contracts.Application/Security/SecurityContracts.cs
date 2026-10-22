using System.Collections.Frozen;

namespace Contracts.Application.Security;

public enum SecurityAction { View, Add, Edit, Delete, Export, Admin }

public enum SecuritySection
{
    Dashboard, Contracts, Parties, Projects, Payments, Receipts, FinancialLedger,
    Guarantees, Amendments, Invoices, Documents, Alerts, Budget, Evaluation,
    Reports, Kpi, Okr, Users, Settings,
}

public static class PermissionKey
{
    public static string Create(SecuritySection section, SecurityAction action) =>
        $"{section.ToString().ToUpperInvariant()}_{action.ToString().ToUpperInvariant()}";

    public static IReadOnlySet<string> All { get; } =
        Enum.GetValues<SecuritySection>()
            .SelectMany(section => Enum.GetValues<SecurityAction>().Select(action => Create(section, action)))
            .ToFrozenSet(StringComparer.Ordinal);
}

public sealed class AuthenticatedPrincipal
{
    private readonly FrozenSet<string> permissions;

    public AuthenticatedPrincipal(long userId, string username, string displayName, long roleId, string roleCode,
        bool mustChangePassword, string policyVersion, IEnumerable<string> permissions)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(userId);
        UserId = userId;
        Username = username;
        DisplayName = displayName;
        RoleId = roleId;
        RoleCode = roleCode;
        MustChangePassword = mustChangePassword;
        PolicyVersion = policyVersion;
        this.permissions = permissions.ToFrozenSet(StringComparer.Ordinal);
    }

    public long UserId { get; }
    public string Username { get; }
    public string DisplayName { get; }
    public long RoleId { get; }
    public string RoleCode { get; }
    public bool MustChangePassword { get; }
    public string PolicyVersion { get; }
    public IReadOnlySet<string> Permissions => permissions;
    public bool HasPermission(SecuritySection section, SecurityAction action) => permissions.Contains(PermissionKey.Create(section, action));
}

public interface ICurrentUserService
{
    public AuthenticatedPrincipal? Current { get; }
    public AuthenticatedPrincipal RequireAuthenticated();
    public void Establish(AuthenticatedPrincipal principal);
    public void Refresh(AuthenticatedPrincipal principal);
    public void Clear();
}

public sealed class CurrentUserService : ICurrentUserService
{
    private AuthenticatedPrincipal? current;
    public AuthenticatedPrincipal? Current => Volatile.Read(ref current);
    public AuthenticatedPrincipal RequireAuthenticated() => Current ?? throw new UnauthorizedAccessException("Authentication is required.");
    public void Establish(AuthenticatedPrincipal principal) { ArgumentNullException.ThrowIfNull(principal); Interlocked.Exchange(ref current, principal); }
    public void Refresh(AuthenticatedPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(principal);
        AuthenticatedPrincipal existing = RequireAuthenticated();
        if (existing.UserId != principal.UserId) throw new InvalidOperationException("A session cannot be refreshed for another user.");
        Interlocked.Exchange(ref current, principal);
    }
    public void Clear() => Interlocked.Exchange(ref current, null);
}

public interface ISystemClock { public DateTimeOffset UtcNow { get; } }
public sealed class SystemClock : ISystemClock { public DateTimeOffset UtcNow => DateTimeOffset.UtcNow; }

public sealed record PasswordDigest(byte[] Hash, byte[] Salt, string Algorithm, int Iterations, int Version);

public interface IPasswordHasher
{
    public PasswordDigest Hash(string password);
    public bool Verify(string password, PasswordDigest digest);
    public void PerformDummyVerification(string password);
}

public sealed class PasswordPolicyOptions
{
    public int MinimumLength { get; set; } = 12;
    public bool RequireUppercase { get; set; } = true;
    public bool RequireLowercase { get; set; } = true;
    public bool RequireDigit { get; set; } = true;
    public bool RequireSymbol { get; set; } = true;
    public int Pbkdf2Iterations { get; set; } = 210_000;
    public int LockoutThreshold { get; set; } = 5;
    public int LockoutMinutes { get; set; } = 15;
    public List<string> BannedPasswordSha256 { get; set; } = [];
}

public sealed record PasswordPolicyResult(bool IsValid, IReadOnlyList<string> Errors);
public interface IPasswordPolicy { public PasswordPolicyResult Validate(string password, string username); }

public sealed record AuthenticationUserRecord(long Id, string Username, string DisplayName, long RoleId, bool IsActive,
    bool MustChangePassword, bool IsLocked, byte[]? PasswordHash, byte[]? PasswordSalt, string? PasswordAlgorithm,
    int PasswordIterations, int PasswordVersion);

public sealed record PrincipalSnapshot(long UserId, string Username, string DisplayName, long RoleId, string RoleCode,
    bool IsActive, bool MustChangePassword, string PolicyVersion, IReadOnlyCollection<string> Permissions);

public sealed record BootstrapAdminData(string Username, string DisplayName, PasswordDigest Digest, string CorrelationId);

public interface ISecurityStore
{
    public Task<bool> HasUsersAsync(CancellationToken cancellationToken);
    public Task<AuthenticationUserRecord?> FindForAuthenticationAsync(string normalizedUsername, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    public Task RegisterFailedLoginAsync(long userId, DateTimeOffset nowUtc, int threshold, TimeSpan duration, CancellationToken cancellationToken);
    public Task RegisterSuccessfulLoginAsync(long userId, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    public Task<PrincipalSnapshot?> GetPrincipalSnapshotAsync(long userId, CancellationToken cancellationToken);
    public Task<bool> TryBootstrapAdministratorAsync(BootstrapAdminData data, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    public Task ChangePasswordAsync(long userId, PasswordDigest digest, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    public Task UpsertPermissionAsync(long roleId, SecuritySection section, IReadOnlySet<SecurityAction> actions, long actorUserId, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    public Task AssignRoleAsync(long userId, long roleId, long actorUserId, DateTimeOffset nowUtc, CancellationToken cancellationToken);
}

public sealed record AuditEvent(long? UserId, string Action, string EntityType, string EntityId, string CorrelationId,
    string Outcome, string? BeforeJson = null, string? AfterJson = null);
public interface IAuditWriter { public Task WriteAsync(AuditEvent auditEvent, CancellationToken cancellationToken); }
public interface ICorrelationIdProvider { public string Create(); }
public sealed class CorrelationIdProvider : ICorrelationIdProvider { public string Create() => Guid.NewGuid().ToString("N"); }
public interface ISecurityDatabaseInitializer { public Task InitializeAsync(CancellationToken cancellationToken); }
