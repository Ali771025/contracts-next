namespace Contracts.Domain.Common;

internal static class Guard
{
    private static readonly Regex StableCodePattern = new("^[A-Z][A-Z0-9_]*$", RegexOptions.CultureInvariant);

    public static string RequiredText(string value, string parameterName, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        string normalized = value.Trim();
        if (normalized.Length > maxLength)
        {
            throw new ArgumentOutOfRangeException(parameterName, $"Maximum length is {maxLength}.");
        }

        return normalized;
    }

    public static string? OptionalText(string? value, string parameterName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return RequiredText(value, parameterName, maxLength);
    }

    public static string StableCode(string value, string parameterName)
    {
        string normalized = RequiredText(value, parameterName, 64);
        if (!StableCodePattern.IsMatch(normalized))
        {
            throw new ArgumentException("Codes must be uppercase English identifiers.", parameterName);
        }

        return normalized;
    }

    public static long PositiveAmount(long value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Amount must be positive.");
        }

        return value;
    }

    public static long NonNegativeAmount(long value, string parameterName)
    {
        if (value < 0)
        {
            throw new ArgumentOutOfRangeException(parameterName, "Amount cannot be negative.");
        }

        return value;
    }

    public static long RequiredId(long value, string parameterName)
    {
        if (value <= 0)
        {
            throw new ArgumentOutOfRangeException(parameterName);
        }

        return value;
    }
}
