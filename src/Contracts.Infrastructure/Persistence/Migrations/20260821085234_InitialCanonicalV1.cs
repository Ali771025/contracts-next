using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional
#pragma warning disable CA1861 // EF Core generates constant arrays for migration operations.
#pragma warning disable IDE0161 // EF Core generates migration classes with block-scoped namespaces.

namespace Contracts.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCanonicalV1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertStatuses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AlertTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractCategories",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractStatuses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContractTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    Scale = table.Column<int>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                    table.CheckConstraint("CK_Currencies_Scale", "Scale >= 0 AND Scale <= 6");
                });

            migrationBuilder.CreateTable(
                name: "DocumentTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactionStatuses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactionStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuaranteeStatuses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuaranteeStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GuaranteeTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GuaranteeTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PartyTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PartyTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentStatuses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PaymentTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Priorities",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Priorities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProjectStatuses",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectStatuses", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TransactionTypes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Code = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    TitleFa = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    SortOrder = table.Column<int>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TransactionTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Parties",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 250, nullable: false),
                    PartyTypeId = table.Column<long>(type: "INTEGER", nullable: false),
                    NationalId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    EconomicCode = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    RegistrationNo = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Parties", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Parties_PartyTypes_PartyTypeId",
                        column: x => x.PartyTypeId,
                        principalTable: "PartyTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    RoleId = table.Column<long>(type: "INTEGER", nullable: false),
                    PermissionCode = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false, collation: "NOCASE"),
                    CanView = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanCreate = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanUpdate = table.Column<bool>(type: "INTEGER", nullable: false),
                    CanDelete = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Username = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    DisplayName = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    RoleId = table.Column<long>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ActorUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Action = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false),
                    EntityType = table.Column<string>(type: "TEXT", maxLength: 150, nullable: false),
                    EntityId = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    OccurredAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    DetailsJson = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectNo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    ProjectStatusId = table.Column<long>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<string>(type: "TEXT", nullable: true),
                    EndDate = table.Column<string>(type: "TEXT", nullable: true),
                    ManagerUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_ProjectStatuses_ProjectStatusId",
                        column: x => x.ProjectStatusId,
                        principalTable: "ProjectStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Projects_Users_ManagerUserId",
                        column: x => x.ManagerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Contracts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContractNo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    ContractTypeId = table.Column<long>(type: "INTEGER", nullable: false),
                    ContractCategoryId = table.Column<long>(type: "INTEGER", nullable: true),
                    ProjectId = table.Column<long>(type: "INTEGER", nullable: true),
                    PartyId = table.Column<long>(type: "INTEGER", nullable: false),
                    ContractStatusId = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrencyId = table.Column<long>(type: "INTEGER", nullable: false),
                    OriginalAmountMinor = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrencyScale = table.Column<int>(type: "INTEGER", nullable: false),
                    ExchangeRateRialPerUnit = table.Column<long>(type: "INTEGER", nullable: false),
                    BaseAmountRial = table.Column<long>(type: "INTEGER", nullable: false),
                    StartDate = table.Column<string>(type: "TEXT", nullable: false),
                    EndDate = table.Column<string>(type: "TEXT", nullable: false),
                    ManagerUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 4000, nullable: true),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contracts", x => x.Id);
                    table.CheckConstraint("CK_Contracts_AuditActors", "CreatedByUserId IS NOT NULL AND UpdatedByUserId IS NOT NULL");
                    table.CheckConstraint("CK_Contracts_BaseAmount", "BaseAmountRial >= 0");
                    table.CheckConstraint("CK_Contracts_Dates", "EndDate >= StartDate");
                    table.CheckConstraint("CK_Contracts_ExchangeRate", "ExchangeRateRialPerUnit > 0");
                    table.CheckConstraint("CK_Contracts_OriginalAmount", "OriginalAmountMinor >= 0");
                    table.CheckConstraint("CK_Contracts_Scale", "CurrencyScale >= 0 AND CurrencyScale <= 6");
                    table.ForeignKey(
                        name: "FK_Contracts_ContractCategories_ContractCategoryId",
                        column: x => x.ContractCategoryId,
                        principalTable: "ContractCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_ContractStatuses_ContractStatusId",
                        column: x => x.ContractStatusId,
                        principalTable: "ContractStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_ContractTypes_ContractTypeId",
                        column: x => x.ContractTypeId,
                        principalTable: "ContractTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contracts_Users_ManagerUserId",
                        column: x => x.ManagerUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProjectBudgets",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjectId = table.Column<long>(type: "INTEGER", nullable: false),
                    FiscalYear = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CurrencyId = table.Column<long>(type: "INTEGER", nullable: false),
                    CurrencyScale = table.Column<int>(type: "INTEGER", nullable: false),
                    ApprovedAmountMinor = table.Column<long>(type: "INTEGER", nullable: false),
                    ConsumedAmountMinor = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectBudgets", x => x.Id);
                    table.CheckConstraint("CK_ProjectBudgets_Approved", "ApprovedAmountMinor >= 0");
                    table.CheckConstraint("CK_ProjectBudgets_Consumed", "ConsumedAmountMinor >= 0 AND ConsumedAmountMinor <= ApprovedAmountMinor");
                    table.CheckConstraint("CK_ProjectBudgets_Scale", "CurrencyScale >= 0 AND CurrencyScale <= 6");
                    table.ForeignKey(
                        name: "FK_ProjectBudgets_Currencies_CurrencyId",
                        column: x => x.CurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProjectBudgets_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContractAmendments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContractId = table.Column<long>(type: "INTEGER", nullable: false),
                    AmendmentNo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    AmendmentDate = table.Column<string>(type: "TEXT", nullable: false),
                    AmountChangeRial = table.Column<long>(type: "INTEGER", nullable: false),
                    TimeExtensionDays = table.Column<int>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractAmendments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractAmendments_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractDocuments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContractId = table.Column<long>(type: "INTEGER", nullable: false),
                    DocumentTypeId = table.Column<long>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", maxLength: 260, nullable: false),
                    StoragePath = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: false),
                    ContentHashSha256 = table.Column<string>(type: "TEXT", maxLength: 64, nullable: true),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractDocuments_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ContractDocuments_DocumentTypes_DocumentTypeId",
                        column: x => x.DocumentTypeId,
                        principalTable: "DocumentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractorEvaluations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContractId = table.Column<long>(type: "INTEGER", nullable: false),
                    PartyId = table.Column<long>(type: "INTEGER", nullable: false),
                    EvaluationDate = table.Column<string>(type: "TEXT", nullable: false),
                    QualityScore = table.Column<int>(type: "INTEGER", nullable: false),
                    TimeScore = table.Column<int>(type: "INTEGER", nullable: false),
                    FinancialScore = table.Column<int>(type: "INTEGER", nullable: false),
                    HseScore = table.Column<int>(type: "INTEGER", nullable: false),
                    Notes = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractorEvaluations", x => x.Id);
                    table.CheckConstraint("CK_Evaluations_Financial", "FinancialScore BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_Evaluations_Hse", "HseScore BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_Evaluations_Quality", "QualityScore BETWEEN 0 AND 100");
                    table.CheckConstraint("CK_Evaluations_Time", "TimeScore BETWEEN 0 AND 100");
                    table.ForeignKey(
                        name: "FK_ContractorEvaluations_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContractorEvaluations_Parties_PartyId",
                        column: x => x.PartyId,
                        principalTable: "Parties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinancialTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TransactionNo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false, collation: "NOCASE"),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    ContractId = table.Column<long>(type: "INTEGER", nullable: false),
                    TransactionTypeId = table.Column<long>(type: "INTEGER", nullable: false),
                    StatusId = table.Column<long>(type: "INTEGER", nullable: false),
                    AmountRial = table.Column<long>(type: "INTEGER", nullable: false),
                    DueDate = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FinancialTransactions", x => x.Id);
                    table.CheckConstraint("CK_FinancialTransactions_Amount", "AmountRial > 0");
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_FinancialTransactionStatuses_StatusId",
                        column: x => x.StatusId,
                        principalTable: "FinancialTransactionStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinancialTransactions_TransactionTypes_TransactionTypeId",
                        column: x => x.TransactionTypeId,
                        principalTable: "TransactionTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Guarantees",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContractId = table.Column<long>(type: "INTEGER", nullable: false),
                    GuaranteeNo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    GuaranteeTypeId = table.Column<long>(type: "INTEGER", nullable: false),
                    GuaranteeStatusId = table.Column<long>(type: "INTEGER", nullable: false),
                    AmountRial = table.Column<long>(type: "INTEGER", nullable: false),
                    BankId = table.Column<long>(type: "INTEGER", nullable: true),
                    IssueDate = table.Column<string>(type: "TEXT", nullable: false),
                    ExpiryDate = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Guarantees", x => x.Id);
                    table.CheckConstraint("CK_Guarantees_Amount", "AmountRial > 0");
                    table.ForeignKey(
                        name: "FK_Guarantees_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Guarantees_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Guarantees_GuaranteeStatuses_GuaranteeStatusId",
                        column: x => x.GuaranteeStatusId,
                        principalTable: "GuaranteeStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Guarantees_GuaranteeTypes_GuaranteeTypeId",
                        column: x => x.GuaranteeTypeId,
                        principalTable: "GuaranteeTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContractId = table.Column<long>(type: "INTEGER", nullable: false),
                    InvoiceNo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    InvoiceDate = table.Column<string>(type: "TEXT", nullable: false),
                    InvoiceTypeId = table.Column<long>(type: "INTEGER", nullable: false),
                    PaymentStatusId = table.Column<long>(type: "INTEGER", nullable: false),
                    AmountRial = table.Column<long>(type: "INTEGER", nullable: false),
                    VatAmountRial = table.Column<long>(type: "INTEGER", nullable: false),
                    DueDate = table.Column<string>(type: "TEXT", nullable: true),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.CheckConstraint("CK_Invoices_Amount", "AmountRial >= 0");
                    table.CheckConstraint("CK_Invoices_Vat", "VatAmountRial >= 0");
                    table.ForeignKey(
                        name: "FK_Invoices_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_InvoiceTypes_InvoiceTypeId",
                        column: x => x.InvoiceTypeId,
                        principalTable: "InvoiceTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_PaymentStatuses_PaymentStatusId",
                        column: x => x.PaymentStatusId,
                        principalTable: "PaymentStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContractId = table.Column<long>(type: "INTEGER", nullable: false),
                    AmountRial = table.Column<long>(type: "INTEGER", nullable: false),
                    PaymentDate = table.Column<string>(type: "TEXT", nullable: false),
                    PaymentTypeId = table.Column<long>(type: "INTEGER", nullable: false),
                    BankId = table.Column<long>(type: "INTEGER", nullable: true),
                    ReferenceNo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.CheckConstraint("CK_Payments_Amount", "AmountRial > 0");
                    table.ForeignKey(
                        name: "FK_Payments_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_PaymentTypes_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "PaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Receipts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContractId = table.Column<long>(type: "INTEGER", nullable: false),
                    AmountRial = table.Column<long>(type: "INTEGER", nullable: false),
                    ReceiptDate = table.Column<string>(type: "TEXT", nullable: false),
                    PaymentTypeId = table.Column<long>(type: "INTEGER", nullable: false),
                    BankId = table.Column<long>(type: "INTEGER", nullable: true),
                    ReferenceNo = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Receipts", x => x.Id);
                    table.CheckConstraint("CK_Receipts_Amount", "AmountRial > 0");
                    table.ForeignKey(
                        name: "FK_Receipts_Banks_BankId",
                        column: x => x.BankId,
                        principalTable: "Banks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Receipts_PaymentTypes_PaymentTypeId",
                        column: x => x.PaymentTypeId,
                        principalTable: "PaymentTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Alerts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ContractId = table.Column<long>(type: "INTEGER", nullable: false),
                    GuaranteeId = table.Column<long>(type: "INTEGER", nullable: true),
                    AlertTypeId = table.Column<long>(type: "INTEGER", nullable: false),
                    AlertStatusId = table.Column<long>(type: "INTEGER", nullable: false),
                    PriorityId = table.Column<long>(type: "INTEGER", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    DueDate = table.Column<string>(type: "TEXT", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    IsDeleted = table.Column<bool>(type: "INTEGER", nullable: false),
                    DeletedAtUtc = table.Column<long>(type: "INTEGER", nullable: true),
                    DeletedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alerts_AlertStatuses_AlertStatusId",
                        column: x => x.AlertStatusId,
                        principalTable: "AlertStatuses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alerts_AlertTypes_AlertTypeId",
                        column: x => x.AlertTypeId,
                        principalTable: "AlertTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alerts_Contracts_ContractId",
                        column: x => x.ContractId,
                        principalTable: "Contracts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Alerts_Guarantees_GuaranteeId",
                        column: x => x.GuaranteeId,
                        principalTable: "Guarantees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Alerts_Priorities_PriorityId",
                        column: x => x.PriorityId,
                        principalTable: "Priorities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PaymentAllocations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FinancialTransactionId = table.Column<long>(type: "INTEGER", nullable: false),
                    PaymentId = table.Column<long>(type: "INTEGER", nullable: false),
                    AmountRial = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentAllocations", x => x.Id);
                    table.CheckConstraint("CK_PaymentAllocations_Amount", "AmountRial > 0");
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_FinancialTransactions_FinancialTransactionId",
                        column: x => x.FinancialTransactionId,
                        principalTable: "FinancialTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PaymentAllocations_Payments_PaymentId",
                        column: x => x.PaymentId,
                        principalTable: "Payments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReceiptAllocations",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FinancialTransactionId = table.Column<long>(type: "INTEGER", nullable: false),
                    ReceiptId = table.Column<long>(type: "INTEGER", nullable: false),
                    AmountRial = table.Column<long>(type: "INTEGER", nullable: false),
                    ConcurrencyVersion = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    CreatedByUserId = table.Column<long>(type: "INTEGER", nullable: true),
                    UpdatedAtUtc = table.Column<long>(type: "INTEGER", nullable: false),
                    UpdatedByUserId = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReceiptAllocations", x => x.Id);
                    table.CheckConstraint("CK_ReceiptAllocations_Amount", "AmountRial > 0");
                    table.ForeignKey(
                        name: "FK_ReceiptAllocations_FinancialTransactions_FinancialTransactionId",
                        column: x => x.FinancialTransactionId,
                        principalTable: "FinancialTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReceiptAllocations_Receipts_ReceiptId",
                        column: x => x.ReceiptId,
                        principalTable: "Receipts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "AlertStatuses",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "OPEN", 1L, 639028224000000000L, null, true, 10, "باز", 639028224000000000L, null },
                    { 2L, "COMPLETED", 1L, 639028224000000000L, null, true, 20, "انجام‌شده", 639028224000000000L, null },
                    { 3L, "DISMISSED", 1L, 639028224000000000L, null, true, 30, "نادیده‌گرفته‌شده", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "AlertTypes",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "CONTRACT_EXPIRY", 1L, 639028224000000000L, null, true, 10, "سررسید قرارداد", 639028224000000000L, null },
                    { 2L, "GUARANTEE_EXPIRY", 1L, 639028224000000000L, null, true, 20, "سررسید تضمین", 639028224000000000L, null },
                    { 3L, "PAYMENT_DUE", 1L, 639028224000000000L, null, true, 30, "سررسید پرداخت", 639028224000000000L, null },
                    { 4L, "RECEIPT_DUE", 1L, 639028224000000000L, null, true, 40, "سررسید دریافت", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "Banks",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "MELLI", 1L, 639028224000000000L, null, true, 10, "بانک ملی ایران", 639028224000000000L, null },
                    { 2L, "SEPAH", 1L, 639028224000000000L, null, true, 20, "بانک سپه", 639028224000000000L, null },
                    { 3L, "POST_BANK", 1L, 639028224000000000L, null, true, 30, "پست بانک ایران", 639028224000000000L, null },
                    { 4L, "KESHAVARZI", 1L, 639028224000000000L, null, true, 40, "بانک کشاورزی", 639028224000000000L, null },
                    { 5L, "MASKAN", 1L, 639028224000000000L, null, true, 50, "بانک مسکن", 639028224000000000L, null },
                    { 6L, "SANAT_MADAN", 1L, 639028224000000000L, null, true, 60, "بانک صنعت و معدن", 639028224000000000L, null },
                    { 7L, "TOSE_SADERAT", 1L, 639028224000000000L, null, true, 70, "بانک توسعه صادرات ایران", 639028224000000000L, null },
                    { 8L, "TOSE_TAAVON", 1L, 639028224000000000L, null, true, 80, "بانک توسعه تعاون", 639028224000000000L, null },
                    { 9L, "MEHR_IRAN", 1L, 639028224000000000L, null, true, 90, "بانک قرض‌الحسنه مهر ایران", 639028224000000000L, null },
                    { 10L, "RESALAT", 1L, 639028224000000000L, null, true, 100, "بانک قرض‌الحسنه رسالت", 639028224000000000L, null },
                    { 11L, "EGHTESAD_NOVIN", 1L, 639028224000000000L, null, true, 110, "بانک اقتصاد نوین", 639028224000000000L, null },
                    { 12L, "KARAFARIN", 1L, 639028224000000000L, null, true, 120, "بانک کارآفرین", 639028224000000000L, null },
                    { 13L, "PARSIAN", 1L, 639028224000000000L, null, true, 130, "بانک پارسیان", 639028224000000000L, null },
                    { 14L, "PASARGAD", 1L, 639028224000000000L, null, true, 140, "بانک پاسارگاد", 639028224000000000L, null },
                    { 15L, "SAMAN", 1L, 639028224000000000L, null, true, 150, "بانک سامان", 639028224000000000L, null },
                    { 16L, "SARMAYEH", 1L, 639028224000000000L, null, true, 160, "بانک سرمایه", 639028224000000000L, null },
                    { 17L, "SINA", 1L, 639028224000000000L, null, true, 170, "بانک سینا", 639028224000000000L, null },
                    { 18L, "SHAHR", 1L, 639028224000000000L, null, true, 180, "بانک شهر", 639028224000000000L, null },
                    { 19L, "DEY", 1L, 639028224000000000L, null, true, 190, "بانک دی", 639028224000000000L, null },
                    { 20L, "MELLAT", 1L, 639028224000000000L, null, true, 200, "بانک ملت", 639028224000000000L, null },
                    { 21L, "REFAH", 1L, 639028224000000000L, null, true, 210, "بانک رفاه کارگران", 639028224000000000L, null },
                    { 22L, "SADERAT", 1L, 639028224000000000L, null, true, 220, "بانک صادرات ایران", 639028224000000000L, null },
                    { 23L, "TEJARAT", 1L, 639028224000000000L, null, true, 230, "بانک تجارت", 639028224000000000L, null },
                    { 24L, "GARDESHGARI", 1L, 639028224000000000L, null, true, 240, "بانک گردشگری", 639028224000000000L, null },
                    { 25L, "KHAVARMIANEH", 1L, 639028224000000000L, null, true, 250, "بانک خاورمیانه", 639028224000000000L, null },
                    { 26L, "IRAN_ZAMIN", 1L, 639028224000000000L, null, true, 260, "بانک ایران زمین", 639028224000000000L, null },
                    { 27L, "AYANDEH", 1L, 639028224000000000L, null, true, 270, "بانک آینده", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "ContractStatuses",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "DRAFT", 1L, 639028224000000000L, null, true, 10, "پیش‌نویس", 639028224000000000L, null },
                    { 2L, "ACTIVE", 1L, 639028224000000000L, null, true, 20, "فعال", 639028224000000000L, null },
                    { 3L, "COMPLETED", 1L, 639028224000000000L, null, true, 30, "تکمیل‌شده", 639028224000000000L, null },
                    { 4L, "CANCELLED", 1L, 639028224000000000L, null, true, 40, "لغوشده", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "ContractTypes",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "PURCHASE", 1L, 639028224000000000L, null, true, 10, "خرید", 639028224000000000L, null },
                    { 2L, "SALE", 1L, 639028224000000000L, null, true, 20, "فروش", 639028224000000000L, null },
                    { 3L, "OUTSOURCE", 1L, 639028224000000000L, null, true, 30, "برون‌سپاری", 639028224000000000L, null },
                    { 4L, "CONTRACTING", 1L, 639028224000000000L, null, true, 40, "پیمانکاری", 639028224000000000L, null },
                    { 5L, "CONSULTING", 1L, 639028224000000000L, null, true, 50, "مشاوره", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "Scale", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "IRR", 1L, 639028224000000000L, null, true, 0, 10, "ریال ایران", 639028224000000000L, null },
                    { 2L, "USD", 1L, 639028224000000000L, null, true, 2, 20, "دلار آمریکا", 639028224000000000L, null },
                    { 3L, "EUR", 1L, 639028224000000000L, null, true, 2, 30, "یورو", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "DocumentTypes",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "CONTRACT", 1L, 639028224000000000L, null, true, 10, "قرارداد", 639028224000000000L, null },
                    { 2L, "AMENDMENT", 1L, 639028224000000000L, null, true, 20, "الحاقیه", 639028224000000000L, null },
                    { 3L, "INVOICE", 1L, 639028224000000000L, null, true, 30, "صورتحساب", 639028224000000000L, null },
                    { 4L, "GUARANTEE", 1L, 639028224000000000L, null, true, 40, "تضمین", 639028224000000000L, null },
                    { 5L, "OTHER", 1L, 639028224000000000L, null, true, 50, "سایر", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "FinancialTransactionStatuses",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "OPEN", 1L, 639028224000000000L, null, true, 10, "باز", 639028224000000000L, null },
                    { 2L, "PARTIAL", 1L, 639028224000000000L, null, true, 20, "تسویه جزئی", 639028224000000000L, null },
                    { 3L, "SETTLED", 1L, 639028224000000000L, null, true, 30, "تسویه‌شده", 639028224000000000L, null },
                    { 4L, "CANCELLED", 1L, 639028224000000000L, null, true, 40, "لغوشده", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "GuaranteeStatuses",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "ACTIVE", 1L, 639028224000000000L, null, true, 10, "فعال", 639028224000000000L, null },
                    { 2L, "RELEASED", 1L, 639028224000000000L, null, true, 20, "آزادشده", 639028224000000000L, null },
                    { 3L, "EXPIRED", 1L, 639028224000000000L, null, true, 30, "منقضی", 639028224000000000L, null },
                    { 4L, "CLAIMED", 1L, 639028224000000000L, null, true, 40, "ضبط‌شده", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "GuaranteeTypes",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "TENDER", 1L, 639028224000000000L, null, true, 10, "شرکت در مناقصه", 639028224000000000L, null },
                    { 2L, "ADVANCE_PAYMENT", 1L, 639028224000000000L, null, true, 20, "پیش‌پرداخت", 639028224000000000L, null },
                    { 3L, "PERFORMANCE", 1L, 639028224000000000L, null, true, 30, "حسن انجام تعهدات", 639028224000000000L, null },
                    { 4L, "RETENTION", 1L, 639028224000000000L, null, true, 40, "کسور وجه‌الضمان", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "InvoiceTypes",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "PURCHASE", 1L, 639028224000000000L, null, true, 10, "خرید", 639028224000000000L, null },
                    { 2L, "SALE", 1L, 639028224000000000L, null, true, 20, "فروش", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "PartyTypes",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "CUSTOMER", 1L, 639028224000000000L, null, true, 10, "مشتری", 639028224000000000L, null },
                    { 2L, "SUPPLIER", 1L, 639028224000000000L, null, true, 20, "تأمین‌کننده", 639028224000000000L, null },
                    { 3L, "CONTRACTOR", 1L, 639028224000000000L, null, true, 30, "پیمانکار", 639028224000000000L, null },
                    { 4L, "CONSULTANT", 1L, 639028224000000000L, null, true, 40, "مشاور", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "PaymentStatuses",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "UNPAID", 1L, 639028224000000000L, null, true, 10, "پرداخت‌نشده", 639028224000000000L, null },
                    { 2L, "PARTIAL", 1L, 639028224000000000L, null, true, 20, "پرداخت جزئی", 639028224000000000L, null },
                    { 3L, "PAID", 1L, 639028224000000000L, null, true, 30, "پرداخت‌شده", 639028224000000000L, null },
                    { 4L, "CANCELLED", 1L, 639028224000000000L, null, true, 40, "لغوشده", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "PaymentTypes",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "CASH", 1L, 639028224000000000L, null, true, 10, "نقدی", 639028224000000000L, null },
                    { 2L, "CHEQUE", 1L, 639028224000000000L, null, true, 20, "چک", 639028224000000000L, null },
                    { 3L, "BANK_TRANSFER", 1L, 639028224000000000L, null, true, 30, "انتقال بانکی", 639028224000000000L, null },
                    { 4L, "ADVANCE", 1L, 639028224000000000L, null, true, 40, "پیش‌پرداخت", 639028224000000000L, null },
                    { 5L, "ON_ACCOUNT", 1L, 639028224000000000L, null, true, 50, "علی‌الحساب", 639028224000000000L, null },
                    { 6L, "RAHSA", 1L, 639028224000000000L, null, true, 60, "رهسا", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "Priorities",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "LOW", 1L, 639028224000000000L, null, true, 10, "کم", 639028224000000000L, null },
                    { 2L, "MEDIUM", 1L, 639028224000000000L, null, true, 20, "متوسط", 639028224000000000L, null },
                    { 3L, "HIGH", 1L, 639028224000000000L, null, true, 30, "زیاد", 639028224000000000L, null },
                    { 4L, "CRITICAL", 1L, 639028224000000000L, null, true, 40, "بحرانی", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "ProjectStatuses",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "PLANNED", 1L, 639028224000000000L, null, true, 10, "برنامه‌ریزی‌شده", 639028224000000000L, null },
                    { 2L, "ACTIVE", 1L, 639028224000000000L, null, true, 20, "فعال", 639028224000000000L, null },
                    { 3L, "COMPLETED", 1L, 639028224000000000L, null, true, 30, "تکمیل‌شده", 639028224000000000L, null },
                    { 4L, "CANCELLED", 1L, 639028224000000000L, null, true, 40, "لغوشده", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "SYSTEM_ADMIN", 1L, 639028224000000000L, null, true, 10, "مدیر سیستم", 639028224000000000L, null },
                    { 2L, "CONTRACT_MANAGER", 1L, 639028224000000000L, null, true, 20, "مدیر قرارداد", 639028224000000000L, null },
                    { 3L, "CHIEF_EXECUTIVE", 1L, 639028224000000000L, null, true, 30, "مدیرعامل", 639028224000000000L, null },
                    { 4L, "VIEWER", 1L, 639028224000000000L, null, true, 40, "مشاهده‌کننده", 639028224000000000L, null },
                    { 5L, "LEGAL_EXPERT", 1L, 639028224000000000L, null, true, 50, "کارشناس حقوقی", 639028224000000000L, null },
                    { 6L, "FINANCE_EXPERT", 1L, 639028224000000000L, null, true, 60, "کارشناس مالی", 639028224000000000L, null }
                });

            migrationBuilder.InsertData(
                table: "TransactionTypes",
                columns: new[] { "Id", "Code", "ConcurrencyVersion", "CreatedAtUtc", "CreatedByUserId", "IsActive", "SortOrder", "TitleFa", "UpdatedAtUtc", "UpdatedByUserId" },
                values: new object[,]
                {
                    { 1L, "CLAIM", 1L, 639028224000000000L, null, true, 10, "مطالبه", 639028224000000000L, null },
                    { 2L, "DEBT", 1L, 639028224000000000L, null, true, 20, "بدهی", 639028224000000000L, null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AlertStatusId_DueDate_PriorityId",
                table: "Alerts",
                columns: new[] { "AlertStatusId", "DueDate", "PriorityId" });

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_AlertTypeId",
                table: "Alerts",
                column: "AlertTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_ContractId",
                table: "Alerts",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_CreatedAtUtc",
                table: "Alerts",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_GuaranteeId",
                table: "Alerts",
                column: "GuaranteeId");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_IsDeleted",
                table: "Alerts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Alerts_PriorityId",
                table: "Alerts",
                column: "PriorityId");

            migrationBuilder.CreateIndex(
                name: "IX_AlertStatuses_Code",
                table: "AlertStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertStatuses_CreatedAtUtc",
                table: "AlertStatuses",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AlertStatuses_IsActive_SortOrder",
                table: "AlertStatuses",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AlertTypes_Code",
                table: "AlertTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AlertTypes_CreatedAtUtc",
                table: "AlertTypes",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AlertTypes_IsActive_SortOrder",
                table: "AlertTypes",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_ActorUserId",
                table: "AuditLogs",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_EntityType_EntityId",
                table: "AuditLogs",
                columns: new[] { "EntityType", "EntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_OccurredAtUtc",
                table: "AuditLogs",
                column: "OccurredAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_Code",
                table: "Banks",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Banks_CreatedAtUtc",
                table: "Banks",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Banks_IsActive_SortOrder",
                table: "Banks",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractAmendments_AmendmentDate",
                table: "ContractAmendments",
                column: "AmendmentDate");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAmendments_ContractId_AmendmentNo",
                table: "ContractAmendments",
                columns: new[] { "ContractId", "AmendmentNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractAmendments_CreatedAtUtc",
                table: "ContractAmendments",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ContractAmendments_IsDeleted",
                table: "ContractAmendments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ContractCategories_Code",
                table: "ContractCategories",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractCategories_CreatedAtUtc",
                table: "ContractCategories",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ContractCategories_IsActive_SortOrder",
                table: "ContractCategories",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractDocuments_ContractId",
                table: "ContractDocuments",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractDocuments_CreatedAtUtc",
                table: "ContractDocuments",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ContractDocuments_DocumentTypeId",
                table: "ContractDocuments",
                column: "DocumentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorEvaluations_ContractId_PartyId_EvaluationDate",
                table: "ContractorEvaluations",
                columns: new[] { "ContractId", "PartyId", "EvaluationDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractorEvaluations_CreatedAtUtc",
                table: "ContractorEvaluations",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorEvaluations_IsDeleted",
                table: "ContractorEvaluations",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ContractorEvaluations_PartyId",
                table: "ContractorEvaluations",
                column: "PartyId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ContractCategoryId",
                table: "Contracts",
                column: "ContractCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ContractNo",
                table: "Contracts",
                column: "ContractNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ContractStatusId_StartDate",
                table: "Contracts",
                columns: new[] { "ContractStatusId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ContractTypeId",
                table: "Contracts",
                column: "ContractTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CreatedAtUtc",
                table: "Contracts",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_CurrencyId",
                table: "Contracts",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_IsDeleted",
                table: "Contracts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ManagerUserId",
                table: "Contracts",
                column: "ManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_PartyId_ContractStatusId",
                table: "Contracts",
                columns: new[] { "PartyId", "ContractStatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_Contracts_ProjectId_ContractStatusId",
                table: "Contracts",
                columns: new[] { "ProjectId", "ContractStatusId" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractStatuses_Code",
                table: "ContractStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractStatuses_CreatedAtUtc",
                table: "ContractStatuses",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ContractStatuses_IsActive_SortOrder",
                table: "ContractStatuses",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ContractTypes_Code",
                table: "ContractTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContractTypes_CreatedAtUtc",
                table: "ContractTypes",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ContractTypes_IsActive_SortOrder",
                table: "ContractTypes",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_Code",
                table: "Currencies",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_CreatedAtUtc",
                table: "Currencies",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_IsActive_SortOrder",
                table: "Currencies",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_Code",
                table: "DocumentTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_CreatedAtUtc",
                table: "DocumentTypes",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentTypes_IsActive_SortOrder",
                table: "DocumentTypes",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_ContractId_TransactionTypeId",
                table: "FinancialTransactions",
                columns: new[] { "ContractId", "TransactionTypeId" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_CreatedAtUtc",
                table: "FinancialTransactions",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_IsDeleted",
                table: "FinancialTransactions",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_StatusId_DueDate",
                table: "FinancialTransactions",
                columns: new[] { "StatusId", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_TransactionNo",
                table: "FinancialTransactions",
                column: "TransactionNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactions_TransactionTypeId",
                table: "FinancialTransactions",
                column: "TransactionTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactionStatuses_Code",
                table: "FinancialTransactionStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactionStatuses_CreatedAtUtc",
                table: "FinancialTransactionStatuses",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_FinancialTransactionStatuses_IsActive_SortOrder",
                table: "FinancialTransactionStatuses",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Guarantees_BankId",
                table: "Guarantees",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Guarantees_ContractId_GuaranteeNo",
                table: "Guarantees",
                columns: new[] { "ContractId", "GuaranteeNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Guarantees_CreatedAtUtc",
                table: "Guarantees",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Guarantees_GuaranteeStatusId_ExpiryDate",
                table: "Guarantees",
                columns: new[] { "GuaranteeStatusId", "ExpiryDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Guarantees_GuaranteeTypeId",
                table: "Guarantees",
                column: "GuaranteeTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Guarantees_IsDeleted",
                table: "Guarantees",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_GuaranteeStatuses_Code",
                table: "GuaranteeStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuaranteeStatuses_CreatedAtUtc",
                table: "GuaranteeStatuses",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_GuaranteeStatuses_IsActive_SortOrder",
                table: "GuaranteeStatuses",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_GuaranteeTypes_Code",
                table: "GuaranteeTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_GuaranteeTypes_CreatedAtUtc",
                table: "GuaranteeTypes",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_GuaranteeTypes_IsActive_SortOrder",
                table: "GuaranteeTypes",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_ContractId_InvoiceNo",
                table: "Invoices",
                columns: new[] { "ContractId", "InvoiceNo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CreatedAtUtc",
                table: "Invoices",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceTypeId",
                table: "Invoices",
                column: "InvoiceTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_IsDeleted",
                table: "Invoices",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_PaymentStatusId_DueDate",
                table: "Invoices",
                columns: new[] { "PaymentStatusId", "DueDate" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceTypes_Code",
                table: "InvoiceTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceTypes_CreatedAtUtc",
                table: "InvoiceTypes",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceTypes_IsActive_SortOrder",
                table: "InvoiceTypes",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Parties_CreatedAtUtc",
                table: "Parties",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_IsDeleted",
                table: "Parties",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_NationalId",
                table: "Parties",
                column: "NationalId");

            migrationBuilder.CreateIndex(
                name: "IX_Parties_PartyTypeId",
                table: "Parties",
                column: "PartyTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PartyTypes_Code",
                table: "PartyTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PartyTypes_CreatedAtUtc",
                table: "PartyTypes",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PartyTypes_IsActive_SortOrder",
                table: "PartyTypes",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_CreatedAtUtc",
                table: "PaymentAllocations",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_FinancialTransactionId_PaymentId",
                table: "PaymentAllocations",
                columns: new[] { "FinancialTransactionId", "PaymentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentAllocations_PaymentId",
                table: "PaymentAllocations",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BankId",
                table: "Payments",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_ContractId_PaymentDate",
                table: "Payments",
                columns: new[] { "ContractId", "PaymentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedAtUtc",
                table: "Payments",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_IsDeleted",
                table: "Payments",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentTypeId",
                table: "Payments",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatuses_Code",
                table: "PaymentStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatuses_CreatedAtUtc",
                table: "PaymentStatuses",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentStatuses_IsActive_SortOrder",
                table: "PaymentStatuses",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypes_Code",
                table: "PaymentTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypes_CreatedAtUtc",
                table: "PaymentTypes",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentTypes_IsActive_SortOrder",
                table: "PaymentTypes",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_Code",
                table: "Priorities",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_CreatedAtUtc",
                table: "Priorities",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Priorities_IsActive_SortOrder",
                table: "Priorities",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBudgets_CreatedAtUtc",
                table: "ProjectBudgets",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBudgets_CurrencyId",
                table: "ProjectBudgets",
                column: "CurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBudgets_IsDeleted",
                table: "ProjectBudgets",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectBudgets_ProjectId_FiscalYear_CurrencyId",
                table: "ProjectBudgets",
                columns: new[] { "ProjectId", "FiscalYear", "CurrencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CreatedAtUtc",
                table: "Projects",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_IsDeleted",
                table: "Projects",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ManagerUserId",
                table: "Projects",
                column: "ManagerUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectNo",
                table: "Projects",
                column: "ProjectNo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_ProjectStatusId_StartDate",
                table: "Projects",
                columns: new[] { "ProjectStatusId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStatuses_Code",
                table: "ProjectStatuses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStatuses_CreatedAtUtc",
                table: "ProjectStatuses",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ProjectStatuses_IsActive_SortOrder",
                table: "ProjectStatuses",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptAllocations_CreatedAtUtc",
                table: "ReceiptAllocations",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptAllocations_FinancialTransactionId_ReceiptId",
                table: "ReceiptAllocations",
                columns: new[] { "FinancialTransactionId", "ReceiptId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReceiptAllocations_ReceiptId",
                table: "ReceiptAllocations",
                column: "ReceiptId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_BankId",
                table: "Receipts",
                column: "BankId");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_ContractId_ReceiptDate",
                table: "Receipts",
                columns: new[] { "ContractId", "ReceiptDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_CreatedAtUtc",
                table: "Receipts",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_IsDeleted",
                table: "Receipts",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Receipts_PaymentTypeId",
                table: "Receipts",
                column: "PaymentTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_CreatedAtUtc",
                table: "RolePermissions",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_RoleId_PermissionCode",
                table: "RolePermissions",
                columns: new[] { "RoleId", "PermissionCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_Code",
                table: "Roles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_CreatedAtUtc",
                table: "Roles",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_IsActive_SortOrder",
                table: "Roles",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypes_Code",
                table: "TransactionTypes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypes_CreatedAtUtc",
                table: "TransactionTypes",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_TransactionTypes_IsActive_SortOrder",
                table: "TransactionTypes",
                columns: new[] { "IsActive", "SortOrder" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAtUtc",
                table: "Users",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Users_IsDeleted",
                table: "Users",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alerts");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "ContractAmendments");

            migrationBuilder.DropTable(
                name: "ContractDocuments");

            migrationBuilder.DropTable(
                name: "ContractorEvaluations");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "PaymentAllocations");

            migrationBuilder.DropTable(
                name: "ProjectBudgets");

            migrationBuilder.DropTable(
                name: "ReceiptAllocations");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "AlertStatuses");

            migrationBuilder.DropTable(
                name: "AlertTypes");

            migrationBuilder.DropTable(
                name: "Guarantees");

            migrationBuilder.DropTable(
                name: "Priorities");

            migrationBuilder.DropTable(
                name: "DocumentTypes");

            migrationBuilder.DropTable(
                name: "InvoiceTypes");

            migrationBuilder.DropTable(
                name: "PaymentStatuses");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "FinancialTransactions");

            migrationBuilder.DropTable(
                name: "Receipts");

            migrationBuilder.DropTable(
                name: "GuaranteeStatuses");

            migrationBuilder.DropTable(
                name: "GuaranteeTypes");

            migrationBuilder.DropTable(
                name: "FinancialTransactionStatuses");

            migrationBuilder.DropTable(
                name: "TransactionTypes");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "Contracts");

            migrationBuilder.DropTable(
                name: "PaymentTypes");

            migrationBuilder.DropTable(
                name: "ContractCategories");

            migrationBuilder.DropTable(
                name: "ContractStatuses");

            migrationBuilder.DropTable(
                name: "ContractTypes");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropTable(
                name: "Parties");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.DropTable(
                name: "PartyTypes");

            migrationBuilder.DropTable(
                name: "ProjectStatuses");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
