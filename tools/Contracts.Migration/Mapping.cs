using System.Globalization;
using System.Text;
using System.Text.Json;

namespace Contracts.Migration;

public sealed record MappingFile(string Version, Dictionary<string, Dictionary<string, string>> Maps)
{
    private static readonly JsonSerializerOptions SerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public static async Task<MappingFile> LoadAsync(string? path, CancellationToken token)
    {
        if (path is null) return Defaults();
        if (!File.Exists(path)) throw new SourceInvalidException("Mapping file does not exist.");
        await using FileStream stream = File.OpenRead(path);
        MappingFile? mapping = await JsonSerializer.DeserializeAsync<MappingFile>(stream, SerializerOptions, token);
        if (mapping is null || string.IsNullOrWhiteSpace(mapping.Version) || mapping.Maps.Count == 0)
            throw new SourceInvalidException("Mapping file is empty or invalid.");
        return mapping.NormalizeKeys();
    }

    public bool TryMap(string category, string? source, out string code)
    {
        code = string.Empty;
        return source is not null && Maps.TryGetValue(category, out Dictionary<string, string>? map) &&
            map.TryGetValue(PersianText.Normalize(source), out code!);
    }

    private MappingFile NormalizeKeys() => new(Version, Maps.ToDictionary(pair => pair.Key,
        pair => pair.Value.ToDictionary(item => PersianText.Normalize(item.Key), item => item.Value,
            StringComparer.Ordinal), StringComparer.OrdinalIgnoreCase));

    private static MappingFile Defaults()
    {
        Dictionary<string, Dictionary<string, string>> maps = new(StringComparer.OrdinalIgnoreCase)
        {
            ["Role"] = Map(("مدیر سیستم", "SYSTEM_ADMIN"), ("مدیر قرارداد", "CONTRACT_MANAGER"),
                ("مدیرعامل", "CHIEF_EXECUTIVE"), ("مشاهده‌کننده", "VIEWER"),
                ("کارشناس حقوقی", "LEGAL_EXPERT"), ("کارشناس مالی", "FINANCE_EXPERT")),
            ["PartyType"] = Map(("حقوقی", "LEGAL_ENTITY"), ("حقیقی", "NATURAL_PERSON")),
            ["ContractType"] = Map(("خرید", "PURCHASE"), ("فروش", "SALE"),
                ("برون‌سپاری", "OUTSOURCE"), ("پیمانکاری", "CONTRACTING"), ("مشاوره", "CONSULTING")),
            ["ContractStatus"] = Map(("پیش‌نویس", "DRAFT"), ("فعال", "ACTIVE"),
                ("تکمیل‌شده", "COMPLETED"), ("لغوشده", "CANCELLED"), ("تعلیق", "SUSPENDED"), ("خاتمه", "TERMINATED")),
            ["ProjectStatus"] = Map(("برنامه‌ریزی‌شده", "PLANNED"), ("فعال", "ACTIVE"),
                ("تکمیل‌شده", "COMPLETED"), ("لغوشده", "CANCELLED"), ("تعلیق", "SUSPENDED"), ("خاتمه", "COMPLETED")),
            ["Currency"] = Map(("ریال", "IRR"), ("IRR", "IRR"), ("دلار", "USD"), ("USD", "USD"), ("یورو", "EUR"), ("EUR", "EUR")),
            ["PaymentType"] = Map(("نقدی", "CASH"), ("چک", "CHEQUE"), ("حواله بانکی", "BANK_TRANSFER"),
                ("انتقال بانکی", "BANK_TRANSFER"), ("پیش‌پرداخت", "ADVANCE"), ("علی‌الحساب", "ON_ACCOUNT"), ("رهسا", "RAHSA")),
            ["GuaranteeType"] = Map(("شرکت در مناقصه", "TENDER"), ("پیش‌پرداخت", "ADVANCE_PAYMENT"),
                ("حسن انجام تعهدات", "PERFORMANCE"), ("حسن انجام کار", "PERFORMANCE"), ("کسور وجه‌الضمان", "RETENTION")),
            ["GuaranteeStatus"] = Map(("فعال", "ACTIVE"), ("معتبر", "ACTIVE"), ("آزادشده", "RELEASED"),
                ("منقضی", "EXPIRED"), ("ضبط‌شده", "CLAIMED")),
            ["InvoiceType"] = Map(("خرید", "PURCHASE"), ("فروش", "SALE"), ("تعدیل", "ADJUSTMENT"), ("صورت‌وضعیت", "PROGRESS")),
            ["PaymentStatus"] = Map(("پرداخت‌نشده", "UNPAID"), ("پرداخت جزئی", "PARTIAL"),
                ("پرداخت‌شده", "PAID"), ("لغوشده", "CANCELLED")),
            ["DocumentType"] = Map(("قرارداد", "CONTRACT"), ("الحاقیه", "AMENDMENT"),
                ("صورتحساب", "INVOICE"), ("تضمین", "GUARANTEE"), ("سایر", "OTHER")),
            ["AlertType"] = Map(("پایان قرارداد", "CONTRACT_EXPIRY"), ("انقضای ضمانت‌نامه", "GUARANTEE_EXPIRY"),
                ("سررسید پرداخت", "PAYMENT_DUE"), ("سررسید دریافت", "RECEIPT_DUE")),
            ["AlertStatus"] = Map(("باز", "OPEN"), ("انجام‌شده", "COMPLETED"), ("نادیده‌گرفته‌شده", "DISMISSED")),
            ["Priority"] = Map(("کم", "LOW"), ("متوسط", "MEDIUM"), ("زیاد", "HIGH"), ("بحرانی", "CRITICAL")),
        };
        return new MappingFile("p04-default-v1", maps).NormalizeKeys();
    }

    private static Dictionary<string, string> Map(params (string Key, string Value)[] pairs) =>
        pairs.ToDictionary(pair => PersianText.Normalize(pair.Key), pair => pair.Value, StringComparer.Ordinal);
}

public static class PersianText
{
    public static string Normalize(string value)
    {
        string normalized = value.Normalize(NormalizationForm.FormKC)
            .Replace('\u064A', '\u06CC').Replace('\u0649', '\u06CC').Replace('\u0643', '\u06A9')
            .Replace("\u200C", " ").Replace("\u200D", string.Empty);
        StringBuilder result = new();
        bool whitespace = false;
        foreach (char character in normalized)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(character);
            if (category is UnicodeCategory.NonSpacingMark or UnicodeCategory.Format) continue;
            if (char.IsWhiteSpace(character)) { whitespace = result.Length > 0; continue; }
            if (whitespace) result.Append(' ');
            whitespace = false;
            result.Append(char.ToUpperInvariant(character));
        }
        return result.ToString().Trim();
    }
}
