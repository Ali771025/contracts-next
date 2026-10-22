namespace Contracts.Domain.ReferenceData;

public abstract class ReferenceEntity : AuditedEntity
{
    protected ReferenceEntity()
    {
        Code = string.Empty;
        TitleFa = string.Empty;
        NormalizedTitleFa = string.Empty;
    }

    protected ReferenceEntity(
        long id,
        string code,
        string titleFa,
        int sortOrder,
        bool isActive,
        AuditStamp stamp)
        : base(stamp)
    {
        Id = Guard.RequiredId(id, nameof(id));
        Code = Guard.StableCode(code, nameof(code));
        TitleFa = Guard.RequiredText(titleFa, nameof(titleFa), 150);
        ArgumentOutOfRangeException.ThrowIfNegative(sortOrder);
        SortOrder = sortOrder;
        IsActive = isActive;
        NormalizedTitleFa = TextNormalizer.SearchKey(TitleFa);
    }

    public string Code { get; private set; }

    public string TitleFa { get; private set; }
    public string NormalizedTitleFa { get; private set; }

    public int SortOrder { get; private set; }

    public bool IsActive { get; private set; }

    public void Deactivate(AuditStamp stamp)
    {
        IsActive = false;
        Touch(stamp);
    }

    public void UpdateMetadata(string titleFa, int sortOrder, bool isActive, AuditStamp stamp)
    {
        TitleFa = Guard.RequiredText(titleFa, nameof(titleFa), 150);
        NormalizedTitleFa = TextNormalizer.SearchKey(TitleFa);
        ArgumentOutOfRangeException.ThrowIfNegative(sortOrder);
        SortOrder = sortOrder; IsActive = isActive; Touch(stamp);
    }
}
