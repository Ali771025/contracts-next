namespace Contracts.Desktop;

public partial class MainWindow : Window
{
    private readonly ShellViewModel viewModel;
    public MainWindow(ShellViewModel viewModel)
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        this.viewModel = viewModel;
        InitializeComponent();
        DataContext = viewModel;
        viewModel.LogoutRequested += HandleLogoutRequested;
        Closed += HandleClosed;
    }

    public event EventHandler? LogoutRequested;
    private void HandleLogoutRequested(object? sender, EventArgs e) => LogoutRequested?.Invoke(this, EventArgs.Empty);
    private void HandleClosed(object? sender, EventArgs e) => viewModel.LogoutRequested -= HandleLogoutRequested;
}
