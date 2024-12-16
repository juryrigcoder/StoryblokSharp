namespace StoryblokSharp.Utilities;

/// <summary>
/// Represents a queue of throttled actions
/// </summary>
public class ThrottleQueue<T>
{
    private readonly Queue<T> _queue = new();
    private readonly SemaphoreSlim _semaphore;
    private readonly int _limit;
    private readonly int _interval;
    private readonly CancellationTokenSource _cts = new();

    public ThrottleQueue(int limit, int interval)
    {
        _limit = limit;
        _interval = interval;
        _semaphore = new SemaphoreSlim(limit);
    }

    public async Task<TResult> EnqueueAsync<TResult>(
        Func<T, Task<TResult>> action,
        T item,
        CancellationToken cancellationToken = default)
    {
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            _cts.Token,
            cancellationToken);

        await _semaphore.WaitAsync(linkedCts.Token);

        try
        {
            var result = await action(item);
            await Task.Delay(_interval, linkedCts.Token);
            return result;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public void Abort()
    {
        _cts.Cancel();
        _queue.Clear();
    }
}