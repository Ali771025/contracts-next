using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1861 // EF-generated migration metadata uses constant arrays.

namespace Contracts.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddLegacyReceiptType : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[] { 7L, "UNSPECIFIED_LEGACY", 1L, 639028224000000000L, null, true, 70, "نامشخص در داده قدیمی", 639028224000000000L, null });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
            migrationBuilder.DeleteData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 7L);
    }
}
