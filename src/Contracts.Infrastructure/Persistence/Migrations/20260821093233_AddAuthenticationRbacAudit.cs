using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1861 // EF-generated migration metadata uses constant arrays.

namespace Contracts.Infrastructure.Persistence.Migrations;

/// <inheritdoc />
public partial class AddAuthenticationRbacAudit : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
            migrationBuilder.RenameColumn(
                name: "PermissionCode",
                table: "RolePermissions",
                newName: "SectionCode");

            migrationBuilder.RenameColumn(
                name: "CanUpdate",
                table: "RolePermissions",
                newName: "CanEdit");

            migrationBuilder.RenameColumn(
                name: "CanCreate",
                table: "RolePermissions",
                newName: "CanAdd");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_RoleId_PermissionCode",
                table: "RolePermissions",
                newName: "IX_RolePermissions_RoleId_SectionCode");

            migrationBuilder.RenameColumn(
                name: "DetailsJson",
                table: "AuditLogs",
                newName: "BeforeJson");

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginCount",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsBootstrapAdministrator",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<long>(
                name: "LockedUntilUtc",
                table: "Users",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordAlgorithm",
                table: "Users",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordHash",
                table: "Users",
                type: "BLOB",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PasswordIterations",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "PasswordSalt",
                table: "Users",
                type: "BLOB",
                maxLength: 32,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PasswordVersion",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "CanExport",
                table: "RolePermissions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CanAdmin",
                table: "RolePermissions",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AfterJson",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorrelationId",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Outcome",
                table: "AuditLogs",
                type: "TEXT",
                maxLength: 32,
                nullable: false,
                defaultValue: "");

            migrationBuilder.InsertData(
                table: "RolePermissions",
                columns: new[] { "Id", "CanAdd", "CanAdmin", "CanDelete", "CanEdit", "CanExport", "CanView", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "RoleId", "SectionCode", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1001L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "DASHBOARD", 639028224000000000L, null },
                    { 1002L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "CONTRACTS", 639028224000000000L, null },
                    { 1003L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "PARTIES", 639028224000000000L, null },
                    { 1004L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "PROJECTS", 639028224000000000L, null },
                    { 1005L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "PAYMENTS", 639028224000000000L, null },
                    { 1006L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "RECEIPTS", 639028224000000000L, null },
                    { 1007L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "FINANCIALLEDGER", 639028224000000000L, null },
                    { 1008L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "GUARANTEES", 639028224000000000L, null },
                    { 1009L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "AMENDMENTS", 639028224000000000L, null },
                    { 1010L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "INVOICES", 639028224000000000L, null },
                    { 1011L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "DOCUMENTS", 639028224000000000L, null },
                    { 1012L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "ALERTS", 639028224000000000L, null },
                    { 1013L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "BUDGET", 639028224000000000L, null },
                    { 1014L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "EVALUATION", 639028224000000000L, null },
                    { 1015L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "REPORTS", 639028224000000000L, null },
                    { 1016L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "KPI", 639028224000000000L, null },
                    { 1017L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "OKR", 639028224000000000L, null },
                    { 1018L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "USERS", 639028224000000000L, null },
                    { 1019L, true, true, true, true, true, true, 1L, 639028224000000000L, null, 1L, "SETTINGS", 639028224000000000L, null },
                    { 1020L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "DASHBOARD", 639028224000000000L, null },
                    { 1021L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "CONTRACTS", 639028224000000000L, null },
                    { 1022L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "PARTIES", 639028224000000000L, null },
                    { 1023L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "PROJECTS", 639028224000000000L, null },
                    { 1024L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "PAYMENTS", 639028224000000000L, null },
                    { 1025L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "RECEIPTS", 639028224000000000L, null },
                    { 1026L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "FINANCIALLEDGER", 639028224000000000L, null },
                    { 1027L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "GUARANTEES", 639028224000000000L, null },
                    { 1028L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "AMENDMENTS", 639028224000000000L, null },
                    { 1029L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "INVOICES", 639028224000000000L, null },
                    { 1030L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "DOCUMENTS", 639028224000000000L, null },
                    { 1031L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "ALERTS", 639028224000000000L, null },
                    { 1032L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "BUDGET", 639028224000000000L, null },
                    { 1033L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "EVALUATION", 639028224000000000L, null },
                    { 1034L, false, false, false, false, true, true, 1L, 639028224000000000L, null, 3L, "REPORTS", 639028224000000000L, null },
                    { 1035L, false, false, false, false, true, true, 1L, 639028224000000000L, null, 3L, "KPI", 639028224000000000L, null },
                    { 1036L, false, false, false, false, false, true, 1L, 639028224000000000L, null, 3L, "OKR", 639028224000000000L, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsBootstrapAdministrator",
                table: "Users",
                column: "IsBootstrapAdministrator",
                unique: true,
                filter: "IsBootstrapAdministrator = 1");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CorrelationId",
                table: "AuditLogs",
                column: "CorrelationId");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
            migrationBuilder.DropIndex(
                name: "IX_Users_IsBootstrapAdministrator",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_AuditLogs_CorrelationId",
                table: "AuditLogs");

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1001L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1002L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1003L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1004L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1005L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1006L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1007L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1008L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1009L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1010L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1011L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1012L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1013L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1014L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1015L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1016L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1017L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1018L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1019L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1020L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1021L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1022L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1023L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1024L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1025L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1026L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1027L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1028L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1029L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1030L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1031L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1032L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1033L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1034L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1035L);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumn: "Id",
                keyValue: 1036L);

            migrationBuilder.DropColumn(
                name: "FailedLoginCount",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsBootstrapAdministrator",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockedUntilUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordAlgorithm",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordIterations",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordSalt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordVersion",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CanExport",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "CanAdmin",
                table: "RolePermissions");

            migrationBuilder.DropColumn(
                name: "AfterJson",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "CorrelationId",
                table: "AuditLogs");

            migrationBuilder.DropColumn(
                name: "Outcome",
                table: "AuditLogs");

            migrationBuilder.RenameColumn(
                name: "SectionCode",
                table: "RolePermissions",
                newName: "PermissionCode");

            migrationBuilder.RenameColumn(
                name: "CanEdit",
                table: "RolePermissions",
                newName: "CanUpdate");

            migrationBuilder.RenameColumn(
                name: "CanAdd",
                table: "RolePermissions",
                newName: "CanCreate");

            migrationBuilder.RenameIndex(
                name: "IX_RolePermissions_RoleId_SectionCode",
                table: "RolePermissions",
                newName: "IX_RolePermissions_RoleId_PermissionCode");

            migrationBuilder.RenameColumn(
                name: "BeforeJson",
                table: "AuditLogs",
                newName: "DetailsJson");
    }
}
