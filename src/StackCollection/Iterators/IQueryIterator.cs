namespace StackCollection.Iterators;

/// <summary>
/// Defines the contract for an iterator in the deferred stack query pipeline, that uses the
/// Static Abstract In Interfaces (SAII) pattern to avoid boxing.
/// <typeparam name="T">The type of the element produced by this iterator.</typeparam>
/// <typeparamref name="TState">The type of state for the previous iterator.</typeparam>
/// </summary>
public interface IQueryIterator<T, TState>
    where T : allows ref struct
    where TState : allows ref struct
{
    /// <summary>
    /// Advances the iterator and applies the current operation's query.
    /// </summary>
    /// <param name="state">The state of the previous iterator.</param>
    /// <param name="current">The element produced by the current operation.</param>
    /// <returns>
    /// <see langword="true"/> if an element was successfully produced; <see langword="false"/> otherwise.
    /// </returns>
    public static abstract bool MoveNext(ref TState state, out T current);
}
