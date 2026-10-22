namespace Contracts.Desktop.Security;

public sealed class RoleAdministrationViewModel : ObservableObject, INavigationTarget
{
    private readonly RoleAdministrationService service;
    private readonly string pageTitle = "مدیریت نقش‌ها و دسترسی‌ها";
    private RoleAdministrationItem? selectedRole;
    private UserRoleAdministrationItem? selectedUser;
    private RoleAdministrationItem? assignmentRole;
    private string code = "", titleFa = "", message = "";
    private int sortOrder;
    private bool isActive = true, isBusy;
    private string? errorMessage;

    public RoleAdministrationViewModel(RoleAdministrationService service)
    {
        this.service = service;
        RefreshCommand = new AsyncRelayCommand(RefreshAsync);
        NewRoleCommand = new RelayCommand(NewRole);
        SaveRoleCommand = new AsyncRelayCommand(SaveRoleAsync);
        SavePermissionsCommand = new AsyncRelayCommand(SavePermissionsAsync);
        AssignRoleCommand = new AsyncRelayCommand(AssignRoleAsync);
    }

    public string Title => pageTitle;
    public ObservableCollection<RoleAdministrationItem> Roles { get; } = [];
    public ObservableCollection<UserRoleAdministrationItem> Users { get; } = [];
    public ObservableCollection<PermissionEditorRow> Permissions { get; } = [];
    public RoleAdministrationItem? SelectedRole { get => selectedRole; set { if (SetProperty(ref selectedRole, value)) _ = SelectRoleAsync(value); } }
    public UserRoleAdministrationItem? SelectedUser { get => selectedUser; set { if (SetProperty(ref selectedUser, value)) AssignmentRole = value is null ? null : Roles.FirstOrDefault(role => role.Id == value.RoleId); } }
    public RoleAdministrationItem? AssignmentRole { get => assignmentRole; set => SetProperty(ref assignmentRole, value); }
    public string Code { get => code; set => SetProperty(ref code, value); }
    public string TitleFa { get => titleFa; set => SetProperty(ref titleFa, value); }
    public int SortOrder { get => sortOrder; set => SetProperty(ref sortOrder, value); }
    public bool IsActive { get => isActive; set => SetProperty(ref isActive, value); }
    public bool IsExistingRole => SelectedRole is not null;
    public bool IsSystemAdministrator => SelectedRole?.Code == "SYSTEM_ADMIN";
    public bool CanEditPermissions => IsExistingRole && !IsSystemAdministrator && IsActive;
    public bool IsBusy { get => isBusy; private set => SetProperty(ref isBusy, value); }
    public string? ErrorMessage { get => errorMessage; private set { if (SetProperty(ref errorMessage, value)) OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public string Message { get => message; private set => SetProperty(ref message, value); }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IRelayCommand NewRoleCommand { get; }
    public IAsyncRelayCommand SaveRoleCommand { get; }
    public IAsyncRelayCommand SavePermissionsCommand { get; }
    public IAsyncRelayCommand AssignRoleCommand { get; }

    public Task OnNavigatedToAsync(CancellationToken cancellationToken) => RefreshAsync(cancellationToken);

    private async Task RefreshAsync(CancellationToken cancellationToken) => await RunAsync(async () =>
    {
        long? roleId = SelectedRole?.Id; long? userId = SelectedUser?.Id;
        var result = await service.LoadAsync(cancellationToken);
        Roles.Clear(); foreach (RoleAdministrationItem role in result.Roles) Roles.Add(role);
        Users.Clear(); foreach (UserRoleAdministrationItem user in result.Users) Users.Add(user);
        SelectedRole = roleId is null ? Roles.FirstOrDefault() : Roles.FirstOrDefault(item => item.Id == roleId);
        SelectedUser = userId is null ? Users.FirstOrDefault() : Users.FirstOrDefault(item => item.Id == userId);
    });

    private async Task SelectRoleAsync(RoleAdministrationItem? role)
    {
        if (role is null) return;
        Code = role.Code; TitleFa = role.TitleFa; SortOrder = role.SortOrder; IsActive = role.IsActive;
        OnPropertyChanged(nameof(IsExistingRole)); OnPropertyChanged(nameof(IsSystemAdministrator)); OnPropertyChanged(nameof(CanEditPermissions));
        await RunAsync(async () =>
        {
            IReadOnlyList<RolePermissionItem> rows = await service.GetPermissionsAsync(role.Id, CancellationToken.None);
            Permissions.Clear(); foreach (RolePermissionItem row in rows) Permissions.Add(new(row));
        });
    }

    private void NewRole()
    {
        SelectedRole = null; Code = TitleFa = ""; SortOrder = Roles.Count == 0 ? 10 : Roles.Max(item => item.SortOrder) + 10; IsActive = true;
        Permissions.Clear(); Message = ""; ErrorMessage = null;
        OnPropertyChanged(nameof(IsExistingRole)); OnPropertyChanged(nameof(IsSystemAdministrator)); OnPropertyChanged(nameof(CanEditPermissions));
    }

    private async Task SaveRoleAsync(CancellationToken cancellationToken) => await RunAsync(async () =>
    {
        long id;
        if (SelectedRole is null) id = await service.CreateRoleAsync(new(Code, TitleFa, SortOrder, IsActive), cancellationToken);
        else { id = SelectedRole.Id; await service.UpdateRoleAsync(new(id, SelectedRole.ConcurrencyVersion, TitleFa, SortOrder, IsActive), cancellationToken); }
        Message = "اطلاعات نقش ذخیره شد."; await RefreshAsync(cancellationToken); SelectedRole = Roles.Single(item => item.Id == id);
    });

    private async Task SavePermissionsAsync(CancellationToken cancellationToken)
    {
        if (SelectedRole is null) return;
        await RunAsync(async () =>
        {
            await service.ReplacePermissionsAsync(new(SelectedRole.Id, SelectedRole.ConcurrencyVersion, Permissions.Select(item => item.ToDto()).ToArray()), cancellationToken);
            Message = "ماتریس دسترسی نقش ذخیره شد. نشست کاربران در بررسی بعدی مجوز، سیاست تازه را دریافت می‌کند.";
            await RefreshAsync(cancellationToken);
        });
    }

    private async Task AssignRoleAsync(CancellationToken cancellationToken)
    {
        if (SelectedUser is null || AssignmentRole is null) { ErrorMessage = "یک کاربر و یک نقش فعال را انتخاب کنید."; return; }
        await RunAsync(async () => { await service.AssignRoleAsync(SelectedUser.Id, AssignmentRole.Id, cancellationToken); Message = "نقش کاربر تغییر کرد."; await RefreshAsync(cancellationToken); });
    }

    private async Task RunAsync(Func<Task> action)
    {
        IsBusy = true; ErrorMessage = null;
        try { await action(); }
        catch (SecurityAdministrationValidationException exception) { ErrorMessage = exception.Message; }
        catch (SecurityAdministrationConflictException exception) { ErrorMessage = exception.Message; }
        catch (UnauthorizedAccessException) { ErrorMessage = "فقط مدیر سیستم اجازه مدیریت نقش‌ها و دسترسی‌ها را دارد."; }
        catch (Exception) { ErrorMessage = "عملیات ناموفق بود. جزئیات در گزارش برنامه ثبت شده است."; }
        finally { IsBusy = false; }
    }
}

public sealed class PermissionEditorRow : ObservableObject
{
    private bool canView, canAdd, canEdit, canDelete, canExport, canAdmin;
    public PermissionEditorRow(RolePermissionItem item)
    {
        Section = item.Section; TitleFa = SectionTitles[item.Section];
        canView = item.Actions.Contains(SecurityAction.View); canAdd = item.Actions.Contains(SecurityAction.Add);
        canEdit = item.Actions.Contains(SecurityAction.Edit); canDelete = item.Actions.Contains(SecurityAction.Delete);
        canExport = item.Actions.Contains(SecurityAction.Export); canAdmin = item.Actions.Contains(SecurityAction.Admin);
    }
    public SecuritySection Section { get; }
    public string TitleFa { get; }
    public bool CanView { get => canView; set => SetProperty(ref canView, value); }
    public bool CanAdd { get => canAdd; set => SetAction(ref canAdd, value); }
    public bool CanEdit { get => canEdit; set => SetAction(ref canEdit, value); }
    public bool CanDelete { get => canDelete; set => SetAction(ref canDelete, value); }
    public bool CanExport { get => canExport; set => SetAction(ref canExport, value); }
    public bool CanAdmin { get => canAdmin; set => SetAction(ref canAdmin, value); }
    private void SetAction(ref bool field, bool value) { if (SetProperty(ref field, value) && value) CanView = true; }
    public RolePermissionItem ToDto()
    {
        HashSet<SecurityAction> actions = [];
        if (CanView) actions.Add(SecurityAction.View); if (CanAdd) actions.Add(SecurityAction.Add); if (CanEdit) actions.Add(SecurityAction.Edit);
        if (CanDelete) actions.Add(SecurityAction.Delete); if (CanExport) actions.Add(SecurityAction.Export); if (CanAdmin) actions.Add(SecurityAction.Admin);
        return new(Section, actions);
    }
    private static readonly Dictionary<SecuritySection, string> SectionTitles = new()
    {
        [SecuritySection.Dashboard]="داشبورد", [SecuritySection.Contracts]="قراردادها", [SecuritySection.Parties]="طرف‌های قرارداد", [SecuritySection.Projects]="پروژه‌ها",
        [SecuritySection.Payments]="پرداخت‌ها", [SecuritySection.Receipts]="دریافت‌ها", [SecuritySection.FinancialLedger]="دفتر مالی", [SecuritySection.Guarantees]="ضمانت‌نامه‌ها",
        [SecuritySection.Amendments]="الحاقیه‌ها", [SecuritySection.Invoices]="صورت‌حساب‌ها", [SecuritySection.Documents]="اسناد", [SecuritySection.Alerts]="هشدارها",
        [SecuritySection.Budget]="بودجه", [SecuritySection.Evaluation]="ارزیابی", [SecuritySection.Reports]="گزارش‌ها", [SecuritySection.Kpi]="شاخص‌های کلیدی",
        [SecuritySection.Okr]="اهداف و نتایج", [SecuritySection.Users]="کاربران", [SecuritySection.Settings]="تنظیمات"
    };
}
