namespace Contracts.Desktop;

public partial class LoginWindow : Window
{
    private readonly LoginViewModel viewModel;
    public LoginStatus? CompletionStatus { get; private set; }

    public LoginWindow(LoginViewModel viewModel)
    {
        this.viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        viewModel.Completed += HandleCompleted;
    }

    private async void HandleLogin(object sender, RoutedEventArgs e) => await viewModel.SubmitAsync(PasswordInput.Password);
    private void HandleCompleted(object? sender, LoginStatus status) { CompletionStatus = status; DialogResult = true; Close(); }
    protected override void OnClosed(EventArgs e) { viewModel.Completed -= HandleCompleted; PasswordInput.Clear(); base.OnClosed(e); }
}
