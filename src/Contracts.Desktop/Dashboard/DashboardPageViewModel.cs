namespace Contracts.Desktop.Dashboard;

using Contracts.Application.Dashboard;

public sealed class DashboardPageViewModel : ObservableObject, INavigationTarget
{
    private readonly IDashboardQueryService query;
    private readonly DashboardHtmlRenderer renderer;
    private readonly IPersianDateService dates;
    private string html = "", error = "", title = "داشبورد مدیریتی", drilldownTitle = "جزئیات";
    private bool busy, isDrilldownVisible; private SecuritySection section = SecuritySection.Dashboard;
    private string dateFrom="",dateTo="";private long? contractTypeId,contractStatusId,partyId,projectId;
    private DashboardSnapshot? snapshot;
    public string Html { get => html; private set => SetProperty(ref html, value); }
    public string Title { get => title; private set => SetProperty(ref title, value); }
    public string DrilldownTitle { get => drilldownTitle; private set => SetProperty(ref drilldownTitle, value); }
    public bool IsBusy { get => busy; private set => SetProperty(ref busy, value); }
    public bool IsDrilldownVisible { get => isDrilldownVisible; private set => SetProperty(ref isDrilldownVisible, value); }
    public string ErrorMessage { get => error; private set { if (SetProperty(ref error, value)) OnPropertyChanged(nameof(HasError)); } }
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);
    public ObservableCollection<DashboardContractDetail> DrilldownContracts { get; } = [];
    public ObservableCollection<DashboardAlertDetail> DrilldownAlerts { get; } = [];
    public ObservableCollection<DashboardEvidenceDetail> DrilldownEvidence { get; } = [];
    public string DateFrom{get=>dateFrom;set=>SetProperty(ref dateFrom,value);}public string DateTo{get=>dateTo;set=>SetProperty(ref dateTo,value);}public long? ContractTypeId{get=>contractTypeId;set=>SetProperty(ref contractTypeId,value);}public long? ContractStatusId{get=>contractStatusId;set=>SetProperty(ref contractStatusId,value);}public long? PartyId{get=>partyId;set=>SetProperty(ref partyId,value);}public long? ProjectId{get=>projectId;set=>SetProperty(ref projectId,value);}
    public IRelayCommand<string> SelectDrilldownCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }

    public DashboardPageViewModel(IDashboardQueryService query, DashboardHtmlRenderer renderer,IPersianDateService dates)
    {
        this.query = query; this.renderer = renderer;this.dates=dates;
        SelectDrilldownCommand = new RelayCommand<string>(SelectDrilldown);
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
    }

    public void ActivateRoute(string route)
    {
        section = route == "kpi" ? SecuritySection.Kpi : SecuritySection.Dashboard;
        Title = section == SecuritySection.Kpi ? "گزارشات KPI" : "داشبورد مدیریتی";
    }
    public Task OnNavigatedToAsync(CancellationToken cancellationToken)
        => LoadAsync(cancellationToken);
    private async Task LoadAsync(CancellationToken ct)
    {
        IsBusy = true; ErrorMessage = "";
        try { snapshot = await query.LoadAsync(new DashboardFilter(Parse(DateFrom),Parse(DateTo),ContractTypeId,ContractStatusId,PartyId,ProjectId), section, ct); Html = renderer.Render(snapshot); IsDrilldownVisible = false; }
        catch (OperationCanceledException) { throw; }
        catch (Exception) { ErrorMessage = "بارگذاری شاخص‌ها ناموفق بود. دسترسی و پایگاه داده را بررسی کنید."; }
        finally { IsBusy = false; }
    }
    private DateOnly? Parse(string value){if(string.IsNullOrWhiteSpace(value))return null;if(dates.TryParse(value,out DateOnly date))return date;throw new ArgumentException("تاریخ باید شمسی و به شکل yyyy/MM/dd باشد.");}
    public void SelectDrilldown(string? value)
    {
        if (snapshot is null || !Enum.TryParse(value, out DashboardDrilldownKind kind) || !snapshot.Drilldowns.TryGetValue(kind, out DashboardDrilldown? detail)) return;
        DrilldownContracts.Clear(); foreach (DashboardContractDetail row in detail.Contracts) DrilldownContracts.Add(row);
        DrilldownAlerts.Clear(); foreach (DashboardAlertDetail row in detail.Alerts) DrilldownAlerts.Add(row);
        DrilldownEvidence.Clear(); foreach (DashboardEvidenceDetail row in detail.Evidence) DrilldownEvidence.Add(row);
        DrilldownTitle = $"جزئیات {kind} — {detail.Count:N0} رکورد — {detail.TotalRial:N0} ریال"; IsDrilldownVisible = true;
    }
}
