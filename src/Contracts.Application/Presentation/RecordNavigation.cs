namespace Contracts.Application.Presentation;

public enum RecordDirection { Previous, Next }
public enum DirtyDecision { Save, Discard, Cancel }
public sealed record RecordNavigationResult<TId>(bool Moved, TId CurrentId, string? Error = null);
public sealed record RecordNeighbor<TId>(bool Exists, TId Id);

public interface IRecordSequence<TId, in TFilter>
{
    public Task<RecordNeighbor<TId>> GetAdjacentAsync(TId currentId, RecordDirection direction, TFilter filter, string stableSort,
        CancellationToken cancellationToken);
}

public interface IEditableRecord
{
    public bool IsDirty { get; }
    public Task SaveAsync(CancellationToken cancellationToken);
    public void DiscardChanges();
}

public interface IDirtyDecisionService
{
    public Task<DirtyDecision> DecideAsync(CancellationToken cancellationToken);
}

public interface IConfirmationService
{
    public Task<bool> ConfirmAsync(string message, CancellationToken cancellationToken);
}

public sealed class RecordNavigator<TId, TFilter>(IRecordSequence<TId, TFilter> sequence,
    IEditableRecord editor, IDirtyDecisionService dirtyDecision)
    where TId : notnull
{
    public async Task<RecordNavigationResult<TId>> MoveAsync(TId currentId, RecordDirection direction,
        TFilter filter, string stableSort, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(stableSort)) throw new ArgumentException("A stable sort/key is required.", nameof(stableSort));
        if (editor.IsDirty)
        {
            DirtyDecision decision = await dirtyDecision.DecideAsync(cancellationToken);
            if (decision == DirtyDecision.Cancel) return new(false, currentId);
            if (decision == DirtyDecision.Discard) editor.DiscardChanges();
            if (decision == DirtyDecision.Save)
            {
                try { await editor.SaveAsync(cancellationToken); }
                catch (Exception exception) when (exception is not OperationCanceledException)
                {
                    return new(false, currentId, exception.Message);
                }
            }
        }
        RecordNeighbor<TId> adjacent = await sequence.GetAdjacentAsync(currentId, direction, filter, stableSort, cancellationToken);
        return adjacent.Exists ? new(true, adjacent.Id) : new(false, currentId);
    }
}
