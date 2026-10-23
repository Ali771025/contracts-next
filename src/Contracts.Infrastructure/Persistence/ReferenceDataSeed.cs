namespace Contracts.Infrastructure.Persistence;

internal static class ReferenceDataSeed
{
    private static readonly AuditStamp SeedStamp = new(null, new DateTimeOffset(2026, 1, 1, 0, 0, 0, TimeSpan.Zero));

    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ContractType>().HasData(
            Ref<ContractType>(1, "PURCHASE", "خرید", 10),
            Ref<ContractType>(2, "SALE", "فروش", 20),
            Ref<ContractType>(3, "OUTSOURCE", "برون‌سپاری", 30),
            Ref<ContractType>(4, "CONTRACTING", "پیمانکاری", 40),
            Ref<ContractType>(5, "CONSULTING", "مشاوره", 50));

        modelBuilder.Entity<Currency>().HasData(
            new Currency(1, "IRR", "ریال ایران", 10, true, 0, SeedStamp),
            new Currency(2, "USD", "دلار آمریکا", 20, true, 2, SeedStamp),
            new Currency(3, "EUR", "یورو", 30, true, 2, SeedStamp));

        modelBuilder.Entity<TransactionType>().HasData(
            Ref<TransactionType>(ReferenceIds.TransactionTypes.Claim, "CLAIM", "مطالبه", 10),
            Ref<TransactionType>(ReferenceIds.TransactionTypes.Debt, "DEBT", "بدهی", 20));

        modelBuilder.Entity<PaymentType>().HasData(
            Ref<PaymentType>(1, "CASH", "نقدی", 10),
            Ref<PaymentType>(2, "CHEQUE", "چک", 20),
            Ref<PaymentType>(3, "BANK_TRANSFER", "انتقال بانکی", 30),
            Ref<PaymentType>(4, "ADVANCE", "پیش‌پرداخت", 40),
            Ref<PaymentType>(5, "ON_ACCOUNT", "علی‌الحساب", 50),
            Ref<PaymentType>(6, "RAHSA", "رهسا", 60),
            Ref<PaymentType>(7, "UNSPECIFIED_LEGACY", "نامشخص در داده قدیمی", 70));

        modelBuilder.Entity<Role>().HasData(
            new Role(1, "SYSTEM_ADMIN", "مدیر سیستم", 10, true, SeedStamp),
            new Role(2, "CONTRACT_MANAGER", "مدیر قرارداد", 20, true, SeedStamp),
            new Role(3, "CHIEF_EXECUTIVE", "مدیرعامل", 30, true, SeedStamp),
            new Role(4, "VIEWER", "مشاهده‌کننده", 40, true, SeedStamp),
            new Role(5, "LEGAL_EXPERT", "کارشناس حقوقی", 50, true, SeedStamp),
            new Role(6, "FINANCE_EXPERT", "کارشناس مالی", 60, true, SeedStamp));

        modelBuilder.Entity<RolePermission>().HasData(SecurityPermissions());

        modelBuilder.Entity<ContractStatus>().HasData(
            Ref<ContractStatus>(1, "DRAFT", "پیش‌نویس", 10),
            Ref<ContractStatus>(2, "ACTIVE", "فعال", 20),
            Ref<ContractStatus>(3, "COMPLETED", "تکمیل‌شده", 30),
            Ref<ContractStatus>(4, "CANCELLED", "لغوشده", 40),
            Ref<ContractStatus>(5, "SUSPENDED", "تعلیق", 50),
            Ref<ContractStatus>(6, "TERMINATED", "خاتمه", 60));

        modelBuilder.Entity<PartyType>().HasData(
            Ref<PartyType>(1, "CUSTOMER", "مشتری", 10),
            Ref<PartyType>(2, "SUPPLIER", "تأمین‌کننده", 20),
            Ref<PartyType>(3, "CONTRACTOR", "پیمانکار", 30),
            Ref<PartyType>(4, "CONSULTANT", "مشاور", 40),
            Ref<PartyType>(5, "LEGAL_ENTITY", "شخص حقوقی", 50),
            Ref<PartyType>(6, "NATURAL_PERSON", "شخص حقیقی", 60));

        modelBuilder.Entity<ProjectStatus>().HasData(
            Ref<ProjectStatus>(1, "PLANNED", "برنامه‌ریزی‌شده", 10),
            Ref<ProjectStatus>(2, "ACTIVE", "فعال", 20),
            Ref<ProjectStatus>(3, "COMPLETED", "تکمیل‌شده", 30),
            Ref<ProjectStatus>(4, "CANCELLED", "لغوشده", 40),
            Ref<ProjectStatus>(5, "SUSPENDED", "تعلیق", 50));

        modelBuilder.Entity<GuaranteeType>().HasData(
            Ref<GuaranteeType>(1, "TENDER", "شرکت در مناقصه", 10),
            Ref<GuaranteeType>(2, "ADVANCE_PAYMENT", "پیش‌پرداخت", 20),
            Ref<GuaranteeType>(3, "PERFORMANCE", "حسن انجام تعهدات", 30),
            Ref<GuaranteeType>(4, "RETENTION", "کسور وجه‌الضمان", 40));

        modelBuilder.Entity<GuaranteeStatus>().HasData(
            Ref<GuaranteeStatus>(1, "ACTIVE", "فعال", 10),
            Ref<GuaranteeStatus>(2, "RELEASED", "آزادشده", 20),
            Ref<GuaranteeStatus>(3, "EXPIRED", "منقضی", 30),
            Ref<GuaranteeStatus>(4, "CLAIMED", "ضبط‌شده", 40));

        modelBuilder.Entity<InvoiceType>().HasData(
            Ref<InvoiceType>(1, "PURCHASE", "خرید", 10),
            Ref<InvoiceType>(2, "SALE", "فروش", 20),
            Ref<InvoiceType>(3, "ADJUSTMENT", "تعدیل", 30),
            Ref<InvoiceType>(4, "PROGRESS", "صورت‌وضعیت", 40));

        modelBuilder.Entity<PaymentStatus>().HasData(
            Ref<PaymentStatus>(1, "UNPAID", "پرداخت‌نشده", 10),
            Ref<PaymentStatus>(2, "PARTIAL", "پرداخت جزئی", 20),
            Ref<PaymentStatus>(3, "PAID", "پرداخت‌شده", 30),
            Ref<PaymentStatus>(4, "CANCELLED", "لغوشده", 40));

        modelBuilder.Entity<DocumentType>().HasData(
            Ref<DocumentType>(1, "CONTRACT", "قرارداد", 10),
            Ref<DocumentType>(2, "AMENDMENT", "الحاقیه", 20),
            Ref<DocumentType>(3, "INVOICE", "صورتحساب", 30),
            Ref<DocumentType>(4, "GUARANTEE", "تضمین", 40),
            Ref<DocumentType>(5, "OTHER", "سایر", 50));

        modelBuilder.Entity<AlertType>().HasData(
            Ref<AlertType>(1, "CONTRACT_EXPIRY", "سررسید قرارداد", 10),
            Ref<AlertType>(2, "GUARANTEE_EXPIRY", "سررسید تضمین", 20),
            Ref<AlertType>(3, "PAYMENT_DUE", "سررسید پرداخت", 30),
            Ref<AlertType>(4, "RECEIPT_DUE", "سررسید دریافت", 40));

        modelBuilder.Entity<AlertStatus>().HasData(
            Ref<AlertStatus>(1, "OPEN", "باز", 10),
            Ref<AlertStatus>(2, "COMPLETED", "انجام‌شده", 20),
            Ref<AlertStatus>(3, "DISMISSED", "نادیده‌گرفته‌شده", 30));

        modelBuilder.Entity<Priority>().HasData(
            Ref<Priority>(1, "LOW", "کم", 10),
            Ref<Priority>(2, "MEDIUM", "متوسط", 20),
            Ref<Priority>(3, "HIGH", "زیاد", 30),
            Ref<Priority>(4, "CRITICAL", "بحرانی", 40));

        modelBuilder.Entity<FinancialTransactionStatus>().HasData(
            Ref<FinancialTransactionStatus>(1, "OPEN", "باز", 10),
            Ref<FinancialTransactionStatus>(2, "PARTIAL", "تسویه جزئی", 20),
            Ref<FinancialTransactionStatus>(3, "SETTLED", "تسویه‌شده", 30),
            Ref<FinancialTransactionStatus>(4, "CANCELLED", "لغوشده", 40));

        modelBuilder.Entity<Bank>().HasData(Banks());
        modelBuilder.Entity<OkrObjective>().HasData(OkrObjectives());
        modelBuilder.Entity<OkrKeyResult>().HasData(OkrKeyResults());
    }

    private static OkrObjective[] OkrObjectives()=>
    [
        new(1,"OBJ-01","مدیریت مؤثر قراردادها","",true,10,SeedStamp),new(2,"OBJ-02","مدیریت مالی قراردادها","",true,20,SeedStamp),
        new(3,"OBJ-03","مدیریت پروژه","",true,30,SeedStamp),new(4,"OBJ-04","مدیریت خرید و برون‌سپاری","",true,40,SeedStamp),
        new(5,"OBJ-05","رضایت مدیرعامل از سامانه","",true,50,SeedStamp)
    ];
    private static OkrKeyResult[] OkrKeyResults()
    {
        (long O,string T,decimal Target,string U,int W,OkrDirection D,string M,OkrDataSourceType S)[] rows=
        [
            (1,"ثبت به‌موقع قراردادها",98,"PERCENT",25,OkrDirection.HigherIsBetter,"CONTRACT_REGISTRATION_ON_TIME",OkrDataSourceType.Manual),
            (1,"زمان ثبت قرارداد",2,"DAY",25,OkrDirection.LowerIsBetter,"CONTRACT_REGISTRATION_LEAD_DAYS",OkrDataSourceType.Manual),
            (1,"قراردادهای بدون نقص",95,"PERCENT",25,OkrDirection.HigherIsBetter,"DEFECT_FREE_CONTRACT_RATE",OkrDataSourceType.Manual),
            (1,"مستندات کامل",100,"PERCENT",25,OkrDirection.HigherIsBetter,"DOCUMENT_COMPLETENESS_RATE",OkrDataSourceType.Manual),
            (2,"مطالبات معوق",5,"PERCENT",30,OkrDirection.LowerIsBetter,"OVERDUE_RECEIVABLE_RATE",OkrDataSourceType.Auto),
            (2,"پرداخت به‌موقع بدهی",95,"PERCENT",30,OkrDirection.HigherIsBetter,"TIMELY_DEBT_PAYMENT_RATE",OkrDataSourceType.Auto),
            (2,"وصول مطالبات",98,"PERCENT",20,OkrDirection.HigherIsBetter,"COLLECTION_RATE",OkrDataSourceType.Auto),
            (2,"مغایرت مالی",0,"COUNT",20,OkrDirection.TargetZero,"FINANCIAL_DISCREPANCY_COUNT",OkrDataSourceType.Manual),
            (3,"پیشرفت مطابق گانت",95,"PERCENT",30,OkrDirection.HigherIsBetter,"GANTT_ON_TRACK_RATE",OkrDataSourceType.Manual),
            (3,"فعالیت تأخیردار",5,"PERCENT",30,OkrDirection.LowerIsBetter,"DELAYED_ACTIVITY_RATE",OkrDataSourceType.Manual),
            (3,"پروژه تحویل‌شده در موعد",90,"PERCENT",20,OkrDirection.HigherIsBetter,"ON_TIME_PROJECT_DELIVERY_RATE",OkrDataSourceType.Manual),
            (3,"انحراف زمانی",10,"PERCENT",20,OkrDirection.LowerIsBetter,"SCHEDULE_VARIANCE_RATE",OkrDataSourceType.Manual),
            (4,"سفارش خرید به‌موقع",95,"PERCENT",25,OkrDirection.HigherIsBetter,"ON_TIME_PURCHASE_ORDER_RATE",OkrDataSourceType.Manual),
            (4,"کاهش زمان تأمین",20,"PERCENT",25,OkrDirection.HigherIsBetter,"SUPPLY_LEAD_TIME_REDUCTION_RATE",OkrDataSourceType.Manual),
            (4,"تحویل به‌موقع پیمانکار",95,"PERCENT",25,OkrDirection.HigherIsBetter,"ON_TIME_CONTRACTOR_DELIVERY_RATE",OkrDataSourceType.Manual),
            (4,"کاهش هزینه خرید",10,"PERCENT",25,OkrDirection.HigherIsBetter,"PURCHASE_COST_REDUCTION_RATE",OkrDataSourceType.Manual),
            (5,"گزارش‌های لحظه‌ای",100,"PERCENT",30,OkrDirection.HigherIsBetter,"REALTIME_REPORT_AVAILABILITY_RATE",OkrDataSourceType.Manual),
            (5,"داشبورد مدیریتی",100,"PERCENT",20,OkrDirection.HigherIsBetter,"EXECUTIVE_DASHBOARD_AVAILABILITY_RATE",OkrDataSourceType.Manual),
            (5,"پاسخگویی سامانه",3,"SECOND",20,OkrDirection.LowerIsBetter,"SYSTEM_RESPONSE_TIME_SECONDS",OkrDataSourceType.Manual),
            (5,"رضایت کاربران",90,"PERCENT",30,OkrDirection.HigherIsBetter,"USER_SATISFACTION_RATE",OkrDataSourceType.Manual)
        ];
        return rows.Select((x,i)=>new OkrKeyResult(i+1,x.O,$"OBJ{x.O:00}-KR{rows.Take(i+1).Count(y=>y.O==x.O):00}",x.T,x.Target,x.U,x.W,x.D,x.M,x.S,x.S==OkrDataSourceType.Auto?"LEDGER_V1":"MANUAL_V1",true,(i%4+1)*10,SeedStamp)).ToArray();
    }

    private static T Ref<T>(long id, string code, string titleFa, int sortOrder)
        where T : ReferenceEntity
    {
        return (T)Activator.CreateInstance(
            typeof(T),
            id,
            code,
            titleFa,
            sortOrder,
            true,
            SeedStamp)!;
    }

    private static Bank[] Banks()
    {
        (string Code, string Title)[] banks =
        [
            ("MELLI", "بانک ملی ایران"),
            ("SEPAH", "بانک سپه"),
            ("POST_BANK", "پست بانک ایران"),
            ("KESHAVARZI", "بانک کشاورزی"),
            ("MASKAN", "بانک مسکن"),
            ("SANAT_MADAN", "بانک صنعت و معدن"),
            ("TOSE_SADERAT", "بانک توسعه صادرات ایران"),
            ("TOSE_TAAVON", "بانک توسعه تعاون"),
            ("MEHR_IRAN", "بانک قرض‌الحسنه مهر ایران"),
            ("RESALAT", "بانک قرض‌الحسنه رسالت"),
            ("EGHTESAD_NOVIN", "بانک اقتصاد نوین"),
            ("KARAFARIN", "بانک کارآفرین"),
            ("PARSIAN", "بانک پارسیان"),
            ("PASARGAD", "بانک پاسارگاد"),
            ("SAMAN", "بانک سامان"),
            ("SARMAYEH", "بانک سرمایه"),
            ("SINA", "بانک سینا"),
            ("SHAHR", "بانک شهر"),
            ("DEY", "بانک دی"),
            ("MELLAT", "بانک ملت"),
            ("REFAH", "بانک رفاه کارگران"),
            ("SADERAT", "بانک صادرات ایران"),
            ("TEJARAT", "بانک تجارت"),
            ("GARDESHGARI", "بانک گردشگری"),
            ("KHAVARMIANEH", "بانک خاورمیانه"),
            ("IRAN_ZAMIN", "بانک ایران زمین"),
            ("AYANDEH", "بانک آینده"),
        ];

        return banks
            .Select((bank, index) => new Bank(index + 1, bank.Code, bank.Title, (index + 1) * 10, true, SeedStamp))
            .ToArray();
    }

    private static RolePermission[] SecurityPermissions()
    {
        string[] sections =
        [
            "DASHBOARD", "CONTRACTS", "PARTIES", "PROJECTS", "PAYMENTS", "RECEIPTS", "FINANCIALLEDGER",
            "GUARANTEES", "AMENDMENTS", "INVOICES", "DOCUMENTS", "ALERTS", "BUDGET", "EVALUATION",
            "REPORTS", "KPI", "OKR", "USERS", "SETTINGS",
        ];
        List<RolePermission> permissions = [];
        long id = 1000;
        permissions.AddRange(sections.Select(section => new RolePermission(++id, 1, section, true, true, true, true, true, true, SeedStamp)));
        string[] chiefExecutiveSections = sections.Where(section => section is not ("USERS" or "SETTINGS")).ToArray();
        permissions.AddRange(chiefExecutiveSections.Select(section => new RolePermission(++id, 3, section, true, false, false, false,
            section is "REPORTS" or "KPI", false, SeedStamp)));
        return permissions.ToArray();
    }
}
