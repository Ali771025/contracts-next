namespace Contracts.Desktop;

public partial class ChangePasswordWindow : Window
{
    private readonly ChangePasswordViewModel viewModel;
    public ChangePasswordWindow(ChangePasswordViewModel viewModel)
    {
        this.viewModel = viewModel; InitializeComponent(); DataContext = viewModel; viewModel.Changed += HandleChanged;
    }
    private async void HandleChange(object sender, RoutedEventArgs e) => await viewModel.SubmitAsync(PasswordInput.Password);
    private void HandleChanged(object? sender, EventArgs e) { DialogResult = true; Close(); }
    protected override void OnClosed(EventArgs e) { viewModel.Changed -= HandleChanged; PasswordInput.Clear(); base.OnClosed(e); }
}
