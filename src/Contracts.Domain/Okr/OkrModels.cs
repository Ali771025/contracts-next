namespace Contracts.Domain.Okr;

public enum OkrDirection { HigherIsBetter, LowerIsBetter, TargetZero, Boolean }
public enum OkrDataSourceType { Manual, Auto }
public enum OkrScoreStatus { NotMeasured, Excellent, Good, Average, CorrectiveAction }

public sealed class OkrObjective : AuditedEntity
{
    private OkrObjective() { Code = TitleFa = Description = string.Empty; }
    public OkrObjective(long id, string code, string titleFa, string description, bool isActive, int sortOrder, AuditStamp stamp) : base(stamp)
    { Id=id; Code=Required(code,32); TitleFa=Required(titleFa,200); Description=description?.Trim()??string.Empty; IsActive=isActive; SortOrder=sortOrder; }
    public string Code { get; private set; }
    public string TitleFa { get; private set; }
    public string Description { get; private set; }
    public bool IsActive { get; private set; }
    public int SortOrder { get; private set; }
    private static string Required(string value,int max)=>string.IsNullOrWhiteSpace(value)||value.Trim().Length>max?throw new ArgumentException("Invalid OKR text."):value.Trim();
}

public sealed class OkrKeyResult : AuditedEntity
{
    private OkrKeyResult() { Code=TitleFa=Unit=MetricCode=MetricVersion=string.Empty; }
    public OkrKeyResult(long id,long objectiveId,string code,string titleFa,decimal targetValue,string unit,int weightPercent,
        OkrDirection direction,string metricCode,OkrDataSourceType dataSourceType,string metricVersion,bool isActive,int sortOrder,AuditStamp stamp) : base(stamp)
    {
        OkrScoringPolicy.ValidateTarget(direction,targetValue);
        if(objectiveId<=0||weightPercent is <0 or >100)throw new ArgumentOutOfRangeException(nameof(weightPercent));
        Id=id;ObjectiveId=objectiveId;Code=Required(code,32);TitleFa=Required(titleFa,200);TargetValue=targetValue;Unit=Required(unit,32);
        WeightPercent=weightPercent;Direction=direction;MetricCode=Required(metricCode,80);DataSourceType=dataSourceType;
        MetricVersion=Required(metricVersion,32);IsActive=isActive;SortOrder=sortOrder;
    }
    public long ObjectiveId{get;private set;} public string Code{get;private set;} public string TitleFa{get;private set;}
    public decimal TargetValue{get;private set;} public string Unit{get;private set;} public int WeightPercent{get;private set;}
    public OkrDirection Direction{get;private set;} public string MetricCode{get;private set;} public OkrDataSourceType DataSourceType{get;private set;}
    public string MetricVersion{get;private set;} public bool IsActive{get;private set;} public int SortOrder{get;private set;}
    private static string Required(string value,int max)=>string.IsNullOrWhiteSpace(value)||value.Trim().Length>max?throw new ArgumentException("Invalid OKR text."):value.Trim();
}

public sealed class OkrMeasurement : AuditedEntity
{
    private OkrMeasurement(){MetricVersion=Note=string.Empty;}
    public OkrMeasurement(long keyResultId,DateOnly measurementDate,decimal actualValue,decimal score,OkrDataSourceType sourceType,
        string metricVersion,string note,DateTimeOffset? asOfUtc,long? evidenceCount,AuditStamp stamp):base(stamp)
    {
        if(keyResultId<=0||actualValue<0||score is <0 or >1)throw new ArgumentOutOfRangeException(nameof(actualValue));
        if(sourceType==OkrDataSourceType.Auto&&(asOfUtc is null||evidenceCount is null or <0))throw new ArgumentException("AUTO provenance is required.");
        KeyResultId=keyResultId;MeasurementDate=measurementDate;ActualValue=actualValue;Score=score;SourceType=sourceType;
        MetricVersion=string.IsNullOrWhiteSpace(metricVersion)?throw new ArgumentException("Metric version is required."):metricVersion.Trim();
        Note=note?.Trim()??string.Empty;AsOfUtc=asOfUtc;EvidenceCount=evidenceCount;
    }
    public long KeyResultId{get;private set;} public DateOnly MeasurementDate{get;private set;} public decimal ActualValue{get;private set;}
    public decimal Score{get;private set;} public OkrDataSourceType SourceType{get;private set;} public string MetricVersion{get;private set;}
    public string Note{get;private set;} public DateTimeOffset? AsOfUtc{get;private set;} public long? EvidenceCount{get;private set;}
}

public static class OkrScoringPolicy
{
    public static void ValidateTarget(OkrDirection direction,decimal target)
    {
        bool valid=direction switch{OkrDirection.HigherIsBetter or OkrDirection.LowerIsBetter=>target>0,OkrDirection.TargetZero=>target==0,OkrDirection.Boolean=>target==1,_=>false};
        if(!valid)throw new ArgumentOutOfRangeException(nameof(target));
    }
    public static decimal Score(OkrDirection direction,decimal target,decimal actual)
    {
        ValidateTarget(direction,target);ArgumentOutOfRangeException.ThrowIfNegative(actual);
        decimal value=direction switch{OkrDirection.HigherIsBetter=>actual/target,OkrDirection.LowerIsBetter=>actual<=target?1:target/actual,
            OkrDirection.TargetZero=>actual==0?1:0,OkrDirection.Boolean=>actual==1?1:0,_=>throw new ArgumentOutOfRangeException(nameof(direction))};
        return decimal.Clamp(value,0,1);
    }
    public static decimal? ObjectiveScore(IEnumerable<(int Weight,decimal? Score)> results)
    {var rows=results.ToArray();if(rows.Sum(x=>x.Weight)!=100)throw new InvalidOperationException("Active key-result weights must total 100.");if(rows.Any(x=>x.Score is null))return null;return rows.Sum(x=>x.Weight*x.Score!.Value/100m);}
    public static decimal? OverallScore(IEnumerable<decimal?> objectives){decimal[] measured=objectives.Where(x=>x.HasValue).Select(x=>x!.Value).ToArray();return measured.Length==0?null:measured.Average();}
    public static OkrScoreStatus Status(decimal? score)=>score is null?OkrScoreStatus.NotMeasured:score>=.90m?OkrScoreStatus.Excellent:score>=.70m?OkrScoreStatus.Good:score>=.50m?OkrScoreStatus.Average:OkrScoreStatus.CorrectiveAction;
}
