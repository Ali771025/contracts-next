namespace Contracts.Desktop;

using System.Diagnostics;
using System.Reflection;
using Microsoft.Web.WebView2.Core;

public sealed record ReleaseIdentity(string SemanticVersion, string InformationalVersion, string FileVersion)
{
    public string Display => $"{SemanticVersion} ({InformationalVersion})";

    public static ReleaseIdentity Current()
    {
        Assembly assembly = Assembly.GetEntryAssembly() ?? typeof(ReleaseIdentity).Assembly;
        string semantic = assembly.GetName().Version?.ToString(3) ?? "unknown";
        string informational = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? semantic;
        string file = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion ?? semantic;
        return new(semantic, informational, file);
    }
}

public sealed record WebView2RuntimeStatus(bool IsAvailable, string? RuntimeVersion, string ActionableMessage)
{
    public static WebView2RuntimeStatus Probe()
    {
        try
        {
            string version = CoreWebView2Environment.GetAvailableBrowserVersionString();
            return new(true, version, $"Microsoft Edge WebView2 Runtime {version}");
        }
        catch (Exception exception) when (exception is WebView2RuntimeNotFoundException or InvalidOperationException or COMException)
        {
            return new(false, null,
                "Microsoft Edge WebView2 Runtime نصب یا قابل راه‌اندازی نیست. پیش‌نمایش Dashboard/PDF غیرفعال است؛ CSV و XLSX همچنان در دسترس‌اند. Runtime رسمی Microsoft را نصب و برنامه را دوباره اجرا کنید؛ نصب خودکار انجام نمی‌شود.");
        }
    }
}
