namespace Contracts.Desktop.MasterData;

public sealed class PartyPageViewModel : ObservableObject, INavigationTarget, IEditableRecord
{
    private readonly PartyCrudService service;
    private readonly IDirtyDecisionService dirtyDecision;
    private readonly IConfirmationService confirmation;
    private readonly string title = "طرف‌های قرارداد";
    private readonly RecordNavigator<long, string> navigator;
    private bool loading;
    private long id, version;
    private string search = string.Empty, name = string.Empty, nationalId = string.Empty, economicCode = string.Empty;
    private string mobile = string.Empty, phone = string.Empty, email = string.Empty, address = string.Empty, contactPerson = string.Empty;
    private long partyTypeId;
    private PartyDto? selectedListItem;
    private bool isDirty, isBusy;
    private string? errorMessage;

    public PartyPageViewModel(PartyCrudService service, IDirtyDecisionService dirtyDecision, IConfirmationService confirmation)
    {
        this.service = service;
        this.dirtyDecision = dirtyDecision;
        this.confirmation = confirmation;
        navigator = new(new PartySequence(service), this, dirtyDecision);
        SearchCommand = new AsyncRelayCommand(SearchAsync); NewCommand = new AsyncRelayCommand(NewAsync);
        OpenCommand = new AsyncRelayCommand(OpenAsync); SaveCommand = new AsyncRelayCommand(SaveCommandAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync); PreviousCommand = new AsyncRelayCommand(ct => MoveAsync(RecordDirection.Previous, ct));
        NextCommand = new AsyncRelayCommand(ct => MoveAsync(RecordDirection.Next, ct));
    }

    public string Title => title;
    public ObservableCollection<PartyDto> Items { get; } = [];
    public ObservableCollection<SimpleLookupDto> PartyTypes { get; } = [];
    public string Search { get => search; set => SetProperty(ref search, value); }
    public PartyDto? SelectedListItem { get => selectedListItem; set => SetProperty(ref selectedListItem, value); }
    public string Name { get => name; set => SetEdit(ref name, value); }
    public long PartyTypeId { get => partyTypeId; set => SetEdit(ref partyTypeId, value); }
    public string NationalId { get => nationalId; set => SetEdit(ref nationalId, value); }
    public string EconomicCode { get => economicCode; set => SetEdit(ref economicCode, value); }
    public string Mobile { get => mobile; set => SetEdit(ref mobile, value); }
    public string Phone { get => phone; set => SetEdit(ref phone, value); }
    public string Email { get => email; set => SetEdit(ref email, value); }
    public string Address { get => address; set => SetEdit(ref address, value); }
    public string ContactPerson { get => contactPerson; set => SetEdit(ref contactPerson, value); }
    public bool IsDirty { get => isDirty; private set => SetProperty(ref isDirty, value); }
    public bool IsBusy { get => isBusy; private set => SetProperty(ref isBusy, value); }
    public string? ErrorMessage { get => errorMessage; private set { if (SetProperty(ref errorMessage, value)) OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public IAsyncRelayCommand SearchCommand { get; } public IAsyncRelayCommand NewCommand { get; }
    public IAsyncRelayCommand OpenCommand { get; } public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; } public IAsyncRelayCommand PreviousCommand { get; } public IAsyncRelayCommand NextCommand { get; }

    public async Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        PartyTypes.Clear(); foreach (SimpleLookupDto item in await service.TypesAsync(cancellationToken)) PartyTypes.Add(item);
        await SearchAsync(cancellationToken);
    }
    private async Task SearchAsync(CancellationToken ct) => await RunAsync(async () =>
    { PageResult<PartyDto> page = await service.SearchAsync(new(Search: Search), ct); Items.Clear(); foreach (PartyDto item in page.Items) Items.Add(item); });
    private void New() { loading = true; id = version = 0; Name = NationalId = EconomicCode = Mobile = Phone = Email = Address = ContactPerson = string.Empty; PartyTypeId = PartyTypes.FirstOrDefault()?.Id ?? 0; loading = false; IsDirty = false; }
    private async Task NewAsync(CancellationToken ct) { if (await ResolveDirtyAsync(ct)) New(); }
    private async Task OpenAsync(CancellationToken ct)
    { if (SelectedListItem is null) return; if (!await ResolveDirtyAsync(ct)) return; Load(await service.GetAsync(SelectedListItem.Id, ct)); }
    private async Task SaveCommandAsync(CancellationToken ct) => await RunAsync(() => SaveCoreAsync(ct));
    private async Task SaveCoreAsync(CancellationToken ct)
    {
        PartyInput input = new(Name, PartyTypeId, NationalId, EconomicCode, Mobile, Phone, Email, Address, ContactPerson);
        PartyDto saved = id == 0 ? await service.CreateAsync(input, ct) : await service.UpdateAsync(new(id, version, input), ct);
        Load(saved); await SearchAsync(ct);
    }
    private async Task DeleteAsync(CancellationToken ct)
    { if (id == 0 || !await confirmation.ConfirmAsync("این طرف قرارداد حذف شود؟", ct)) return; await RunAsync(async () => { await service.DeleteAsync(id, version, ct); New(); await SearchAsync(ct); }); }
    private async Task MoveAsync(RecordDirection direction, CancellationToken ct)
    {
        if (id == 0) return;
        RecordNavigationResult<long> result = await navigator.MoveAsync(id, direction, Search, "NormalizedName,Id", ct);
        if (result.Error is not null) { ErrorMessage = result.Error; return; }
        if (result.Moved) Load(await service.GetAsync(result.CurrentId, ct));
    }
    private void Load(PartyDto? item)
    {
        if (item is null) return; loading = true; id = item.Id; version = item.ConcurrencyVersion; Name = item.Name; PartyTypeId = item.PartyTypeId;
        NationalId = item.NationalId ?? ""; EconomicCode = item.EconomicCode ?? ""; Mobile = item.Mobile ?? ""; Phone = item.Phone ?? "";
        Email = item.Email ?? ""; Address = item.Address ?? ""; ContactPerson = item.ContactPerson ?? ""; loading = false; IsDirty = false;
    }
    private async Task<bool> ResolveDirtyAsync(CancellationToken ct)
    {
        if (!IsDirty) return true;
        DirtyDecision result = await dirtyDecision.DecideAsync(ct);
        if (result == DirtyDecision.Cancel) return false;
        if (result == DirtyDecision.Discard) { DiscardChanges(); return true; }
        try { await SaveCoreAsync(ct); return true; } catch (Exception exception) { ErrorMessage = MasterDataErrorMapper.Map(exception); return false; }
    }
    async Task IEditableRecord.SaveAsync(CancellationToken ct) => await SaveCoreAsync(ct);
    public void DiscardChanges() { IsDirty = false; }
    private bool SetEdit<T>(ref T field, T value) { bool changed = SetProperty(ref field, value); if (changed && !loading) IsDirty = true; return changed; }
    private async Task RunAsync(Func<Task> action) { IsBusy = true; ErrorMessage = null; try { await action(); } catch (Exception ex) { ErrorMessage = MasterDataErrorMapper.Map(ex); } finally { IsBusy = false; } }
    private sealed class PartySequence(PartyCrudService service) : IRecordSequence<long, string>
    { public Task<RecordNeighbor<long>> GetAdjacentAsync(long currentId, RecordDirection direction, string filter, string stableSort, CancellationToken ct) => service.AdjacentAsync(currentId, direction, filter, ct); }
}

public static class MasterDataErrorMapper
{
    public static string Map(Exception exception) => exception switch
    {
        MasterDataValidationException validation => string.Join(Environment.NewLine, validation.Errors),
        MasterDataConflictException or MasterDataDependencyException or MasterDataDuplicateException => exception.Message,
        UnauthorizedAccessException => "دسترسی لازم برای این عملیات وجود ندارد.",
        _ => "عملیات ناموفق بود. دوباره تلاش کنید.",
    };
}
