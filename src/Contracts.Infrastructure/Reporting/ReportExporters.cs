namespace Contracts.Infrastructure.Reporting;

using System.Text;
using ClosedXML.Excel;
using Contracts.Application.Reporting;

public sealed class CsvReportExporter:IReportExporter
{
    public ReportExportFormat Format=>ReportExportFormat.Csv;
    public async Task ExportAsync(ReportDocument report,string destinationPath,CancellationToken ct)
    {string path=Validate(destinationPath,".csv"),temp=path+"."+Guid.NewGuid().ToString("N")+".tmp";try{{await using StreamWriter writer=new(temp,false,new UTF8Encoding(true));await Row(writer,[report.Title],ct);await Row(writer,[report.ParameterSummary],ct);await Row(writer,["Generated UTC",report.GeneratedAtUtc.ToString("O",CultureInfo.InvariantCulture)],ct);await writer.WriteLineAsync();await Row(writer,report.Columns.Select(x=>x.Title),ct);foreach(ReportRow row in report.Rows){ct.ThrowIfCancellationRequested();await Row(writer,row.Cells.Select(Cell),ct);}await Row(writer,report.Footer.Cells.Select(Cell),ct);await writer.FlushAsync(ct);}File.Move(temp,path,true);}catch{if(File.Exists(temp))File.Delete(temp);throw;}}
    private static string Cell(ReportCell cell)=>cell.IntegerValue?.ToString(CultureInfo.InvariantCulture)??Protect(cell.Text);
    private static string Protect(string value){string trimmed=value.TrimStart();return trimmed.Length>0&&(trimmed[0] is'='or'+'or'-'or'@'||trimmed[0] is'\t'or'\r')?"'"+value:value;}
    private static async Task Row(StreamWriter writer,IEnumerable<string> values,CancellationToken ct)=>await writer.WriteLineAsync(string.Join(',',values.Select(Escape)).AsMemory(),ct);
    private static string Escape(string value)=>value.IndexOfAny([',','"','\r','\n'])>=0?'"'+value.Replace("\"","\"\"")+'"':value;
    internal static string Validate(string destination,string extension){if(string.IsNullOrWhiteSpace(destination))throw new ArgumentException("Destination is required.",nameof(destination));string path=Path.GetFullPath(destination);if(!string.Equals(Path.GetExtension(path),extension,StringComparison.OrdinalIgnoreCase))throw new ArgumentException($"Destination must use {extension}.",nameof(destination));string? directory=Path.GetDirectoryName(path);if(string.IsNullOrEmpty(directory)||!Directory.Exists(directory))throw new DirectoryNotFoundException(directory);return path;}
}

public sealed class ClosedXmlReportExporter:IReportExporter
{
    public ReportExportFormat Format=>ReportExportFormat.Xlsx;
    public Task ExportAsync(ReportDocument report,string destinationPath,CancellationToken ct)
    {ct.ThrowIfCancellationRequested();string path=CsvReportExporter.Validate(destinationPath,".xlsx"),temp=path+"."+Guid.NewGuid().ToString("N")+".tmp.xlsx";try{using XLWorkbook book=new();IXLWorksheet sheet=book.AddWorksheet("Report");sheet.RightToLeft=true;sheet.Cell(1,1).Value=report.Title;sheet.Cell(2,1).Value=report.ParameterSummary;int row=4;for(int i=0;i<report.Columns.Count;i++)sheet.Cell(row,i+1).Value=report.Columns[i].Title;sheet.Row(row).Style.Font.Bold=true;foreach(ReportRow item in report.Rows){row++;for(int i=0;i<item.Cells.Count;i++){ReportCell cell=item.Cells[i];if(cell.IntegerValue.HasValue)sheet.Cell(row,i+1).Value=cell.IntegerValue.Value;else sheet.Cell(row,i+1).SetValue(cell.Text);}}row++;for(int i=0;i<report.Footer.Cells.Count;i++){ReportCell cell=report.Footer.Cells[i];if(cell.IntegerValue.HasValue)sheet.Cell(row,i+1).Value=cell.IntegerValue.Value;else sheet.Cell(row,i+1).SetValue(cell.Text);}sheet.Row(row).Style.Font.Bold=true;sheet.SheetView.FreezeRows(4);sheet.Columns().AdjustToContents(8,60);ct.ThrowIfCancellationRequested();book.SaveAs(temp);File.Move(temp,path,true);return Task.CompletedTask;}catch{if(File.Exists(temp))File.Delete(temp);throw;}}
}
