namespace Contracts.Desktop;

public sealed class LoginViewModel(LoginUseCase useCase) : ObservableObject
{
    private string username = string.Empty;
    private string error = string.Empty;
    private bool isBusy;

    public event EventHandler<LoginStatus>? Completed;
    public string Username { get => username; set => SetProperty(ref username, value); }
    public string Error { get => error; private set => SetProperty(ref error, value); }
    public bool IsBusy { get => isBusy; private set => SetProperty(ref isBusy, value); }

    public async Task SubmitAsync(string password)
    {
        if (IsBusy) return;
        IsBusy = true;
        try
        {
            LoginResult result = await useCase.ExecuteAsync(Username, password, CancellationToken.None);
            if (result.Status == LoginStatus.Failed) Error = result.Message;
            else Completed?.Invoke(this, result.Status);
        }
        finally { IsBusy = false; }
    }
}
