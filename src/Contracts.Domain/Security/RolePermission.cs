namespace Contracts.Domain.Security;

public sealed class RolePermission : AuditedEntity
{
    private RolePermission() => SectionCode = string.Empty;

    public RolePermission(
        long roleId,
        string sectionCode,
        bool canView,
        bool canAdd,
        bool canEdit,
        bool canDelete,
        bool canExport,
        bool canAdmin,
        AuditStamp stamp)
        : base(stamp)
    {
        RoleId = Guard.RequiredId(roleId, nameof(roleId));
        SectionCode = Guard.StableCode(sectionCode, nameof(sectionCode));
        CanView = canView;
        CanAdd = canAdd;
        CanEdit = canEdit;
        CanDelete = canDelete;
        CanExport = canExport;
        CanAdmin = canAdmin;
    }

    public RolePermission(
        long id,
        long roleId,
        string sectionCode,
        bool canView,
        bool canAdd,
        bool canEdit,
        bool canDelete,
        bool canExport,
        bool canAdmin,
        AuditStamp stamp)
        : this(roleId, sectionCode, canView, canAdd, canEdit, canDelete, canExport, canAdmin, stamp)
    {
        Id = Guard.RequiredId(id, nameof(id));
    }

    public long RoleId { get; private set; }
    public string SectionCode { get; private set; }
    public bool CanView { get; private set; }
    public bool CanAdd { get; private set; }
    public bool CanEdit { get; private set; }
    public bool CanDelete { get; private set; }
    public bool CanExport { get; private set; }
    public bool CanAdmin { get; private set; }

    public void SetActions(bool canView, bool canAdd, bool canEdit, bool canDelete, bool canExport, bool canAdmin, AuditStamp stamp)
    {
        CanView = canView;
        CanAdd = canAdd;
        CanEdit = canEdit;
        CanDelete = canDelete;
        CanExport = canExport;
        CanAdmin = canAdmin;
        Touch(stamp);
    }
}
