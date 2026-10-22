namespace Contracts.Domain.Contracts;

public sealed class Contract : SoftDeletableEntity
{
    private readonly List<ContractAmendment> amendments = [];

    private Contract()
    {
        ContractNo = string.Empty;
        NormalizedContractNo = string.Empty;
        Title = string.Empty;
        NormalizedTitle = string.Empty;
    }

    public Contract(
        string contractNo,
        string title,
        long contractTypeId,
        long? contractCategoryId,
        long? projectId,
        long partyId,
        long contractStatusId,
        long currencyId,
        long originalAmountMinor,
        int currencyScale,
        long exchangeRateRialPerUnit,
        DateOnly startDate,
        DateOnly endDate,
        long? managerUserId,
        string? description,
        AuditStamp stamp)
        : base(stamp)
    {
        if (stamp.UserId is null)
        {
            throw new ArgumentException("Contract audit actor is required.", nameof(stamp));
        }

        if (endDate < startDate)
        {
            throw new ArgumentException("Contract end date cannot precede its start date.", nameof(endDate));
        }

        ContractNo = Guard.RequiredText(contractNo, nameof(contractNo), 100);
        NormalizedContractNo = TextNormalizer.IdentifierKey(ContractNo);
        Title = Guard.RequiredText(title, nameof(title), 300);
        NormalizedTitle = TextNormalizer.SearchKey(Title);
        ContractTypeId = Guard.RequiredId(contractTypeId, nameof(contractTypeId));
        ContractCategoryId = contractCategoryId;
        ProjectId = projectId;
        PartyId = Guard.RequiredId(partyId, nameof(partyId));
        ContractStatusId = Guard.RequiredId(contractStatusId, nameof(contractStatusId));
        CurrencyId = Guard.RequiredId(currencyId, nameof(currencyId));
        OriginalAmountMinor = Guard.NonNegativeAmount(originalAmountMinor, nameof(originalAmountMinor));
        if (currencyScale is < 0 or > 6)
        {
            throw new ArgumentOutOfRangeException(nameof(currencyScale));
        }

        CurrencyScale = currencyScale;
        ExchangeRateRialPerUnit = Guard.PositiveAmount(exchangeRateRialPerUnit, nameof(exchangeRateRialPerUnit));
        BaseAmountRial = MoneyConverter.ToRial(OriginalAmountMinor, CurrencyScale, ExchangeRateRialPerUnit);
        StartDate = startDate;
        EndDate = endDate;
        ManagerUserId = managerUserId;
        Description = Guard.OptionalText(description, nameof(description), 4000);
    }

    public string ContractNo { get; private set; }
    public string NormalizedContractNo { get; private set; }
    public string Title { get; private set; }
    public string NormalizedTitle { get; private set; }
    public long ContractTypeId { get; private set; }
    public long? ContractCategoryId { get; private set; }
    public long? ProjectId { get; private set; }
    public long PartyId { get; private set; }
    public long ContractStatusId { get; private set; }
    public long CurrencyId { get; private set; }
    public long OriginalAmountMinor { get; private set; }
    public int CurrencyScale { get; private set; }
    public long ExchangeRateRialPerUnit { get; private set; }
    public long BaseAmountRial { get; private set; }
    public DateOnly StartDate { get; private set; }
    public DateOnly EndDate { get; private set; }
    public long? ManagerUserId { get; private set; }
    public string? Description { get; private set; }
    public IReadOnlyCollection<ContractAmendment> Amendments => amendments.AsReadOnly();
    public int DurationDays => EndDate.DayNumber - StartDate.DayNumber + 1;
    public DateOnly EffectiveEndDate => ContractTimelinePolicy.EffectiveEndDate(EndDate, amendments.Where(item => !item.IsDeleted));
    public long FinalAmountRial => amendments.Where(amendment => !amendment.IsDeleted)
        .Aggregate(BaseAmountRial, (total, amendment) => checked(total + amendment.AmountChangeRial));

    public void Update(string contractNo, string title, long contractTypeId, long? contractCategoryId,
        long? projectId, long partyId, long contractStatusId, long currencyId, long originalAmountMinor,
        int currencyScale, long exchangeRateRialPerUnit, DateOnly startDate, DateOnly endDate,
        long? managerUserId, string? description, AuditStamp stamp)
    {
        if (stamp.UserId is null) throw new ArgumentException("Contract audit actor is required.", nameof(stamp));
        if (endDate < startDate) throw new ArgumentException("Contract end date cannot precede its start date.", nameof(endDate));
        string validNo = Guard.RequiredText(contractNo, nameof(contractNo), 100);
        string validTitle = Guard.RequiredText(title, nameof(title), 300);
        long validType = Guard.RequiredId(contractTypeId, nameof(contractTypeId));
        long validParty = Guard.RequiredId(partyId, nameof(partyId));
        long validStatus = Guard.RequiredId(contractStatusId, nameof(contractStatusId));
        long validCurrency = Guard.RequiredId(currencyId, nameof(currencyId));
        long validAmount = Guard.NonNegativeAmount(originalAmountMinor, nameof(originalAmountMinor));
        if (currencyScale is < 0 or > 6) throw new ArgumentOutOfRangeException(nameof(currencyScale));
        long validRate = Guard.PositiveAmount(exchangeRateRialPerUnit, nameof(exchangeRateRialPerUnit));
        long baseAmount = MoneyConverter.ToRial(validAmount, currencyScale, validRate);
        long amendmentTotal = amendments.Where(amendment => !amendment.IsDeleted)
            .Aggregate(0L, (total, amendment) => checked(total + amendment.AmountChangeRial));
        if (checked(baseAmount + amendmentTotal) < 0)
            throw new InvalidOperationException("Contract update cannot make the final amount negative.");
        string? validDescription = Guard.OptionalText(description, nameof(description), 4000);

        ContractNo = validNo; NormalizedContractNo = TextNormalizer.IdentifierKey(validNo); Title = validTitle;
        NormalizedTitle = TextNormalizer.SearchKey(validTitle);
        ContractTypeId = validType; ContractCategoryId = contractCategoryId; ProjectId = projectId;
        PartyId = validParty; ContractStatusId = validStatus; CurrencyId = validCurrency;
        OriginalAmountMinor = validAmount; CurrencyScale = currencyScale;
        ExchangeRateRialPerUnit = validRate; BaseAmountRial = baseAmount;
        StartDate = startDate; EndDate = endDate; ManagerUserId = managerUserId; Description = validDescription;
        Touch(stamp);
    }

    public ContractAmendment AddAmendment(
        string amendmentNo,
        DateOnly amendmentDate,
        long amountChangeRial,
        int timeExtensionDays,
        string? subject,
        string? attachmentPath,
        string? description,
        AuditStamp stamp)
    {
        long prospectiveFinal = checked(FinalAmountRial + amountChangeRial);
        if (prospectiveFinal < 0)
        {
            throw new InvalidOperationException("An amendment cannot make the contract final amount negative.");
        }

        ContractAmendment amendment = new(
            amendmentNo,
            amendmentDate,
            amountChangeRial,
            timeExtensionDays,
            subject,
            attachmentPath,
            description,
            stamp);
        amendments.Add(amendment);
        return amendment;
    }

    public ContractAmendment AddAmendment(string amendmentNo, DateOnly amendmentDate, long amountChangeRial,
        int timeExtensionDays, string? description, AuditStamp stamp) =>
        AddAmendment(amendmentNo, amendmentDate, amountChangeRial, timeExtensionDays, null, null, description, stamp);
}
