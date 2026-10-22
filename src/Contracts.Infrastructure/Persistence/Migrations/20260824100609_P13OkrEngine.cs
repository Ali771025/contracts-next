using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1861 // Generated EF seed operations use constant column/value arrays.
#pragma warning disable IDE0161 // Preserve the deterministic EF migration scaffold shape.

namespace Contracts.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class P13OkrEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OkrObjectives",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OkrObjectives", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OkrKeyResults",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ObjectiveId = table.Column<long>(type: "INTEGER", nullable: false),
                    Code = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    TargetValue = table.Column<decimal>(type: "TEXT", precision: 20, scale: 6, nullable: false),
                    Unit = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    WeightPercent = table.Column<int>(type: "INTEGER", nullable: false),
                    Direction = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    MetricCode = table.Column<string>(type: "TEXT", maxLength: 80, nullable: false, collation: "NOCASE"),
                    DataSourceType = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    MetricVersion = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OkrKeyResults", x => x.Id);
                    table.CheckConstraint("CK_OkrKeyResults_Target", "TargetValue >= 0");
                    table.CheckConstraint("CK_OkrKeyResults_Weight", "WeightPercent BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_OkrKeyResults_OkrObjectives_ObjectiveId",
                        column: x => x.ObjectiveId,
                        principalTable: "OkrObjectives",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OkrMeasurements",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    KeyResultId = table.Column<long>(type: "INTEGER", nullable: false),
                    MeasurementDate = table.Column<string>(type: "TEXT", nullable: false),
                    ActualValue = table.Column<decimal>(type: "TEXT", precision: 20, scale: 6, nullable: false),
                    Score = table.Column<decimal>(type: "TEXT", precision: 9, scale: 6, nullable: false),
                    SourceType = table.Column<string>(type: "TEXT", maxLength: 16, nullable: false),
                    MetricVersion = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    AsOfUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    EvidenceCount = table.Column<long>(type: "INTEGER", nullable: true),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OkrMeasurements", x => x.Id);
                    table.CheckConstraint("CK_OkrMeasurements_Actual", "ActualValue >= 0");
                    table.CheckConstraint("CK_OkrMeasurements_AutoEvidence", "SourceType <> 'Auto' OR (AsOfUtc IS NOT NULL AND EvidenceCount IS NOT NULL AND EvidenceCount >= 0)");
                    table.CheckConstraint("CK_OkrMeasurements_Score", "Score BETWEEN 0 AND 1");
                    table.ForeignKey(
                        name: "FK_OkrMeasurements_OkrKeyResults_KeyResultId",
                        column: x => x.KeyResultId,
                        principalTable: "OkrKeyResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OkrMeasurements_Users_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "OkrObjectives",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "Description", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "OBJ-01", 1L, 639028224000000000L, null, "", true, 10, "مدیریت مؤثر قراردادها", 639028224000000000L, null },
                    { 2L, "OBJ-02", 1L, 639028224000000000L, null, "", true, 20, "مدیریت مالی قراردادها", 639028224000000000L, null },
                    { 3L, "OBJ-03", 1L, 639028224000000000L, null, "", true, 30, "مدیریت پروژه", 639028224000000000L, null },
                    { 4L, "OBJ-04", 1L, 639028224000000000L, null, "", true, 40, "مدیریت خرید و برون‌سپاری", 639028224000000000L, null },
                    { 5L, "OBJ-05", 1L, 639028224000000000L, null, "", true, 50, "رضایت مدیرعامل از سامانه", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "OkrKeyResults",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "DataSourceType", "Direction", "IsActive", "MetricCode", "MetricVersion", "ObjectiveId", "SortOrder", "TargetValue", "TitleFa", "Unit", "UpdatedAtUtc", "UpdatedByUserId", "WeightPercent" },
                values: new object[,]
                {
                    { 1L, "OBJ01-KR01", 1L, 639028224000000000L, null, "Manual", "HigherIsBetter", true, "CONTRACT_REGISTRATION_ON_TIME", "MANUAL_V1", 1L, 10, 98m, "ثبت به‌موقع قراردادها", "PERCENT", 639028224000000000L, null, 25 },
                    { 2L, "OBJ01-KR02", 1L, 639028224000000000L, null, "Manual", "LowerIsBetter", true, "CONTRACT_REGISTRATION_LEAD_DAYS", "MANUAL_V1", 1L, 20, 2m, "زمان ثبت قرارداد", "DAY", 639028224000000000L, null, 25 },
                    { 3L, "OBJ01-KR03", 1L, 639028224000000000L, null, "Manual", "HigherIsBetter", true, "DEFECT_FREE_CONTRACT_RATE", "MANUAL_V1", 1L, 30, 95m, "قراردادهای بدون نقص", "PERCENT", 639028224000000000L, null, 25 },
                    { 4L, "OBJ01-KR04", 1L, 639028224000000000L, null, "Manual", "HigherIsBetter", true, "DOCUMENT_COMPLETENESS_RATE", "MANUAL_V1", 1L, 40, 100m, "مستندات کامل", "PERCENT", 639028224000000000L, null, 25 },
                    { 5L, "OBJ02-KR01", 1L, 639028224000000000L, null, "Auto", "LowerIsBetter", true, "OVERDUE_RECEIVABLE_RATE", "LEDGER_V1", 2L, 10, 5m, "مطالبات معوق", "PERCENT", 639028224000000000L, null, 30 },
                    { 6L, "OBJ02-KR02", 1L, 639028224000000000L, null, "Auto", "HigherIsBetter", true, "TIMELY_DEBT_PAYMENT_RATE", "LEDGER_V1", 2L, 20, 95m, "پرداخت به‌موقع بدهی", "PERCENT", 639028224000000000L, null, 30 },
                    { 7L, "OBJ02-KR03", 1L, 639028224000000000L, null, "Auto", "HigherIsBetter", true, "COLLECTION_RATE", "LEDGER_V1", 2L, 30, 98m, "وصول مطالبات", "PERCENT", 639028224000000000L, null, 20 },
                    { 8L, "OBJ02-KR04", 1L, 639028224000000000L, null, "Manual", "TargetZero", true, "FINANCIAL_DISCREPANCY_COUNT", "MANUAL_V1", 2L, 40, 0m, "مغایرت مالی", "COUNT", 639028224000000000L, null, 20 },
                    { 9L, "OBJ03-KR01", 1L, 639028224000000000L, null, "Manual", "HigherIsBetter", true, "GANTT_ON_TRACK_RATE", "MANUAL_V1", 3L, 10, 95m, "پیشرفت مطابق گانت", "PERCENT", 639028224000000000L, null, 30 },
                    { 10L, "OBJ03-KR02", 1L, 639028224000000000L, null, "Manual", "LowerIsBetter", true, "DELAYED_ACTIVITY_RATE", "MANUAL_V1", 3L, 20, 5m, "فعالیت تأخیردار", "PERCENT", 639028224000000000L, null, 30 },
                    { 11L, "OBJ03-KR03", 1L, 639028224000000000L, null, "Manual", "HigherIsBetter", true, "ON_TIME_PROJECT_DELIVERY_RATE", "MANUAL_V1", 3L, 30, 90m, "پروژه تحویل‌شده در موعد", "PERCENT", 639028224000000000L, null, 20 },
                    { 12L, "OBJ03-KR04", 1L, 639028224000000000L, null, "Manual", "LowerIsBetter", true, "SCHEDULE_VARIANCE_RATE", "MANUAL_V1", 3L, 40, 10m, "انحراف زمانی", "PERCENT", 639028224000000000L, null, 20 },
                    { 13L, "OBJ04-KR01", 1L, 639028224000000000L, null, "Manual", "HigherIsBetter", true, "ON_TIME_PURCHASE_ORDER_RATE", "MANUAL_V1", 4L, 10, 95m, "سفارش خرید به‌موقع", "PERCENT", 639028224000000000L, null, 25 },
                    { 14L, "OBJ04-KR02", 1L, 639028224000000000L, null, "Manual", "HigherIsBetter", true, "SUPPLY_LEAD_TIME_REDUCTION_RATE", "MANUAL_V1", 4L, 20, 20m, "کاهش زمان تأمین", "PERCENT", 639028224000000000L, null, 25 },
                    { 15L, "OBJ04-KR03", 1L, 639028224000000000L, null, "Manual", "HigherIsBetter", true, "ON_TIME_CONTRACTOR_DELIVERY_RATE", "MANUAL_V1", 4L, 30, 95m, "تحویل به‌موقع پیمانکار", "PERCENT", 639028224000000000L, null, 25 },
                    { 16L, "OBJ04-KR04", 1L, 639028224000000000L, null, "Manual", "HigherIsBetter", true, "PURCHASE_COST_REDUCTION_RATE", "MANUAL_V1", 4L, 40, 10m, "کاهش هزینه خرید", "PERCENT", 639028224000000000L, null, 25 },
                    { 17L, "OBJ05-KR01", 1L, 639028224000000000L, null, "Manual", "HigherIsBetter", true, "REALTIME_REPORT_AVAILABILITY_RATE", "MANUAL_V1", 5L, 10, 100m, "گزارش‌های لحظه‌ای", "PERCENT", 639028224000000000L, null, 30 },
                    { 18L, "OBJ05-KR02", 1L, 639028224000000000L, null, "Manual", "HigherIsBetter", true, "EXECUTIVE_DASHBOARD_AVAILABILITY_RATE", "MANUAL_V1", 5L, 20, 100m, "داشبورد مدیریتی", "PERCENT", 639028224000000000L, null, 20 },
                    { 19L, "OBJ05-KR03", 1L, 639028224000000000L, null, "Manual", "LowerIsBetter", true, "SYSTEM_RESPONSE_TIME_SECONDS", "MANUAL_V1", 5L, 30, 3m, "پاسخگویی سامانه", "SECOND", 639028224000000000L, null, 20 },
                    { 20L, "OBJ05-KR04", 1L, 639028224000000000L, null, "Manual", "HigherIsBetter", true, "USER_SATISFACTION_RATE", "MANUAL_V1", 5L, 40, 90m, "رضایت کاربران", "PERCENT", 639028224000000000L, null, 30 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OkrKeyResults_Code",
                table: "OkrKeyResults",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OkrKeyResults_CreatedAtUtc",
                table: "OkrKeyResults",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OkrKeyResults_MetricCode",
                table: "OkrKeyResults",
                column: "MetricCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OkrKeyResults_ObjectiveId_SortOrder",
                table: "OkrKeyResults",
                columns: new[] { "ObjectiveId", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_OkrMeasurements_CreatedAtUtc",
                table: "OkrMeasurements",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OkrMeasurements_CreatedByUserId",
                table: "OkrMeasurements",
                column: "CreatedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_OkrMeasurements_KeyResultId_MeasurementDate_Id",
                table: "OkrMeasurements",
                columns: new[] { "KeyResultId", "MeasurementDate", "Id" });

            migrationBuilder.CreateIndex(
                name: "IX_OkrObjectives_Code",
                table: "OkrObjectives",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OkrObjectives_CreatedAtUtc",
                table: "OkrObjectives",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OkrObjectives_IsActive_SortOrder",
                table: "OkrObjectives",
                columns: new[] { "IsActive", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OkrMeasurements");

            migrationBuilder.DropTable(
                name: "OkrKeyResults");

            migrationBuilder.DropTable(
                name: "OkrObjectives");
        }
    }
}
