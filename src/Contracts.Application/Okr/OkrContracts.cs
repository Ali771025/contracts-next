namespace Contracts.Application.Okr;

using global::Contracts.Domain.Okr;

public sealed record OkrMeasurementDto(long Id,long ConcurrencyVersion,DateOnly Date,decimal Actual,decimal Score,
    OkrDataSourceType Source,string MetricVersion,DateTimeOffset? AsOfUtc,long? EvidenceCount,string Note,DateTimeOffset CreatedAtUtc,long? CreatedByUserId);
public sealed record OkrKeyResultDto(long Id,string Code,string TitleFa,decimal Target,string Unit,int Weight,OkrDirection Direction,
    string MetricCode,OkrDataSourceType DataSource,string MetricVersion,OkrMeasurementDto? Latest);
public sealed record OkrObjectiveDto(long Id,string Code,string TitleFa,string Description,decimal? Score,OkrScoreStatus Status,IReadOnlyList<OkrKeyResultDto> KeyResults);
public sealed record OkrSnapshot(decimal? Overall,OkrScoreStatus Status,DateTimeOffset GeneratedAtUtc,IReadOnlyList<OkrObjectiveDto> Objectives);
public sealed record ManualOkrMeasurement(long KeyResultId,DateOnly MeasurementDate,decimal ActualValue,string Note);
public sealed record AutoMetricValue(string MetricCode,decimal? ActualValue,string MetricVersion,DateTimeOffset AsOfUtc,long EvidenceCount,string EvidenceNote);
public sealed record OkrDefinitionRow(long Id,long ObjectiveId,string ObjectiveCode,string ObjectiveTitle,string ObjectiveDescription,int ObjectiveSort,
    string Code,string Title,decimal Target,string Unit,int Weight,OkrDirection Direction,string MetricCode,OkrDataSourceType DataSource,string MetricVersion,int Sort);

public interface IOkrStore
{
    public Task<IReadOnlyList<OkrDefinitionRow>> DefinitionsAsync(CancellationToken ct);
    public Task<IReadOnlyDictionary<long,OkrMeasurementDto>> LatestMeasurementsAsync(CancellationToken ct);
    public Task AddMeasurementAsync(long keyResultId,DateOnly measurementDate,decimal actual,decimal score,OkrDataSourceType source,string metricVersion,
        string note,DateTimeOffset? asOfUtc,long? evidenceCount,AuditStamp stamp,CancellationToken ct);
    public Task<IReadOnlyList<AutoMetricValue>> ReadLedgerMetricsAsync(DateOnly today,DateTimeOffset asOfUtc,CancellationToken ct);
}

public sealed class OkrService(IOkrStore store,IAuthorizationService authorization,IBusinessDateService businessDate,
    ISystemClock clock,ICurrentUserService currentUser)
{
    public async Task<OkrSnapshot> LoadAsync(CancellationToken ct)
    {
        await authorization.RequireAsync(SecuritySection.Okr,SecurityAction.View,ct);
        IReadOnlyList<OkrDefinitionRow> definitions=await store.DefinitionsAsync(ct);
        IReadOnlyDictionary<long,OkrMeasurementDto> latest=await store.LatestMeasurementsAsync(ct);
        return Build(definitions,latest,clock.UtcNow);
    }
    public async Task AddManualAsync(ManualOkrMeasurement input,CancellationToken ct)
    {
        await authorization.RequireAsync(SecuritySection.Okr,SecurityAction.Add,ct);
        OkrDefinitionRow definition=(await store.DefinitionsAsync(ct)).SingleOrDefault(x=>x.Id==input.KeyResultId)
            ??throw new KeyNotFoundException("Key result was not found.");
        if(definition.DataSource!=OkrDataSourceType.Manual)throw new InvalidOperationException("AUTO metrics cannot accept manual actuals.");
        if(string.IsNullOrWhiteSpace(input.Note))throw new ArgumentException("A provenance note is required.");
        decimal score=OkrScoringPolicy.Score(definition.Direction,definition.Target,input.ActualValue);
        AuthenticatedPrincipal actor=currentUser.RequireAuthenticated();
        await store.AddMeasurementAsync(definition.Id,input.MeasurementDate,input.ActualValue,score,OkrDataSourceType.Manual,
            definition.MetricVersion,input.Note,null,null,new(actor.UserId,clock.UtcNow),ct);
    }
    public async Task<int> RefreshAutoAsync(CancellationToken ct)
    {
        await authorization.RequireAsync(SecuritySection.Okr,SecurityAction.Admin,ct);
        IReadOnlyList<OkrDefinitionRow> definitions=await store.DefinitionsAsync(ct);
        DateTimeOffset asOf=clock.UtcNow;IReadOnlyList<AutoMetricValue> metrics=await store.ReadLedgerMetricsAsync(businessDate.Today,asOf,ct);
        AuthenticatedPrincipal actor=currentUser.RequireAuthenticated();int count=0;
        foreach(AutoMetricValue metric in metrics.Where(x=>x.ActualValue.HasValue))
        {
            OkrDefinitionRow definition=definitions.Single(x=>x.MetricCode==metric.MetricCode&&x.DataSource==OkrDataSourceType.Auto);
            decimal score=OkrScoringPolicy.Score(definition.Direction,definition.Target,metric.ActualValue!.Value);
            await store.AddMeasurementAsync(definition.Id,businessDate.Today,metric.ActualValue.Value,score,OkrDataSourceType.Auto,
                metric.MetricVersion,metric.EvidenceNote,metric.AsOfUtc,metric.EvidenceCount,new(actor.UserId,asOf),ct);count++;
        }
        return count;
    }
    public static OkrSnapshot Build(IReadOnlyList<OkrDefinitionRow> definitions,IReadOnlyDictionary<long,OkrMeasurementDto> latest,DateTimeOffset generated)
    {
        OkrObjectiveDto[] objectives=definitions.GroupBy(x=>new{x.ObjectiveId,x.ObjectiveCode,x.ObjectiveTitle,x.ObjectiveDescription,x.ObjectiveSort})
            .OrderBy(g=>g.Key.ObjectiveSort).Select(g=>
            {
                OkrKeyResultDto[] krs=g.OrderBy(x=>x.Sort).Select(x=>new OkrKeyResultDto(x.Id,x.Code,x.Title,x.Target,x.Unit,x.Weight,x.Direction,x.MetricCode,x.DataSource,x.MetricVersion,latest.GetValueOrDefault(x.Id))).ToArray();
                decimal? score=OkrScoringPolicy.ObjectiveScore(krs.Select(x=>(x.Weight,x.Latest?.Score)));
                return new OkrObjectiveDto(g.Key.ObjectiveId,g.Key.ObjectiveCode,g.Key.ObjectiveTitle,g.Key.ObjectiveDescription,score,OkrScoringPolicy.Status(score),krs);
            }).ToArray();
        decimal? overall=OkrScoringPolicy.OverallScore(objectives.Select(x=>x.Score));return new(overall,OkrScoringPolicy.Status(overall),generated,objectives);
    }
}
