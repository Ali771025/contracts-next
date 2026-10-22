using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
// EF-generated migration shape and composite-index array are intentionally retained.
#pragma warning disable IDE0161, CA1861

namespace Contracts.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ContractManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedContractNo",
                table: "Contracts",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitle",
                table: "Contracts",
                type: "TEXT",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE Contracts SET
                  NormalizedContractNo = UPPER(TRIM(REPLACE(REPLACE(REPLACE(REPLACE(
                    REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                    REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
                    ContractNo,'۰','0'),'۱','1'),'۲','2'),'۳','3'),'۴','4'),'۵','5'),'۶','6'),'۷','7'),'۸','8'),'۹','9'),
                    '٠','0'),'١','1'),'٢','2'),'٣','3'),'٤','4'),'٥','5'),'٦','6'),'٧','7'),'٨','8'),'٩','9'),
                    'ي','ی'),'ى','ی'),'ك','ک'),'  ',' '))),
                  NormalizedTitle = UPPER(TRIM(REPLACE(REPLACE(REPLACE(REPLACE(Title,'ي','ی'),'ى','ی'),'ك','ک'),'  ',' ')));
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_NormalizedContractNo",
                table: "Contracts",
                column: "NormalizedContractNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_NormalizedTitle_Id",
                table: "Contracts",
                columns: new[] { "NormalizedTitle", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Contracts_NormalizedContractNo",
                table: "Contracts");

            migrationBuilder.DropIndex(
                name: "IX_Contracts_NormalizedTitle_Id",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "NormalizedContractNo",
                table: "Contracts");

            migrationBuilder.DropColumn(
                name: "NormalizedTitle",
                table: "Contracts");
        }
    }
}
