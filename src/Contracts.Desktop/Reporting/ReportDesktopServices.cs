namespace Contracts.Desktop.Reporting;

using Contracts.Application.Reporting;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Win32;

public interface IReportFileDialogService{public string? Save(ReportExportFormat format);public string? SavePdf();}
public sealed class ReportFileDialogService:IReportFileDialogService
{
    public string? Save(ReportExportFormat format)=>Show(format==ReportExportFormat.Csv?"CSV UTF-8 (*.csv)|*.csv":"Excel (*.xlsx)|*.xlsx",format==ReportExportFormat.Csv?"csv":"xlsx");
    public string? SavePdf()=>Show("PDF (*.pdf)|*.pdf","pdf");
    private static string? Show(string filter,string extension){SaveFileDialog dialog=new(){Filter=filter,DefaultExt=extension,AddExtension=true,OverwritePrompt=true};return dialog.ShowDialog()==true?dialog.FileName:null;}
}
public sealed class WebView2ReportExporter(ReportHtmlRenderer renderer,WebView2RuntimeStatus runtimeStatus):IReportExporter
{
    public ReportExportFormat Format=>ReportExportFormat.Pdf;
    public Task ExportAsync(ReportDocument report,string destinationPath,CancellationToken ct)
    {if(!runtimeStatus.IsAvailable)throw new InvalidOperationException(runtimeStatus.ActionableMessage);return ExportHtmlAsync(renderer.Render(report),destinationPath,ct);}
    private static async Task ExportHtmlAsync(string html,string destination,CancellationToken ct)
    {string path=Path.GetFullPath(destination);if(!string.Equals(Path.GetExtension(path),".pdf",StringComparison.OrdinalIgnoreCase))throw new ArgumentException("PDF destination must use .pdf.",nameof(destination));string? dir=Path.GetDirectoryName(path);if(string.IsNullOrEmpty(dir)||!Directory.Exists(dir))throw new DirectoryNotFoundException(dir);string temp=path+"."+Guid.NewGuid().ToString("N")+".tmp.pdf";WebView2 browser=new();Window host=new(){Width=1100,Height=800,Left=-32000,Top=-32000,ShowInTaskbar=false,WindowStyle=WindowStyle.None,Content=browser};try{host.Show();await browser.EnsureCoreWebView2Async();TaskCompletionSource navigation=new(TaskCreationOptions.RunContinuationsAsynchronously);browser.NavigationCompleted+=(_,args)=>{if(args.IsSuccess)navigation.TrySetResult();else navigation.TrySetException(new InvalidOperationException($"Report preview navigation failed: {args.WebErrorStatus}."));};browser.NavigateToString(html);await navigation.Task.WaitAsync(ct);bool ok=await browser.CoreWebView2.PrintToPdfAsync(temp).WaitAsync(ct);if(!ok)throw new IOException("WebView2 did not create the PDF.");File.Move(temp,path,true);}finally{host.Close();if(File.Exists(temp))File.Delete(temp);}}
}
