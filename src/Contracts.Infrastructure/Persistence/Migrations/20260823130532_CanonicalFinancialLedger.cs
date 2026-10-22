using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
// EF-generated migration shape and composite-index array are intentionally retained.
#pragma warning disable IDE0161, CA1861

namespace Contracts.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class CanonicalFinancialLedger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "Receipts",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChequeNo",
                table: "Receipts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNo",
                table: "Receipts",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayerDescription",
                table: "Receipts",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReceiptNo",
                table: "Receipts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "NOCASE");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentPath",
                table: "Payments",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChequeNo",
                table: "Payments",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNo",
                table: "Payments",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayerDescription",
                table: "Payments",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentNo",
                table: "Payments",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                collation: "NOCASE");

            migrationBuilder.AddColumn<string>(
                name: "TransactionDate",
                table: "FinancialTransactions",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE Payments
                SET PaymentNo = COALESCE(NULLIF(TRIM(ReferenceNo), ''), 'PAY') || '-' || Id;
                UPDATE Receipts
                SET ReceiptNo = COALESCE(NULLIF(TRIM(ReferenceNo), ''), 'REC') || '-' || Id;
                UPDATE FinancialTransactions
                SET TransactionDate = DueDate;
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_ReceiptNo",
                table: "Receipts",
                column: "ReceiptNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentNo",
                table: "Payments",
                column: "PaymentNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_TransactionDate_Id",
                table: "FinancialTransactions",
                columns: new[] { "TransactionDate", "Id" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinancialTransactions_Dates",
                table: "FinancialTransactions",
                sql: "DueDate >= TransactionDate");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Receipts_ReceiptNo",
                table: "Receipts");

            migrationBuilder.DropIndex(
                name: "IX_Payments_PaymentNo",
                table: "Payments");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactions_TransactionDate_Id",
                table: "FinancialTransactions");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinancialTransactions_Dates",
                table: "FinancialTransactions");

            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "ChequeNo",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "InvoiceNo",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "PayerDescription",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "ReceiptNo",
                table: "Receipts");

            migrationBuilder.DropColumn(
                name: "AttachmentPath",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "ChequeNo",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "InvoiceNo",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PayerDescription",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "PaymentNo",
                table: "Payments");

            migrationBuilder.DropColumn(
                name: "TransactionDate",
                table: "FinancialTransactions");
        }
    }
}
