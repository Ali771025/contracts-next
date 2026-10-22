namespace Contracts.Desktop.MasterData;

public sealed class LookupAdministrationViewModel : ObservableObject, INavigationTarget
{
    private readonly LookupAdministrationService service;
    private readonly IConfirmationService confirmation;
    private readonly string title = "مدیریت اطلاعات پایه";
    private AdminLookupKind kind;
    private LookupDto? selected;
    private string search = "", code = "", titleFa = "";
    private int sortOrder;
    private bool isActive = true, isBusy;
    private string? errorMessage;

    public LookupAdministrationViewModel(LookupAdministrationService service, IConfirmationService confirmation)
    {
        this.service = service;
        this.confirmation = confirmation;
        SearchCommand = new AsyncRelayCommand(SearchAsync); NewCommand = new RelayCommand(New);
        SaveCommand = new AsyncRelayCommand(SaveAsync); DeactivateCommand = new AsyncRelayCommand(DeactivateAsync);
    }
    public string Title => title;
    public ObservableCollection<LookupDto> Items { get; } = [];
    public IReadOnlyList<AdminLookupKind> Kinds { get; } = Enum.GetValues<AdminLookupKind>();
    public AdminLookupKind Kind { get => kind; set { if (SetProperty(ref kind, value)) _ = SearchCommand.ExecuteAsync(null); } }
    public LookupDto? Selected { get => selected; set { if (SetProperty(ref selected, value) && value is not null) Load(value); } }
    public string Search { get => search; set => SetProperty(ref search, value); }
    public string Code { get => code; set => SetProperty(ref code, value); }
    public string TitleFa { get => titleFa; set => SetProperty(ref titleFa, value); }
    public int SortOrder { get => sortOrder; set => SetProperty(ref sortOrder, value); }
    public bool IsActive { get => isActive; set => SetProperty(ref isActive, value); }
    public bool IsCodeReadOnly => Selected is not null;
    public bool IsBusy { get => isBusy; private set => SetProperty(ref isBusy, value); }
    public string? ErrorMessage { get => errorMessage; private set { if (SetProperty(ref errorMessage, value)) OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    public IAsyncRelayCommand SearchCommand { get; } public IRelayCommand NewCommand { get; }
    public IAsyncRelayCommand SaveCommand { get; } public IAsyncRelayCommand DeactivateCommand { get; }

    public Task OnNavigatedToAsync(CancellationToken cancellationToken) => SearchAsync(cancellationToken);
    private async Task SearchAsync(CancellationToken ct) => await RunAsync(async () =>
    { PageResult<LookupDto> page = await service.SearchAsync(Kind, new(Search: Search, Sort: "sort"), ct); Items.Clear(); foreach (LookupDto item in page.Items) Items.Add(item); });
    private void New() { Selected = null; Code = TitleFa = ""; SortOrder = 0; IsActive = true; OnPropertyChanged(nameof(IsCodeReadOnly)); }
    private void Load(LookupDto item) { Code = item.Code; TitleFa = item.TitleFa; SortOrder = item.SortOrder; IsActive = item.IsActive; OnPropertyChanged(nameof(IsCodeReadOnly)); }
    private async Task SaveAsync(CancellationToken ct) => await RunAsync(async () =>
    {
        LookupDto saved = Selected is null
            ? await service.CreateAsync(Kind, new(Code, TitleFa, SortOrder, IsActive), ct)
            : await service.UpdateAsync(Kind, new(Selected.Id, Selected.ConcurrencyVersion, TitleFa, SortOrder, IsActive), ct);
        await SearchAsync(ct); Selected = Items.Single(item => item.Id == saved.Id);
    });
    private async Task DeactivateAsync(CancellationToken ct)
    { if (Selected is null || !await confirmation.ConfirmAsync("این مقدار پایه غیرفعال شود؟", ct)) return; await RunAsync(async () => { await service.DeactivateAsync(Kind, Selected.Id, Selected.ConcurrencyVersion, ct); await SearchAsync(ct); New(); }); }
    private async Task RunAsync(Func<Task> action) { IsBusy = true; ErrorMessage = null; try { await action(); } catch (Exception ex) { ErrorMessage = MasterDataErrorMapper.Map(ex); } finally { IsBusy = false; } }
}
