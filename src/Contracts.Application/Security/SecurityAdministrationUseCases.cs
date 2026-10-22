namespace Contracts.Application.Security;

public sealed record UpdatePermissionRequest(long RoleId, SecuritySection Section, IReadOnlySet<SecurityAction> Actions);

public sealed class UpdateRolePermissionUseCase(IAuthorizationService authorization, ISecurityStore store,
    ICurrentUserService currentUser, IAuditWriter audit, ISystemClock clock, ICorrelationIdProvider correlation)
{
    public async Task ExecuteAsync(UpdatePermissionRequest request, CancellationToken cancellationToken)
    {
        await authorization.RequireAsync(SecuritySection.Settings, SecurityAction.Admin, cancellationToken);
        AuthenticatedPrincipal actor = currentUser.RequireAuthenticated();
        await store.UpsertPermissionAsync(request.RoleId, request.Section, request.Actions, actor.UserId, clock.UtcNow, cancellationToken);
        await audit.WriteAsync(new(actor.UserId, "PERMISSION_CHANGE", "ROLE", request.RoleId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            correlation.Create(), "SUCCESS", null, "{\"policyChanged\":true}"), cancellationToken);
    }
}

public sealed class AssignUserRoleUseCase(IAuthorizationService authorization, ISecurityStore store,
    ICurrentUserService currentUser, IAuditWriter audit, ISystemClock clock, ICorrelationIdProvider correlation)
{
    public async Task ExecuteAsync(long userId, long roleId, CancellationToken cancellationToken)
    {
        await authorization.RequireAsync(SecuritySection.Users, SecurityAction.Admin, cancellationToken);
        AuthenticatedPrincipal actor = currentUser.RequireAuthenticated();
        await store.AssignRoleAsync(userId, roleId, actor.UserId, clock.UtcNow, cancellationToken);
        await audit.WriteAsync(new(actor.UserId, "ROLE_CHANGE", "USER", userId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            correlation.Create(), "SUCCESS", null, "{\"roleChanged\":true}"), cancellationToken);
    }
}
