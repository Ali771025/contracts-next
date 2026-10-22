namespace Contracts.Desktop.Reporting;

using System.Globalization;
using System.Net;
using System.Text;
using Contracts.Application.Administration;
using Contracts.Application.Reporting;

public sealed class ReportHtmlRenderer
{
    private readonly string css;private readonly SystemSettingsState settings;
    public ReportHtmlRenderer(SystemSettingsState settings){this.settings=settings;string path=Path.Combine(AppContext.BaseDirectory,"Reporting","Assets","report.css");css=File.ReadAllText(path,Encoding.UTF8);}
    public string Render(ReportDocument report)
    {
        StringBuilder b=new();SystemSettings? branding=settings.Current;b.Append("<!doctype html><html lang=\"fa\" dir=\"rtl\"><head><meta charset=\"utf-8\"><meta name=\"viewport\" content=\"width=device-width\"><style>").Append(css).Append("</style></head><body>");
        if(branding is not null)b.Append("<div class=\"meta\">").Append(E(branding.CompanyName)).Append(" — ").Append(E(branding.ReportHeader)).Append("</div>");
        b.Append("<h1>").Append(E(report.Title)).Append("</h1><div class=\"meta\">").Append(E(report.ParameterSummary)).Append("\nشناسه snapshot: ").Append(report.SnapshotId.ToString("N")).Append(" | تولید UTC: ").Append(E(report.GeneratedAtUtc.ToString("O",CultureInfo.InvariantCulture))).Append("</div><div class=\"table-wrap\"><table><thead><tr>");
        foreach(ReportColumn c in report.Columns)b.Append("<th>").Append(E(c.Title)).Append("</th>");b.Append("</tr></thead><tbody>");foreach(ReportRow row in report.Rows){b.Append("<tr>");for(int i=0;i<row.Cells.Count;i++){string cls=report.Columns[i].Kind==ReportValueKind.Numeric?" class=\"numeric\"":"";b.Append("<td").Append(cls).Append('>').Append(E(row.Cells[i].Text)).Append("</td>");}b.Append("</tr>");}b.Append("</tbody><tfoot><tr>");foreach(ReportCell cell in report.Footer.Cells)b.Append("<td>").Append(E(cell.Text)).Append("</td>");return b.Append("</tr></tfoot></table></div></body></html>").ToString();
    }
    private static string E(string? value)=>WebUtility.HtmlEncode(value??string.Empty);
}
