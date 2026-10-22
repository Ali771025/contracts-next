namespace Contracts.Application.Security;

public enum LoginStatus { Success, Failed, PasswordChangeRequired }
public sealed record LoginResult(LoginStatus Status, string Message);

public sealed class LoginUseCase(
    ISecurityStore store,
    IPasswordHasher hasher,
    ICurrentUserService currentUser,
    IAuditWriter audit,
    ISystemClock clock,
    ICorrelationIdProvider correlation,
    PasswordPolicyOptions options)
{
    public const string GenericFailureMessage = "نام کاربری یا گذرواژه معتبر نیست.";

    public async Task<LoginResult> ExecuteAsync(string username, string password, CancellationToken cancellationToken)
    {
        string normalized = (username ?? string.Empty).Trim();
        DateTimeOffset now = clock.UtcNow;
        AuthenticationUserRecord? user = await store.FindForAuthenticationAsync(normalized, now, cancellationToken);
        bool credentialValid = false;
        if (user?.PasswordHash is { Length: > 0 } hash && user.PasswordSalt is { Length: > 0 } salt &&
            user.PasswordAlgorithm is not null && user.PasswordIterations >= 100_000 && user.PasswordVersion > 0)
        {
            credentialValid = hasher.Verify(password, new(hash, salt, user.PasswordAlgorithm, user.PasswordIterations, user.PasswordVersion));
        }
        else hasher.PerformDummyVerification(password);

        if (user is null || !credentialValid || !user.IsActive || user.IsLocked)
        {
            if (user is not null && !credentialValid)
                await store.RegisterFailedLoginAsync(user.Id, now, options.LockoutThreshold, TimeSpan.FromMinutes(options.LockoutMinutes), cancellationToken);
            await audit.WriteAsync(new(user?.Id, "LOGIN_FAILURE", "USER", user is null ? "UNKNOWN" : user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
                correlation.Create(), "FAILURE"), cancellationToken);
            return new(LoginStatus.Failed, GenericFailureMessage);
        }

        await store.RegisterSuccessfulLoginAsync(user.Id, now, cancellationToken);
        PrincipalSnapshot snapshot = await store.GetPrincipalSnapshotAsync(user.Id, cancellationToken)
            ?? throw new InvalidOperationException("Authenticated user policy is unavailable.");
        currentUser.Establish(PrincipalFactory.Create(snapshot));
        await audit.WriteAsync(new(user.Id, "LOGIN_SUCCESS", "USER", user.Id.ToString(System.Globalization.CultureInfo.InvariantCulture),
            correlation.Create(), "SUCCESS"), cancellationToken);
        return snapshot.MustChangePassword
            ? new(LoginStatus.PasswordChangeRequired, "Password change is required.")
            : new(LoginStatus.Success, "Authenticated.");
    }
}

public sealed record BootstrapRequest(string Username, string DisplayName, string Password);
public sealed record BootstrapResult(bool Created, IReadOnlyList<string> Errors);

public sealed class BootstrapAdministratorUseCase(ISecurityStore store, IPasswordPolicy policy, IPasswordHasher hasher,
    ISystemClock clock, ICorrelationIdProvider correlation)
{
    public Task<bool> IsRequiredAsync(CancellationToken cancellationToken) => IsRequiredCoreAsync(cancellationToken);
    private async Task<bool> IsRequiredCoreAsync(CancellationToken cancellationToken) => !await store.HasUsersAsync(cancellationToken);

    public async Task<BootstrapResult> ExecuteAsync(BootstrapRequest request, CancellationToken cancellationToken)
    {
        PasswordPolicyResult validation = policy.Validate(request.Password, request.Username);
        if (!validation.IsValid) return new(false, validation.Errors);
        PasswordDigest digest = hasher.Hash(request.Password);
        bool created = await store.TryBootstrapAdministratorAsync(new(request.Username.Trim(), request.DisplayName.Trim(), digest, correlation.Create()), clock.UtcNow, cancellationToken);
        if (!created) return new(false, ["Security initialization has already completed."]);
        return new(true, []);
    }
}

public sealed class ChangePasswordUseCase(ISecurityStore store, IPasswordPolicy policy, IPasswordHasher hasher,
    ICurrentUserService currentUser, IAuditWriter audit, ISystemClock clock, ICorrelationIdProvider correlation)
{
    public async Task<PasswordPolicyResult> ExecuteAsync(string newPassword, CancellationToken cancellationToken)
    {
        AuthenticatedPrincipal principal = currentUser.RequireAuthenticated();
        PasswordPolicyResult validation = policy.Validate(newPassword, principal.Username);
        if (!validation.IsValid) return validation;
        await store.ChangePasswordAsync(principal.UserId, hasher.Hash(newPassword), clock.UtcNow, cancellationToken);
        PrincipalSnapshot snapshot = await store.GetPrincipalSnapshotAsync(principal.UserId, cancellationToken)
            ?? throw new InvalidOperationException("Updated user policy is unavailable.");
        currentUser.Refresh(PrincipalFactory.Create(snapshot));
        await audit.WriteAsync(new(principal.UserId, "PASSWORD_RESET", "USER", principal.UserId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            correlation.Create(), "SUCCESS"), cancellationToken);
        return new(true, []);
    }
}
