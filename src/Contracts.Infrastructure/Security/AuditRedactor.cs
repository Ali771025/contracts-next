using System.Text.Json;
using System.Text.Json.Nodes;

namespace Contracts.Infrastructure.Security;

public static class AuditRedactor
{
    private static readonly string[] SensitiveTerms = ["password", "salt", "token", "secret", "attachment", "content"];
    private static readonly JsonSerializerOptions SerializerOptions = new() { WriteIndented = false };

    public static string? Redact(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            JsonNode? node = JsonNode.Parse(json);
            RedactNode(node);
            return node?.ToJsonString(SerializerOptions);
        }
        catch (JsonException) { return "\"[REDACTED_INVALID_JSON]\""; }
    }

    private static void RedactNode(JsonNode? node)
    {
        if (node is JsonObject item)
        {
            foreach (string key in item.Select(pair => pair.Key).ToArray())
            {
                if (SensitiveTerms.Any(term => key.Contains(term, StringComparison.OrdinalIgnoreCase))) item[key] = "[REDACTED]";
                else RedactNode(item[key]);
            }
        }
        else if (node is JsonArray array)
        {
            foreach (JsonNode? child in array) RedactNode(child);
        }
    }
}
