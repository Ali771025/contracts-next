namespace Contracts.Desktop;

public sealed class ChangePasswordViewModel(ChangePasswordUseCase useCase) : ObservableObject
{
    private string error = string.Empty;
    public event EventHandler? Changed;
    public string Error { get => error; private set => SetProperty(ref error, value); }
    public async Task SubmitAsync(string password)
    {
        PasswordPolicyResult result = await useCase.ExecuteAsync(password, CancellationToken.None);
        if (result.IsValid) Changed?.Invoke(this, EventArgs.Empty);
        else Error = string.Join(Environment.NewLine, result.Errors);
    }
}
