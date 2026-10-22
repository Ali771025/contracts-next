using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Contracts.Infrastructure.Persistence.Migrations;

    /// <inheritdoc />
    public partial class P10OperationalFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StoragePath",
                table: "ContractDocuments",
                newName: "RelativePath");

            migrationBuilder.RenameColumn(
                name: "FileName",
                table: "ContractDocuments",
                newName: "StoredFileName");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Invoices",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Guarantees",
                type: "TEXT",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "FileSize",
                table: "ContractDocuments",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<string>(
                name: "OriginalFileName",
                table: "ContractDocuments",
                type: "TEXT",
                maxLength: 260,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "ContractAmendments",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subject",
                table: "ContractAmendments",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeduplicationKey",
                table: "Alerts",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                collation: "NOCASE");

            migrationBuilder.AddColumn<long>(
                name: "FinancialTransactionId",
                table: "Alerts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ResolvedAtUtc",
                table: "Alerts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ResolvedByUserId",
                table: "Alerts",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE ContractDocuments
                SET OriginalFileName = StoredFileName,
                    StoredFileName = 'legacy-' || Id,
                    RelativePath = 'legacy/' || Id || '/legacy-' || Id,
                    FileSize = 0;
                """);

            migrationBuilder.Sql("""
                UPDATE Alerts
                SET DeduplicationKey = 'LEGACY_ALERT:' || Id;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ContractDocuments_RelativePath",
                table: "ContractDocuments",
                column: "RelativePath",
                unique: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_ContractDocuments_Size",
                table: "ContractDocuments",
                sql: "FileSize >= 0");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_DeduplicationKey",
                table: "Alerts",
                column: "DeduplicationKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_FinancialTransactionId",
                table: "Alerts",
                column: "FinancialTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Alerts_FinancialTransactions_FinancialTransactionId",
                table: "Alerts",
                column: "FinancialTransactionId",
                principalTable: "FinancialTransactions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Alerts_FinancialTransactions_FinancialTransactionId",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_ContractDocuments_RelativePath",
                table: "ContractDocuments");

            migrationBuilder.DropCheckConstraint(
                name: "CK_ContractDocuments_Size",
                table: "ContractDocuments");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_DeduplicationKey",
                table: "Alerts");

            migrationBuilder.DropIndex(
                name: "IX_Alerts_FinancialTransactionId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Invoices");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "Guarantees");

            migrationBuilder.DropColumn(
                name: "FileSize",
                table: "ContractDocuments");

            migrationBuilder.DropColumn(
                name: "OriginalFileName",
                table: "ContractDocuments");

            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "ContractAmendments");

            migrationBuilder.DropColumn(
                name: "Subject",
                table: "ContractAmendments");

            migrationBuilder.DropColumn(
                name: "DeduplicationKey",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "FinancialTransactionId",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ResolvedAtUtc",
                table: "Alerts");

            migrationBuilder.DropColumn(
                name: "ResolvedByUserId",
                table: "Alerts");

            migrationBuilder.RenameColumn(
                name: "StoredFileName",
                table: "ContractDocuments",
                newName: "FileName");

            migrationBuilder.RenameColumn(
                name: "RelativePath",
                table: "ContractDocuments",
                newName: "StoragePath");
        }
    }
