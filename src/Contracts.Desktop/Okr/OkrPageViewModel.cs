namespace Contracts.Desktop.Okr;

using Contracts.Application.Okr;
using Contracts.Domain.Okr;
using System.Globalization;

public sealed record OkrRow(long Id,string Objective,string ObjectiveScore,string ObjectiveStatus,string Code,string Title,string Target,string Weight,string Actual,string Score,string Source,string Provenance,bool IsManual);
public sealed class OkrPageViewModel:ObservableObject,INavigationTarget
{
    private readonly OkrService service;private readonly IPersianDateService dates;private IReadOnlyList<OkrRow> rows=[];private OkrRow? selected;
    private string overall="—",status="NotMeasured",measurementDate="",actual="",note="",error="";private bool busy;
    public OkrPageViewModel(OkrService service,IPersianDateService dates){this.service=service;this.dates=dates;LoadCommand=new AsyncRelayCommand(LoadAsync);SaveManualCommand=new AsyncRelayCommand(SaveAsync);RefreshAutoCommand=new AsyncRelayCommand(RefreshAsync);}
    public IReadOnlyList<OkrRow> Rows{get=>rows;private set{SetProperty(ref rows,value);OnPropertyChanged(nameof(ManualRows));}}
    public IReadOnlyList<OkrRow> ManualRows=>Rows.Where(x=>x.IsManual).ToArray();public OkrRow? SelectedManual{get=>selected;set=>SetProperty(ref selected,value);}
    public string Overall{get=>overall;private set=>SetProperty(ref overall,value);}public string Status{get=>status;private set=>SetProperty(ref status,value);}
    public string MeasurementDate{get=>measurementDate;set=>SetProperty(ref measurementDate,value);}public string Actual{get=>actual;set=>SetProperty(ref actual,value);}public string Note{get=>note;set=>SetProperty(ref note,value);}
    public string ErrorMessage{get=>error;private set{if(SetProperty(ref error,value))OnPropertyChanged(nameof(HasError));}}public bool HasError=>error.Length>0;public bool IsBusy{get=>busy;private set=>SetProperty(ref busy,value);}
    public IAsyncRelayCommand LoadCommand{get;}public IAsyncRelayCommand SaveManualCommand{get;}public IAsyncRelayCommand RefreshAutoCommand{get;}
    public Task OnNavigatedToAsync(CancellationToken cancellationToken)=>LoadAsync(cancellationToken);
    private async Task LoadAsync(CancellationToken ct)=>await Run(async()=>{OkrSnapshot s=await service.LoadAsync(ct);Overall=s.Overall is null?"اندازه‌گیری نشده":s.Overall.Value.ToString("P1",CultureInfo.InvariantCulture);Status=Translate(s.Status);
        Rows=s.Objectives.SelectMany(o=>o.KeyResults.Select(k=>new OkrRow(k.Id,$"{o.Code} — {o.TitleFa}",o.Score is null?"—":o.Score.Value.ToString("P1",CultureInfo.InvariantCulture),Translate(o.Status),k.Code,k.TitleFa,$"{k.Target:0.######} {k.Unit}",$"{k.Weight}%",k.Latest is null?"—":k.Latest.Actual.ToString("0.######",CultureInfo.InvariantCulture),k.Latest is null?"NotMeasured":k.Latest.Score.ToString("P1",CultureInfo.InvariantCulture),k.DataSource==OkrDataSourceType.Auto?"AUTO":"MANUAL",k.Latest is null?$"{k.MetricVersion}; بدون evidence":$"{k.Latest.MetricVersion}; as-of={k.Latest.AsOfUtc:O}; evidence={k.Latest.EvidenceCount?.ToString(CultureInfo.InvariantCulture)??"manual"}",k.DataSource==OkrDataSourceType.Manual))).ToArray();SelectedManual=ManualRows.Count==0?null:ManualRows[0];if(string.IsNullOrEmpty(MeasurementDate))MeasurementDate=dates.Format(DateOnly.FromDateTime(DateTime.Today));});
    private async Task SaveAsync(CancellationToken ct)=>await Run(async()=>{if(SelectedManual is null)throw new InvalidOperationException("یک نتیجهٔ کلیدی دستی انتخاب کنید.");if(!dates.TryParse(MeasurementDate,out DateOnly d)||!decimal.TryParse(Actual,NumberStyles.Number,CultureInfo.InvariantCulture,out decimal a))throw new ArgumentException("تاریخ یا مقدار معتبر نیست.");await service.AddManualAsync(new(SelectedManual.Id,d,a,Note),ct);await LoadAsync(ct);Actual=Note="";});
    private async Task RefreshAsync(CancellationToken ct)=>await Run(async()=>{await service.RefreshAutoAsync(ct);await LoadAsync(ct);});
    private async Task Run(Func<Task> action){IsBusy=true;ErrorMessage="";try{await action();}catch(OperationCanceledException){throw;}catch(Exception ex){ErrorMessage=ex.Message;}finally{IsBusy=false;}}
    private static string Translate(OkrScoreStatus value)=>value switch{OkrScoreStatus.Excellent=>"عالی",OkrScoreStatus.Good=>"خوب",OkrScoreStatus.Average=>"متوسط",OkrScoreStatus.CorrectiveAction=>"نیازمند اقدام اصلاحی",_=>"اندازه‌گیری نشده"};
}
