namespace Contracts.Desktop;

internal static class UiRouteSmokeRunner
{
    private static readonly System.Text.Json.JsonSerializerOptions JsonOptions=new(){WriteIndented=true};

    public static async Task<bool> RunAsync(Window window,ShellViewModel shell,string outputPath)
    {
        ArgumentNullException.ThrowIfNull(window);ArgumentNullException.ThrowIfNull(shell);
        if(!Path.IsPathFullyQualified(outputPath))throw new ArgumentException("UI route smoke output must be absolute.",nameof(outputPath));
        List<RouteSmokeResult> results=[];
        foreach(MenuItemViewModel item in shell.MenuItems)
        {
            try
            {
                await item.NavigateCommand.ExecuteAsync(null);
                window.UpdateLayout();
                await window.Dispatcher.InvokeAsync(window.UpdateLayout,System.Windows.Threading.DispatcherPriority.Render);
                if(shell.ErrorMessage is not null)throw new InvalidOperationException($"Navigation reported: {shell.ErrorMessage}");
                results.Add(new(item.Route.Key,"PASS",null));
            }
            catch(Exception exception)
            {
                results.Add(new(item.Route.Key,"FAIL",exception.GetType().Name+": "+exception.Message));
            }
            Write(outputPath,results);
        }
        return results.All(result=>result.Status=="PASS");
    }

    private static void Write(string outputPath,IReadOnlyCollection<RouteSmokeResult> results)
    {
        string? directory=Path.GetDirectoryName(outputPath);
        if(string.IsNullOrWhiteSpace(directory))throw new ArgumentException("UI route smoke output has no directory.",nameof(outputPath));
        Directory.CreateDirectory(directory);
        object evidence=new
        {
            schema_version="contracts-ui-route-smoke-v1",
            generated_at_utc=DateTimeOffset.UtcNow,
            route_count=results.Count,
            passed=results.Count(result=>result.Status=="PASS"),
            failed=results.Count(result=>result.Status=="FAIL"),
            routes=results,
        };
        string temporary=outputPath+".tmp";
        File.WriteAllText(temporary,System.Text.Json.JsonSerializer.Serialize(evidence,JsonOptions));
        File.Move(temporary,outputPath,true);
    }

    private sealed record RouteSmokeResult(string Route,string Status,string? Error);
}
