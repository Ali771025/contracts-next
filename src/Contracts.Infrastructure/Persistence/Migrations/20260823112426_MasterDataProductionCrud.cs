using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
// EF-generated migration shape and composite-index arrays are intentionally retained.
#pragma warning disable IDE0161, CA1861

namespace Contracts.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class MasterDataProductionCrud : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "TransactionTypes",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "Roles",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "ProjectStatuses",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Department",
                table: "Projects",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Employer",
                table: "Projects",
                type: "TEXT",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedProjectNo",
                table: "Projects",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitle",
                table: "Projects",
                type: "TEXT",
                maxLength: 300,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "Priorities",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "PaymentTypes",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "PaymentStatuses",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "PartyTypes",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ContactPerson",
                table: "Parties",
                type: "TEXT",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Parties",
                type: "TEXT",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mobile",
                table: "Parties",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NormalizedName",
                table: "Parties",
                type: "TEXT",
                maxLength: 250,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "InvoiceTypes",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "GuaranteeTypes",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "GuaranteeStatuses",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "FinancialTransactionStatuses",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "DocumentTypes",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "Currencies",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "ContractTypes",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "ContractStatuses",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "ContractCategories",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "Banks",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "AlertTypes",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NormalizedTitleFa",
                table: "AlertStatuses",
                type: "TEXT",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            // Backfill rows that may have been imported before P07. Seed updates below
            // cover known rows, while these statements make an existing production
            // database safe to upgrade before the unique indexes are created.
            migrationBuilder.Sql("UPDATE Parties SET NormalizedName = UPPER(TRIM(REPLACE(REPLACE(REPLACE(Name, 'ي', 'ی'), 'ى', 'ی'), 'ك', 'ک')));");
            migrationBuilder.Sql("UPDATE Projects SET NormalizedProjectNo = UPPER(TRIM(REPLACE(REPLACE(REPLACE(ProjectNo, 'ي', 'ی'), 'ى', 'ی'), 'ك', 'ک'))), NormalizedTitle = UPPER(TRIM(REPLACE(REPLACE(REPLACE(Title, 'ي', 'ی'), 'ى', 'ی'), 'ك', 'ک')));");
            string[] referenceTables =
            [
                "AlertStatuses", "AlertTypes", "Banks", "ContractCategories", "ContractStatuses",
                "ContractTypes", "Currencies", "DocumentTypes", "FinancialTransactionStatuses",
                "GuaranteeStatuses", "GuaranteeTypes", "InvoiceTypes", "PartyTypes", "PaymentStatuses",
                "PaymentTypes", "Priorities", "ProjectStatuses", "Roles", "TransactionTypes",
            ];
            foreach (string table in referenceTables)
            {
                migrationBuilder.Sql($"UPDATE {table} SET NormalizedTitleFa = UPPER(TRIM(REPLACE(REPLACE(REPLACE(TitleFa, 'ي', 'ی'), 'ى', 'ی'), 'ك', 'ک')));");
            }

            migrationBuilder.UpdateData(
                table: "AlertStatuses",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "باز");

            migrationBuilder.UpdateData(
                table: "AlertStatuses",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "انجام شده");

            migrationBuilder.UpdateData(
                table: "AlertStatuses",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "نادیده گرفته شده");

            migrationBuilder.UpdateData(
                table: "AlertTypes",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "سررسید قرارداد");

            migrationBuilder.UpdateData(
                table: "AlertTypes",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "سررسید تضمین");

            migrationBuilder.UpdateData(
                table: "AlertTypes",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "سررسید پرداخت");

            migrationBuilder.UpdateData(
                table: "AlertTypes",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "سررسید دریافت");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "بانک ملی ایران");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "بانک سپه");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "پست بانک ایران");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "بانک کشاورزی");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 5L,
                column: "NormalizedTitleFa",
                value: "بانک مسکن");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 6L,
                column: "NormalizedTitleFa",
                value: "بانک صنعت و معدن");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 7L,
                column: "NormalizedTitleFa",
                value: "بانک توسعه صادرات ایران");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 8L,
                column: "NormalizedTitleFa",
                value: "بانک توسعه تعاون");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 9L,
                column: "NormalizedTitleFa",
                value: "بانک قرض الحسنه مهر ایران");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 10L,
                column: "NormalizedTitleFa",
                value: "بانک قرض الحسنه رسالت");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 11L,
                column: "NormalizedTitleFa",
                value: "بانک اقتصاد نوین");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 12L,
                column: "NormalizedTitleFa",
                value: "بانک کارآفرین");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 13L,
                column: "NormalizedTitleFa",
                value: "بانک پارسیان");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 14L,
                column: "NormalizedTitleFa",
                value: "بانک پاسارگاد");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 15L,
                column: "NormalizedTitleFa",
                value: "بانک سامان");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 16L,
                column: "NormalizedTitleFa",
                value: "بانک سرمایه");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 17L,
                column: "NormalizedTitleFa",
                value: "بانک سینا");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 18L,
                column: "NormalizedTitleFa",
                value: "بانک شهر");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 19L,
                column: "NormalizedTitleFa",
                value: "بانک دی");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 20L,
                column: "NormalizedTitleFa",
                value: "بانک ملت");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 21L,
                column: "NormalizedTitleFa",
                value: "بانک رفاه کارگران");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 22L,
                column: "NormalizedTitleFa",
                value: "بانک صادرات ایران");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 23L,
                column: "NormalizedTitleFa",
                value: "بانک تجارت");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 24L,
                column: "NormalizedTitleFa",
                value: "بانک گردشگری");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 25L,
                column: "NormalizedTitleFa",
                value: "بانک خاورمیانه");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 26L,
                column: "NormalizedTitleFa",
                value: "بانک ایران زمین");

            migrationBuilder.UpdateData(
                table: "Banks",
                keyColumn: "Id",
                keyValue: 27L,
                column: "NormalizedTitleFa",
                value: "بانک آینده");

            migrationBuilder.UpdateData(
                table: "ContractStatuses",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "پیش نویس");

            migrationBuilder.UpdateData(
                table: "ContractStatuses",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "فعال");

            migrationBuilder.UpdateData(
                table: "ContractStatuses",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "تکمیل شده");

            migrationBuilder.UpdateData(
                table: "ContractStatuses",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "لغوشده");

            migrationBuilder.UpdateData(
                table: "ContractStatuses",
                keyColumn: "Id",
                keyValue: 5L,
                column: "NormalizedTitleFa",
                value: "تعلیق");

            migrationBuilder.UpdateData(
                table: "ContractStatuses",
                keyColumn: "Id",
                keyValue: 6L,
                column: "NormalizedTitleFa",
                value: "خاتمه");

            migrationBuilder.UpdateData(
                table: "ContractTypes",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "خرید");

            migrationBuilder.UpdateData(
                table: "ContractTypes",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "فروش");

            migrationBuilder.UpdateData(
                table: "ContractTypes",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "برون سپاری");

            migrationBuilder.UpdateData(
                table: "ContractTypes",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "پیمانکاری");

            migrationBuilder.UpdateData(
                table: "ContractTypes",
                keyColumn: "Id",
                keyValue: 5L,
                column: "NormalizedTitleFa",
                value: "مشاوره");

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "ریال ایران");

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "دلار آمریکا");

            migrationBuilder.UpdateData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "یورو");

            migrationBuilder.UpdateData(
                table: "DocumentTypes",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "قرارداد");

            migrationBuilder.UpdateData(
                table: "DocumentTypes",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "الحاقیه");

            migrationBuilder.UpdateData(
                table: "DocumentTypes",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "صورتحساب");

            migrationBuilder.UpdateData(
                table: "DocumentTypes",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "تضمین");

            migrationBuilder.UpdateData(
                table: "DocumentTypes",
                keyColumn: "Id",
                keyValue: 5L,
                column: "NormalizedTitleFa",
                value: "سایر");

            migrationBuilder.UpdateData(
                table: "FinancialTransactionStatuses",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "باز");

            migrationBuilder.UpdateData(
                table: "FinancialTransactionStatuses",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "تسویه جزئی");

            migrationBuilder.UpdateData(
                table: "FinancialTransactionStatuses",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "تسویه شده");

            migrationBuilder.UpdateData(
                table: "FinancialTransactionStatuses",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "لغوشده");

            migrationBuilder.UpdateData(
                table: "GuaranteeStatuses",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "فعال");

            migrationBuilder.UpdateData(
                table: "GuaranteeStatuses",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "آزادشده");

            migrationBuilder.UpdateData(
                table: "GuaranteeStatuses",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "منقضی");

            migrationBuilder.UpdateData(
                table: "GuaranteeStatuses",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "ضبط شده");

            migrationBuilder.UpdateData(
                table: "GuaranteeTypes",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "شرکت در مناقصه");

            migrationBuilder.UpdateData(
                table: "GuaranteeTypes",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "پیش پرداخت");

            migrationBuilder.UpdateData(
                table: "GuaranteeTypes",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "حسن انجام تعهدات");

            migrationBuilder.UpdateData(
                table: "GuaranteeTypes",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "کسور وجه الضمان");

            migrationBuilder.UpdateData(
                table: "InvoiceTypes",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "خرید");

            migrationBuilder.UpdateData(
                table: "InvoiceTypes",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "فروش");

            migrationBuilder.UpdateData(
                table: "InvoiceTypes",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "تعدیل");

            migrationBuilder.UpdateData(
                table: "InvoiceTypes",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "صورت وضعیت");

            migrationBuilder.UpdateData(
                table: "PartyTypes",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "مشتری");

            migrationBuilder.UpdateData(
                table: "PartyTypes",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "تأمین کننده");

            migrationBuilder.UpdateData(
                table: "PartyTypes",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "پیمانکار");

            migrationBuilder.UpdateData(
                table: "PartyTypes",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "مشاور");

            migrationBuilder.UpdateData(
                table: "PartyTypes",
                keyColumn: "Id",
                keyValue: 5L,
                column: "NormalizedTitleFa",
                value: "شخص حقوقی");

            migrationBuilder.UpdateData(
                table: "PartyTypes",
                keyColumn: "Id",
                keyValue: 6L,
                column: "NormalizedTitleFa",
                value: "شخص حقیقی");

            migrationBuilder.UpdateData(
                table: "PaymentStatuses",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "پرداخت نشده");

            migrationBuilder.UpdateData(
                table: "PaymentStatuses",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "پرداخت جزئی");

            migrationBuilder.UpdateData(
                table: "PaymentStatuses",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "پرداخت شده");

            migrationBuilder.UpdateData(
                table: "PaymentStatuses",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "لغوشده");

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "نقدی");

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "چک");

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "انتقال بانکی");

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "پیش پرداخت");

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 5L,
                column: "NormalizedTitleFa",
                value: "علی الحساب");

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 6L,
                column: "NormalizedTitleFa",
                value: "رهسا");

            migrationBuilder.UpdateData(
                table: "PaymentTypes",
                keyColumn: "Id",
                keyValue: 7L,
                column: "NormalizedTitleFa",
                value: "نامشخص در داده قدیمی");

            migrationBuilder.UpdateData(
                table: "Priorities",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "کم");

            migrationBuilder.UpdateData(
                table: "Priorities",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "متوسط");

            migrationBuilder.UpdateData(
                table: "Priorities",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "زیاد");

            migrationBuilder.UpdateData(
                table: "Priorities",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "بحرانی");

            migrationBuilder.UpdateData(
                table: "ProjectStatuses",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "برنامه ریزی شده");

            migrationBuilder.UpdateData(
                table: "ProjectStatuses",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "فعال");

            migrationBuilder.UpdateData(
                table: "ProjectStatuses",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "تکمیل شده");

            migrationBuilder.UpdateData(
                table: "ProjectStatuses",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "لغوشده");

            migrationBuilder.UpdateData(
                table: "ProjectStatuses",
                keyColumn: "Id",
                keyValue: 5L,
                column: "NormalizedTitleFa",
                value: "تعلیق");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "مدیر سیستم");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "مدیر قرارداد");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3L,
                column: "NormalizedTitleFa",
                value: "مدیرعامل");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 4L,
                column: "NormalizedTitleFa",
                value: "مشاهده کننده");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 5L,
                column: "NormalizedTitleFa",
                value: "کارشناس حقوقی");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 6L,
                column: "NormalizedTitleFa",
                value: "کارشناس مالی");

            migrationBuilder.UpdateData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 1L,
                column: "NormalizedTitleFa",
                value: "مطالبه");

            migrationBuilder.UpdateData(
                table: "TransactionTypes",
                keyColumn: "Id",
                keyValue: 2L,
                column: "NormalizedTitleFa",
                value: "بدهی");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypes_NormalizedTitleFa",
                table: "TransactionTypes",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_NormalizedTitleFa",
                table: "Roles",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStatuses_NormalizedTitleFa",
                table: "ProjectStatuses",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_NormalizedProjectNo",
                table: "Projects",
                column: "NormalizedProjectNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_NormalizedTitle_Id",
                table: "Projects",
                columns: new[] { "NormalizedTitle", "Id" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_Projects_Dates",
                table: "Projects",
                sql: "EndDate IS NULL OR StartDate IS NULL OR EndDate >= StartDate");

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_NormalizedTitleFa",
                table: "Priorities",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypes_NormalizedTitleFa",
                table: "PaymentTypes",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatuses_NormalizedTitleFa",
                table: "PaymentStatuses",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyTypes_NormalizedTitleFa",
                table: "PartyTypes",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Parties_NormalizedName_Id",
                table: "Parties",
                columns: new[] { "NormalizedName", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceTypes_NormalizedTitleFa",
                table: "InvoiceTypes",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuaranteeTypes_NormalizedTitleFa",
                table: "GuaranteeTypes",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuaranteeStatuses_NormalizedTitleFa",
                table: "GuaranteeStatuses",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactionStatuses_NormalizedTitleFa",
                table: "FinancialTransactionStatuses",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_NormalizedTitleFa",
                table: "DocumentTypes",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_NormalizedTitleFa",
                table: "Currencies",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractTypes_NormalizedTitleFa",
                table: "ContractTypes",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractStatuses_NormalizedTitleFa",
                table: "ContractStatuses",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractCategories_NormalizedTitleFa",
                table: "ContractCategories",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Banks_NormalizedTitleFa",
                table: "Banks",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertTypes_NormalizedTitleFa",
                table: "AlertTypes",
                column: "NormalizedTitleFa",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertStatuses_NormalizedTitleFa",
                table: "AlertStatuses",
                column: "NormalizedTitleFa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TransactionTypes_NormalizedTitleFa",
                table: "TransactionTypes");

            migrationBuilder.DropIndex(
                name: "IX_Roles_NormalizedTitleFa",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_ProjectStatuses_NormalizedTitleFa",
                table: "ProjectStatuses");

            migrationBuilder.DropIndex(
                name: "IX_Projects_NormalizedProjectNo",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_NormalizedTitle_Id",
                table: "Projects");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Projects_Dates",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Priorities_NormalizedTitleFa",
                table: "Priorities");

            migrationBuilder.DropIndex(
                name: "IX_PaymentTypes_NormalizedTitleFa",
                table: "PaymentTypes");

            migrationBuilder.DropIndex(
                name: "IX_PaymentStatuses_NormalizedTitleFa",
                table: "PaymentStatuses");

            migrationBuilder.DropIndex(
                name: "IX_PartyTypes_NormalizedTitleFa",
                table: "PartyTypes");

            migrationBuilder.DropIndex(
                name: "IX_Parties_NormalizedName_Id",
                table: "Parties");

            migrationBuilder.DropIndex(
                name: "IX_InvoiceTypes_NormalizedTitleFa",
                table: "InvoiceTypes");

            migrationBuilder.DropIndex(
                name: "IX_GuaranteeTypes_NormalizedTitleFa",
                table: "GuaranteeTypes");

            migrationBuilder.DropIndex(
                name: "IX_GuaranteeStatuses_NormalizedTitleFa",
                table: "GuaranteeStatuses");

            migrationBuilder.DropIndex(
                name: "IX_FinancialTransactionStatuses_NormalizedTitleFa",
                table: "FinancialTransactionStatuses");

            migrationBuilder.DropIndex(
                name: "IX_DocumentTypes_NormalizedTitleFa",
                table: "DocumentTypes");

            migrationBuilder.DropIndex(
                name: "IX_Currencies_NormalizedTitleFa",
                table: "Currencies");

            migrationBuilder.DropIndex(
                name: "IX_ContractTypes_NormalizedTitleFa",
                table: "ContractTypes");

            migrationBuilder.DropIndex(
                name: "IX_ContractStatuses_NormalizedTitleFa",
                table: "ContractStatuses");

            migrationBuilder.DropIndex(
                name: "IX_ContractCategories_NormalizedTitleFa",
                table: "ContractCategories");

            migrationBuilder.DropIndex(
                name: "IX_Banks_NormalizedTitleFa",
                table: "Banks");

            migrationBuilder.DropIndex(
                name: "IX_AlertTypes_NormalizedTitleFa",
                table: "AlertTypes");

            migrationBuilder.DropIndex(
                name: "IX_AlertStatuses_NormalizedTitleFa",
                table: "AlertStatuses");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "TransactionTypes");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "ProjectStatuses");

            migrationBuilder.DropColumn(
                name: "Department",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "Employer",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "NormalizedProjectNo",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "NormalizedTitle",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "Priorities");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "PaymentTypes");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "PaymentStatuses");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "PartyTypes");

            migrationBuilder.DropColumn(
                name: "ContactPerson",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "Mobile",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "NormalizedName",
                table: "Parties");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "InvoiceTypes");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "GuaranteeTypes");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "GuaranteeStatuses");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "FinancialTransactionStatuses");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "DocumentTypes");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "Currencies");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "ContractTypes");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "ContractStatuses");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "ContractCategories");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "Banks");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "AlertTypes");

            migrationBuilder.DropColumn(
                name: "NormalizedTitleFa",
                table: "AlertStatuses");
        }
    }
}
