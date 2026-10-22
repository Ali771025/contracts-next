namespace Contracts.Application.Security;

public interface IAuthorizationService
{
    public Task<bool> IsAllowedAsync(SecuritySection section, SecurityAction action, CancellationToken cancellationToken);
    public Task RequireAsync(SecuritySection section, SecurityAction action, CancellationToken cancellationToken);
}

public sealed class AuthorizationService(ICurrentUserService currentUser, ISecurityStore store) : IAuthorizationService
{
    public async Task<bool> IsAllowedAsync(SecuritySection section, SecurityAction action, CancellationToken cancellationToken)
    {
        AuthenticatedPrincipal? principal = currentUser.Current;
        if (principal is null || principal.MustChangePassword) return false;
        PrincipalSnapshot? latest = await store.GetPrincipalSnapshotAsync(principal.UserId, cancellationToken);
        if (latest is null || !latest.IsActive) { currentUser.Clear(); return false; }
        if (!string.Equals(latest.PolicyVersion, principal.PolicyVersion, StringComparison.Ordinal))
        {
            principal = PrincipalFactory.Create(latest);
            currentUser.Refresh(principal);
        }
        return principal.HasPermission(section, action);
    }

    public async Task RequireAsync(SecuritySection section, SecurityAction action, CancellationToken cancellationToken)
    {
        if (!await IsAllowedAsync(section, action, cancellationToken)) throw new UnauthorizedAccessException("The requested operation is not authorized.");
    }
}

internal static class PrincipalFactory
{
    public static AuthenticatedPrincipal Create(PrincipalSnapshot snapshot)
    {
        IEnumerable<string> permissions = string.Equals(snapshot.RoleCode, "SYSTEM_ADMIN", StringComparison.Ordinal)
            ? PermissionKey.All : snapshot.Permissions;
        return new(snapshot.UserId, snapshot.Username, snapshot.DisplayName, snapshot.RoleId, snapshot.RoleCode,
            snapshot.MustChangePassword, snapshot.PolicyVersion, permissions);
    }
}
