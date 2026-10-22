using Contracts.Application.Security;

namespace Contracts.Application.Presentation;

public sealed record NavigationRoute(string Key, SecuritySection Section, SecurityAction Action = SecurityAction.View);

public interface INavigationTarget
{
    public Task OnNavigatedToAsync(CancellationToken cancellationToken);
    public Task OnNavigatedFromAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public interface INavigationTargetFactory
{
    public INavigationTarget Create(NavigationRoute route);
}

public interface INavigationService
{
    public INavigationTarget? Current { get; }
    public NavigationRoute? CurrentRoute { get; }
    public bool CanGoBack { get; }
    public event EventHandler? CurrentChanged;
    public Task NavigateAsync(NavigationRoute route, CancellationToken cancellationToken);
    public Task<bool> GoBackAsync(CancellationToken cancellationToken);
}

public sealed class NavigationService(IAuthorizationService authorization, INavigationTargetFactory factory) : INavigationService, IDisposable
{
    private readonly Stack<NavigationRoute> history = new();
    private CancellationTokenSource? activeNavigation;

    public INavigationTarget? Current { get; private set; }
    public NavigationRoute? CurrentRoute { get; private set; }
    public bool CanGoBack => history.Count > 0;
    public event EventHandler? CurrentChanged;

    public async Task NavigateAsync(NavigationRoute route, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(route);
        await authorization.RequireAsync(route.Section, route.Action, cancellationToken);
        if (CurrentRoute == route) return;
        NavigationRoute? previous = CurrentRoute;
        await ActivateAsync(route, cancellationToken);
        if (previous is not null) history.Push(previous);
    }

    public async Task<bool> GoBackAsync(CancellationToken cancellationToken)
    {
        if (!history.TryPeek(out NavigationRoute? route)) return false;
        await authorization.RequireAsync(route.Section, route.Action, cancellationToken);
        await ActivateAsync(route, cancellationToken);
        history.Pop();
        return true;
    }

    private async Task ActivateAsync(NavigationRoute route, CancellationToken externalToken)
    {
        activeNavigation?.Cancel();
        activeNavigation?.Dispose();
        activeNavigation = CancellationTokenSource.CreateLinkedTokenSource(externalToken);
        CancellationToken token = activeNavigation.Token;
        INavigationTarget target = factory.Create(route);
        if (Current is not null) await Current.OnNavigatedFromAsync(token);
        await target.OnNavigatedToAsync(token);
        token.ThrowIfCancellationRequested();
        Current = target;
        CurrentRoute = route;
        CurrentChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose() { activeNavigation?.Cancel(); activeNavigation?.Dispose(); }
}
