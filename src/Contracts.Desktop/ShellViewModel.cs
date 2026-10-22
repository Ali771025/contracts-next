namespace Contracts.Desktop;

using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;
using Contracts.Application.Presentation;

public sealed class MenuItemViewModel : ObservableObject
{
    private bool isVisible;
    public MenuItemViewModel(string title, NavigationRoute route, Func<MenuItemViewModel, CancellationToken, Task> navigate)
    {
        Title = title; Route = route;
        NavigateCommand = new AsyncRelayCommand(ct => navigate(this, ct));
    }
    public string Title { get; }
    public NavigationRoute Route { get; }
    public bool IsVisible { get => isVisible; set => SetProperty(ref isVisible, value); }
    public IAsyncRelayCommand NavigateCommand { get; }
}

public sealed class ShellViewModel : ObservableObject, IDisposable
{
    private readonly INavigationService navigation;
    private readonly IAuthorizationService authorization;
    private readonly ICurrentUserService currentUser;
    private readonly IPersianDateService persianDate;
    private readonly string title = "سامانه مدیریت قراردادها";
    private CancellationTokenSource? navigationCancellation;
    private object? currentContent;
    private bool isBusy;
    private string? errorMessage;

    public ShellViewModel(INavigationService navigation, IAuthorizationService authorization,
        ICurrentUserService currentUser, IPersianDateService persianDate)
    {
        this.navigation = navigation;
        this.authorization = authorization;
        this.currentUser = currentUser;
        this.persianDate = persianDate;
        MenuItems = new(MenuDefinitions.Select(item => new MenuItemViewModel(item.Title,
            new(item.Route, item.Section), NavigateAsync)));
        LogoutCommand = new RelayCommand(Logout);
        BackCommand = new AsyncRelayCommand(GoBackAsync);
        navigation.CurrentChanged += HandleCurrentChanged;
    }

    public string Title => title;
    public string CurrentUserName => currentUser.Current?.DisplayName ?? "—";
    public string CurrentDate => persianDate.Format(DateOnly.FromDateTime(DateTime.Today));
    public ObservableCollection<MenuItemViewModel> MenuItems { get; }
    public object? CurrentContent { get => currentContent; private set => SetProperty(ref currentContent, value); }
    public bool IsBusy { get => isBusy; private set => SetProperty(ref isBusy, value); }
    public string? ErrorMessage
    {
        get => errorMessage;
        private set { if (SetProperty(ref errorMessage, value)) OnPropertyChanged(nameof(HasError)); }
    }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public IRelayCommand LogoutCommand { get; }
    public IAsyncRelayCommand BackCommand { get; }
    public event EventHandler? LogoutRequested;

    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        OnPropertyChanged(nameof(CurrentUserName));
        foreach (MenuItemViewModel item in MenuItems)
            item.IsVisible = await authorization.IsAllowedAsync(item.Route.Section, item.Route.Action, cancellationToken);
        MenuItemViewModel? initial = MenuItems.FirstOrDefault(item => item.IsVisible && item.Route.Section == SecuritySection.Dashboard)
            ?? MenuItems.FirstOrDefault(item => item.IsVisible);
        if (initial is not null) await NavigateAsync(initial, cancellationToken);
    }

    private async Task NavigateAsync(MenuItemViewModel item, CancellationToken cancellationToken)
    {
        navigationCancellation?.Cancel();
        navigationCancellation?.Dispose();
        navigationCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        IsBusy = true; ErrorMessage = null;
        try { await navigation.NavigateAsync(item.Route, navigationCancellation.Token); }
        catch (OperationCanceledException) when (navigationCancellation.IsCancellationRequested) { }
        catch (UnauthorizedAccessException) { ErrorMessage = "دسترسی شما به این بخش مجاز نیست."; }
        catch (Exception) { ErrorMessage = "نمایش این بخش ناموفق بود. دوباره تلاش کنید."; }
        finally { IsBusy = false; }
    }

    private async Task GoBackAsync(CancellationToken cancellationToken)
    {
        IsBusy = true; ErrorMessage = null;
        try { await navigation.GoBackAsync(cancellationToken); }
        catch (UnauthorizedAccessException) { ErrorMessage = "دسترسی به مسیر قبلی دیگر مجاز نیست."; }
        finally { IsBusy = false; }
    }

    private void HandleCurrentChanged(object? sender, EventArgs e)
    {
        CurrentContent = navigation.Current;
        BackCommand.NotifyCanExecuteChanged();
    }

    private void Logout() { currentUser.Clear(); LogoutRequested?.Invoke(this, EventArgs.Empty); }
    public void Dispose()
    {
        navigation.CurrentChanged -= HandleCurrentChanged;
        navigationCancellation?.Cancel(); navigationCancellation?.Dispose();
    }

    private static readonly (string Title, string Route, SecuritySection Section)[] MenuDefinitions =
    [
        ("داشبورد", "dashboard", SecuritySection.Dashboard), ("قراردادها", "contracts", SecuritySection.Contracts),
        ("طرف‌های قرارداد", "parties", SecuritySection.Parties), ("پروژه‌ها", "projects", SecuritySection.Projects),
        ("پرداخت‌ها", "payments", SecuritySection.Payments), ("دریافت‌ها", "receipts", SecuritySection.Receipts),
        ("دفتر مالی", "ledger", SecuritySection.FinancialLedger), ("تضمین‌ها", "guarantees", SecuritySection.Guarantees),
        ("الحاقیه‌ها", "amendments", SecuritySection.Amendments), ("صورتحساب‌ها", "invoices", SecuritySection.Invoices),
        ("اسناد", "documents", SecuritySection.Documents), ("هشدارها", "alerts", SecuritySection.Alerts),
        ("بودجه", "budget", SecuritySection.Budget), ("ارزیابی", "evaluation", SecuritySection.Evaluation),
        ("گزارش‌ها", "reports", SecuritySection.Reports), ("شاخص‌های کلیدی", "kpi", SecuritySection.Kpi),
        ("اهداف و نتایج", "okr", SecuritySection.Okr), ("کاربران", "users", SecuritySection.Users),
        ("تنظیمات", "settings", SecuritySection.Settings),
    ];
}
