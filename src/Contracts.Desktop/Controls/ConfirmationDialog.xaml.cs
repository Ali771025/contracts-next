namespace Contracts.Desktop.Controls;

using Contracts.Application.Presentation;

public partial class ConfirmationDialog : Window
{
    public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
        nameof(Message), typeof(string), typeof(ConfirmationDialog), new PropertyMetadata(string.Empty));
    public ConfirmationDialog() => InitializeComponent();
    public string Message { get => (string)GetValue(MessageProperty); set => SetValue(MessageProperty, value); }
    public DirtyDecision Decision { get; private set; } = DirtyDecision.Cancel;
    private void Save_Click(object sender, RoutedEventArgs e) { Decision = DirtyDecision.Save; DialogResult = true; }
    private void Discard_Click(object sender, RoutedEventArgs e) { Decision = DirtyDecision.Discard; DialogResult = true; }
    private void Cancel_Click(object sender, RoutedEventArgs e) { Decision = DirtyDecision.Cancel; DialogResult = false; }
}

public sealed class WpfDirtyDecisionService : IDirtyDecisionService
{
    public Task<DirtyDecision> DecideAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ConfirmationDialog dialog = new()
        {
            Owner = System.Windows.Application.Current?.MainWindow,
            Message = "تغییرات ذخیره نشده‌اند. پیش از جابه‌جایی چه کاری انجام شود؟",
        };
        dialog.ShowDialog();
        return Task.FromResult(dialog.Decision);
    }
}

public sealed class WpfConfirmationService : IConfirmationService
{
    public Task<bool> ConfirmAsync(string message, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        MessageBoxResult result = MessageBox.Show(System.Windows.Application.Current?.MainWindow, message,
            "تأیید عملیات", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No,
            MessageBoxOptions.RtlReading | MessageBoxOptions.RightAlign);
        return Task.FromResult(result == MessageBoxResult.Yes);
    }
}
