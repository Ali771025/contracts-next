namespace Contracts.Desktop.Administration;

using System.Globalization;
using Contracts.Application.Administration;
using Contracts.Desktop.MasterData;
using Microsoft.Win32;

public interface IBackupPackagePicker{public string? PickRestorePackage();}
public sealed class BackupPackagePicker:IBackupPackagePicker{public string? PickRestorePackage(){OpenFileDialog dialog=new(){Title="انتخاب بسته پشتیبان",Filter="ContractsNext Backup (*.cnbak)|*.cnbak",CheckFileExists=true,Multiselect=false};return dialog.ShowDialog()==true?dialog.FileName:null;}}

public sealed class AdministrationPageViewModel:ObservableObject,INavigationTarget
{
    private readonly OperationalService service;private readonly IConfirmationService confirmation;private readonly IBackupPackagePicker picker;
    private string companyName="",reportHeader="",backupPath="",message="",error="";private int contractDays,guaranteeDays,financialDays,criticalDays,autoHours,retention;private bool autoEnabled,busy;private HealthSnapshot? health;
    public AdministrationPageViewModel(OperationalService service,IConfirmationService confirmation,IBackupPackagePicker picker,LookupAdministrationViewModel lookups,ReleaseIdentity releaseIdentity,WebView2RuntimeStatus webView2Status)
    {this.service=service;this.confirmation=confirmation;this.picker=picker;Lookups=lookups;ReleaseVersion=releaseIdentity.Display;WebView2Status=webView2Status.ActionableMessage;SaveCommand=new AsyncRelayCommand(SaveAsync);BackupCommand=new AsyncRelayCommand(BackupAsync);RestoreCommand=new AsyncRelayCommand(RestoreAsync);RefreshHealthCommand=new AsyncRelayCommand(HealthAsync);}
    public LookupAdministrationViewModel Lookups{get;}public string CompanyName{get=>companyName;set=>SetProperty(ref companyName,value);}public string ReportHeader{get=>reportHeader;set=>SetProperty(ref reportHeader,value);}public string BackupPath{get=>backupPath;set=>SetProperty(ref backupPath,value);}
    public string ReleaseVersion{get;}public string WebView2Status{get;}
    public int ContractWarningDays{get=>contractDays;set=>SetProperty(ref contractDays,value);}public int GuaranteeWarningDays{get=>guaranteeDays;set=>SetProperty(ref guaranteeDays,value);}public int FinancialWarningDays{get=>financialDays;set=>SetProperty(ref financialDays,value);}public int CriticalWarningDays{get=>criticalDays;set=>SetProperty(ref criticalDays,value);}
    public bool AutoBackupEnabled{get=>autoEnabled;set=>SetProperty(ref autoEnabled,value);}public int AutoBackupIntervalHours{get=>autoHours;set=>SetProperty(ref autoHours,value);}public int RetentionCount{get=>retention;set=>SetProperty(ref retention,value);}
    public HealthSnapshot? Health{get=>health;private set{if(SetProperty(ref health,value)){OnPropertyChanged(nameof(HealthItems));OnPropertyChanged(nameof(HealthSummary));}}}public IReadOnlyList<HealthCheckItem> HealthItems=>Health?.Items??[];public string HealthSummary=>Health is null?"—":$"{Health.Overall} | schema {Health.SchemaVersion} | app {Health.AppVersion} | {Health.MaskedDataRoot}";
    public string Message{get=>message;private set=>SetProperty(ref message,value);}public string ErrorMessage{get=>error;private set{if(SetProperty(ref error,value))OnPropertyChanged(nameof(HasError));}}public bool HasError=>error.Length>0;public bool IsBusy{get=>busy;private set=>SetProperty(ref busy,value);}
    public IAsyncRelayCommand SaveCommand{get;}public IAsyncRelayCommand BackupCommand{get;}public IAsyncRelayCommand RestoreCommand{get;}public IAsyncRelayCommand RefreshHealthCommand{get;}
    public async Task OnNavigatedToAsync(CancellationToken cancellationToken){await Run(async()=>{Load(await service.LoadSettingsAsync(cancellationToken));await Lookups.OnNavigatedToAsync(cancellationToken);Health=await service.HealthAsync(cancellationToken);});}
    private async Task SaveAsync(CancellationToken ct)=>await Run(async()=>{await service.SaveSettingsAsync(Current(),ct);Message="تنظیمات با audit ذخیره شد.";Health=await service.HealthAsync(ct);});
    private async Task BackupAsync(CancellationToken ct)=>await Run(async()=>{await service.SaveSettingsAsync(Current(),ct);BackupResult result=await service.BackupAsync(ct);Message=$"پشتیبان ساخته شد: {Path.GetFileName(result.PackagePath)} | SHA-256 {result.PackageSha256}";Health=await service.HealthAsync(ct);});
    private async Task RestoreAsync(CancellationToken ct)
    {string? package=picker.PickRestorePackage();if(package is null||!await confirmation.ConfirmAsync("بازیابی، یک pre-restore backup می‌سازد و سپس پایگاه و اسناد را اتمیک جایگزین می‌کند. ادامه؟",ct))return;await Run(async()=>{Window? window=System.Windows.Application.Current?.MainWindow;if(window is not null)window.IsEnabled=false;try{RestoreResult result=await service.RestoreAsync(package,ct);Message=$"بازیابی موفق؛ {result.ContractCount.ToString(CultureInfo.InvariantCulture)} قرارداد. برای تازه‌سازی کامل برنامه را بازراه‌اندازی کنید.";Health=await service.HealthAsync(ct);}finally{if(window is not null)window.IsEnabled=true;}});}
    private Task HealthAsync(CancellationToken ct)=>Run(async()=>Health=await service.HealthAsync(ct));
    private SystemSettings Current()=>new(CompanyName,ReportHeader,ContractWarningDays,GuaranteeWarningDays,FinancialWarningDays,CriticalWarningDays,BackupPath,AutoBackupEnabled,AutoBackupIntervalHours,RetentionCount);
    private void Load(SystemSettings s){CompanyName=s.CompanyName;ReportHeader=s.ReportHeader;ContractWarningDays=s.ContractWarningDays;GuaranteeWarningDays=s.GuaranteeWarningDays;FinancialWarningDays=s.FinancialWarningDays;CriticalWarningDays=s.CriticalWarningDays;BackupPath=s.BackupPath;AutoBackupEnabled=s.AutoBackupEnabled;AutoBackupIntervalHours=s.AutoBackupIntervalHours;RetentionCount=s.RetentionCount;}
    private async Task Run(Func<Task> action){IsBusy=true;ErrorMessage="";Message="";try{await action();}catch(OperationCanceledException){throw;}catch(Exception ex){ErrorMessage=ex.Message;}finally{IsBusy=false;}}
}
