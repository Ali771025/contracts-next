namespace Contracts.Infrastructure;

using Contracts.Infrastructure.Security;
using Contracts.Infrastructure.MasterData;
using Contracts.Application.MasterData;
using Contracts.Application.Contracts;
using Contracts.Infrastructure.ContractManagement;
using Contracts.Application.Finance;
using Contracts.Infrastructure.Finance;
using Contracts.Application.Operations;
using Contracts.Infrastructure.Operations;
using Contracts.Application.Reporting;
using Contracts.Infrastructure.Reporting;
using Contracts.Application.Dashboard;
using Contracts.Infrastructure.Dashboard;
using Contracts.Application.Okr;
using Contracts.Infrastructure.Okr;
using Contracts.Application.Administration;
using Contracts.Infrastructure.Administration;

public static class DependencyInjection
{
    public static IServiceCollection AddContractsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        ApplicationPaths paths=ApplicationPathPolicy.Resolve(configuration);
        string? explicitConnection=configuration.GetConnectionString("Contracts");
        string connectionString;
        if(string.IsNullOrWhiteSpace(explicitConnection))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(paths.DatabasePath)!);Directory.CreateDirectory(paths.DocumentsPath);Directory.CreateDirectory(Path.GetDirectoryName(paths.ConfigurationPath)!);
            ApplicationPathPolicy.EnsurePrivateDirectory(paths.RootPath);
            connectionString=$"Data Source={paths.DatabasePath};Foreign Keys=True;Pooling=False";
        }
        else
        {
            Microsoft.Data.Sqlite.SqliteConnectionStringBuilder parsed=new(explicitConnection);if(parsed.DataSource is not ":memory:")ApplicationPathPolicy.ValidateDatabasePath(parsed.DataSource);connectionString=explicitConnection;
        }
        services.AddSingleton(paths);

        services.AddSingleton<SqliteForeignKeysInterceptor>();
        services.AddDbContextFactory<ContractsDbContext>((provider, options) =>
            options
                .UseSqlite(connectionString)
                .AddInterceptors(provider.GetRequiredService<SqliteForeignKeysInterceptor>()));

        PasswordPolicyOptions securityOptions = ReadSecurityOptions(configuration);
        services.AddSingleton(securityOptions);
        services.AddSingleton<ICurrentUserService, CurrentUserService>();
        services.AddSingleton<ISystemClock, SystemClock>();
        services.AddSingleton<ICorrelationIdProvider, CorrelationIdProvider>();
        services.AddSingleton<IPasswordPolicy, PasswordPolicy>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<SecurityStore>();
        services.AddSingleton<ISecurityStore>(provider => provider.GetRequiredService<SecurityStore>());
        services.AddSingleton<ISecurityAdministrationStore>(provider => provider.GetRequiredService<SecurityStore>());
        services.AddSingleton<IAuditWriter, AuditWriter>();
        services.AddSingleton<IAuthorizationService, AuthorizationService>();
        services.AddSingleton<ISecurityDatabaseInitializer, SecurityDatabaseInitializer>();
        services.AddTransient<LoginUseCase>();
        services.AddTransient<BootstrapAdministratorUseCase>();
        services.AddTransient<ChangePasswordUseCase>();
        services.AddTransient<UpdateRolePermissionUseCase>();
        services.AddTransient<AssignUserRoleUseCase>();
        services.AddTransient<RoleAdministrationService>();
        services.AddSingleton<IMasterDataStore, MasterDataStore>();
        services.AddTransient<PartyCrudService>();
        services.AddTransient<ProjectCrudService>();
        services.AddTransient<LookupAdministrationService>();
        services.AddSingleton<IContractStore, ContractStore>();
        services.AddTransient<ContractService>();
        services.AddSingleton<IBusinessDateService, BusinessDateService>();
        services.AddSingleton<IFinanceStore, FinanceStore>();
        services.AddTransient<FinanceService>();
        DocumentStorageOptions documentOptions = ReadDocumentOptions(configuration,paths);
        services.AddSingleton(documentOptions);
        AlertRuleOptions alertOptions = ReadAlertOptions(configuration);
        services.AddSingleton(alertOptions);
        SystemSettingsState settingsState=new();settingsState.Update(new("شرکت","گزارش سامانه قراردادها",alertOptions.ContractWarningDays,alertOptions.GuaranteeWarningDays,alertOptions.FinancialWarningDays,7,paths.DefaultBackupPath,false,24,10));
        if(string.IsNullOrWhiteSpace(explicitConnection)&&File.Exists(paths.ConfigurationPath))
        {
            SystemSettings? persisted=System.Text.Json.JsonSerializer.Deserialize<SystemSettings>(File.ReadAllText(paths.ConfigurationPath));
            if(persisted is not null){persisted.Validate();settingsState.Update(persisted);alertOptions.ContractWarningDays=persisted.ContractWarningDays;alertOptions.GuaranteeWarningDays=persisted.GuaranteeWarningDays;alertOptions.FinancialWarningDays=persisted.FinancialWarningDays;}
        }
        services.AddSingleton(settingsState);
        services.AddSingleton<IOperationsStore, OperationsStore>();
        services.AddTransient<OperationsService>();
        services.AddSingleton<IOperationalReportSource,OperationalReportSource>();
        services.AddSingleton<IReportExporter,CsvReportExporter>();
        services.AddSingleton<IReportExporter,ClosedXmlReportExporter>();
        services.AddTransient<ReportingQueryService>();
        services.AddTransient<ReportingExportService>();
        services.AddSingleton<IDashboardStore,DashboardStore>();
        services.AddTransient<IDashboardQueryService,DashboardService>();
        services.AddSingleton<IOkrStore,OkrStore>();
        services.AddTransient<OkrService>();
        services.AddSingleton<IOperationalStore,OperationalStore>();
        services.AddSingleton<IRestoreFaultInjector,NoRestoreFaultInjector>();
        services.AddTransient<OperationalService>();
        if(string.IsNullOrWhiteSpace(explicitConnection))services.AddHostedService<AutoBackupHostedService>();

        return services;
    }

    private static DocumentStorageOptions ReadDocumentOptions(IConfiguration configuration,ApplicationPaths paths)
    {
        DocumentStorageOptions options=new();IConfigurationSection section=configuration.GetSection("Documents");
        options.RootPath=paths.DocumentsPath;
        if(!string.IsNullOrWhiteSpace(section["RootPath"]))throw new InvalidOperationException("Documents:RootPath is obsolete; configure the canonical Paths:Root instead.");
        if(long.TryParse(section["MaxFileSizeBytes"],CultureInfo.InvariantCulture,out long max))options.MaxFileSizeBytes=max;
        string[] extensions=section.GetSection("AllowedExtensions").GetChildren().Select(x=>x.Value).Where(x=>!string.IsNullOrWhiteSpace(x)).Cast<string>().ToArray();
        if(extensions.Length>0)options.AllowedExtensions=extensions.Select(x=>x.StartsWith('.')?x:"."+x).ToHashSet(StringComparer.OrdinalIgnoreCase);
        if(options.MaxFileSizeBytes<=0||options.AllowedExtensions.Count==0)throw new InvalidOperationException("Document policy is invalid.");return options;
    }

    private static AlertRuleOptions ReadAlertOptions(IConfiguration configuration)
    {
        AlertRuleOptions options=new();IConfigurationSection s=configuration.GetSection("Alerts");
        if(int.TryParse(s["ContractWarningDays"],out int c))options.ContractWarningDays=c;if(int.TryParse(s["GuaranteeWarningDays"],out int g))options.GuaranteeWarningDays=g;if(int.TryParse(s["FinancialWarningDays"],out int f))options.FinancialWarningDays=f;return options;
    }

    private static PasswordPolicyOptions ReadSecurityOptions(IConfiguration configuration)
    {
        PasswordPolicyOptions options = new();
        IConfigurationSection section = configuration.GetSection("Security:PasswordPolicy");
        if (int.TryParse(section["MinimumLength"], CultureInfo.InvariantCulture, out int minimumLength)) options.MinimumLength = minimumLength;
        if (int.TryParse(section["Pbkdf2Iterations"], CultureInfo.InvariantCulture, out int iterations)) options.Pbkdf2Iterations = iterations;
        if (int.TryParse(section["LockoutThreshold"], CultureInfo.InvariantCulture, out int threshold)) options.LockoutThreshold = threshold;
        if (int.TryParse(section["LockoutMinutes"], CultureInfo.InvariantCulture, out int minutes)) options.LockoutMinutes = minutes;
        if (bool.TryParse(section["RequireUppercase"], out bool uppercase)) options.RequireUppercase = uppercase;
        if (bool.TryParse(section["RequireLowercase"], out bool lowercase)) options.RequireLowercase = lowercase;
        if (bool.TryParse(section["RequireDigit"], out bool digit)) options.RequireDigit = digit;
        if (bool.TryParse(section["RequireSymbol"], out bool symbol)) options.RequireSymbol = symbol;
        options.BannedPasswordSha256 = section.GetSection("BannedPasswordSha256").GetChildren()
            .Select(item => item.Value).Where(item => !string.IsNullOrWhiteSpace(item)).Cast<string>().ToList();
        if (options.MinimumLength < 12 || options.Pbkdf2Iterations < 100_000 || options.LockoutThreshold <= 0 || options.LockoutMinutes <= 0)
            throw new InvalidOperationException("Security password policy configuration is below the supported minimum.");
        return options;
    }
}
