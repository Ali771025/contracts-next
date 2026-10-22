namespace Contracts.Desktop.MasterData;

public sealed class ProjectPageViewModel : ObservableObject, INavigationTarget, IEditableRecord
{
    private readonly ProjectCrudService service; private readonly IPersianDateService dates; private readonly INumberDisplayService numbers; private readonly IDirtyDecisionService dirtyDecision; private readonly IConfirmationService confirmation; private readonly RecordNavigator<long, string> navigator;
    private readonly string title = "پروژه‌ها";
    private bool loading, isDirty, isBusy; private long id, version, statusId; private long? managerUserId;
    private string search = "", code = "", name = "", employer = "", department = "", startDate = "", endDate = "", description = "";
    private string fiscalYear = ""; private long budgetCurrencyId, approvedAmountMinor, consumedAmountMinor;
    private ProjectDto? selectedListItem; private ProjectBudgetRow? selectedBudget; private string? errorMessage;

    public ProjectPageViewModel(ProjectCrudService service, IPersianDateService dates, INumberDisplayService numbers, IDirtyDecisionService dirtyDecision, IConfirmationService confirmation)
    {
        this.service = service; this.dates = dates; this.numbers = numbers; this.dirtyDecision = dirtyDecision; this.confirmation = confirmation; navigator = new(new ProjectSequence(service), this, dirtyDecision);
        SearchCommand = new AsyncRelayCommand(SearchAsync); NewCommand = new AsyncRelayCommand(NewAsync); OpenCommand = new AsyncRelayCommand(OpenAsync);
        SaveCommand = new AsyncRelayCommand(SaveCommandAsync); DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        PreviousCommand = new AsyncRelayCommand(ct => MoveAsync(RecordDirection.Previous, ct)); NextCommand = new AsyncRelayCommand(ct => MoveAsync(RecordDirection.Next, ct));
        SaveBudgetCommand = new AsyncRelayCommand(SaveBudgetAsync); DeleteBudgetCommand = new AsyncRelayCommand(DeleteBudgetAsync);
    }
    public string Title => title; public ObservableCollection<ProjectDto> Items { get; } = [];
    public ObservableCollection<SimpleLookupDto> Statuses { get; } = []; public ObservableCollection<SimpleLookupDto> Currencies { get; } = [];
    public ObservableCollection<ProjectBudgetRow> Budgets { get; } = [];
    public string Search { get => search; set => SetProperty(ref search, value); } public ProjectDto? SelectedListItem { get => selectedListItem; set => SetProperty(ref selectedListItem, value); }
    public ProjectBudgetRow? SelectedBudget { get => selectedBudget; set { if (SetProperty(ref selectedBudget, value) && value is not null) LoadBudget(value.Value); } }
    public string Code { get => code; set => SetEdit(ref code, value); } public string Name { get => name; set => SetEdit(ref name, value); }
    public string Employer { get => employer; set => SetEdit(ref employer, value); } public string Department { get => department; set => SetEdit(ref department, value); }
    public long StatusId { get => statusId; set => SetEdit(ref statusId, value); } public long? ManagerUserId { get => managerUserId; set => SetEdit(ref managerUserId, value); }
    public string StartDate { get => startDate; set => SetEdit(ref startDate, value); } public string EndDate { get => endDate; set => SetEdit(ref endDate, value); }
    public string Description { get => description; set => SetEdit(ref description, value); }
    public string FiscalYear { get => fiscalYear; set => SetProperty(ref fiscalYear, value); } public long BudgetCurrencyId { get => budgetCurrencyId; set => SetProperty(ref budgetCurrencyId, value); }
    public long ApprovedAmountMinor { get => approvedAmountMinor; set => SetProperty(ref approvedAmountMinor, value); } public long ConsumedAmountMinor { get => consumedAmountMinor; set => SetProperty(ref consumedAmountMinor, value); }
    public bool IsDirty { get => isDirty; private set => SetProperty(ref isDirty, value); } public bool IsBusy { get => isBusy; private set => SetProperty(ref isBusy, value); }
    public string? ErrorMessage { get => errorMessage; private set { if (SetProperty(ref errorMessage, value)) OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public IAsyncRelayCommand SearchCommand { get; } public IAsyncRelayCommand NewCommand { get; } public IAsyncRelayCommand OpenCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; } public IAsyncRelayCommand DeleteCommand { get; } public IAsyncRelayCommand PreviousCommand { get; } public IAsyncRelayCommand NextCommand { get; }
    public IAsyncRelayCommand SaveBudgetCommand { get; } public IAsyncRelayCommand DeleteBudgetCommand { get; }

    public async Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        Statuses.Clear(); foreach (SimpleLookupDto item in await service.StatusesAsync(cancellationToken)) Statuses.Add(item);
        Currencies.Clear(); foreach (SimpleLookupDto item in await service.CurrenciesAsync(cancellationToken)) Currencies.Add(item);
        await SearchAsync(cancellationToken);
    }
    private async Task SearchAsync(CancellationToken ct) => await RunAsync(async () =>
    { PageResult<ProjectDto> page = await service.SearchAsync(new(Search: Search), ct); Items.Clear(); foreach (ProjectDto item in page.Items) Items.Add(item); });
    private void New() { loading = true; id = version = 0; Code = Name = Employer = Department = StartDate = EndDate = Description = ""; StatusId = Statuses.FirstOrDefault()?.Id ?? 0; ManagerUserId = null; loading = false; IsDirty = false; Budgets.Clear(); NewBudget(); }
    private async Task NewAsync(CancellationToken ct) { if (await ResolveDirtyAsync(ct)) New(); }
    private async Task OpenAsync(CancellationToken ct) { if (SelectedListItem is null || !await ResolveDirtyAsync(ct)) return; Load(await service.GetAsync(SelectedListItem.Id, ct)); await LoadBudgetsAsync(ct); }
    private async Task SaveCommandAsync(CancellationToken ct) => await RunAsync(() => SaveCoreAsync(ct));
    private async Task SaveCoreAsync(CancellationToken ct)
    {
        DateOnly? start = ParseOptionalDate(StartDate); DateOnly? end = ParseOptionalDate(EndDate);
        ProjectInput input = new(Code, Name, Employer, Department, ManagerUserId, start, end, StatusId, Description);
        ProjectDto saved = id == 0 ? await service.CreateAsync(input, ct) : await service.UpdateAsync(new(id, version, input), ct);
        Load(saved); await SearchAsync(ct);
    }
    private async Task DeleteAsync(CancellationToken ct) { if (id == 0 || !await confirmation.ConfirmAsync("این پروژه حذف شود؟", ct)) return; await RunAsync(async () => { await service.DeleteAsync(id, version, ct); New(); await SearchAsync(ct); }); }
    private async Task MoveAsync(RecordDirection direction, CancellationToken ct)
    {
        if (id == 0) return; RecordNavigationResult<long> result = await navigator.MoveAsync(id, direction, Search, "NormalizedProjectNo,Id", ct);
        if (result.Error is not null) { ErrorMessage = result.Error; return; } if (result.Moved) { Load(await service.GetAsync(result.CurrentId, ct)); await LoadBudgetsAsync(ct); }
    }
    private void Load(ProjectDto? item)
    {
        if (item is null) return; loading = true; id = item.Id; version = item.ConcurrencyVersion; Code = item.Code; Name = item.Name; Employer = item.Employer ?? "";
        Department = item.Department ?? ""; ManagerUserId = item.ManagerUserId; StatusId = item.StatusId; StartDate = item.StartDate.HasValue ? dates.Format(item.StartDate.Value) : "";
        EndDate = item.EndDate.HasValue ? dates.Format(item.EndDate.Value) : ""; Description = item.Description ?? ""; loading = false; IsDirty = false;
    }
    private DateOnly? ParseOptionalDate(string value) { if (string.IsNullOrWhiteSpace(value)) return null; if (dates.TryParse(value, out DateOnly result)) return result; throw new MasterDataValidationException(["تاریخ باید شمسی و به شکل yyyy/MM/dd باشد."]); }
    private async Task LoadBudgetsAsync(CancellationToken ct) { Budgets.Clear(); foreach (ProjectBudgetDto item in await service.BudgetsAsync(id, ct)) Budgets.Add(new(item, numbers)); NewBudget(); }
    private void NewBudget() { SelectedBudget = null; FiscalYear = ""; BudgetCurrencyId = Currencies.FirstOrDefault()?.Id ?? 0; ApprovedAmountMinor = ConsumedAmountMinor = 0; }
    private void LoadBudget(ProjectBudgetDto item) { FiscalYear = item.FiscalYear; BudgetCurrencyId = item.CurrencyId; ApprovedAmountMinor = item.ApprovedAmountMinor; ConsumedAmountMinor = item.ConsumedAmountMinor; }
    private async Task SaveBudgetAsync(CancellationToken ct)
    {
        if (id == 0) { ErrorMessage = "ابتدا پروژه را ذخیره کنید."; return; }
        await RunAsync(async () => { ProjectBudgetInput input = new(FiscalYear, BudgetCurrencyId, ApprovedAmountMinor, ConsumedAmountMinor);
            if (SelectedBudget is null) await service.CreateBudgetAsync(id, input, ct); else await service.UpdateBudgetAsync(new(SelectedBudget.Value.Id, SelectedBudget.Value.ConcurrencyVersion, input), ct); await LoadBudgetsAsync(ct); });
    }
    private async Task DeleteBudgetAsync(CancellationToken ct) { if (SelectedBudget is null || !await confirmation.ConfirmAsync("این ردیف بودجه حذف شود؟", ct)) return; await RunAsync(async () => { await service.DeleteBudgetAsync(SelectedBudget.Value.Id, SelectedBudget.Value.ConcurrencyVersion, ct); await LoadBudgetsAsync(ct); }); }
    private async Task<bool> ResolveDirtyAsync(CancellationToken ct)
    {
        if (!IsDirty) return true; DirtyDecision decision = await dirtyDecision.DecideAsync(ct);
        if (decision == DirtyDecision.Cancel) return false;
        if (decision == DirtyDecision.Discard) { DiscardChanges(); return true; }
        try { await SaveCoreAsync(ct); return true; } catch (Exception exception) { ErrorMessage = MasterDataErrorMapper.Map(exception); return false; }
    }
    public void DiscardChanges() => IsDirty = false; async Task IEditableRecord.SaveAsync(CancellationToken ct) => await SaveCoreAsync(ct);
    private bool SetEdit<T>(ref T field, T value) { bool changed = SetProperty(ref field, value); if (changed && !loading) IsDirty = true; return changed; }
    private async Task RunAsync(Func<Task> action) { IsBusy = true; ErrorMessage = null; try { await action(); } catch (Exception ex) { ErrorMessage = MasterDataErrorMapper.Map(ex); } finally { IsBusy = false; } }
    private sealed class ProjectSequence(ProjectCrudService service) : IRecordSequence<long, string>
    { public Task<RecordNeighbor<long>> GetAdjacentAsync(long currentId, RecordDirection direction, string filter, string stableSort, CancellationToken ct) => service.AdjacentAsync(currentId, direction, filter, ct); }

    public sealed class ProjectBudgetRow(ProjectBudgetDto value, INumberDisplayService numbers)
    {
        public ProjectBudgetDto Value { get; } = value;
        public string FiscalYear => Value.FiscalYear;
        public string CurrencyCode => Value.CurrencyCode;
        public string Approved => numbers.FormatMinor(Value.ApprovedAmountMinor, Value.CurrencyScale, Value.CurrencyCode);
        public string Consumed => numbers.FormatMinor(Value.ConsumedAmountMinor, Value.CurrencyScale, Value.CurrencyCode);
        public string Remaining => numbers.FormatMinor(Value.RemainingAmountMinor, Value.CurrencyScale, Value.CurrencyCode);
    }
}
