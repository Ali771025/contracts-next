namespace Contracts.Desktop;

public sealed class BootstrapViewModel(BootstrapAdministratorUseCase useCase) : ObservableObject
{
    private string username = string.Empty;
    private string displayName = string.Empty;
    private string error = string.Empty;
    private bool isBusy;
    public event EventHandler? Created;
    public string Username { get => username; set => SetProperty(ref username, value); }
    public string DisplayName { get => displayName; set => SetProperty(ref displayName, value); }
    public string Error { get => error; private set => SetProperty(ref error, value); }
    public bool IsBusy { get => isBusy; private set => SetProperty(ref isBusy, value); }

    public async Task SubmitAsync(string password)
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            BootstrapResult result = await useCase.ExecuteAsync(new(Username, DisplayName, password), CancellationToken.None);
            if (result.Created) Created?.Invoke(this, EventArgs.Empty);
            else Error = string.Join(Environment.NewLine, result.Errors);
        }
        finally { IsBusy = false; }
    }
}
