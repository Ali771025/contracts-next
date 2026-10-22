using System.Data;
using System.Data.OleDb;
using System.Globalization;
using System.Security.Cryptography;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;

namespace Contracts.Migration;

public sealed class ProviderUnavailableException(string message, Exception? inner = null) : Exception(message, inner);
public sealed class SourceInvalidException(string message, Exception? inner = null) : Exception(message, inner);

public sealed record LegacyRow(long Id, IReadOnlyDictionary<string, object?> Values)
{
    public string? Text(string name) => Values.TryGetValue(name, out object? value) && value is not null && value is not DBNull
        ? Convert.ToString(value, CultureInfo.InvariantCulture)?.Trim() : null;
    public long? KeyValue(string name) => Values.TryGetValue(name, out object? value) && value is not null && value is not DBNull
        ? checked(Convert.ToInt64(value, CultureInfo.InvariantCulture)) : null;
    [SuppressMessage("Naming", "CA1720:Identifier contains type name", Justification = "Names the Access integer field conversion explicitly.")]
    public long? Integer(string name) => KeyValue(name);
    public decimal? Number(string name) => Values.TryGetValue(name, out object? value) && value is not null && value is not DBNull
        ? Convert.ToDecimal(value, CultureInfo.InvariantCulture) : null;
    public bool Bool(string name, bool fallback = false) => Values.TryGetValue(name, out object? value) && value is not null && value is not DBNull
        ? Convert.ToBoolean(value, CultureInfo.InvariantCulture) : fallback;
    public DateTime? Date(string name) => Values.TryGetValue(name, out object? value) && value is not null && value is not DBNull
        ? Convert.ToDateTime(value, CultureInfo.InvariantCulture) : null;
}

public sealed record LegacySnapshot(
    string SourceSha256,
    string Provider,
    IReadOnlyDictionary<string, IReadOnlyList<LegacyRow>> Tables,
    IReadOnlyDictionary<string, bool> OptionalObjects)
{
    public IReadOnlyList<LegacyRow> Table(string name) => Tables.GetValueOrDefault(name) ?? [];
}

public interface ILegacySourceReader
{
    public Task<LegacySnapshot> ReadAsync(string sourcePath, CancellationToken cancellationToken);
}

public sealed class AccessLegacySourceReader : ILegacySourceReader
{
    private static readonly (string Table, string Key)[] RequiredTables =
    [
        ("tblLookups", "LookupID"), ("tblUsers", "UserID"), ("tblRolePermissions", ""),
        ("tblParties", "PartyID"), ("tblProjects", "ProjectID"), ("tblContracts", "ContractID"),
        ("tblAmendments", "AmendmentID"), ("tblPayments", "PaymentID"), ("tblReceipts", "ReceiptID"),
        ("tblGuarantees", "GuaranteeID"), ("tblInvoices", "InvoiceID"), ("tblDocuments", "DocumentID"),
        ("tblAlerts", "AlertID"), ("tblProjectBudget", "BudgetID"),
        ("tblContractorEvaluation", "EvaluationID"), ("tblContractCategory", "CategoryID"),
    ];

    public async Task<LegacySnapshot> ReadAsync(string sourcePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(sourcePath) || !string.Equals(Path.GetExtension(sourcePath), ".accdb", StringComparison.OrdinalIgnoreCase))
        {
            throw new SourceInvalidException("Source must be an existing .accdb file.");
        }

        string before = await HashAsync(sourcePath, cancellationToken);
        Exception? last = null;
        foreach (string provider in new[] { "Microsoft.ACE.OLEDB.16.0", "Microsoft.ACE.OLEDB.12.0" })
        {
            try
            {
                string cs = $"Provider={provider};Data Source={sourcePath};Mode=Read;Persist Security Info=False;";
                await using OleDbConnection connection = new(cs);
                await connection.OpenAsync(cancellationToken);
                HashSet<string> actualTables = connection.GetSchema("Tables").Rows.Cast<DataRow>()
                    .Where(row => string.Equals(Convert.ToString(row["TABLE_TYPE"], CultureInfo.InvariantCulture), "TABLE", StringComparison.OrdinalIgnoreCase))
                    .Select(row => Convert.ToString(row["TABLE_NAME"], CultureInfo.InvariantCulture) ?? string.Empty)
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);
                string[] missing = RequiredTables.Select(item => item.Table).Where(item => !actualTables.Contains(item)).ToArray();
                if (missing.Length > 0)
                {
                    throw new SourceInvalidException($"Required Access objects are missing ({missing.Length}).");
                }

                Dictionary<string, IReadOnlyList<LegacyRow>> tables = new(StringComparer.OrdinalIgnoreCase);
                foreach ((string table, string key) in RequiredTables)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    tables[table] = await ReadTableAsync(connection, table, key, cancellationToken);
                }

                string after = await HashAsync(sourcePath, cancellationToken);
                if (!string.Equals(before, after, StringComparison.Ordinal))
                {
                    throw new SourceInvalidException("Source SHA-256 changed while it was being read.");
                }

                return new(before, provider, tables, new Dictionary<string, bool>
                {
                    ["tblTradeBank"] = actualTables.Contains("tblTradeBank"),
                    ["tblSupplierBalance"] = actualTables.Contains("tblSupplierBalance"),
                });
            }
            catch (SourceInvalidException) { throw; }
            catch (Exception exception) when (exception is OleDbException or InvalidOperationException or PlatformNotSupportedException)
            {
                last = exception;
            }
        }

        throw new ProviderUnavailableException("Neither Microsoft.ACE.OLEDB.16.0 nor 12.0 could open the source read-only.", last);
    }

    private static async Task<IReadOnlyList<LegacyRow>> ReadTableAsync(OleDbConnection connection, string table, string key, CancellationToken token)
    {
        string order = string.IsNullOrEmpty(key) ? "[Role], [SectionKey]" : $"[{key}]";
        await using OleDbCommand command = connection.CreateCommand();
        string projection = string.Equals(table, "tblUsers", StringComparison.OrdinalIgnoreCase)
            ? "[UserID], [FullName], [UserName], [Department], [Role], [Mobile], [Email], [IsActive], [LastLogin], [CreatedAt], [CreatedBy]"
            : "*";
        command.CommandText = $"SELECT {projection} FROM [{table}] ORDER BY {order}";
        await using System.Data.Common.DbDataReader reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess, token)
            ?? throw new SourceInvalidException($"Unable to read required table {table}.");
        List<LegacyRow> rows = [];
        long ordinalId = 0;
        while (await reader.ReadAsync(token))
        {
            Dictionary<string, object?> values = new(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < reader.FieldCount; i++)
            {
                string name = reader.GetName(i);
                values[name] = await reader.IsDBNullAsync(i, token) ? null : reader.GetValue(i);
            }
            long id = string.IsNullOrEmpty(key) ? ++ordinalId : Convert.ToInt64(values[key], CultureInfo.InvariantCulture);
            rows.Add(new(id, values));
        }
        return rows;
    }

    public static async Task<string> HashAsync(string path, CancellationToken token)
    {
        await using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        byte[] hash = await SHA256.HashDataAsync(stream, token);
        return Convert.ToHexString(hash);
    }
}

public sealed class JsonFixtureSourceReader : ILegacySourceReader
{
    public async Task<LegacySnapshot> ReadAsync(string sourcePath, CancellationToken cancellationToken)
    {
        if (!File.Exists(sourcePath)) throw new SourceInvalidException("Fixture source does not exist.");
        await using FileStream stream = File.OpenRead(sourcePath);
        using JsonDocument document = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (document.RootElement.GetProperty("fixture_schema").GetString() != "contracts-migration-fixture-v1")
            throw new SourceInvalidException("Fixture schema marker is invalid.");
        Dictionary<string, IReadOnlyList<LegacyRow>> tables = new(StringComparer.OrdinalIgnoreCase);
        foreach (JsonProperty table in document.RootElement.GetProperty("tables").EnumerateObject())
        {
            List<LegacyRow> rows = [];
            foreach (JsonElement element in table.Value.EnumerateArray())
            {
                Dictionary<string, object?> values = new(StringComparer.OrdinalIgnoreCase);
                foreach (JsonProperty property in element.EnumerateObject()) values[property.Name] = ToObject(property.Value);
                long id = Convert.ToInt64(values.Values.First(), CultureInfo.InvariantCulture);
                rows.Add(new(id, values));
            }
            tables[table.Name] = rows.OrderBy(row => row.Id).ToArray();
        }
        string hash = await AccessLegacySourceReader.HashAsync(sourcePath, cancellationToken);
        return new(hash, "JSON_FIXTURE", tables, new Dictionary<string, bool> { ["tblTradeBank"] = false, ["tblSupplierBalance"] = false });
    }

    private static object? ToObject(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.Null => null,
        JsonValueKind.String when value.TryGetDateTime(out DateTime date) => date,
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Number when value.TryGetInt64(out long number) => number,
        JsonValueKind.Number => value.GetDecimal(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        _ => value.GetRawText(),
    };
}
