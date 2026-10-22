namespace Contracts.Desktop.Navigation;

using Contracts.Application.Presentation;

public sealed class DesktopNavigationTargetFactory(IServiceProvider services) : INavigationTargetFactory
{
    private static readonly Dictionary<string, (string Title, string Description)> Routes =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
        {
            ["dashboard"] = ("داشبورد", "نمای مدیریتی سامانه"), ["contracts"] = ("قراردادها", "مدیریت قراردادها"),
            ["parties"] = ("طرف‌های قرارداد", "اشخاص و سازمان‌ها"), ["projects"] = ("پروژه‌ها", "مدیریت پروژه‌ها"),
            ["payments"] = ("پرداخت‌ها", "پرداخت‌های قرارداد"), ["receipts"] = ("دریافت‌ها", "دریافت‌های قرارداد"),
            ["ledger"] = ("دفتر مالی", "تعهدات مالی"), ["guarantees"] = ("تضمین‌ها", "ضمانت‌نامه‌ها"),
            ["amendments"] = ("الحاقیه‌ها", "تغییرات قرارداد"), ["invoices"] = ("صورتحساب‌ها", "صورتحساب‌های قرارداد"),
            ["documents"] = ("اسناد", "پیوست‌ها و مستندات"), ["alerts"] = ("هشدارها", "هشدارها و سررسیدها"),
            ["budget"] = ("بودجه", "بودجه پروژه‌ها"), ["evaluation"] = ("ارزیابی", "ارزیابی پیمانکاران"),
            ["reports"] = ("گزارش‌ها", "گزارش‌های مدیریتی"), ["kpi"] = ("شاخص‌های کلیدی", "پایش KPI"),
            ["okr"] = ("اهداف و نتایج", "فضای رزروشده برای فاز P13"), ["users"] = ("کاربران", "مدیریت کاربران"),
            ["settings"] = ("تنظیمات", "تنظیمات سامانه"),
        };

    public INavigationTarget Create(NavigationRoute route)
    {
        if (route.Key == "users") return services.GetRequiredService<Security.RoleAdministrationViewModel>();
        if (route.Key == "parties") return services.GetRequiredService<MasterData.PartyPageViewModel>();
        if (route.Key == "projects") return services.GetRequiredService<MasterData.ProjectPageViewModel>();
        if (route.Key == "settings") return services.GetRequiredService<Administration.AdministrationPageViewModel>();
        if (route.Key == "contracts") return services.GetRequiredService<ContractManagement.ContractPageViewModel>();
        if (route.Key is "reports" or "kpi" or "okr")
        {
            Reporting.ReportPageViewModel reports=services.GetRequiredService<Reporting.ReportPageViewModel>();
            if(route.Key=="kpi")reports.ActivateKpi();else if(route.Key=="okr")reports.ActivateOkr();return reports;
        }
        if (route.Key == "dashboard")
        {
            Dashboard.DashboardPageViewModel dashboard = services.GetRequiredService<Dashboard.DashboardPageViewModel>();
            dashboard.ActivateRoute(route.Key); return dashboard;
        }
        if (route.Key is "ledger" or "payments" or "receipts")
        {
            Finance.FinancePageViewModel finance = services.GetRequiredService<Finance.FinancePageViewModel>();
            finance.ActivateRoute(route.Key); return finance;
        }
        if (route.Key is "amendments" or "guarantees" or "invoices" or "documents" or "alerts" or "evaluation")
        {
            Operations.OperationsPageViewModel operations = services.GetRequiredService<Operations.OperationsPageViewModel>();
            operations.ActivateRoute(route.Key); return operations;
        }
        if (!Routes.TryGetValue(route.Key, out (string Title, string Description) definition))
            throw new InvalidOperationException($"Navigation route '{route.Key}' is not registered.");
        return new SectionPageViewModel(definition.Title, definition.Description);
    }
}

public sealed class SectionPageViewModel(string title, string description) : ObservableObject, INavigationTarget
{
    public string Title { get; } = title;
    public string Description { get; } = description;
    public Task OnNavigatedToAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
