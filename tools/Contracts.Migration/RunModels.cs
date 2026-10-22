namespace Contracts.Migration;

public sealed record StageCounter(string Stage, int Source, int Imported, int Skipped, int Warnings, int Failed);
public sealed record UnresolvedMapping(string Category, string NormalizedValue, bool Required, int Count, IReadOnlyList<long> LegacyIds);
public sealed record ReconciliationMetric(string Name, long SourceCount, long TargetCount, long SkippedCount, long FailedCount, long SourceTotalRial, long TargetTotalRial);

public sealed record MigrationRunRecord(
    string SchemaVersion,
    string ToolVersion,
    string SourceSha256,
    string SourceProvider,
    string MappingVersion,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    string Mode,
    string OutcomeFingerprintSha256,
    IReadOnlyList<StageCounter> Stages,
    int WarningCount,
    int ErrorCount,
    bool SourceHashUnchanged,
    IReadOnlyDictionary<string, bool> ProfileOnlyObjects);

public sealed class MigrationState
{
    public Dictionary<string, Dictionary<long, long>> Ids { get; } = new(StringComparer.OrdinalIgnoreCase);
    public List<StageCounter> Stages { get; } = [];
    public List<UnresolvedMapping> Unresolved { get; } = [];
    public List<string> Warnings { get; } = [];
    public Dictionary<string, long> ReferenceIds { get; } = new(StringComparer.OrdinalIgnoreCase);
    public long ActorUserId { get; set; }
    public Dictionary<long, string> Usernames { get; } = [];
    public Dictionary<long, long> CategoryIds { get; } = [];
}
