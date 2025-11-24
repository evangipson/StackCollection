using System.Runtime.CompilerServices;

namespace StackCollection.Iterators;

/// <summary>
/// Implements the Where (filtering) operation.
/// </summary>
public readonly ref struct WhereIterator<T, TState>(ref TState state, Func<T, bool> predicate) : IQueryIterator<T, WhereIterator<T, TState>>
    where T : allows ref struct
    where TState : IQueryIterator<T, TState>, allows ref struct
{
    private readonly Func<T, bool> _predicate = predicate;
    private readonly TState _state = state;

    private readonly ref TState State => ref Unsafe.AsRef(in _state);

    /// <inheritdoc />
    public static bool MoveNext(ref WhereIterator<T, TState> state, out T current)
    {
        // get the previous iterator to perform the previous steps
        while (TState.MoveNext(ref state.State, out T element))
        {
            // apply the where logic (predicate)
            if (state._predicate(element))
            {
                current = element;
                return true;
            }
        }
        current = default!;
        return false;
    }
}
