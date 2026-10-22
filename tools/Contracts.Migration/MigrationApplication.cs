using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Globalization;
using Contracts.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Contracts.Migration;

public static class MigrationApplication
{
    public static async Task<int> RunAsync(string[] args, CancellationToken token)
    {
        ParseResult parsed = MigrationCli.Parse(args, Environment.CurrentDirectory);
        if (!parsed.IsValid)
        {
            Console.Error.WriteLine(parsed.Error);
            Console.Error.WriteLine("Usage: Contracts.Migration --source <file.accdb> --target <file.db> [--dry-run|--execute] [--mapping <file.json>] [--output <dir>] [--log-level <level>]");
            return (int)MigrationExitCode.UsageInvalid;
        }

        MigrationOptions options = parsed.Options!;
        string? scratch = null;
        try
        {
            if (!File.Exists(options.SourcePath)) throw new SourceInvalidException("Source file does not exist.");
            if (options.MappingPath is not null && !File.Exists(options.MappingPath)) throw new SourceInvalidException("Mapping file does not exist.");
            if (options.Mode == MigrationMode.Execute && File.Exists(options.TargetPath))
                throw new SourceInvalidException("Execute refuses an existing target. Move it aside or provide a new target path.");

            MappingFile mapping = await MappingFile.LoadAsync(options.MappingPath, token);
            ILegacySourceReader reader = string.Equals(Path.GetExtension(options.SourcePath), ".json", StringComparison.OrdinalIgnoreCase)
                ? new JsonFixtureSourceReader() : new AccessLegacySourceReader();
            DateTimeOffset started = DateTimeOffset.UtcNow;
            LegacySnapshot snapshot = await reader.ReadAsync(options.SourcePath, token);
            Console.WriteLine($"Source opened read-only via {snapshot.Provider}; SHA-256 {snapshot.SourceSha256}.");

            ContractsDbContext? db = null;
            if (options.Mode == MigrationMode.Execute)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(options.TargetPath)!);
                scratch = options.TargetPath + $".partial-{Environment.ProcessId}";
                DbContextOptionsBuilder<ContractsDbContext> builder = new();
                builder.UseSqlite($"Data Source={scratch};Foreign Keys=True;Pooling=False");
                db = new ContractsDbContext(builder.Options);
                await db.Database.MigrateAsync(token);
            }

            MigrationPipeline pipeline = new(mapping, started);
            MigrationOutcome outcome;
            try
            {
                outcome = await pipeline.RunAsync(snapshot, db, options.SourcePath, token);
            }
            finally
            {
                if (db is not null) await db.DisposeAsync();
            }

            string afterHash = await AccessLegacySourceReader.HashAsync(options.SourcePath, token);
            bool unchanged = string.Equals(snapshot.SourceSha256, afterHash, StringComparison.Ordinal);
            if (!unchanged) throw new SourceInvalidException("Source SHA-256 changed during migration.");
            if (!outcome.Reconciled) return await WriteAndReturnAsync(options, snapshot, mapping, started, outcome, unchanged, MigrationExitCode.ReconciliationFailed, token);
            if (outcome.Errors.Count > 0) return await WriteAndReturnAsync(options, snapshot, mapping, started, outcome, unchanged, MigrationExitCode.MigrationFailed, token);

            if (options.Mode == MigrationMode.Execute)
            {
                File.Move(scratch!, options.TargetPath, false);
                scratch = null;
            }

            MigrationExitCode result = outcome.Warnings.Count > 0 ? MigrationExitCode.ValidationWarnings : MigrationExitCode.Success;
            return await WriteAndReturnAsync(options, snapshot, mapping, started, outcome, unchanged, result, token);
        }
        catch (ProviderUnavailableException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return (int)MigrationExitCode.ProviderUnavailable;
        }
        catch (SourceInvalidException exception)
        {
            Console.Error.WriteLine(exception.Message);
            return (int)MigrationExitCode.SourceInvalid;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine($"Migration failed: {exception.GetType().Name}: {exception.Message}");
            return (int)MigrationExitCode.MigrationFailed;
        }
        finally
        {
            if (scratch is not null && File.Exists(scratch)) File.Delete(scratch);
        }
    }

    private static async Task<int> WriteAndReturnAsync(MigrationOptions options, LegacySnapshot source, MappingFile mapping,
        DateTimeOffset started, MigrationOutcome outcome, bool unchanged, MigrationExitCode exit, CancellationToken token)
    {
        Directory.CreateDirectory(options.OutputDirectory);
        DateTimeOffset completed = DateTimeOffset.UtcNow;
        string fingerprintInput = JsonSerializer.Serialize(new { source.SourceSha256, mapping.Version, options.Mode, outcome.Stages, outcome.Reconciliation });
        string fingerprint = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(fingerprintInput)));
        MigrationRunRecord run = new("1.0", typeof(MigrationApplication).Assembly.GetName().Version?.ToString() ?? "1.0.0",
            source.SourceSha256, source.Provider, mapping.Version, started, completed, options.Mode.ToString().ToUpperInvariant(),
            fingerprint, outcome.Stages, outcome.Warnings.Count, outcome.Errors.Count, unchanged, source.OptionalObjects);
        JsonSerializerOptions json = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };
        await File.WriteAllTextAsync(Path.Combine(options.OutputDirectory, "MIGRATION_RUN.json"), JsonSerializer.Serialize(run, json), token);
        await File.WriteAllTextAsync(Path.Combine(options.OutputDirectory, "UNRESOLVED_MAPPINGS.json"),
            JsonSerializer.Serialize(new { schema_version = "1.0", mapping_version = mapping.Version, items = outcome.Unresolved }, json), token);
        await File.WriteAllTextAsync(Path.Combine(options.OutputDirectory, "MIGRATION_RECONCILIATION.md"),
            ReconciliationWriter.Write(source, options, outcome, unchanged), token);
        Console.WriteLine($"Completed with exit code {(int)exit}; warnings={outcome.Warnings.Count}, errors={outcome.Errors.Count}.");
        return (int)exit;
    }
}

public sealed record MigrationOutcome(IReadOnlyList<StageCounter> Stages, IReadOnlyList<UnresolvedMapping> Unresolved,
    IReadOnlyList<string> Warnings, IReadOnlyList<string> Errors, IReadOnlyList<ReconciliationMetric> Reconciliation, bool Reconciled);

internal static class ReconciliationWriter
{
    public static string Write(LegacySnapshot source, MigrationOptions options, MigrationOutcome outcome, bool hashUnchanged)
    {
        StringBuilder text = new();
        text.AppendLine("# Migration reconciliation").AppendLine();
        text.AppendLine(CultureInfo.InvariantCulture, $"- Mode: `{options.Mode.ToString().ToUpperInvariant()}`");
        text.AppendLine(CultureInfo.InvariantCulture, $"- Source SHA-256 unchanged: `{hashUnchanged}`");
        text.AppendLine(CultureInfo.InvariantCulture, $"- Provider: `{source.Provider}`");
        text.AppendLine(CultureInfo.InvariantCulture, $"- TradeBank profile only / imported: `{source.OptionalObjects["tblTradeBank"]}` / `false`");
        text.AppendLine(CultureInfo.InvariantCulture, $"- SupplierBalance profile only / imported: `{source.OptionalObjects["tblSupplierBalance"]}` / `false`").AppendLine();
        text.AppendLine("| Metric | Source | Target/projected | Skipped | Failed | Source total IRR | Target/projected IRR |");
        text.AppendLine("|---|---:|---:|---:|---:|---:|---:|");
        foreach (ReconciliationMetric metric in outcome.Reconciliation)
            text.AppendLine(CultureInfo.InvariantCulture, $"| {metric.Name} | {metric.SourceCount} | {metric.TargetCount} | {metric.SkippedCount} | {metric.FailedCount} | {metric.SourceTotalRial} | {metric.TargetTotalRial} |");
        if (outcome.Warnings.Count > 0)
        {
            text.AppendLine().AppendLine("## Review warnings").AppendLine();
            foreach (string warning in outcome.Warnings.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
                text.AppendLine(CultureInfo.InvariantCulture, $"- `{warning}`");
        }
        if (outcome.Errors.Count > 0)
        {
            text.AppendLine().AppendLine("## Errors").AppendLine();
            foreach (string error in outcome.Errors.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
                text.AppendLine(CultureInfo.InvariantCulture, $"- `{error}`");
        }
        text.AppendLine().AppendLine(CultureInfo.InvariantCulture, $"Result: `{(outcome.Reconciled ? "RECONCILED" : "FAILED")}`");
        text.AppendLine("Credentials were not copied or logged. Imported users are forced to change password.");
        return text.ToString();
    }
}
