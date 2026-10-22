namespace Contracts.Application.Security;

using System.Globalization;

public sealed record RoleAdministrationItem(long Id,string Code,string TitleFa,int SortOrder,bool IsActive,long ConcurrencyVersion,int UserCount);
public sealed record UserRoleAdministrationItem(long Id,string Username,string DisplayName,long RoleId,string RoleTitle,bool IsActive,bool MustChangePassword);
public sealed record RolePermissionItem(SecuritySection Section,IReadOnlySet<SecurityAction> Actions);
public sealed record CreateRoleRequest(string Code,string TitleFa,int SortOrder,bool IsActive);
public sealed record UpdateRoleRequest(long Id,long ConcurrencyVersion,string TitleFa,int SortOrder,bool IsActive);
public sealed record ReplaceRolePermissionsRequest(long RoleId,long ConcurrencyVersion,IReadOnlyCollection<RolePermissionItem> Permissions);

public sealed class SecurityAdministrationValidationException : Exception
{
    public SecurityAdministrationValidationException(string message) : base(message) { }
    public SecurityAdministrationValidationException(string message, Exception innerException) : base(message, innerException) { }
}
public sealed class SecurityAdministrationConflictException : Exception
{
    public SecurityAdministrationConflictException(string message) : base(message) { }
    public SecurityAdministrationConflictException(string message, Exception innerException) : base(message, innerException) { }
}

public interface ISecurityAdministrationStore
{
    public Task<IReadOnlyList<RoleAdministrationItem>> GetRolesAsync(CancellationToken cancellationToken);
    public Task<IReadOnlyList<UserRoleAdministrationItem>> GetUsersAsync(CancellationToken cancellationToken);
    public Task<IReadOnlyList<RolePermissionItem>> GetPermissionsAsync(long roleId,CancellationToken cancellationToken);
    public Task<long> CreateRoleAsync(string code,string titleFa,int sortOrder,bool isActive,long actorUserId,DateTimeOffset nowUtc,CancellationToken cancellationToken);
    public Task UpdateRoleAsync(UpdateRoleRequest request,long actorUserId,DateTimeOffset nowUtc,CancellationToken cancellationToken);
    public Task ReplacePermissionsAsync(ReplaceRolePermissionsRequest request,long actorUserId,DateTimeOffset nowUtc,CancellationToken cancellationToken);
    public Task AssignRoleSafelyAsync(long userId,long roleId,long actorUserId,DateTimeOffset nowUtc,CancellationToken cancellationToken);
}

public sealed class RoleAdministrationService(
    IAuthorizationService authorization,ISecurityAdministrationStore store,ICurrentUserService currentUser,
    IAuditWriter audit,ISystemClock clock,ICorrelationIdProvider correlation)
{
    public async Task<(IReadOnlyList<RoleAdministrationItem> Roles,IReadOnlyList<UserRoleAdministrationItem> Users)> LoadAsync(CancellationToken cancellationToken)
    {
        await authorization.RequireAsync(SecuritySection.Users,SecurityAction.Admin,cancellationToken);
        return(await store.GetRolesAsync(cancellationToken),await store.GetUsersAsync(cancellationToken));
    }

    public async Task<IReadOnlyList<RolePermissionItem>> GetPermissionsAsync(long roleId,CancellationToken cancellationToken)
    {
        await authorization.RequireAsync(SecuritySection.Users,SecurityAction.Admin,cancellationToken);
        return await store.GetPermissionsAsync(roleId,cancellationToken);
    }

    public async Task<long> CreateRoleAsync(CreateRoleRequest request,CancellationToken cancellationToken)
    {
        await authorization.RequireAsync(SecuritySection.Users,SecurityAction.Admin,cancellationToken);
        string code=NormalizeCode(request.Code);string title=RequiredTitle(request.TitleFa);ValidateSort(request.SortOrder);
        AuthenticatedPrincipal actor=currentUser.RequireAuthenticated();
        long id=await store.CreateRoleAsync(code,title,request.SortOrder,request.IsActive,actor.UserId,clock.UtcNow,cancellationToken);
        await audit.WriteAsync(new(actor.UserId,"ROLE_CREATE","ROLE",id.ToString(CultureInfo.InvariantCulture),correlation.Create(),"SUCCESS",null,$"{{\"code\":\"{code}\"}}"),cancellationToken);
        return id;
    }

    public async Task UpdateRoleAsync(UpdateRoleRequest request,CancellationToken cancellationToken)
    {
        await authorization.RequireAsync(SecuritySection.Users,SecurityAction.Admin,cancellationToken);
        string title=RequiredTitle(request.TitleFa);ValidateSort(request.SortOrder);AuthenticatedPrincipal actor=currentUser.RequireAuthenticated();
        await store.UpdateRoleAsync(request with{TitleFa=title},actor.UserId,clock.UtcNow,cancellationToken);
        await audit.WriteAsync(new(actor.UserId,"ROLE_UPDATE","ROLE",request.Id.ToString(CultureInfo.InvariantCulture),correlation.Create(),"SUCCESS",null,"{\"metadataChanged\":true}"),cancellationToken);
    }

    public async Task ReplacePermissionsAsync(ReplaceRolePermissionsRequest request,CancellationToken cancellationToken)
    {
        await authorization.RequireAsync(SecuritySection.Users,SecurityAction.Admin,cancellationToken);
        Dictionary<SecuritySection,HashSet<SecurityAction>> supplied=request.Permissions.ToDictionary(item=>item.Section,item=>NormalizeActions(item.Actions));
        RolePermissionItem[] complete=Enum.GetValues<SecuritySection>().Select(section=>new RolePermissionItem(section,supplied.GetValueOrDefault(section)??new HashSet<SecurityAction>())).ToArray();
        AuthenticatedPrincipal actor=currentUser.RequireAuthenticated();
        await store.ReplacePermissionsAsync(request with{Permissions=complete},actor.UserId,clock.UtcNow,cancellationToken);
        await audit.WriteAsync(new(actor.UserId,"PERMISSION_CHANGE","ROLE",request.RoleId.ToString(CultureInfo.InvariantCulture),correlation.Create(),"SUCCESS",null,"{\"policyChanged\":true}"),cancellationToken);
    }

    public async Task AssignRoleAsync(long userId,long roleId,CancellationToken cancellationToken)
    {
        await authorization.RequireAsync(SecuritySection.Users,SecurityAction.Admin,cancellationToken);
        AuthenticatedPrincipal actor=currentUser.RequireAuthenticated();
        await store.AssignRoleSafelyAsync(userId,roleId,actor.UserId,clock.UtcNow,cancellationToken);
        await audit.WriteAsync(new(actor.UserId,"ROLE_CHANGE","USER",userId.ToString(CultureInfo.InvariantCulture),correlation.Create(),"SUCCESS",null,$"{{\"roleId\":{roleId.ToString(CultureInfo.InvariantCulture)}}}"),cancellationToken);
    }

    private static string NormalizeCode(string value)
    {
        string code=(value??string.Empty).Trim().ToUpperInvariant();
        if(code.Length is <2 or >64||!char.IsAsciiLetter(code[0])||code.Any(character=>!(char.IsAsciiLetterUpper(character)||char.IsAsciiDigit(character)||character=='_')))
            throw new SecurityAdministrationValidationException("Role code must be 2-64 uppercase English letters, digits, or underscore and start with a letter.");
        return code;
    }
    private static string RequiredTitle(string value){string title=(value??string.Empty).Trim();if(title.Length is <2 or >150)throw new SecurityAdministrationValidationException("Role title must contain 2-150 characters.");return title;}
    private static void ValidateSort(int value){if(value<0)throw new SecurityAdministrationValidationException("Sort order cannot be negative.");}
    private static HashSet<SecurityAction> NormalizeActions(IEnumerable<SecurityAction> actions){HashSet<SecurityAction> result=actions.ToHashSet();if(result.Any(action=>action!=SecurityAction.View))result.Add(SecurityAction.View);return result;}
}
