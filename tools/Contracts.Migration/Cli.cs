namespace Contracts.Migration;

public enum MigrationExitCode
{
    Success = 0,
    ValidationWarnings = 2,
    ProviderUnavailable = 10,
    SourceInvalid = 11,
    UsageInvalid = 12,
    MigrationFailed = 20,
    ReconciliationFailed = 21,
}

public enum MigrationMode { DryRun, Execute }

public sealed record MigrationOptions(
    string SourcePath,
    string TargetPath,
    MigrationMode Mode,
    string? MappingPath,
    string OutputDirectory,
    string LogLevel);

public sealed record ParseResult(MigrationOptions? Options, string? Error)
{
    public bool IsValid => Options is not null;
}

public static class MigrationCli
{
    private static readonly HashSet<string> LogLevels = new(StringComparer.OrdinalIgnoreCase)
        { "Trace", "Debug", "Information", "Warning", "Error", "Critical" };

    public static ParseResult Parse(IReadOnlyList<string> args, string currentDirectory)
    {
        Dictionary<string, string?> values = new(StringComparer.OrdinalIgnoreCase);
        bool dryRun = false;
        bool execute = false;
        for (int i = 0; i < args.Count; i++)
        {
            string token = args[i];
            if (token is "--dry-run" or "--execute")
            {
                dryRun |= token == "--dry-run";
                execute |= token == "--execute";
                continue;
            }

            if (token is not ("--source" or "--target" or "--mapping" or "--output" or "--log-level"))
            {
                return new(null, $"Unknown option: {token}");
            }

            if (++i >= args.Count || args[i].StartsWith("--", StringComparison.Ordinal))
            {
                return new(null, $"Missing value for {token}.");
            }

            if (!values.TryAdd(token, args[i]))
            {
                return new(null, $"Duplicate option: {token}");
            }
        }

        if (dryRun && execute)
        {
            return new(null, "--dry-run and --execute are mutually exclusive.");
        }

        if (!values.TryGetValue("--source", out string? source) || string.IsNullOrWhiteSpace(source) ||
            !values.TryGetValue("--target", out string? target) || string.IsNullOrWhiteSpace(target))
        {
            return new(null, "--source and --target are required.");
        }

        string level = values.GetValueOrDefault("--log-level") ?? "Information";
        if (!LogLevels.Contains(level))
        {
            return new(null, "--log-level must be Trace, Debug, Information, Warning, Error, or Critical.");
        }

        string fullSource = Path.GetFullPath(source, currentDirectory);
        string fullTarget = Path.GetFullPath(target, currentDirectory);
        string? mapping = values.GetValueOrDefault("--mapping");
        mapping = mapping is null ? null : Path.GetFullPath(mapping, currentDirectory);
        string output = Path.GetFullPath(values.GetValueOrDefault("--output") ?? "migration-output", currentDirectory);
        if (string.Equals(fullSource, fullTarget, StringComparison.OrdinalIgnoreCase))
        {
            return new(null, "Source and target must be different paths.");
        }

        return new(new(fullSource, fullTarget, execute ? MigrationMode.Execute : MigrationMode.DryRun,
            mapping, output, level), null);
    }
}
