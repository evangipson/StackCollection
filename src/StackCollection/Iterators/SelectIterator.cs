using System.Runtime.CompilerServices;

namespace StackCollection.Iterators;

/// <summary>
/// Implements the Select (projection) operation.
/// </summary>
public readonly ref struct SelectIterator<T, TDest, TState>(ref TState state, Func<T, TDest?> selector) : IQueryIterator<TDest, SelectIterator<T, TDest, TState>>
    where T : allows ref struct
    where TDest : allows ref struct
    where TState : IQueryIterator<T, TState>, allows ref struct
{
    private readonly Func<T, TDest?> _selector = selector;
    private readonly TState _state = state;

    private readonly ref TState State => ref Unsafe.AsRef(in _state);

    /// <inheritdoc />
    public static bool MoveNext(ref SelectIterator<T, TDest, TState> state, out TDest current)
    {
        // get the previous iterator to perform the previous steps
        while (TState.MoveNext(ref state.State, out T element))
        {
            // Apply the Select logic (selector)
            if (state._selector(element) is TDest result)
            {
                current = result;
                return true;
            }
        }
        current = default!;
        return false;
    }
}
