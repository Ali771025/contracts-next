namespace Contracts.Desktop;

public partial class BootstrapWindow : Window
{
    private readonly BootstrapViewModel viewModel;
    public BootstrapWindow(BootstrapViewModel viewModel)
    {
        this.viewModel = viewModel;
        InitializeComponent(); DataContext = viewModel; viewModel.Created += HandleCreated;
    }
    private async void HandleCreate(object sender, RoutedEventArgs e) => await viewModel.SubmitAsync(PasswordInput.Password);
    private void HandleCreated(object? sender, EventArgs e) { DialogResult = true; Close(); }
    protected override void OnClosed(EventArgs e) { viewModel.Created -= HandleCreated; PasswordInput.Clear(); base.OnClosed(e); }
}
