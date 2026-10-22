using System.Globalization;

namespace Contracts.Application.Presentation;

public interface INumberDisplayService
{
    public string FormatMoney(long amount);
    public string FormatMinor(long amountMinor, int scale, string? currencyCode = null);
}

public sealed class NumberDisplayService(PersianDateOptions options) : INumberDisplayService
{
    private static readonly char[] PersianDigits = "۰۱۲۳۴۵۶۷۸۹".ToCharArray();
    private static readonly NumberFormatInfo PersianNumberFormat = CreateNumberFormat();
    public string FormatMoney(long amount)
    {
        string formatted = amount.ToString("N0", PersianNumberFormat);
        return options.UsePersianDigits ? string.Concat(formatted.Select(character =>
            character is >= '0' and <= '9' ? PersianDigits[character - '0'] : character)) : formatted;
    }

    public string FormatMinor(long amountMinor, int scale, string? currencyCode = null)
    {
        if (scale is < 0 or > 9) throw new ArgumentOutOfRangeException(nameof(scale));
        decimal amount = amountMinor / (decimal)Pow10(scale);
        NumberFormatInfo format = (NumberFormatInfo)PersianNumberFormat.Clone();
        format.NumberDecimalDigits = scale;
        string value = amount.ToString($"N{scale}", format);
        if (options.UsePersianDigits)
            value = string.Concat(value.Select(character => character is >= '0' and <= '9' ? PersianDigits[character - '0'] : character));
        return string.IsNullOrWhiteSpace(currencyCode) ? value : $"{value} {currencyCode}";
    }

    private static long Pow10(int scale)
    {
        long value = 1;
        for (int index = 0; index < scale; index++) value = checked(value * 10);
        return value;
    }

    private static NumberFormatInfo CreateNumberFormat()
    {
        NumberFormatInfo format = (NumberFormatInfo)CultureInfo.GetCultureInfo("fa-IR").NumberFormat.Clone();
        format.NumberDecimalDigits = 0;
        return format;
    }
}
