using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace StoryblokSharp.Utilities.RichText;

/// <summary>
/// Provides pooling of StringBuilder instances to improve performance
/// and reduce memory allocations in string-heavy operations
/// </summary>
public sealed class StringBuilderCache
{
    private readonly ObjectPool<StringBuilder> _pool;
    private readonly int _maxPoolSize;
    private readonly int _initialCapacity;

    /// <summary>
    /// Initializes a new instance of StringBuilderCache with default settings
    /// </summary>
public StringBuilderCache() : this(128, 32, 256) { }

    /// <summary>
    /// Initializes a new instance of StringBuilderCache with specified settings
    /// </summary>
    /// <param name="initialCapacity">Initial capacity of new StringBuilder instances</param>
    /// <param name="maxPoolSize">Maximum number of instances to keep in pool</param>
    /// <param name="maxRetainedCapacity">Maximum capacity to retain before returning to pool</param>
    public StringBuilderCache(int initialCapacity, int maxPoolSize, int maxRetainedCapacity)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(initialCapacity);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxPoolSize);
        if (maxRetainedCapacity < initialCapacity) throw new ArgumentException("Must be >= initialCapacity", nameof(maxRetainedCapacity));

        _initialCapacity = initialCapacity;
        _maxPoolSize = maxPoolSize;

        var policy = new StringBuilderPooledObjectPolicy(initialCapacity, maxRetainedCapacity);
        _pool = new DefaultObjectPool<StringBuilder>(policy, maxPoolSize);
    }

    /// <summary>
    /// Acquires a StringBuilder from the pool or creates a new one if needed
    /// </summary>
    /// <returns>A StringBuilder instance ready for use</returns>
    public StringBuilder Acquire()
    {
        var sb = _pool.Get();
        Debug.Assert(sb.Length == 0); // Should always be empty when acquired
        return sb;
    }

    /// <summary>
    /// Returns a StringBuilder to the pool after clearing it
    /// </summary>
    /// <param name="builder">The StringBuilder to return to the pool</param>
    public void Release(StringBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Clear();
        _pool.Return(builder);
    }

    /// <summary>
    /// Performs a string operation using a pooled StringBuilder
    /// </summary>
    /// <param name="action">Action to perform with the StringBuilder</param>
    /// <returns>The resulting string</returns>
    public string UseStringBuilder(Action<StringBuilder> action)
    {
        var sb = Acquire();
        try
        {
            action(sb);
            return sb.ToString();
        }
        finally
        {
            Release(sb);
        }
    }

    /// <summary>
    /// Performs an async string operation using a pooled StringBuilder
    /// </summary>
    /// <param name="action">Async action to perform with the StringBuilder</param>
    /// <returns>The resulting string</returns>
    public async Task<string> UseStringBuilderAsync(Func<StringBuilder, Task> action)
    {
        var sb = Acquire();
        try
        {
            await action(sb).ConfigureAwait(false);
            return sb.ToString();
        }
        finally
        {
            Release(sb);
        }
    }
}

/// <summary>
/// Policy for creating and managing pooled StringBuilder instances
/// </summary>
internal sealed class StringBuilderPooledObjectPolicy : PooledObjectPolicy<StringBuilder>
{
    private readonly int _initialCapacity;
    private readonly int _maxRetainedCapacity;

    public StringBuilderPooledObjectPolicy(int initialCapacity, int maxRetainedCapacity)
    {
        _initialCapacity = initialCapacity;
        _maxRetainedCapacity = maxRetainedCapacity;
    }

    public override StringBuilder Create()
    {
        return new StringBuilder(_initialCapacity);
    }

    public override bool Return(StringBuilder obj)
    {
        if (obj.Capacity > _maxRetainedCapacity)
        {
            // Don't keep StringBuilder instances that have grown too large
            return false;
        }

        obj.Clear();
        return true;
    }
}