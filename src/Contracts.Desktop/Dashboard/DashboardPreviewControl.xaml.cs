namespace Contracts.Desktop.Dashboard;

using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;

public partial class DashboardPreviewControl : UserControl
{
    private bool initialized;
    public static readonly DependencyProperty HtmlProperty = DependencyProperty.Register(nameof(Html), typeof(string), typeof(DashboardPreviewControl), new PropertyMetadata(string.Empty, Changed));
    public static readonly DependencyProperty DrilldownCommandProperty = DependencyProperty.Register(nameof(DrilldownCommand), typeof(ICommand), typeof(DashboardPreviewControl));
    public DashboardPreviewControl() { InitializeComponent(); Loaded += HandleLoaded; }
    public string Html { get => (string)GetValue(HtmlProperty); set => SetValue(HtmlProperty, value); }
    public ICommand? DrilldownCommand { get => (ICommand?)GetValue(DrilldownCommandProperty); set => SetValue(DrilldownCommandProperty, value); }
    private async void HandleLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (initialized) { Navigate(); return; }
            await Browser.EnsureCoreWebView2Async();
            Browser.CoreWebView2.SetVirtualHostNameToFolderMapping("contractsnext.local",Path.Combine(AppContext.BaseDirectory,"Dashboard","Assets"),CoreWebView2HostResourceAccessKind.DenyCors);
            Browser.CoreWebView2.NavigationStarting += (_, args) => { if (!args.Uri.StartsWith("about:blank", StringComparison.Ordinal)) args.Cancel = true; };
            Browser.CoreWebView2.NewWindowRequested += (_, args) => args.Handled = true;
            Browser.CoreWebView2.WebMessageReceived += HandleMessage;
            initialized = true;
            Navigate();
        }
        catch (Exception) { Browser.Visibility = Visibility.Collapsed; Fallback.Visibility = Visibility.Visible; FallbackMessage.Text = WebView2RuntimeStatus.Probe().ActionableMessage; }
    }
    private void HandleMessage(object? sender, CoreWebView2WebMessageReceivedEventArgs e)
    { string value = e.TryGetWebMessageAsString(); if (DrilldownCommand?.CanExecute(value) == true) DrilldownCommand.Execute(value); }
    private static void Changed(DependencyObject d, DependencyPropertyChangedEventArgs e) { if (d is DashboardPreviewControl view && view.IsLoaded) view.Navigate(); }
    private void Navigate() { if (Browser.CoreWebView2 is not null && !string.IsNullOrEmpty(Html)) Browser.NavigateToString(Html); }
}
