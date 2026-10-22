namespace Contracts.Desktop.Dashboard;

using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Contracts.Application.Dashboard;

public sealed class DashboardHtmlRenderer
{
    private readonly string assetName;
    public DashboardHtmlRenderer()
    {
        assetName="echarts-6.1.0.min.js";
        string path = Path.Combine(AppContext.BaseDirectory, "Dashboard", "Assets", assetName);
        if (!File.Exists(path)) throw new FileNotFoundException("Pinned ECharts asset is missing.", path);
    }

    public string Render(DashboardSnapshot snapshot)
    {
        StringBuilder b = new("<!doctype html><html lang=\"fa\" dir=\"rtl\"><head><meta charset=\"utf-8\"><meta http-equiv=\"Content-Security-Policy\" content=\"default-src 'none'; style-src 'unsafe-inline'; script-src 'unsafe-inline' https://contractsnext.local\"><meta name=\"viewport\" content=\"width=device-width\"><style>");
        b.Append("body{font-family:Tahoma,Segoe UI,sans-serif;background:#f4f7f9;color:#17242b;margin:0;padding:16px}.meta{color:#647780;font-size:12px;margin-bottom:12px}.cards{display:grid;grid-template-columns:repeat(auto-fit,minmax(170px,1fr));gap:10px}.card{background:#fff;border:1px solid #dbe5e9;border-radius:10px;padding:12px;cursor:pointer;text-align:right}.card:hover{border-color:#087f8c}.label{color:#60737b;font-size:12px}.value{font-size:22px;font-weight:700;margin-top:8px}.charts{display:grid;grid-template-columns:repeat(auto-fit,minmax(320px,1fr));gap:12px;margin-top:12px}.chart,.panel{height:300px;background:#fff;border:1px solid #dbe5e9;border-radius:10px}.panel{height:auto;padding:12px;margin-top:12px}table{width:100%;border-collapse:collapse}th,td{padding:7px;border-bottom:1px solid #e7eef1;text-align:right}th{color:#52656d;font-size:12px}.na{color:#8a6d1d}.critical{color:#a02020;font-weight:700}</style><script src=\"https://contractsnext.local/").Append(assetName).Append("\"></script></head><body>");
        b.Append("<div class=\"meta\">").Append(E(snapshot.FilterSummary)).Append(" | Snapshot: ").Append(snapshot.SnapshotId.ToString("N")).Append(" | ").Append(snapshot.QueryCount.ToString(CultureInfo.InvariantCulture)).Append(" query | ").Append(snapshot.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture)).Append(" ms</div><div class=\"cards\">");
        foreach (DashboardMetric m in snapshot.MainMetrics)
            b.Append("<button class=\"card\" onclick=\"openDrill('").Append(m.Drilldown).Append("')\"><div class=\"label\">").Append(E(m.TitleFa)).Append("</div><div class=\"value\">").Append(E(Format(m.Value, m.Unit))).Append("</div></button>");
        b.Append("</div><div class=\"cards\" style=\"margin-top:10px\">");
        foreach (DashboardRatio m in snapshot.AdvancedMetrics)
            b.Append("<button class=\"card\" onclick=\"openDrill('").Append(m.Drilldown).Append("')\"><div class=\"label\">").Append(E(m.TitleFa)).Append("</div><div class=\"value ").Append(m.Status == KpiValueStatus.NotApplicable ? "na" : "").Append("\">").Append(E(m.Status == KpiValueStatus.NotApplicable ? "قابل محاسبه نیست" : Format(m.Value!.Value, m.Unit))).Append("</div></button>");
        b.Append("</div><div class=\"charts\"><div id=\"types\" class=\"chart\"></div><div id=\"balances\" class=\"chart\"></div></div>");
        Table(b, "بیشترین مطالبات", snapshot.TopReceivables, true); Table(b, "بیشترین بدهی‌ها", snapshot.TopPayables, false);
        b.Append("<div class=\"panel\"><strong>هشدارهای باز</strong><table><thead><tr><th>قرارداد</th><th>عنوان</th><th>اولویت</th><th>سررسید</th></tr></thead><tbody>");
        foreach (DashboardAlertDetail a in snapshot.OpenAlerts) b.Append("<tr><td>").Append(E(a.ContractNo)).Append("</td><td>").Append(E(a.Title)).Append("</td><td class=\"").Append(a.PriorityCode == "CRITICAL" ? "critical" : "").Append("\">").Append(E(a.PriorityTitle)).Append("</td><td>").Append(E(a.DueDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))).Append("</td></tr>");
        b.Append("</tbody></table></div><script>const typeNames=").Append(JsonSerializer.Serialize(snapshot.ContractTypes.Select(x => x.TitleFa))).Append(";const counts=").Append(JsonSerializer.Serialize(snapshot.ContractTypes.Select(x => x.ContractCount))).Append(";const active=").Append(JsonSerializer.Serialize(snapshot.ContractTypes.Select(x => x.ActiveCount))).Append(";const recv=").Append(JsonSerializer.Serialize(snapshot.ContractTypes.Select(x => x.ReceivableRial))).Append(";const pay=").Append(JsonSerializer.Serialize(snapshot.ContractTypes.Select(x => x.PayableRial))).Append(";function openDrill(k){window.chrome.webview.postMessage(k)};const common={animation:false,textStyle:{fontFamily:'Tahoma'},tooltip:{trigger:'axis'},legend:{bottom:4},grid:{left:65,right:20,top:45,bottom:55}};const t=echarts.init(document.getElementById('types'));t.setOption({...common,title:{text:'قراردادها به تفکیک نوع',right:12},xAxis:{type:'category',data:typeNames},yAxis:{type:'value'},series:[{name:'کل',type:'bar',data:counts},{name:'فعال',type:'bar',data:active}]});const f=echarts.init(document.getElementById('balances'));f.setOption({...common,title:{text:'مانده مالی به تفکیک نوع',right:12},xAxis:{type:'category',data:typeNames},yAxis:{type:'value'},series:[{name:'طلب',type:'bar',data:recv},{name:'بدهی',type:'bar',data:pay}]});addEventListener('resize',()=>{t.resize();f.resize()});</script></body></html>");
        return b.ToString();
    }

    private static void Table(StringBuilder b, string title, IEnumerable<DashboardContractDetail> rows, bool receivable)
    {
        b.Append("<div class=\"panel\"><strong>").Append(E(title)).Append("</strong><table><thead><tr><th>شماره</th><th>عنوان</th><th>طرف</th><th>مبلغ (ریال)</th></tr></thead><tbody>");
        foreach (DashboardContractDetail x in rows) b.Append("<tr><td>").Append(E(x.ContractNo)).Append("</td><td>").Append(E(x.Title)).Append("</td><td>").Append(E(x.PartyName)).Append("</td><td>").Append(E((receivable ? x.ReceivableRial : x.PayableRial).ToString("N0", CultureInfo.GetCultureInfo("fa-IR")))).Append("</td></tr>");
        b.Append("</tbody></table></div>");
    }
    private static string Format(long value, string unit) => unit == "count" ? value.ToString("N0", CultureInfo.GetCultureInfo("fa-IR")) : value.ToString("N0", CultureInfo.GetCultureInfo("fa-IR")) + " ریال";
    private static string Format(decimal value, string unit) => unit == "percent" ? value.ToString("N2", CultureInfo.GetCultureInfo("fa-IR")) + "٪" : value.ToString("N0", CultureInfo.GetCultureInfo("fa-IR")) + " ریال";
    private static string E(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);
}
