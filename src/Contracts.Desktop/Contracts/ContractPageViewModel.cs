namespace Contracts.Desktop.ContractManagement;

public sealed class ContractPageViewModel : ObservableObject, INavigationTarget, IEditableRecord
{
    private readonly ContractService service; private readonly IPersianDateService dates; private readonly INumberDisplayService numbers;
    private readonly IDirtyDecisionService dirtyDecision; private readonly IConfirmationService confirmation;
    private readonly RecordNavigator<long, ContractFilter> navigator;
    private bool loading, isDirty, isBusy; private string? errorMessage; private long id, version;
    private string search = "", endFrom = "", endTo = "", contractNo = "", title = "", startDate = "", endDate = "", effectiveEndDate = "", description = "";
    private long? filterTypeId, filterStatusId, filterPartyId, filterProjectId, categoryId, projectId, managerUserId;
    private long typeId, partyId, statusId, currencyId, originalAmountMinor; private long? exchangeRate;
    private int page = 1, totalCount, durationDays; private long baseAmountRial, finalAmountRial; private string createdBy = "";
    private string sort = "number"; private bool descending;
    private ContractListItem? selectedListItem;
    private string financialSummaryMessage = "خلاصه مالی پس از انتخاب قرارداد نمایش داده می‌شود.";

    public ContractPageViewModel(ContractService service, IPersianDateService dates, INumberDisplayService numbers,
        IDirtyDecisionService dirtyDecision, IConfirmationService confirmation)
    {
        this.service = service; this.dates = dates; this.numbers = numbers; this.dirtyDecision = dirtyDecision; this.confirmation = confirmation;
        navigator = new(new ContractSequence(service), this, dirtyDecision);
        SearchCommand = new AsyncRelayCommand(SearchFirstPageAsync); NextPageCommand = new AsyncRelayCommand(NextPageAsync);
        PreviousPageCommand = new AsyncRelayCommand(PreviousPageAsync); NewCommand = new AsyncRelayCommand(NewAsync);
        ClearFiltersCommand = new RelayCommand(ClearFilters);
        OpenCommand = new AsyncRelayCommand(OpenAsync); SaveCommand = new AsyncRelayCommand(SaveCommandAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync); PreviousCommand = new AsyncRelayCommand(ct => MoveAsync(RecordDirection.Previous, ct));
        NextCommand = new AsyncRelayCommand(ct => MoveAsync(RecordDirection.Next, ct));
    }

    public ObservableCollection<ContractListItem> Items { get; } = [];
    public ObservableCollection<SimpleLookupDto> Types { get; } = []; public ObservableCollection<SimpleLookupDto> Categories { get; } = [];
    public ObservableCollection<SimpleLookupDto> Statuses { get; } = []; public ObservableCollection<CurrencyOption> Currencies { get; } = [];
    public ObservableCollection<ContractOption> Parties { get; } = []; public ObservableCollection<ContractOption> Projects { get; } = [];
    public ObservableCollection<AmendmentRow> Amendments { get; } = []; public ObservableCollection<PaymentRow> Payments { get; } = [];
    public ObservableCollection<ReceiptRow> Receipts { get; } = []; public ObservableCollection<InvoiceRow> Invoices { get; } = [];
    public ObservableCollection<GuaranteeRow> Guarantees { get; } = []; public ObservableCollection<DocumentRow> Documents { get; } = [];
    public ObservableCollection<ContractHistoryRow> History { get; } = [];

    public string Search { get => search; set => SetProperty(ref search, value); } public string EndFrom { get => endFrom; set => SetProperty(ref endFrom, value); }
    public string EndTo { get => endTo; set => SetProperty(ref endTo, value); } public long? FilterTypeId { get => filterTypeId; set => SetProperty(ref filterTypeId, value); }
    public long? FilterStatusId { get => filterStatusId; set => SetProperty(ref filterStatusId, value); } public long? FilterPartyId { get => filterPartyId; set => SetProperty(ref filterPartyId, value); }
    public long? FilterProjectId { get => filterProjectId; set => SetProperty(ref filterProjectId, value); } public int Page { get => page; private set => SetProperty(ref page, value); }
    public int TotalCount { get => totalCount; private set => SetProperty(ref totalCount, value); } public ContractListItem? SelectedListItem { get => selectedListItem; set => SetProperty(ref selectedListItem, value); }
    public IReadOnlyList<SortOption> SortOptions { get; } = [new("number", "شماره"), new("title", "عنوان"), new("enddate", "تاریخ پایان"), new("final", "مبلغ نهایی")];
    public string Sort { get => sort; set => SetProperty(ref sort, value); } public bool Descending { get => descending; set => SetProperty(ref descending, value); }
    public string ContractNo { get => contractNo; set => SetEdit(ref contractNo, value); } public string ContractTitle { get => title; set => SetEdit(ref title, value); }
    public long TypeId { get => typeId; set => SetEdit(ref typeId, value); } public long? CategoryId { get => categoryId; set => SetEdit(ref categoryId, value); }
    public long? ProjectId { get => projectId; set => SetEdit(ref projectId, value); } public long PartyId { get => partyId; set => SetEdit(ref partyId, value); }
    public long StatusId { get => statusId; set => SetEdit(ref statusId, value); }
    public long CurrencyId { get => currencyId; set { if (SetEdit(ref currencyId, value)) OnPropertyChanged(nameof(IsForeignCurrency)); } }
    public long OriginalAmountMinor { get => originalAmountMinor; set => SetEdit(ref originalAmountMinor, value); }
    public long? ExchangeRateRialPerUnit { get => exchangeRate; set => SetEdit(ref exchangeRate, value); }
    public string StartDate { get => startDate; set => SetEdit(ref startDate, value); } public string EndDate { get => endDate; set => SetEdit(ref endDate, value); }
    public string EffectiveEndDate { get => effectiveEndDate; private set => SetProperty(ref effectiveEndDate, value); }
    public long? ManagerUserId { get => managerUserId; set => SetEdit(ref managerUserId, value); } public string Description { get => description; set => SetEdit(ref description, value); }
    public bool IsForeignCurrency => Currencies.FirstOrDefault(item => item.Id == CurrencyId)?.Code is not (null or "IRR");
    public string BaseAmountDisplay => id == 0 ? "پس از ذخیره محاسبه می‌شود" : numbers.FormatMoney(baseAmountRial);
    public string FinalAmountDisplay => id == 0 ? "پس از ذخیره محاسبه می‌شود" : numbers.FormatMoney(finalAmountRial);
    public int DurationDays { get => durationDays; private set => SetProperty(ref durationDays, value); } public string CreatedBy { get => createdBy; private set => SetProperty(ref createdBy, value); }
    public bool IsDirty { get => isDirty; private set => SetProperty(ref isDirty, value); } public bool IsBusy { get => isBusy; private set => SetProperty(ref isBusy, value); }
    public string? ErrorMessage { get => errorMessage; private set { if (SetProperty(ref errorMessage, value)) OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage); public bool HasAmendments => Amendments.Count > 0; public bool HasPayments => Payments.Count > 0;
    public bool HasReceipts => Receipts.Count > 0; public bool HasInvoices => Invoices.Count > 0; public bool HasGuarantees => Guarantees.Count > 0;
    public bool HasDocuments => Documents.Count > 0; public bool HasHistory => History.Count > 0;
    public string FinancialSummaryMessage { get => financialSummaryMessage; private set => SetProperty(ref financialSummaryMessage, value); }

    public IAsyncRelayCommand SearchCommand { get; } public IAsyncRelayCommand NextPageCommand { get; } public IAsyncRelayCommand PreviousPageCommand { get; } public IRelayCommand ClearFiltersCommand { get; }
    public IAsyncRelayCommand NewCommand { get; } public IAsyncRelayCommand OpenCommand { get; } public IAsyncRelayCommand SaveCommand { get; }
    public IAsyncRelayCommand DeleteCommand { get; } public IAsyncRelayCommand PreviousCommand { get; } public IAsyncRelayCommand NextCommand { get; }

    public async Task OnNavigatedToAsync(CancellationToken cancellationToken)
    {
        ContractReferenceData refs = await service.ReferencesAsync(cancellationToken); Replace(Types, refs.Types); Replace(Categories, refs.Categories);
        Replace(Statuses, refs.Statuses); Replace(Currencies, refs.Currencies); Replace(Parties, refs.Parties); Replace(Projects, refs.Projects);
        await SearchAsync(cancellationToken);
    }
    private async Task SearchFirstPageAsync(CancellationToken ct) { Page = 1; await SearchAsync(ct); }
    private async Task NextPageAsync(CancellationToken ct) { if (Page * 25 >= TotalCount) return; Page++; await SearchAsync(ct); }
    private async Task PreviousPageAsync(CancellationToken ct) { if (Page <= 1) return; Page--; await SearchAsync(ct); }
    private void ClearFilters() { Search = EndFrom = EndTo = ""; FilterTypeId = FilterStatusId = FilterPartyId = FilterProjectId = null; Sort = "number"; Descending = false; Page = 1; }
    private async Task SearchAsync(CancellationToken ct) => await RunAsync(async () =>
    { PageResult<ContractListItem> result = await service.SearchAsync(BuildFilter(), ct); Replace(Items, result.Items); TotalCount = result.TotalCount; });
    private ContractFilter BuildFilter() => new(Page, 25, Search, FilterTypeId, FilterStatusId, FilterPartyId, FilterProjectId,
        ParseOptionalDate(EndFrom), ParseOptionalDate(EndTo), Sort, Descending);
    private async Task NewAsync(CancellationToken ct) { if (!await ResolveDirtyAsync(ct)) return; loading = true; id = version = 0; ContractNo = ContractTitle = StartDate = EndDate = Description = "";
        TypeId = Types.FirstOrDefault()?.Id ?? 0; StatusId = Statuses.FirstOrDefault()?.Id ?? 0; PartyId = Parties.FirstOrDefault()?.Id ?? 0; CategoryId = ProjectId = ManagerUserId = null;
        CurrencyId = Currencies.FirstOrDefault(item => item.Code == "IRR")?.Id ?? 0; OriginalAmountMinor = 0; ExchangeRateRialPerUnit = null; baseAmountRial = finalAmountRial = 0; DurationDays = 0; EffectiveEndDate = CreatedBy = "";
        ClearRelated(); loading = false; IsDirty = false; NotifyDerived(); }
    private async Task OpenAsync(CancellationToken ct) { if (SelectedListItem is null || !await ResolveDirtyAsync(ct)) return; await RunAsync(() => LoadAsync(SelectedListItem.Id, ct)); }
    private async Task LoadAsync(long contractId, CancellationToken ct) { ContractDetail? detail = await service.GetAsync(contractId, ct); if (detail is null) return; Load(detail); LoadRelated(await service.RelatedAsync(contractId, ct)); }
    private async Task SaveCommandAsync(CancellationToken ct) => await RunAsync(() => SaveCoreAsync(ct));
    private async Task SaveCoreAsync(CancellationToken ct)
    {
        ContractInput input = new(ContractNo, ContractTitle, TypeId, CategoryId, ProjectId, PartyId, StatusId, CurrencyId,
            OriginalAmountMinor, ExchangeRateRialPerUnit, ParseRequiredDate(StartDate), ParseRequiredDate(EndDate), ManagerUserId, Description);
        ContractDetail saved = id == 0 ? await service.CreateAsync(input, ct) : await service.UpdateAsync(new(id, version, input), ct);
        Load(saved); LoadRelated(await service.RelatedAsync(saved.Id, ct)); await SearchAsync(ct);
    }
    private async Task DeleteAsync(CancellationToken ct) { if (id == 0 || !await confirmation.ConfirmAsync("این قرارداد حذف شود؟", ct)) return;
        await RunAsync(async () => { await service.DeleteAsync(id, version, ct); IsDirty = false; await NewAsync(ct); await SearchAsync(ct); }); }
    private async Task MoveAsync(RecordDirection direction, CancellationToken ct) { if (id == 0) return; await RunAsync(async () => { RecordNavigationResult<long> result = await navigator.MoveAsync(id, direction, BuildFilter(), "NormalizedContractNo,Id", ct);
        if (result.Error is not null) { ErrorMessage = result.Error; return; } if (result.Moved) await LoadAsync(result.CurrentId, ct); }); }
    private void Load(ContractDetail item) { loading = true; id = item.Id; version = item.ConcurrencyVersion; ContractNo = item.ContractNo; ContractTitle = item.Title;
        TypeId = item.ContractTypeId; CategoryId = item.ContractCategoryId; ProjectId = item.ProjectId; PartyId = item.PartyId; StatusId = item.ContractStatusId; CurrencyId = item.CurrencyId;
        OriginalAmountMinor = item.OriginalAmountMinor; ExchangeRateRialPerUnit = item.CurrencyCode == "IRR" ? null : item.ExchangeRateRialPerUnit;
        StartDate = dates.Format(item.StartDate); EndDate = dates.Format(item.EndDate); EffectiveEndDate = dates.Format(item.EffectiveEndDate); ManagerUserId = item.ManagerUserId; Description = item.Description ?? "";
        baseAmountRial = item.BaseAmountRial; finalAmountRial = item.FinalAmountRial; DurationDays = item.DurationDays; CreatedBy = $"{item.CreatedByDisplayName} - {item.CreatedAtUtc:yyyy-MM-dd HH:mm} UTC";
        loading = false; IsDirty = false; NotifyDerived(); }
    private void LoadRelated(ContractRelatedData data) { Replace(Amendments, data.Amendments); Replace(Payments, data.Payments); Replace(Receipts, data.Receipts); Replace(Invoices, data.Invoices);
        Replace(Guarantees, data.Guarantees); Replace(Documents, data.Documents); Replace(History, data.History);
        FinancialSummaryDto s = data.FinancialSummary; FinancialSummaryMessage = s.State == FinancialSummaryState.Available
            ? $"کل طلب: {numbers.FormatMoney(s.TotalClaims!.Value)} | کل بدهی: {numbers.FormatMoney(s.TotalDebts!.Value)} | پرداخت: {numbers.FormatMoney(s.TotalPaid!.Value)} | دریافت: {numbers.FormatMoney(s.TotalReceived!.Value)} | مانده دریافتنی: {numbers.FormatMoney(s.ReceivableBalance!.Value)} | مانده پرداختنی: {numbers.FormatMoney(s.PayableBalance!.Value)} | مانده مرتبط: {(s.ContextualBalance.HasValue ? numbers.FormatMoney(s.ContextualBalance.Value) : "تعریف نشده برای این نوع قرارداد")}"
            : "خلاصه مالی در دسترس نیست."; NotifyRelated(); }
    private void ClearRelated() { Amendments.Clear(); Payments.Clear(); Receipts.Clear(); Invoices.Clear(); Guarantees.Clear(); Documents.Clear(); History.Clear(); FinancialSummaryMessage = "خلاصه مالی پس از انتخاب قرارداد نمایش داده می‌شود."; NotifyRelated(); }
    private void NotifyRelated() { OnPropertyChanged(nameof(HasAmendments)); OnPropertyChanged(nameof(HasPayments)); OnPropertyChanged(nameof(HasReceipts)); OnPropertyChanged(nameof(HasInvoices)); OnPropertyChanged(nameof(HasGuarantees)); OnPropertyChanged(nameof(HasDocuments)); OnPropertyChanged(nameof(HasHistory)); }
    private void NotifyDerived() { OnPropertyChanged(nameof(IsForeignCurrency)); OnPropertyChanged(nameof(BaseAmountDisplay)); OnPropertyChanged(nameof(FinalAmountDisplay)); }
    private DateOnly ParseRequiredDate(string value) => dates.TryParse(value, out DateOnly result) ? result : throw new MasterDataValidationException(["تاریخ باید شمسی و به شکل yyyy/MM/dd باشد."]);
    private DateOnly? ParseOptionalDate(string value) => string.IsNullOrWhiteSpace(value) ? null : ParseRequiredDate(value);
    private async Task<bool> ResolveDirtyAsync(CancellationToken ct) { if (!IsDirty) return true; DirtyDecision decision = await dirtyDecision.DecideAsync(ct); if (decision == DirtyDecision.Cancel) return false;
        if (decision == DirtyDecision.Discard) { DiscardChanges(); return true; } try { await SaveCoreAsync(ct); return true; } catch (Exception exception) { ErrorMessage = MasterData.MasterDataErrorMapper.Map(exception); return false; } }
    public void DiscardChanges() => IsDirty = false; async Task IEditableRecord.SaveAsync(CancellationToken ct) => await SaveCoreAsync(ct);
    private bool SetEdit<T>(ref T field, T value) { bool changed = SetProperty(ref field, value); if (changed && !loading) IsDirty = true; return changed; }
    private async Task RunAsync(Func<Task> action) { IsBusy = true; ErrorMessage = null; try { await action(); } catch (Exception exception) { ErrorMessage = MasterData.MasterDataErrorMapper.Map(exception); } finally { IsBusy = false; } }
    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source) { target.Clear(); foreach (T item in source) target.Add(item); }
    private sealed class ContractSequence(ContractService service) : IRecordSequence<long, ContractFilter>
    { public Task<RecordNeighbor<long>> GetAdjacentAsync(long currentId, RecordDirection direction, ContractFilter filter, string stableSort, CancellationToken ct) => service.AdjacentAsync(currentId, direction, filter, ct); }
    public sealed record SortOption(string Code, string Title);
}
