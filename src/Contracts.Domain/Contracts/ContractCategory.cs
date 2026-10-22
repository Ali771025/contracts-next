namespace Contracts.Domain.Contracts;

public sealed class ContractCategory : ReferenceEntity
{
    private ContractCategory()
    {
    }

    public ContractCategory(long id, string code, string titleFa, int sortOrder, bool isActive, AuditStamp stamp)
        : base(id, code, titleFa, sortOrder, isActive, stamp)
    {
    }
}
