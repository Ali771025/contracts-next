namespace Contracts.Infrastructure.Persistence;

internal static class CanonicalModelConfiguration
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        ConfigureReferences(modelBuilder);
        ConfigureSecurity(modelBuilder);
        ConfigurePartiesAndProjects(modelBuilder);
        ConfigureContracts(modelBuilder);
        ConfigureFinance(modelBuilder);
        ConfigureOkr(modelBuilder);
    }

    private static void ConfigureOkr(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<OkrObjective> objective=modelBuilder.Entity<OkrObjective>();ConfigureAudited(objective,"OkrObjectives");
        objective.Property(x=>x.Id).ValueGeneratedNever();objective.Property(x=>x.Code).HasMaxLength(32).UseCollation("NOCASE");objective.Property(x=>x.TitleFa).HasMaxLength(200);objective.Property(x=>x.Description).HasMaxLength(2000);
        objective.HasIndex(x=>x.Code).IsUnique();objective.HasIndex(x=>new{x.IsActive,x.SortOrder});
        EntityTypeBuilder<OkrKeyResult> kr=modelBuilder.Entity<OkrKeyResult>();ConfigureAudited(kr,"OkrKeyResults",t=>{t.HasCheckConstraint("CK_OkrKeyResults_Weight","WeightPercent BETWEEN 0 AND 100");t.HasCheckConstraint("CK_OkrKeyResults_Target","TargetValue >= 0");});
        kr.Property(x=>x.Id).ValueGeneratedNever();kr.Property(x=>x.Code).HasMaxLength(32).UseCollation("NOCASE");kr.Property(x=>x.TitleFa).HasMaxLength(200);kr.Property(x=>x.TargetValue).HasPrecision(20,6);kr.Property(x=>x.Unit).HasMaxLength(32);
        kr.Property(x=>x.Direction).HasConversion<string>().HasMaxLength(32);kr.Property(x=>x.MetricCode).HasMaxLength(80).UseCollation("NOCASE");kr.Property(x=>x.DataSourceType).HasConversion<string>().HasMaxLength(16);kr.Property(x=>x.MetricVersion).HasMaxLength(32);
        kr.HasIndex(x=>x.Code).IsUnique();kr.HasIndex(x=>x.MetricCode).IsUnique();kr.HasIndex(x=>new{x.ObjectiveId,x.SortOrder});kr.HasOne<OkrObjective>().WithMany().HasForeignKey(x=>x.ObjectiveId).OnDelete(DeleteBehavior.Restrict);
        EntityTypeBuilder<OkrMeasurement> m=modelBuilder.Entity<OkrMeasurement>();ConfigureAudited(m,"OkrMeasurements",t=>{t.HasCheckConstraint("CK_OkrMeasurements_Actual","ActualValue >= 0");t.HasCheckConstraint("CK_OkrMeasurements_Score","Score BETWEEN 0 AND 1");t.HasCheckConstraint("CK_OkrMeasurements_AutoEvidence","SourceType <> 'Auto' OR (AsOfUtc IS NOT NULL AND EvidenceCount IS NOT NULL AND EvidenceCount >= 0)");});
        m.Property(x=>x.ActualValue).HasPrecision(20,6);m.Property(x=>x.Score).HasPrecision(9,6);m.Property(x=>x.SourceType).HasConversion<string>().HasMaxLength(16);m.Property(x=>x.MetricVersion).HasMaxLength(32);m.Property(x=>x.Note).HasMaxLength(2000);
        m.HasIndex(x=>new{x.KeyResultId,x.MeasurementDate,x.Id});m.HasOne<OkrKeyResult>().WithMany().HasForeignKey(x=>x.KeyResultId).OnDelete(DeleteBehavior.Restrict);m.HasOne<User>().WithMany().HasForeignKey(x=>x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureReferences(ModelBuilder modelBuilder)
    {
        ConfigureReference<ContractType>(modelBuilder, "ContractTypes");
        ConfigureReference<ContractStatus>(modelBuilder, "ContractStatuses");
        ConfigureReference<PartyType>(modelBuilder, "PartyTypes");
        ConfigureReference<ProjectStatus>(modelBuilder, "ProjectStatuses");
        ConfigureReference<Currency>(modelBuilder, "Currencies");
        ConfigureReference<Bank>(modelBuilder, "Banks");
        ConfigureReference<PaymentType>(modelBuilder, "PaymentTypes");
        ConfigureReference<GuaranteeType>(modelBuilder, "GuaranteeTypes");
        ConfigureReference<GuaranteeStatus>(modelBuilder, "GuaranteeStatuses");
        ConfigureReference<InvoiceType>(modelBuilder, "InvoiceTypes");
        ConfigureReference<PaymentStatus>(modelBuilder, "PaymentStatuses");
        ConfigureReference<DocumentType>(modelBuilder, "DocumentTypes");
        ConfigureReference<AlertType>(modelBuilder, "AlertTypes");
        ConfigureReference<AlertStatus>(modelBuilder, "AlertStatuses");
        ConfigureReference<Priority>(modelBuilder, "Priorities");
        ConfigureReference<TransactionType>(modelBuilder, "TransactionTypes");
        ConfigureReference<FinancialTransactionStatus>(modelBuilder, "FinancialTransactionStatuses");
        ConfigureReference<Role>(modelBuilder, "Roles");
        ConfigureReference<ContractCategory>(modelBuilder, "ContractCategories");

        modelBuilder.Entity<Currency>()
            .ToTable("Currencies", table =>
                table.HasCheckConstraint("CK_Currencies_Scale", "Scale >= 0 AND Scale <= 6"));
    }

    private static void ConfigureSecurity(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<User> user = modelBuilder.Entity<User>();
        ConfigureSoftDeletable(user, "Users");
        user.Property(item => item.Username).HasMaxLength(100).UseCollation("NOCASE");
        user.Property(item => item.DisplayName).HasMaxLength(200);
        user.Property(item => item.PasswordHash).HasMaxLength(64);
        user.Property(item => item.PasswordSalt).HasMaxLength(32);
        user.Property(item => item.PasswordAlgorithm).HasMaxLength(50);
        user.HasIndex(item => item.Username).IsUnique();
        user.HasIndex(item => item.IsBootstrapAdministrator).IsUnique().HasFilter("IsBootstrapAdministrator = 1");
        user.HasIndex(item => item.RoleId);
        user.HasOne<Role>().WithMany().HasForeignKey(item => item.RoleId).OnDelete(DeleteBehavior.Restrict);

        EntityTypeBuilder<RolePermission> permission = modelBuilder.Entity<RolePermission>();
        ConfigureAudited(permission, "RolePermissions");
        permission.Property(item => item.SectionCode).HasMaxLength(64).UseCollation("NOCASE");
        permission.HasIndex(item => new { item.RoleId, item.SectionCode }).IsUnique();
        permission.HasOne<Role>().WithMany(item => item.Permissions).HasForeignKey(item => item.RoleId).OnDelete(DeleteBehavior.Cascade);

        EntityTypeBuilder<AuditLog> audit = modelBuilder.Entity<AuditLog>();
        ConfigureEntity(audit, "AuditLogs");
        audit.Property(item => item.Action).HasMaxLength(64);
        audit.Property(item => item.EntityType).HasMaxLength(150);
        audit.Property(item => item.EntityId).HasMaxLength(100);
        audit.Property(item => item.BeforeJson).HasMaxLength(4000);
        audit.Property(item => item.AfterJson).HasMaxLength(4000);
        audit.Property(item => item.CorrelationId).HasMaxLength(100);
        audit.Property(item => item.Outcome).HasMaxLength(32);
        audit.HasIndex(item => item.OccurredAtUtc);
        audit.HasIndex(item => item.CorrelationId);
        audit.HasIndex(item => new { item.EntityType, item.EntityId });
        audit.HasOne<User>().WithMany().HasForeignKey(item => item.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurePartiesAndProjects(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<Party> party = modelBuilder.Entity<Party>();
        ConfigureSoftDeletable(party, "Parties");
        party.Property(item => item.Name).HasMaxLength(250);
        party.Property(item => item.NormalizedName).HasMaxLength(250);
        party.Property(item => item.NationalId).HasMaxLength(20);
        party.Property(item => item.EconomicCode).HasMaxLength(30);
        party.Property(item => item.RegistrationNo).HasMaxLength(30);
        party.Property(item => item.Phone).HasMaxLength(30);
        party.Property(item => item.Mobile).HasMaxLength(20);
        party.Property(item => item.Email).HasMaxLength(254);
        party.Property(item => item.ContactPerson).HasMaxLength(200);
        party.Property(item => item.Address).HasMaxLength(1000);
        party.HasIndex(item => item.PartyTypeId);
        party.HasIndex(item => item.NationalId);
        party.HasIndex(item => new { item.NormalizedName, item.Id });
        party.HasOne<PartyType>().WithMany().HasForeignKey(item => item.PartyTypeId).OnDelete(DeleteBehavior.Restrict);

        EntityTypeBuilder<Project> project = modelBuilder.Entity<Project>();
        ConfigureSoftDeletable(project, "Projects", table =>
            table.HasCheckConstraint("CK_Projects_Dates", "EndDate IS NULL OR StartDate IS NULL OR EndDate >= StartDate"));
        project.Property(item => item.ProjectNo).HasMaxLength(100).UseCollation("NOCASE");
        project.Property(item => item.NormalizedProjectNo).HasMaxLength(100);
        project.Property(item => item.Title).HasMaxLength(300);
        project.Property(item => item.NormalizedTitle).HasMaxLength(300);
        project.Property(item => item.Employer).HasMaxLength(250);
        project.Property(item => item.Department).HasMaxLength(200);
        project.Property(item => item.Description).HasMaxLength(2000);
        project.HasIndex(item => item.ProjectNo).IsUnique();
        project.HasIndex(item => item.NormalizedProjectNo).IsUnique();
        project.HasIndex(item => new { item.NormalizedTitle, item.Id });
        project.HasIndex(item => new { item.ProjectStatusId, item.StartDate });
        project.HasOne<ProjectStatus>().WithMany().HasForeignKey(item => item.ProjectStatusId).OnDelete(DeleteBehavior.Restrict);
        project.HasOne<User>().WithMany().HasForeignKey(item => item.ManagerUserId).OnDelete(DeleteBehavior.Restrict);

        EntityTypeBuilder<ProjectBudget> budget = modelBuilder.Entity<ProjectBudget>();
        ConfigureSoftDeletable(budget, "ProjectBudgets", table =>
        {
            table.HasCheckConstraint("CK_ProjectBudgets_Approved", "ApprovedAmountMinor >= 0");
            table.HasCheckConstraint("CK_ProjectBudgets_Consumed", "ConsumedAmountMinor >= 0 AND ConsumedAmountMinor <= ApprovedAmountMinor");
            table.HasCheckConstraint("CK_ProjectBudgets_Scale", "CurrencyScale >= 0 AND CurrencyScale <= 6");
        });
        budget.Property(item => item.FiscalYear).HasMaxLength(20);
        budget.HasIndex(item => new { item.ProjectId, item.FiscalYear, item.CurrencyId }).IsUnique();
        budget.HasOne<Project>().WithMany().HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Cascade);
        budget.HasOne<Currency>().WithMany().HasForeignKey(item => item.CurrencyId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureContracts(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<Contract> contract = modelBuilder.Entity<Contract>();
        ConfigureSoftDeletable(contract, "Contracts", table =>
        {
            table.HasCheckConstraint("CK_Contracts_OriginalAmount", "OriginalAmountMinor >= 0");
            table.HasCheckConstraint("CK_Contracts_BaseAmount", "BaseAmountRial >= 0");
            table.HasCheckConstraint("CK_Contracts_ExchangeRate", "ExchangeRateRialPerUnit > 0");
            table.HasCheckConstraint("CK_Contracts_Scale", "CurrencyScale >= 0 AND CurrencyScale <= 6");
            table.HasCheckConstraint("CK_Contracts_Dates", "EndDate >= StartDate");
            table.HasCheckConstraint("CK_Contracts_AuditActors", "CreatedByUserId IS NOT NULL AND UpdatedByUserId IS NOT NULL");
        });
        contract.Property(item => item.ContractNo).HasMaxLength(100).UseCollation("NOCASE");
        contract.Property(item => item.NormalizedContractNo).HasMaxLength(100);
        contract.Property(item => item.Title).HasMaxLength(300);
        contract.Property(item => item.NormalizedTitle).HasMaxLength(300);
        contract.Property(item => item.Description).HasMaxLength(4000);
        contract.HasIndex(item => item.ContractNo).IsUnique();
        contract.HasIndex(item => item.NormalizedContractNo).IsUnique();
        contract.HasIndex(item => new { item.NormalizedTitle, item.Id });
        contract.HasIndex(item => new { item.ContractStatusId, item.StartDate });
        contract.HasIndex(item => new { item.PartyId, item.ContractStatusId });
        contract.HasIndex(item => new { item.ProjectId, item.ContractStatusId });
        contract.HasIndex(item => item.ContractTypeId);
        contract.HasOne<ContractType>().WithMany().HasForeignKey(item => item.ContractTypeId).OnDelete(DeleteBehavior.Restrict);
        contract.HasOne<ContractCategory>().WithMany().HasForeignKey(item => item.ContractCategoryId).OnDelete(DeleteBehavior.Restrict);
        contract.HasOne<Project>().WithMany().HasForeignKey(item => item.ProjectId).OnDelete(DeleteBehavior.Restrict);
        contract.HasOne<Party>().WithMany().HasForeignKey(item => item.PartyId).OnDelete(DeleteBehavior.Restrict);
        contract.HasOne<ContractStatus>().WithMany().HasForeignKey(item => item.ContractStatusId).OnDelete(DeleteBehavior.Restrict);
        contract.HasOne<Currency>().WithMany().HasForeignKey(item => item.CurrencyId).OnDelete(DeleteBehavior.Restrict);
        contract.HasOne<User>().WithMany().HasForeignKey(item => item.ManagerUserId).OnDelete(DeleteBehavior.Restrict);
        contract.HasMany(item => item.Amendments).WithOne().HasForeignKey(item => item.ContractId).OnDelete(DeleteBehavior.Restrict);

        EntityTypeBuilder<ContractAmendment> amendment = modelBuilder.Entity<ContractAmendment>();
        ConfigureSoftDeletable(amendment, "ContractAmendments");
        amendment.Property(item => item.AmendmentNo).HasMaxLength(100).UseCollation("NOCASE");
        amendment.Property(item => item.Subject).HasMaxLength(300);
        amendment.Property(item => item.AttachmentPath).HasMaxLength(1000);
        amendment.Property(item => item.Description).HasMaxLength(2000);
        amendment.HasIndex(item => new { item.ContractId, item.AmendmentNo }).IsUnique();
        amendment.HasIndex(item => item.AmendmentDate);

        EntityTypeBuilder<Invoice> invoice = modelBuilder.Entity<Invoice>();
        ConfigureSoftDeletable(invoice, "Invoices", table =>
        {
            table.HasCheckConstraint("CK_Invoices_Amount", "AmountRial >= 0");
            table.HasCheckConstraint("CK_Invoices_Vat", "VatAmountRial >= 0");
        });
        invoice.Ignore(item => item.TotalAmountRial);
        invoice.Property(item => item.InvoiceNo).HasMaxLength(100).UseCollation("NOCASE");
        invoice.Property(item => item.Description).HasMaxLength(2000);
        invoice.HasIndex(item => new { item.ContractId, item.InvoiceNo }).IsUnique();
        invoice.HasIndex(item => new { item.PaymentStatusId, item.DueDate });
        invoice.HasOne<Contract>().WithMany().HasForeignKey(item => item.ContractId).OnDelete(DeleteBehavior.Restrict);
        invoice.HasOne<InvoiceType>().WithMany().HasForeignKey(item => item.InvoiceTypeId).OnDelete(DeleteBehavior.Restrict);
        invoice.HasOne<PaymentStatus>().WithMany().HasForeignKey(item => item.PaymentStatusId).OnDelete(DeleteBehavior.Restrict);

        EntityTypeBuilder<Guarantee> guarantee = modelBuilder.Entity<Guarantee>();
        ConfigureSoftDeletable(guarantee, "Guarantees", table =>
            table.HasCheckConstraint("CK_Guarantees_Amount", "AmountRial > 0"));
        guarantee.Property(item => item.GuaranteeNo).HasMaxLength(100).UseCollation("NOCASE");
        guarantee.Property(item => item.Description).HasMaxLength(2000);
        guarantee.HasIndex(item => new { item.ContractId, item.GuaranteeNo }).IsUnique();
        guarantee.HasIndex(item => new { item.GuaranteeStatusId, item.ExpiryDate });
        guarantee.HasOne<Contract>().WithMany().HasForeignKey(item => item.ContractId).OnDelete(DeleteBehavior.Restrict);
        guarantee.HasOne<GuaranteeType>().WithMany().HasForeignKey(item => item.GuaranteeTypeId).OnDelete(DeleteBehavior.Restrict);
        guarantee.HasOne<GuaranteeStatus>().WithMany().HasForeignKey(item => item.GuaranteeStatusId).OnDelete(DeleteBehavior.Restrict);
        guarantee.HasOne<Bank>().WithMany().HasForeignKey(item => item.BankId).OnDelete(DeleteBehavior.Restrict);

        EntityTypeBuilder<ContractDocument> document = modelBuilder.Entity<ContractDocument>();
        ConfigureAudited(document, "ContractDocuments");
        document.Property(item => item.OriginalFileName).HasMaxLength(260);
        document.Property(item => item.StoredFileName).HasMaxLength(260);
        document.Property(item => item.RelativePath).HasMaxLength(1000);
        document.Property(item => item.ContentHashSha256).HasMaxLength(64);
        document.ToTable("ContractDocuments", table => table.HasCheckConstraint("CK_ContractDocuments_Size", "FileSize >= 0"));
        document.HasIndex(item => item.RelativePath).IsUnique();
        document.HasIndex(item => item.ContractId);
        document.HasOne<Contract>().WithMany().HasForeignKey(item => item.ContractId).OnDelete(DeleteBehavior.Cascade);
        document.HasOne<DocumentType>().WithMany().HasForeignKey(item => item.DocumentTypeId).OnDelete(DeleteBehavior.Restrict);

        EntityTypeBuilder<Alert> alert = modelBuilder.Entity<Alert>();
        ConfigureSoftDeletable(alert, "Alerts");
        alert.Property(item => item.Title).HasMaxLength(300);
        alert.Property(item => item.DeduplicationKey).HasMaxLength(200).UseCollation("NOCASE");
        alert.HasIndex(item => item.DeduplicationKey).IsUnique();
        alert.HasIndex(item => new { item.AlertStatusId, item.DueDate, item.PriorityId });
        alert.HasOne<Contract>().WithMany().HasForeignKey(item => item.ContractId).OnDelete(DeleteBehavior.Cascade);
        alert.HasOne<Guarantee>().WithMany().HasForeignKey(item => item.GuaranteeId).OnDelete(DeleteBehavior.Restrict);
        alert.HasOne<FinancialTransaction>().WithMany().HasForeignKey(item => item.FinancialTransactionId).OnDelete(DeleteBehavior.Restrict);
        alert.HasOne<AlertType>().WithMany().HasForeignKey(item => item.AlertTypeId).OnDelete(DeleteBehavior.Restrict);
        alert.HasOne<AlertStatus>().WithMany().HasForeignKey(item => item.AlertStatusId).OnDelete(DeleteBehavior.Restrict);
        alert.HasOne<Priority>().WithMany().HasForeignKey(item => item.PriorityId).OnDelete(DeleteBehavior.Restrict);

        EntityTypeBuilder<ContractorEvaluation> evaluation = modelBuilder.Entity<ContractorEvaluation>();
        ConfigureSoftDeletable(evaluation, "ContractorEvaluations", table =>
        {
            table.HasCheckConstraint("CK_Evaluations_Quality", "QualityScore BETWEEN 0 AND 100");
            table.HasCheckConstraint("CK_Evaluations_Time", "TimeScore BETWEEN 0 AND 100");
            table.HasCheckConstraint("CK_Evaluations_Financial", "FinancialScore BETWEEN 0 AND 100");
            table.HasCheckConstraint("CK_Evaluations_Hse", "HseScore BETWEEN 0 AND 100");
        });
        evaluation.Ignore(item => item.TotalScore);
        evaluation.Property(item => item.Notes).HasMaxLength(2000);
        evaluation.HasIndex(item => new { item.ContractId, item.PartyId, item.EvaluationDate });
        evaluation.HasOne<Contract>().WithMany().HasForeignKey(item => item.ContractId).OnDelete(DeleteBehavior.Restrict);
        evaluation.HasOne<Party>().WithMany().HasForeignKey(item => item.PartyId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureFinance(ModelBuilder modelBuilder)
    {
        EntityTypeBuilder<Payment> payment = modelBuilder.Entity<Payment>();
        ConfigureSoftDeletable(payment, "Payments", table =>
            table.HasCheckConstraint("CK_Payments_Amount", "AmountRial > 0"));
        payment.Ignore(item => item.RemainingAmountRial);
        payment.Property(item => item.PaymentNo).HasMaxLength(100).UseCollation("NOCASE");
        payment.Property(item => item.ReferenceNo).HasMaxLength(100);
        payment.Property(item => item.InvoiceNo).HasMaxLength(100);
        payment.Property(item => item.ChequeNo).HasMaxLength(100);
        payment.Property(item => item.PayerDescription).HasMaxLength(500);
        payment.Property(item => item.AttachmentPath).HasMaxLength(1000);
        payment.Property(item => item.Description).HasMaxLength(2000);
        payment.HasIndex(item => item.PaymentNo).IsUnique();
        payment.HasIndex(item => new { item.ContractId, item.PaymentDate });
        payment.HasOne<Contract>().WithMany().HasForeignKey(item => item.ContractId).OnDelete(DeleteBehavior.Restrict);
        payment.HasOne<PaymentType>().WithMany().HasForeignKey(item => item.PaymentTypeId).OnDelete(DeleteBehavior.Restrict);
        payment.HasOne<Bank>().WithMany().HasForeignKey(item => item.BankId).OnDelete(DeleteBehavior.Restrict);

        EntityTypeBuilder<Receipt> receipt = modelBuilder.Entity<Receipt>();
        ConfigureSoftDeletable(receipt, "Receipts", table =>
            table.HasCheckConstraint("CK_Receipts_Amount", "AmountRial > 0"));
        receipt.Ignore(item => item.RemainingAmountRial);
        receipt.Property(item => item.ReceiptNo).HasMaxLength(100).UseCollation("NOCASE");
        receipt.Property(item => item.ReferenceNo).HasMaxLength(100);
        receipt.Property(item => item.InvoiceNo).HasMaxLength(100);
        receipt.Property(item => item.ChequeNo).HasMaxLength(100);
        receipt.Property(item => item.PayerDescription).HasMaxLength(500);
        receipt.Property(item => item.AttachmentPath).HasMaxLength(1000);
        receipt.Property(item => item.Description).HasMaxLength(2000);
        receipt.HasIndex(item => item.ReceiptNo).IsUnique();
        receipt.HasIndex(item => new { item.ContractId, item.ReceiptDate });
        receipt.HasOne<Contract>().WithMany().HasForeignKey(item => item.ContractId).OnDelete(DeleteBehavior.Restrict);
        receipt.HasOne<PaymentType>().WithMany().HasForeignKey(item => item.PaymentTypeId).OnDelete(DeleteBehavior.Restrict);
        receipt.HasOne<Bank>().WithMany().HasForeignKey(item => item.BankId).OnDelete(DeleteBehavior.Restrict);

        EntityTypeBuilder<FinancialTransaction> transaction = modelBuilder.Entity<FinancialTransaction>();
        ConfigureSoftDeletable(transaction, "FinancialTransactions", table =>
        {
            table.HasCheckConstraint("CK_FinancialTransactions_Amount", "AmountRial > 0");
            table.HasCheckConstraint("CK_FinancialTransactions_Dates", "DueDate >= TransactionDate");
        });
        transaction.Ignore(item => item.AllocatedAmountRial);
        transaction.Ignore(item => item.RemainingAmountRial);
        transaction.Property(item => item.TransactionNo).HasMaxLength(100).UseCollation("NOCASE");
        transaction.Property(item => item.Title).HasMaxLength(300);
        transaction.Property(item => item.Description).HasMaxLength(2000);
        transaction.HasIndex(item => item.TransactionNo).IsUnique();
        transaction.HasIndex(item => new { item.StatusId, item.DueDate });
        transaction.HasIndex(item => new { item.TransactionDate, item.Id });
        transaction.HasIndex(item => new { item.ContractId, item.TransactionTypeId });
        transaction.HasOne<Contract>().WithMany().HasForeignKey(item => item.ContractId).OnDelete(DeleteBehavior.Restrict);
        transaction.HasOne<TransactionType>().WithMany().HasForeignKey(item => item.TransactionTypeId).OnDelete(DeleteBehavior.Restrict);
        transaction.HasOne<FinancialTransactionStatus>().WithMany().HasForeignKey(item => item.StatusId).OnDelete(DeleteBehavior.Restrict);

        EntityTypeBuilder<PaymentAllocation> paymentAllocation = modelBuilder.Entity<PaymentAllocation>();
        ConfigureAudited(paymentAllocation, "PaymentAllocations", table =>
            table.HasCheckConstraint("CK_PaymentAllocations_Amount", "AmountRial > 0"));
        paymentAllocation.HasIndex(item => new { item.FinancialTransactionId, item.PaymentId }).IsUnique();
        paymentAllocation.HasOne(item => item.FinancialTransaction).WithMany(item => item.PaymentAllocations).HasForeignKey(item => item.FinancialTransactionId).OnDelete(DeleteBehavior.Cascade);
        paymentAllocation.HasOne(item => item.Payment).WithMany(item => item.Allocations).HasForeignKey(item => item.PaymentId).OnDelete(DeleteBehavior.Restrict);

        EntityTypeBuilder<ReceiptAllocation> receiptAllocation = modelBuilder.Entity<ReceiptAllocation>();
        ConfigureAudited(receiptAllocation, "ReceiptAllocations", table =>
            table.HasCheckConstraint("CK_ReceiptAllocations_Amount", "AmountRial > 0"));
        receiptAllocation.HasIndex(item => new { item.FinancialTransactionId, item.ReceiptId }).IsUnique();
        receiptAllocation.HasOne(item => item.FinancialTransaction).WithMany(item => item.ReceiptAllocations).HasForeignKey(item => item.FinancialTransactionId).OnDelete(DeleteBehavior.Cascade);
        receiptAllocation.HasOne(item => item.Receipt).WithMany(item => item.Allocations).HasForeignKey(item => item.ReceiptId).OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureReference<T>(ModelBuilder modelBuilder, string tableName)
        where T : ReferenceEntity
    {
        EntityTypeBuilder<T> builder = modelBuilder.Entity<T>();
        ConfigureAudited(builder, tableName);
        builder.Property(item => item.Id).ValueGeneratedNever();
        builder.Property(item => item.Code).HasMaxLength(64).UseCollation("NOCASE");
        builder.Property(item => item.TitleFa).HasMaxLength(150);
        builder.Property(item => item.NormalizedTitleFa).HasMaxLength(150);
        builder.HasIndex(item => item.Code).IsUnique();
        builder.HasIndex(item => item.NormalizedTitleFa).IsUnique();
        builder.HasIndex(item => new { item.IsActive, item.SortOrder });
    }

    private static void ConfigureEntity<T>(EntityTypeBuilder<T> builder, string tableName)
        where T : Entity
    {
        builder.ToTable(tableName);
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedOnAdd();
        builder.Property(item => item.ConcurrencyVersion).IsConcurrencyToken();
    }

    private static void ConfigureAudited<T>(
        EntityTypeBuilder<T> builder,
        string tableName,
        Action<TableBuilder<T>>? tableConfiguration = null)
        where T : AuditedEntity
    {
        if (tableConfiguration is null)
        {
            builder.ToTable(tableName);
        }
        else
        {
            builder.ToTable(tableName, tableConfiguration);
        }
        builder.HasKey(item => item.Id);
        builder.Property(item => item.Id).ValueGeneratedOnAdd();
        builder.Property(item => item.ConcurrencyVersion).IsConcurrencyToken();
        builder.HasIndex(item => item.CreatedAtUtc);
    }

    private static void ConfigureSoftDeletable<T>(
        EntityTypeBuilder<T> builder,
        string tableName,
        Action<TableBuilder<T>>? tableConfiguration = null)
        where T : SoftDeletableEntity
    {
        ConfigureAudited(builder, tableName, tableConfiguration);
        builder.HasIndex(item => item.IsDeleted);
    }
}
