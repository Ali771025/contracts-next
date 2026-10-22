namespace Contracts.Domain.Security;

public sealed class Role : ReferenceEntity
{
    private readonly List<RolePermission> permissions = [];

    private Role()
    {
    }

    public Role(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp)
        : base(id, code, titleFa, sortOrder, isActive, stamp)
    {
    }

    public IReadOnlyCollection<RolePermission> Permissions => permissions.AsReadOnly();
}
