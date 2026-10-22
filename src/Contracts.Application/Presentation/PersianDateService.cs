using System.Globalization;

namespace Contracts.Application.Presentation;

public sealed class PersianDateOptions
{
    public bool UsePersianDigits { get; set; } = true;
}

public interface IPersianDateService
{
    public DateOnly MinSupportedDate { get; }
    public DateOnly MaxSupportedDate { get; }
    public string Format(DateOnly value);
    public bool TryParse(string? text, out DateOnly result);
}

public sealed class PersianDateService(PersianDateOptions options) : IPersianDateService
{
    private static readonly PersianCalendar Calendar = new();
    private static readonly char[] PersianDigits = "۰۱۲۳۴۵۶۷۸۹".ToCharArray();
    private static readonly char[] ArabicDigits = "٠١٢٣٤٥٦٧٨٩".ToCharArray();

    public DateOnly MinSupportedDate { get; } = DateOnly.FromDateTime(Calendar.MinSupportedDateTime);
    public DateOnly MaxSupportedDate { get; } = DateOnly.FromDateTime(Calendar.MaxSupportedDateTime);

    public string Format(DateOnly value)
    {
        EnsureSupported(value);
        DateTime dateTime = value.ToDateTime(TimeOnly.MinValue);
        string result = string.Create(CultureInfo.InvariantCulture,
            $"{Calendar.GetYear(dateTime):0000}/{Calendar.GetMonth(dateTime):00}/{Calendar.GetDayOfMonth(dateTime):00}");
        return options.UsePersianDigits ? ToPersianDigits(result) : result;
    }

    public bool TryParse(string? text, out DateOnly result)
    {
        result = default;
        string normalized = NormalizeDigits(text?.Trim() ?? string.Empty);
        string[] parts = normalized.Split('/');
        if (parts.Length != 3 || parts[0].Length != 4 || parts[1].Length != 2 || parts[2].Length != 2 ||
            !int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out int year) ||
            !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int month) ||
            !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out int day)) return false;
        try
        {
            DateTime converted = Calendar.ToDateTime(year, month, day, 0, 0, 0, 0);
            result = DateOnly.FromDateTime(converted);
            return result >= MinSupportedDate && result <= MaxSupportedDate;
        }
        catch (ArgumentOutOfRangeException) { return false; }
    }

    private static string NormalizeDigits(string value)
    {
        Span<char> buffer = stackalloc char[value.Length];
        for (int index = 0; index < value.Length; index++)
        {
            int persian = Array.IndexOf(PersianDigits, value[index]);
            int arabic = Array.IndexOf(ArabicDigits, value[index]);
            buffer[index] = persian >= 0 ? (char)('0' + persian) : arabic >= 0 ? (char)('0' + arabic) : value[index];
        }
        return new(buffer);
    }

    private static string ToPersianDigits(string value) => string.Concat(value.Select(character =>
        character is >= '0' and <= '9' ? PersianDigits[character - '0'] : character));

    private void EnsureSupported(DateOnly date)
    {
        if (date < MinSupportedDate || date > MaxSupportedDate)
            throw new ArgumentOutOfRangeException(nameof(date), date, "Date is outside the Persian calendar supported range.");
    }
}
