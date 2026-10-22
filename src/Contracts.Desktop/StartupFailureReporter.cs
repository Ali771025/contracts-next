namespace Contracts.Desktop;

internal static class StartupFailureReporter
{
    private static readonly Action<ILogger, Exception> LogStartupFailure =
        LoggerMessage.Define(
            LogLevel.Critical,
            new EventId(1, nameof(LogStartupFailure)),
            "Desktop host startup failed.");

    public static string Report(Exception exception, IServiceProvider? services)
    {
        ArgumentNullException.ThrowIfNull(exception);

        string directory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ContractsNext",
            "logs");
        string path = Path.Combine(directory, "startup-failure.log");

        try
        {
            ILogger? logger = services?.GetService<ILoggerFactory>()?.CreateLogger(typeof(App));
            if (logger is not null)
            {
                LogStartupFailure(logger, exception);
            }

            Directory.CreateDirectory(directory);
            File.AppendAllText(
                path,
                $"{DateTimeOffset.UtcNow:O}{Environment.NewLine}{exception}{Environment.NewLine}");
        }
        catch (Exception reportingException)
        {
            Debug.WriteLine(reportingException);
        }

        return path;
    }
}
