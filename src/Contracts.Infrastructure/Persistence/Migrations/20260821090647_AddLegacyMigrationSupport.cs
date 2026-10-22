using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1861 // EF-generated migration metadata uses constant arrays.

namespace Contracts.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddLegacyMigrationSupport : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
            migrationBuilder.AddColumn<bool>(
                name: "MustChangePassword",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.InsertData(
                table: "ContractStatuses",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 5L, "SUSPENDED", 1L, 639028224000000000L, null, true, 50, "تعلیق", 639028224000000000L, null },
                    { 6L, "TERMINATED", 1L, 639028224000000000L, null, true, 60, "خاتمه", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "InvoiceTypes",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 3L, "ADJUSTMENT", 1L, 639028224000000000L, null, true, 30, "تعدیل", 639028224000000000L, null },
                    { 4L, "PROGRESS", 1L, 639028224000000000L, null, true, 40, "صورت‌وضعیت", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "PartyTypes",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 5L, "LEGAL_ENTITY", 1L, 639028224000000000L, null, true, 50, "شخص حقوقی", 639028224000000000L, null },
                    { 6L, "NATURAL_PERSON", 1L, 639028224000000000L, null, true, 60, "شخص حقیقی", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "ProjectStatuses",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[] { 5L, "SUSPENDED", 1L, 639028224000000000L, null, true, 50, "تعلیق", 639028224000000000L, null });
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
            migrationBuilder.DeleteData(
                table: "ContractStatuses",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "ContractStatuses",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "InvoiceTypes",
                keyColumn: "Id",
                keyValue: 3L);

            migrationBuilder.DeleteData(
                table: "InvoiceTypes",
                keyColumn: "Id",
                keyValue: 4L);

            migrationBuilder.DeleteData(
                table: "PartyTypes",
                keyColumn: "Id",
                keyValue: 5L);

            migrationBuilder.DeleteData(
                table: "PartyTypes",
                keyColumn: "Id",
                keyValue: 6L);

            migrationBuilder.DeleteData(
                table: "ProjectStatuses",
                keyColumn: "Id",
                keyValue: 5L);


            migrationBuilder.DropColumn(
                name: "MustChangePassword",
                table: "Users");
    }
}
