using System.Runtime.CompilerServices;
using StackCollection.Iterators;

namespace StackCollection.Queries;

/// <summary>
/// The single, generic ref struct representing a deferred query pipeline.
/// <para>
/// This handles all chaining operations (.Where, .Select) and the final execution (.ToStackCollection).
/// </para>
/// </summary>
/// <typeparam name="TSource">The initial element type of the query (from the source stack collection).</typeparam>
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public readonly ref struct Query<TSource, TDest, TFinalDest, TState>(TState state)
    where TSource : allows ref struct
    where TDest : allows ref struct
    where TFinalDest : allows ref struct
    where TState : IQueryIterator<TFinalDest, TState>, allows ref struct
{
    private readonly TState _state = state;

    private readonly ref TState State => ref Unsafe.AsRef(in _state);

    /// <summary>
    /// Continues the query chain with a Where (filtering) operation, the new element type remains <typeparamref name="TSource"/>.
    /// </summary>
    public readonly Query<TSource, TDest, TFinalDest, WhereIterator<TFinalDest, TState>> Where(Func<TFinalDest, bool> predicate)
    {
        WhereIterator<TFinalDest, TState> whereIterator = new(ref State, predicate);
        return new(whereIterator);
    }

    /// <summary>
    /// Continues the query chain with a Select (projection) operation, changing the output type to <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The new output type of the query chain.</typeparam>
    public readonly Query<TSource, TDest, TResult, SelectIterator<TFinalDest, TResult, TState>> Select<TResult>(Func<TFinalDest, TResult?> selector)
        where TResult : allows ref struct
    {
        SelectIterator<TFinalDest, TResult, TState> selectIterator = new(ref State, selector);
        return new(selectIterator);
    }

    /// <summary>
    /// Executes the deferred query and writes the results to the specified destination collection.
    /// The result collection type MUST match the current query output type, <typeparamref name="TFinalDest"/>.
    /// </summary>
    /// <param name="results">The consumer-allocated stack collection where the results are stored.</param>
    /// <returns>A reference to the modified <paramref name="results"/> collection.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref StackCollection<TFinalDest> ToStackCollection(ref StackCollection<TFinalDest> results)
    {
        results.Clear();

        if (results.Capacity == 0)
        {
            return ref results;
        }

        // Execute the query by repeatedly calling MoveNext on the final iterator
        while (TState.MoveNext(ref State, out TFinalDest element))
        {
            results.Add(element);
        }

        return ref results;
    }
}