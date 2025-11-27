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
    /// Continues the query chain with a Where (filtering) operation, the new element type is <typeparamref name="TFinalDest"/>.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query<TSource, TDest, TFinalDest, WhereIterator<TFinalDest, TState>> Where(Func<TFinalDest, bool> predicate)
    {
        WhereIterator<TFinalDest, TState> whereIterator = new(ref State, predicate);
        return new(whereIterator);
    }

    /// <summary>
    /// Continues the query chain with a Select (projection) operation, changing the output type to <typeparamref name="TResult"/>.
    /// </summary>
    /// <typeparam name="TResult">The new output type of the query chain.</typeparam>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Query<TSource, TDest, TResult, SelectIterator<TFinalDest, TResult, TState>> Select<TResult>(Func<TFinalDest, TResult?> selector)
        where TResult : allows ref struct
    {
        SelectIterator<TFinalDest, TResult, TState> selectIterator = new(ref State, selector);
        return new(selectIterator);
    }

    /// <summary>
    /// Executes the deferred query pipeline and checks if any element exists that satisfies the optional predicate.
    /// The execution stops on the first match.
    /// </summary>
    /// <param name="predicate">An optional predicate to filter elements.</param>
    /// <returns>
    /// <see langword="true"/> if at least one element satisfies the condition, <see langword="false"/> otherwise.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly bool Any(Func<TFinalDest, bool>? predicate = null)
    {
        // if no predicate, we just check if MoveNext produces any result
        if (predicate == null)
        {
            // TState is the final iterator type (i.e.: WhereIterator or SelectIterator).
            return TState.MoveNext(ref State, out _);
        }

        // execute the query, checking the predicate for each element produced
        while (TState.MoveNext(ref State, out TFinalDest element))
        {
            // found a match, stop immediately
            if (predicate(element))
            {
                return true;
            }
        }

        // no elements matched the predicate
        return false;
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

    /// <summary>
    /// Gets the enumerator for this deferred query; required for the foreach pattern.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Enumerator GetEnumerator() => new(ref State);

    /// <summary>
    /// Enumerator struct for the Query, allowing foreach loop execution.
    /// </summary>
    public ref struct Enumerator
    {
        private readonly TState _state;
        private TFinalDest _current;

        private readonly ref TState State => ref Unsafe.AsRef(in _state);

        public readonly ref TFinalDest Current => ref Unsafe.AsRef(in _current);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal Enumerator(ref TState state)
        {
            _state = state;
            _current = default!;
        }

        /// <summary>
        /// Advances the enumerator to the next element of the query result.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            // call the static MoveNext on the final iterator type, passing the mutable state reference
            return TState.MoveNext(ref State, out _current);
        }

        public readonly void Dispose() { }
    }
}