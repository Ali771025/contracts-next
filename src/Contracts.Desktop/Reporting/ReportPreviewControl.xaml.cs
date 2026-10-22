namespace Contracts.Desktop.Reporting;

using System.Windows.Controls;

public partial class ReportPreviewControl:UserControl
{
    public static readonly DependencyProperty HtmlProperty=DependencyProperty.Register(nameof(Html),typeof(string),typeof(ReportPreviewControl),new PropertyMetadata(string.Empty,Changed));
    public ReportPreviewControl(){InitializeComponent();Loaded+=HandleLoaded;}
    public string Html{get=>(string)GetValue(HtmlProperty);set=>SetValue(HtmlProperty,value);}
    private async void HandleLoaded(object sender,RoutedEventArgs e){try{await Browser.EnsureCoreWebView2Async();Navigate();}catch(Exception){Browser.Visibility=Visibility.Collapsed;Fallback.Visibility=Visibility.Visible;FallbackMessage.Text=WebView2RuntimeStatus.Probe().ActionableMessage;}}
    private static void Changed(DependencyObject d,DependencyPropertyChangedEventArgs e){if(d is ReportPreviewControl view&&view.IsLoaded)view.Navigate();}
    private void Navigate(){if(Browser.CoreWebView2 is not null&&!string.IsNullOrEmpty(Html))Browser.NavigateToString(Html);}
}
