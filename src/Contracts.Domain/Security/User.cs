namespace Contracts.Domain.Security;

public sealed class User : SoftDeletableEntity
{
    private User()
    {
        Username = string.Empty;
        DisplayName = string.Empty;
    }

    public User(string username, string displayName, long roleId, bool isActive, AuditStamp stamp)
        : base(stamp)
    {
        Username = Guard.RequiredText(username, nameof(username), 100);
        DisplayName = Guard.RequiredText(displayName, nameof(displayName), 200);
        RoleId = Guard.RequiredId(roleId, nameof(roleId));
        IsActive = isActive;
        MustChangePassword = true;
    }

    public string Username { get; private set; }
    public string DisplayName { get; private set; }
    public long RoleId { get; private set; }
    public bool IsActive { get; private set; }
    public bool MustChangePassword { get; private set; }
    public byte[]? PasswordHash { get; private set; }
    public byte[]? PasswordSalt { get; private set; }
    public string? PasswordAlgorithm { get; private set; }
    public int PasswordIterations { get; private set; }
    public int PasswordVersion { get; private set; }
    public int FailedLoginCount { get; private set; }
    public DateTimeOffset? LockedUntilUtc { get; private set; }
    public bool IsBootstrapAdministrator { get; private set; }

    public bool HasCredential => PasswordHash is { Length: > 0 } && PasswordSalt is { Length: > 0 };

    public void SetCredential(
        byte[] hash,
        byte[] salt,
        string algorithm,
        int iterations,
        int version,
        bool mustChangePassword,
        AuditStamp stamp)
    {
        ArgumentNullException.ThrowIfNull(hash);
        ArgumentNullException.ThrowIfNull(salt);
        if (hash.Length < 32 || salt.Length < 16) throw new ArgumentException("Credential material is too short.");
        ArgumentOutOfRangeException.ThrowIfLessThan(iterations, 100_000);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(version);
        PasswordHash = hash.ToArray();
        PasswordSalt = salt.ToArray();
        PasswordAlgorithm = Guard.RequiredText(algorithm, nameof(algorithm), 50);
        PasswordIterations = iterations;
        PasswordVersion = version;
        MustChangePassword = mustChangePassword;
        FailedLoginCount = 0;
        LockedUntilUtc = null;
        Touch(stamp);
    }

    public void MarkBootstrapAdministrator() => IsBootstrapAdministrator = true;

    public void RegisterFailedLogin(DateTimeOffset nowUtc, int threshold, TimeSpan lockoutDuration)
    {
        if (nowUtc.Offset != TimeSpan.Zero) throw new ArgumentException("Timestamp must be UTC.", nameof(nowUtc));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(threshold);
        FailedLoginCount = checked(FailedLoginCount + 1);
        if (FailedLoginCount >= threshold) LockedUntilUtc = nowUtc.Add(lockoutDuration);
        Touch(new AuditStamp(null, nowUtc));
    }

    public void RegisterSuccessfulLogin(AuditStamp stamp)
    {
        FailedLoginCount = 0;
        LockedUntilUtc = null;
        Touch(stamp);
    }

    public bool IsLocked(DateTimeOffset nowUtc) => LockedUntilUtc is not null && LockedUntilUtc > nowUtc;

    public void AssignRole(long roleId, AuditStamp stamp)
    {
        RoleId = Guard.RequiredId(roleId, nameof(roleId));
        Touch(stamp);
    }
}
