using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace Contracts.Infrastructure.Security;

public sealed class SecurityStore(IDbContextFactory<ContractsDbContext> contextFactory) : ISecurityStore,ISecurityAdministrationStore
{
    public async Task<bool> HasUsersAsync(CancellationToken cancellationToken)
    {
        await using ContractsDbContext db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Users.AnyAsync(item => !item.IsDeleted, cancellationToken);
    }

    public async Task<AuthenticationUserRecord?> FindForAuthenticationAsync(string normalizedUsername, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        await using ContractsDbContext db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await(from user in db.Users.AsNoTracking() join role in db.Roles.AsNoTracking() on user.RoleId equals role.Id
            where !user.IsDeleted&&user.Username==normalizedUsername&&role.IsActive
            select new AuthenticationUserRecord(user.Id,user.Username,user.DisplayName,user.RoleId,user.IsActive,
                user.MustChangePassword,user.LockedUntilUtc!=null&&user.LockedUntilUtc>nowUtc,
                user.PasswordHash,user.PasswordSalt,user.PasswordAlgorithm,user.PasswordIterations,user.PasswordVersion))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task RegisterFailedLoginAsync(long userId, DateTimeOffset nowUtc, int threshold, TimeSpan duration, CancellationToken cancellationToken)
    {
        await using ContractsDbContext db = await contextFactory.CreateDbContextAsync(cancellationToken);
        User user = await db.Users.SingleAsync(item => item.Id == userId, cancellationToken);
        user.RegisterFailedLogin(nowUtc, threshold, duration);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RegisterSuccessfulLoginAsync(long userId, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        await using ContractsDbContext db = await contextFactory.CreateDbContextAsync(cancellationToken);
        User user = await db.Users.SingleAsync(item => item.Id == userId, cancellationToken);
        user.RegisterSuccessfulLogin(new(userId, nowUtc));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PrincipalSnapshot?> GetPrincipalSnapshotAsync(long userId, CancellationToken cancellationToken)
    {
        await using ContractsDbContext db = await contextFactory.CreateDbContextAsync(cancellationToken);
        var identity = await (from user in db.Users.AsNoTracking()
                              join role in db.Roles.AsNoTracking() on user.RoleId equals role.Id
                              where user.Id == userId && !user.IsDeleted && role.IsActive
                              select new { User = user, Role = role }).SingleOrDefaultAsync(cancellationToken);
        if (identity is null) return null;
        RolePermission[] permissions = await db.RolePermissions.AsNoTracking().Where(item => item.RoleId == identity.Role.Id).ToArrayAsync(cancellationToken);
        HashSet<string> keys = [];
        foreach (RolePermission permission in permissions)
        {
            if (!Enum.TryParse(permission.SectionCode, true, out SecuritySection section)) continue;
            if (permission.CanView) keys.Add(PermissionKey.Create(section, SecurityAction.View));
            if (permission.CanAdd) keys.Add(PermissionKey.Create(section, SecurityAction.Add));
            if (permission.CanEdit) keys.Add(PermissionKey.Create(section, SecurityAction.Edit));
            if (permission.CanDelete) keys.Add(PermissionKey.Create(section, SecurityAction.Delete));
            if (permission.CanExport) keys.Add(PermissionKey.Create(section, SecurityAction.Export));
            if (permission.CanAdmin) keys.Add(PermissionKey.Create(section, SecurityAction.Admin));
        }
        long permissionVersion = permissions.Length == 0 ? 0 : permissions.Max(item => item.ConcurrencyVersion);
        string version = $"{identity.User.ConcurrencyVersion}:{identity.Role.ConcurrencyVersion}:{permissions.Length}:{permissionVersion}";
        return new(identity.User.Id, identity.User.Username, identity.User.DisplayName, identity.Role.Id, identity.Role.Code,
            identity.User.IsActive, identity.User.MustChangePassword, version, keys);
    }

    public async Task<bool> TryBootstrapAdministratorAsync(BootstrapAdminData data, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        await using ContractsDbContext db = await contextFactory.CreateDbContextAsync(cancellationToken);
        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
            if (await db.Users.AnyAsync(item => !item.IsDeleted, cancellationToken)) return false;
            User user = new(data.Username, data.DisplayName, 1, true, new(null, nowUtc));
            user.MarkBootstrapAdministrator();
            user.SetCredential(data.Digest.Hash, data.Digest.Salt, data.Digest.Algorithm, data.Digest.Iterations, data.Digest.Version, false, new(null, nowUtc));
            db.Users.Add(user);
            await db.SaveChangesAsync(cancellationToken);
            db.AuditLogs.Add(new(user.Id, "BOOTSTRAP_ADMIN_CREATED", "USER", user.Id.ToString(CultureInfo.InvariantCulture),
                nowUtc, data.CorrelationId, "SUCCESS", null, "{\"systemBootstrap\":true}"));
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }
        catch (Exception exception) when (exception is DbUpdateException or Microsoft.Data.Sqlite.SqliteException)
        {
            if (transaction is not null) await transaction.RollbackAsync(cancellationToken);
            return false;
        }
        finally { if (transaction is not null) await transaction.DisposeAsync(); }
    }

    public async Task ChangePasswordAsync(long userId, PasswordDigest digest, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        await using ContractsDbContext db = await contextFactory.CreateDbContextAsync(cancellationToken);
        User user = await db.Users.SingleAsync(item => item.Id == userId && !item.IsDeleted, cancellationToken);
        user.SetCredential(digest.Hash, digest.Salt, digest.Algorithm, digest.Iterations, digest.Version, false, new(userId, nowUtc));
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpsertPermissionAsync(long roleId, SecuritySection section, IReadOnlySet<SecurityAction> actions,
        long actorUserId, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        await using ContractsDbContext db = await contextFactory.CreateDbContextAsync(cancellationToken);
        AuditStamp stamp = new(actorUserId, nowUtc);
        string code = section.ToString().ToUpperInvariant();
        RolePermission? permission = await db.RolePermissions.SingleOrDefaultAsync(item => item.RoleId == roleId && item.SectionCode == code, cancellationToken);
        bool Has(SecurityAction action) => actions.Contains(action);
        if (permission is null) db.RolePermissions.Add(new(roleId, code, Has(SecurityAction.View), Has(SecurityAction.Add),
            Has(SecurityAction.Edit), Has(SecurityAction.Delete), Has(SecurityAction.Export), Has(SecurityAction.Admin), stamp));
        else permission.SetActions(Has(SecurityAction.View), Has(SecurityAction.Add), Has(SecurityAction.Edit), Has(SecurityAction.Delete), Has(SecurityAction.Export), Has(SecurityAction.Admin), stamp);
        Role role = await db.Roles.SingleAsync(item => item.Id == roleId, cancellationToken);
        role.Touch(stamp);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task AssignRoleAsync(long userId, long roleId, long actorUserId, DateTimeOffset nowUtc, CancellationToken cancellationToken)
        =>await AssignRoleSafelyAsync(userId,roleId,actorUserId,nowUtc,cancellationToken);

    public async Task<IReadOnlyList<RoleAdministrationItem>> GetRolesAsync(CancellationToken cancellationToken)
    {
        await using ContractsDbContext db = await contextFactory.CreateDbContextAsync(cancellationToken);
        return await db.Roles.AsNoTracking().OrderBy(item=>item.SortOrder).ThenBy(item=>item.Id)
            .Select(item=>new RoleAdministrationItem(item.Id,item.Code,item.TitleFa,item.SortOrder,item.IsActive,item.ConcurrencyVersion,
                db.Users.Count(user=>!user.IsDeleted&&user.RoleId==item.Id))).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserRoleAdministrationItem>> GetUsersAsync(CancellationToken cancellationToken)
    {
        await using ContractsDbContext db=await contextFactory.CreateDbContextAsync(cancellationToken);
        return await(from user in db.Users.AsNoTracking() join role in db.Roles.AsNoTracking() on user.RoleId equals role.Id
            where !user.IsDeleted orderby user.Username
            select new UserRoleAdministrationItem(user.Id,user.Username,user.DisplayName,user.RoleId,role.TitleFa,user.IsActive,user.MustChangePassword)).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<RolePermissionItem>> GetPermissionsAsync(long roleId,CancellationToken cancellationToken)
    {
        await using ContractsDbContext db=await contextFactory.CreateDbContextAsync(cancellationToken);
        Role role=await db.Roles.AsNoTracking().SingleOrDefaultAsync(item=>item.Id==roleId,cancellationToken)
            ??throw new SecurityAdministrationValidationException("Role was not found.");
        if(role.Code=="SYSTEM_ADMIN")return Enum.GetValues<SecuritySection>().Select(section=>new RolePermissionItem(section,Enum.GetValues<SecurityAction>().ToHashSet())).ToArray();
        RolePermission[] rows=await db.RolePermissions.AsNoTracking().Where(item=>item.RoleId==roleId).ToArrayAsync(cancellationToken);
        Dictionary<string,RolePermission> bySection=rows.ToDictionary(item=>item.SectionCode,StringComparer.OrdinalIgnoreCase);
        return Enum.GetValues<SecuritySection>().Select(section=>
        {
            if(!bySection.TryGetValue(section.ToString(),out RolePermission? row))return new RolePermissionItem(section,new HashSet<SecurityAction>());
            HashSet<SecurityAction> actions=[];if(row.CanView)actions.Add(SecurityAction.View);if(row.CanAdd)actions.Add(SecurityAction.Add);if(row.CanEdit)actions.Add(SecurityAction.Edit);if(row.CanDelete)actions.Add(SecurityAction.Delete);if(row.CanExport)actions.Add(SecurityAction.Export);if(row.CanAdmin)actions.Add(SecurityAction.Admin);
            return new RolePermissionItem(section,actions);
        }).ToArray();
    }

    public async Task<long> CreateRoleAsync(string code,string titleFa,int sortOrder,bool isActive,long actorUserId,DateTimeOffset nowUtc,CancellationToken cancellationToken)
    {
        await using ContractsDbContext db=await contextFactory.CreateDbContextAsync(cancellationToken);await using IDbContextTransaction transaction=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,cancellationToken);
        try
        {
            long id=checked((await db.Roles.MaxAsync(item=>(long?)item.Id,cancellationToken)??0)+1);db.Roles.Add(new(id,code,titleFa,sortOrder,isActive,new(actorUserId,nowUtc)));await db.SaveChangesAsync(cancellationToken);await transaction.CommitAsync(cancellationToken);return id;
        }
        catch(DbUpdateException exception){await transaction.RollbackAsync(cancellationToken);throw new SecurityAdministrationValidationException("Role code or title already exists.",exception);}
    }

    public async Task UpdateRoleAsync(UpdateRoleRequest request,long actorUserId,DateTimeOffset nowUtc,CancellationToken cancellationToken)
    {
        await using ContractsDbContext db=await contextFactory.CreateDbContextAsync(cancellationToken);Role role=await db.Roles.SingleOrDefaultAsync(item=>item.Id==request.Id,cancellationToken)??throw new SecurityAdministrationValidationException("Role was not found.");
        if(role.ConcurrencyVersion!=request.ConcurrencyVersion)throw new SecurityAdministrationConflictException("Role was changed by another user. Refresh and retry.");
        if(role.Code=="SYSTEM_ADMIN"&&!request.IsActive)throw new SecurityAdministrationValidationException("System Administrator cannot be deactivated.");
        if(!request.IsActive&&role.IsActive&&await db.Users.AnyAsync(user=>!user.IsDeleted&&user.RoleId==role.Id,cancellationToken))throw new SecurityAdministrationValidationException("A role assigned to users cannot be deactivated.");
        role.UpdateMetadata(request.TitleFa,request.SortOrder,request.IsActive,new(actorUserId,nowUtc));
        try{await db.SaveChangesAsync(cancellationToken);}catch(DbUpdateConcurrencyException exception){throw new SecurityAdministrationConflictException("Role was changed by another user.",exception);}catch(DbUpdateException exception){throw new SecurityAdministrationValidationException("Role title already exists.",exception);}
    }

    public async Task ReplacePermissionsAsync(ReplaceRolePermissionsRequest request,long actorUserId,DateTimeOffset nowUtc,CancellationToken cancellationToken)
    {
        await using ContractsDbContext db=await contextFactory.CreateDbContextAsync(cancellationToken);await using IDbContextTransaction transaction=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,cancellationToken);
        Role role=await db.Roles.SingleOrDefaultAsync(item=>item.Id==request.RoleId,cancellationToken)??throw new SecurityAdministrationValidationException("Role was not found.");
        if(role.Code=="SYSTEM_ADMIN")throw new SecurityAdministrationValidationException("System Administrator permissions are fixed and always complete.");
        if(!role.IsActive)throw new SecurityAdministrationValidationException("Permissions of an inactive role cannot be changed.");
        if(role.ConcurrencyVersion!=request.ConcurrencyVersion)throw new SecurityAdministrationConflictException("Role permissions changed. Refresh and retry.");
        RolePermission[] existing=await db.RolePermissions.Where(item=>item.RoleId==role.Id).ToArrayAsync(cancellationToken);Dictionary<string,RolePermission> bySection=existing.ToDictionary(item=>item.SectionCode,StringComparer.OrdinalIgnoreCase);AuditStamp stamp=new(actorUserId,nowUtc);
        foreach(RolePermissionItem item in request.Permissions)
        {
            string code=item.Section.ToString().ToUpperInvariant();bool Has(SecurityAction action)=>item.Actions.Contains(action);
            if(bySection.TryGetValue(code,out RolePermission? row))row.SetActions(Has(SecurityAction.View),Has(SecurityAction.Add),Has(SecurityAction.Edit),Has(SecurityAction.Delete),Has(SecurityAction.Export),Has(SecurityAction.Admin),stamp);
            else db.RolePermissions.Add(new(role.Id,code,Has(SecurityAction.View),Has(SecurityAction.Add),Has(SecurityAction.Edit),Has(SecurityAction.Delete),Has(SecurityAction.Export),Has(SecurityAction.Admin),stamp));
        }
        role.Touch(stamp);
        try{await db.SaveChangesAsync(cancellationToken);await transaction.CommitAsync(cancellationToken);}catch(DbUpdateConcurrencyException exception){await transaction.RollbackAsync(cancellationToken);throw new SecurityAdministrationConflictException("Role permissions changed. Refresh and retry.",exception);}
    }

    public async Task AssignRoleSafelyAsync(long userId,long roleId,long actorUserId,DateTimeOffset nowUtc,CancellationToken cancellationToken)
    {
        await using ContractsDbContext db=await contextFactory.CreateDbContextAsync(cancellationToken);await using IDbContextTransaction transaction=await db.Database.BeginTransactionAsync(IsolationLevel.Serializable,cancellationToken);
        User user=await db.Users.SingleOrDefaultAsync(item=>item.Id==userId&&!item.IsDeleted,cancellationToken)??throw new SecurityAdministrationValidationException("User was not found.");
        Role target=await db.Roles.SingleOrDefaultAsync(item=>item.Id==roleId,cancellationToken)??throw new SecurityAdministrationValidationException("Role was not found.");
        if(!target.IsActive)throw new SecurityAdministrationValidationException("An inactive role cannot be assigned.");
        Role current=await db.Roles.SingleAsync(item=>item.Id==user.RoleId,cancellationToken);
        if(current.Code=="SYSTEM_ADMIN"&&target.Code!="SYSTEM_ADMIN")
        {
            int administrators=await(from candidate in db.Users join role in db.Roles on candidate.RoleId equals role.Id where !candidate.IsDeleted&&candidate.IsActive&&role.Code=="SYSTEM_ADMIN" select candidate).CountAsync(cancellationToken);
            if(administrators<=1)throw new SecurityAdministrationValidationException("The last active System Administrator cannot lose that role.");
        }
        user.AssignRole(roleId,new(actorUserId,nowUtc));await db.SaveChangesAsync(cancellationToken);await transaction.CommitAsync(cancellationToken);
    }
}
