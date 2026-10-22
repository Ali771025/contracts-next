namespace Contracts.Desktop;

public partial class App : System.Windows.Application
{
    private static readonly Action<ILogger, string, bool, string, Exception?> LogStartup = LoggerMessage.Define<string, bool, string>(
        LogLevel.Information, new EventId(1600, "ReleaseStartup"),
        "ContractsNext {InformationalVersion} started; WebView2 available={Available}, runtime={RuntimeVersion}.");
    private IHost? host;
    private string? visualSmokeOutput;
    private string? routeSmokeOutput;
    private bool reportPdfSmoke;

    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            HostApplicationBuilder builder = Host.CreateApplicationBuilder(
                new HostApplicationBuilderSettings
                {
                    Args = e.Args,
                    ContentRootPath = AppContext.BaseDirectory,
                });

            builder.Logging.SetMinimumLevel(LogLevel.Information);
            Contracts.Infrastructure.Administration.ApplicationPaths runtimePaths =
                Contracts.Infrastructure.Administration.ApplicationPathPolicy.Resolve(builder.Configuration);
            routeSmokeOutput=e.Args.FirstOrDefault(argument=>argument.StartsWith("--ui-route-smoke=",StringComparison.Ordinal))?.Split('=',2)[1];
            bool routeSmokeRootWasClean=!Directory.Exists(runtimePaths.RootPath)||!Directory.EnumerateFileSystemEntries(runtimePaths.RootPath).Any();
            if(routeSmokeOutput is not null)ValidateRouteSmokePaths(builder.Configuration["Paths:Root"],runtimePaths.RootPath,routeSmokeOutput);
            Environment.SetEnvironmentVariable("WEBVIEW2_USER_DATA_FOLDER", Path.Combine(runtimePaths.RootPath, "WebView2"), EnvironmentVariableTarget.Process);
            builder.Services.AddContractsInfrastructure(builder.Configuration);
            ReleaseIdentity releaseIdentity = ReleaseIdentity.Current();
            WebView2RuntimeStatus webView2Status = WebView2RuntimeStatus.Probe();
            builder.Services.AddSingleton(releaseIdentity);
            builder.Services.AddSingleton(webView2Status);
            bool usePersianDigits = !bool.TryParse(builder.Configuration["Culture:UsePersianDigits"], out bool configuredDigits) || configuredDigits;
            builder.Services.AddSingleton(new PersianDateOptions { UsePersianDigits = usePersianDigits });
            builder.Services.AddSingleton<IPersianDateService, PersianDateService>();
            builder.Services.AddSingleton<INumberDisplayService, NumberDisplayService>();
            builder.Services.AddSingleton<INavigationTargetFactory, DesktopNavigationTargetFactory>();
            builder.Services.AddSingleton<INavigationService, NavigationService>();
            builder.Services.AddTransient<IDirtyDecisionService, Controls.WpfDirtyDecisionService>();
            builder.Services.AddTransient<IConfirmationService, Controls.WpfConfirmationService>();
            builder.Services.AddTransient<MasterData.PartyPageViewModel>();
            builder.Services.AddTransient<MasterData.ProjectPageViewModel>();
            builder.Services.AddTransient<MasterData.LookupAdministrationViewModel>();
            builder.Services.AddSingleton<Administration.IBackupPackagePicker,Administration.BackupPackagePicker>();
            builder.Services.AddTransient<Administration.AdministrationPageViewModel>();
            builder.Services.AddTransient<Security.RoleAdministrationViewModel>();
            builder.Services.AddTransient<ContractManagement.ContractPageViewModel>();
            builder.Services.AddTransient<Finance.FinancePageViewModel>();
            builder.Services.AddSingleton<Operations.IFilePickerService, Operations.WpfFilePickerService>();
            builder.Services.AddTransient<Operations.OperationsPageViewModel>();
            builder.Services.AddSingleton<Reporting.ReportHtmlRenderer>();
            builder.Services.AddSingleton<Reporting.IReportFileDialogService,Reporting.ReportFileDialogService>();
            builder.Services.AddTransient<Reporting.WebView2ReportExporter>();
            builder.Services.AddTransient<IReportExporter>(provider=>provider.GetRequiredService<Reporting.WebView2ReportExporter>());
            builder.Services.AddTransient<Reporting.ReportPageViewModel>();
            builder.Services.AddSingleton<Dashboard.DashboardHtmlRenderer>();
            builder.Services.AddTransient<Dashboard.DashboardPageViewModel>();
            builder.Services.AddTransient<Okr.OkrPageViewModel>();
            builder.Services.AddSingleton<ShellViewModel>();
            builder.Services.AddSingleton<MainWindow>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<LoginWindow>();
            builder.Services.AddTransient<BootstrapViewModel>();
            builder.Services.AddTransient<BootstrapWindow>();
            builder.Services.AddTransient<ChangePasswordViewModel>();
            builder.Services.AddTransient<ChangePasswordWindow>();

            host = builder.Build();
            await host.StartAsync().ConfigureAwait(true);
            ILogger logger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("ContractsNext.Startup");
            LogStartup(logger, releaseIdentity.InformationalVersion, webView2Status.IsAvailable, webView2Status.RuntimeVersion ?? "unavailable", null);

            if (e.Args.Contains("--first-run-probe", StringComparer.Ordinal))
            {
                await host.Services.GetRequiredService<ISecurityDatabaseInitializer>().InitializeAsync(CancellationToken.None);
                bool bootstrapRequired = await host.Services.GetRequiredService<BootstrapAdministratorUseCase>().IsRequiredAsync(CancellationToken.None);
                Shutdown(bootstrapRequired ? 0 : 2);
                return;
            }

            string? pdfSmokeOutput=e.Args.FirstOrDefault(argument=>argument.StartsWith("--report-pdf-smoke=",StringComparison.Ordinal))?.Split('=',2)[1];
            reportPdfSmoke=pdfSmokeOutput is not null;
            if(pdfSmokeOutput is not null)
            {
                ReportDocument empty=new(Guid.NewGuid(),DateTimeOffset.UtcNow,DateOnly.FromDateTime(DateTime.Today),ReportKind.GeneralContracts,"آزمون تولید PDF","بدون داده؛ آزمون فنی WebView2",[new("status","وضعیت",ReportValueKind.Text)],[],new([new("۰ ردیف")]));
                await host.Services.GetRequiredService<Reporting.WebView2ReportExporter>().ExportAsync(empty,pdfSmokeOutput,CancellationToken.None);
                Shutdown();return;
            }

            bool bootProbe = e.Args.Contains("--boot-probe", StringComparer.Ordinal);
            visualSmokeOutput = e.Args.FirstOrDefault(argument => argument.StartsWith("--visual-smoke-output=", StringComparison.Ordinal))
                ?.Split('=', 2)[1];
            if(routeSmokeOutput is not null)await PrepareRouteSmokeSessionAsync(routeSmokeRootWasClean);
            else if (!bootProbe && !await AuthenticateAsync())
            {
                Shutdown(); return;
            }

            ShellViewModel shell = host.Services.GetRequiredService<ShellViewModel>();
            await shell.InitializeAsync(CancellationToken.None);
            MainWindow window = host.Services.GetRequiredService<MainWindow>();
            MainWindow = window;
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            if(routeSmokeOutput is not null)window.ContentRendered+=HandleRouteSmokeCompleted;
            else if (visualSmokeOutput is not null) window.ContentRendered += HandleVisualSmokeCompleted;
            else if (bootProbe) window.ContentRendered += HandleBootProbeCompleted;
            else window.LogoutRequested += HandleLogoutRequested;
            window.Show();
        }
        catch (Exception exception)
        {
            string logPath = StartupFailureReporter.Report(exception, host?.Services);
            if(!reportPdfSmoke)MessageBox.Show(
                $"راه‌اندازی برنامه ناموفق بود. جزئیات در این مسیر ثبت شد:{Environment.NewLine}{logPath}",
                "ContractsNext",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        if (host is not null)
        {
            try
            {
                await host.StopAsync(TimeSpan.FromSeconds(5)).ConfigureAwait(true);
            }
            finally
            {
                host.Dispose();
            }
        }

        base.OnExit(e);
    }

    private void HandleBootProbeCompleted(object? sender, EventArgs e)
    {
        if (sender is Window window)
        {
            window.ContentRendered -= HandleBootProbeCompleted;
        }

        Shutdown();
    }

    private void HandleVisualSmokeCompleted(object? sender, EventArgs e)
    {
        if (sender is not Window window || visualSmokeOutput is null) { Shutdown(2); return; }
        window.ContentRendered -= HandleVisualSmokeCompleted;
        VisualSmokeCapture.Capture(window, visualSmokeOutput);
        Shutdown();
    }

    private async void HandleRouteSmokeCompleted(object? sender,EventArgs e)
    {
        if(sender is not Window window||routeSmokeOutput is null){Shutdown(2);return;}
        window.ContentRendered-=HandleRouteSmokeCompleted;
        try
        {
            bool passed=await UiRouteSmokeRunner.RunAsync(window,host!.Services.GetRequiredService<ShellViewModel>(),routeSmokeOutput);
            Shutdown(passed?0:3);
        }
        catch(Exception exception)
        {
            StartupFailureReporter.Report(exception,host?.Services);Shutdown(3);
        }
    }

    private async Task PrepareRouteSmokeSessionAsync(bool rootWasClean)
    {
        if(host is null)throw new InvalidOperationException("Desktop host is unavailable.");
        if(!rootWasClean)throw new InvalidOperationException("UI route smoke requires a new empty data root.");
        await host.Services.GetRequiredService<ISecurityDatabaseInitializer>().InitializeAsync(CancellationToken.None);
        BootstrapAdministratorUseCase bootstrap=host.Services.GetRequiredService<BootstrapAdministratorUseCase>();
        if(!await bootstrap.IsRequiredAsync(CancellationToken.None))throw new InvalidOperationException("UI route smoke refuses a database containing users.");
        string suffix=Guid.NewGuid().ToString("N");string username="ui-smoke-"+suffix;string password="Aa!9"+Guid.NewGuid().ToString("N");
        BootstrapResult created=await bootstrap.ExecuteAsync(new(username,"UI Route Smoke",password),CancellationToken.None);
        if(!created.Created)throw new InvalidOperationException("UI route smoke administrator creation failed: "+string.Join("; ",created.Errors));
        LoginResult login=await host.Services.GetRequiredService<LoginUseCase>().ExecuteAsync(username,password,CancellationToken.None);
        if(login.Status!=LoginStatus.Success)throw new InvalidOperationException("UI route smoke login failed.");
    }

    private static void ValidateRouteSmokePaths(string? configuredRoot,string resolvedRoot,string outputPath)
    {
        if(string.IsNullOrWhiteSpace(configuredRoot)||!Path.IsPathFullyQualified(configuredRoot))
            throw new InvalidOperationException("UI route smoke requires an explicit absolute Paths:Root.");
        if(!Path.IsPathFullyQualified(outputPath))throw new InvalidOperationException("UI route smoke output must be absolute.");
        string root=Path.GetFullPath(resolvedRoot).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
        string output=Path.GetFullPath(outputPath);
        if(!output.StartsWith(root,StringComparison.OrdinalIgnoreCase))throw new InvalidOperationException("UI route smoke output must remain inside its isolated data root.");
    }

    private async Task<bool> AuthenticateAsync()
    {
        if (host is null) return false;
        await host.Services.GetRequiredService<ISecurityDatabaseInitializer>().InitializeAsync(CancellationToken.None);
        BootstrapAdministratorUseCase bootstrap = host.Services.GetRequiredService<BootstrapAdministratorUseCase>();
        if (await bootstrap.IsRequiredAsync(CancellationToken.None) &&
            host.Services.GetRequiredService<BootstrapWindow>().ShowDialog() != true) return false;
        LoginWindow loginWindow = host.Services.GetRequiredService<LoginWindow>();
        if (loginWindow.ShowDialog() != true) return false;
        if (loginWindow.CompletionStatus != LoginStatus.PasswordChangeRequired) return true;
        if (host.Services.GetRequiredService<ChangePasswordWindow>().ShowDialog() == true) return true;
        host.Services.GetRequiredService<ICurrentUserService>().Clear();
        return false;
    }

    private async void HandleLogoutRequested(object? sender, EventArgs e)
    {
        if (sender is not MainWindow window || host is null) return;
        window.Hide();
        if (!await AuthenticateAsync()) { window.Close(); return; }
        await host.Services.GetRequiredService<ShellViewModel>().InitializeAsync(CancellationToken.None);
        window.Show();
    }
}
