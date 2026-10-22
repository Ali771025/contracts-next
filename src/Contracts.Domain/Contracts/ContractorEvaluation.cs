namespace Contracts.Domain.Contracts;

public sealed class ContractorEvaluation : SoftDeletableEntity
{
    private ContractorEvaluation()
    {
    }

    public ContractorEvaluation(
        long contractId,
        long partyId,
        DateOnly evaluationDate,
        int qualityScore,
        int timeScore,
        int financialScore,
        int hseScore,
        string? notes,
        AuditStamp stamp)
        : base(stamp)
    {
        ContractId = Guard.RequiredId(contractId, nameof(contractId));
        PartyId = Guard.RequiredId(partyId, nameof(partyId));
        EvaluationDate = evaluationDate;
        QualityScore = ValidateScore(qualityScore, nameof(qualityScore));
        TimeScore = ValidateScore(timeScore, nameof(timeScore));
        FinancialScore = ValidateScore(financialScore, nameof(financialScore));
        HseScore = ValidateScore(hseScore, nameof(hseScore));
        Notes = Guard.OptionalText(notes, nameof(notes), 2000);
    }

    public long ContractId { get; private set; }
    public long PartyId { get; private set; }
    public DateOnly EvaluationDate { get; private set; }
    public int QualityScore { get; private set; }
    public int TimeScore { get; private set; }
    public int FinancialScore { get; private set; }
    public int HseScore { get; private set; }
    public string? Notes { get; private set; }
    public decimal TotalScore => EvaluationScorePolicy.Total(QualityScore,TimeScore,FinancialScore,HseScore);

    public void Update(DateOnly date,int quality,int time,int financial,int hse,string? notes,AuditStamp stamp)
    {EvaluationDate=date;QualityScore=ValidateScore(quality,nameof(quality));TimeScore=ValidateScore(time,nameof(time));FinancialScore=ValidateScore(financial,nameof(financial));HseScore=ValidateScore(hse,nameof(hse));Notes=Guard.OptionalText(notes,nameof(notes),2000);Touch(stamp);}

    private static int ValidateScore(int score, string parameterName)
    {
        if (score is < 0 or > 100)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        return score;
    }
}
