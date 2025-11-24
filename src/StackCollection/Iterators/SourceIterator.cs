using System.Runtime.CompilerServices;

namespace StackCollection.Iterators;

/// <summary>
/// The starting iterator that simply enumerates a <see cref="StackCollection{T}"/>.
/// </summary>
/// <typeparam name="T">The type of elements to iterate.</typeparam>
public readonly ref struct SourceIterator<T> : IQueryIterator<T, SourceIterator<T>>
    where T : allows ref struct
{
    private readonly StackCollection<T>.Enumerator _enumerator;

    private readonly ref StackCollection<T>.Enumerator Enumerator => ref Unsafe.AsRef(in _enumerator);

    public SourceIterator(StackCollection<T> source)
    {
        _enumerator = source.GetEnumerator();
    }

    /// <inheritdoc />
    public static bool MoveNext(ref SourceIterator<T> state, out T current)
    {
        // check if we are still within bounds
        if (state.Enumerator.MoveNext())
        {
            // get the element
            ref T elementRef = ref state.Enumerator.Current;

            // copy the value out
            current = elementRef;
            return true;
        }

        current = default!;
        return false;
    }
}
