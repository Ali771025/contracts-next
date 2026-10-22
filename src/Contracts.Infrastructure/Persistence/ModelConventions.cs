namespace Contracts.Infrastructure.Persistence;

internal static class ModelConventions
{
    private static readonly ValueConverter<DateTimeOffset, long> UtcTimestampConverter = new(
        value => value.UtcTicks,
        value => new DateTimeOffset(value, TimeSpan.Zero));

    private static readonly ValueConverter<DateTimeOffset?, long?> NullableUtcTimestampConverter = new(
        value => value.HasValue ? value.Value.UtcTicks : null,
        value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);

    private static readonly ValueConverter<DateOnly, string> DateOnlyConverter = new(
        value => value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        value => DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None));

    private static readonly ValueConverter<DateOnly?, string?> NullableDateOnlyConverter = new(
        value => value.HasValue ? value.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) : null,
        value => value == null ? null : DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None));

    public static void Apply(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (IMutableProperty property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTimeOffset))
                {
                    property.SetValueConverter(UtcTimestampConverter);
                    property.SetColumnType("INTEGER");
                }
                else if (property.ClrType == typeof(DateTimeOffset?))
                {
                    property.SetValueConverter(NullableUtcTimestampConverter);
                    property.SetColumnType("INTEGER");
                }
                else if (property.ClrType == typeof(DateOnly))
                {
                    property.SetValueConverter(DateOnlyConverter);
                    property.SetColumnType("TEXT");
                }
                else if (property.ClrType == typeof(DateOnly?))
                {
                    property.SetValueConverter(NullableDateOnlyConverter);
                    property.SetColumnType("TEXT");
                }
            }
        }
    }
}
