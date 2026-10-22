namespace Contracts.Domain.Common;

using System.Text;

public static class TextNormalizer
{
    public static string SearchKey(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        string mapped = value.Normalize(NormalizationForm.FormKC)
            .Replace('ي', 'ی').Replace('ى', 'ی').Replace('ك', 'ک')
            .Replace('\u200C', ' ').Replace('\u200F', ' ').Replace('\u200E', ' ');
        return string.Join(' ', mapped.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).ToUpperInvariant();
    }

    public static string Digits(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;
        const string persian = "۰۱۲۳۴۵۶۷۸۹";
        const string arabic = "٠١٢٣٤٥٦٧٨٩";
        return string.Concat(value.Trim().Select(character =>
        {
            int index = persian.IndexOf(character, StringComparison.Ordinal);
            if (index < 0) index = arabic.IndexOf(character, StringComparison.Ordinal);
            return index >= 0 ? (char)('0' + index) : character;
        }));
    }

    public static string IdentifierKey(string? value) => SearchKey(Digits(value));
}
