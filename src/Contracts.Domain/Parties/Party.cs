namespace Contracts.Domain.Parties;

public sealed class Party : SoftDeletableEntity
{
    private Party()
    {
        Name = string.Empty;
        NormalizedName = string.Empty;
    }

    public Party(
        string name,
        long partyTypeId,
        string? nationalId,
        string? economicCode,
        string? registrationNo,
        string? phone,
        string? address,
        AuditStamp stamp)
        : base(stamp)
    {
        Name = Guard.RequiredText(name, nameof(name), 250);
        PartyTypeId = Guard.RequiredId(partyTypeId, nameof(partyTypeId));
        NationalId = Guard.OptionalText(nationalId, nameof(nationalId), 20);
        EconomicCode = Guard.OptionalText(economicCode, nameof(economicCode), 30);
        RegistrationNo = Guard.OptionalText(registrationNo, nameof(registrationNo), 30);
        Phone = Guard.OptionalText(phone, nameof(phone), 30);
        Address = Guard.OptionalText(address, nameof(address), 1000);
        NormalizedName = TextNormalizer.SearchKey(Name);
    }

    public Party(string name, long partyTypeId, string? nationalId, string? economicCode, string? registrationNo,
        string? mobile, string? phone, string? email, string? address, string? contactPerson, AuditStamp stamp)
        : this(name, partyTypeId, nationalId, economicCode, registrationNo, phone, address, stamp)
    {
        Mobile = Guard.OptionalText(mobile, nameof(mobile), 20);
        Email = Guard.OptionalText(email, nameof(email), 254);
        ContactPerson = Guard.OptionalText(contactPerson, nameof(contactPerson), 200);
    }

    public string Name { get; private set; }
    public string NormalizedName { get; private set; }

    public long PartyTypeId { get; private set; }

    public string? NationalId { get; private set; }

    public string? EconomicCode { get; private set; }

    public string? RegistrationNo { get; private set; }

    public string? Phone { get; private set; }

    public string? Mobile { get; private set; }

    public string? Email { get; private set; }

    public string? Address { get; private set; }

    public string? ContactPerson { get; private set; }

    public void Update(string name, long partyTypeId, string? nationalId, string? economicCode, string? registrationNo,
        string? mobile, string? phone, string? email, string? address, string? contactPerson, AuditStamp stamp)
    {
        Name = Guard.RequiredText(name, nameof(name), 250);
        NormalizedName = TextNormalizer.SearchKey(Name);
        PartyTypeId = Guard.RequiredId(partyTypeId, nameof(partyTypeId));
        NationalId = Guard.OptionalText(nationalId, nameof(nationalId), 20);
        EconomicCode = Guard.OptionalText(economicCode, nameof(economicCode), 30);
        RegistrationNo = Guard.OptionalText(registrationNo, nameof(registrationNo), 30);
        Mobile = Guard.OptionalText(mobile, nameof(mobile), 20);
        Phone = Guard.OptionalText(phone, nameof(phone), 30);
        Email = Guard.OptionalText(email, nameof(email), 254);
        Address = Guard.OptionalText(address, nameof(address), 1000);
        ContactPerson = Guard.OptionalText(contactPerson, nameof(contactPerson), 200);
        Touch(stamp);
    }
}
