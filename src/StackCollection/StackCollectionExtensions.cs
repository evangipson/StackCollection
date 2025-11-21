using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StackCollection;

/// <summary>
/// A <see langword="static"/> collection of methods to extend the functionality of a stack collection.
/// </summary>
public static class StackCollection
{
    /// <summary>
    /// A <see langword="delegate"/> that will accept a <see cref="StackCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of stack collection element.</typeparam>
    /// <param name="collection">The stack collection.</param>
    public delegate StackCollection<T> StackCollectionAction<T>(StackCollection<T> collection)
        where T : allows ref struct;

    /// <summary>
    /// Creates a dynamically-sized <see cref="StackCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of stack collection element.</typeparam>
    /// <param name="capacity">The maximum number of elements.</param>
    /// <param name="action">
    /// An optional <see langword="delegate"/> to customize the <see cref="StackCollection{T}"/>
    /// at create-time.
    /// </param>
    /// <returns>
    /// A <see langword="new"/> dynamically-sized <see cref="StackCollection{T}"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StackCollection<T> Create<T>(int capacity, StackCollectionAction<T>? action = null)
        where T : allows ref struct
    {
        // calculate required bytes
        int itemSize = Unsafe.SizeOf<T>();
        int totalBytes = itemSize * capacity;

        // allocate RAW stack memory (allowed with variable size!)
        Span<byte> rawMemory = stackalloc byte[totalBytes];

        // get the reference to the start of memory
        ref byte memoryStart = ref MemoryMarshal.GetReference(rawMemory);

        // create the collection wrapper and collection
        ref T typedStart = ref Unsafe.As<byte, T>(ref memoryStart);
        var collection = new StackCollection<T>(ref typedStart, capacity);

        // execute the delegate if one is provided
        return action == null
            ? collection
            : action.Invoke(collection);
    }

    /// <summary>
    /// Filters a stack collection for any that return <see langword="true"/> from <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The type of stack collection element.</typeparam>
    /// <param name="collection">The stack collection to filter.</param>
    /// <param name="predicate">Returns <see langword="true"/> when the element should be included.</param>
    /// <returns>A <see langword="new"/> stack collection with filtered elements.</returns>
    public static StackCollection<T> Where<T>(this StackCollection<T> collection, Func<T, bool> predicate)
    {
        // if there is no predicate, there is nothing to do, return an empty collection
        if (predicate == null)
        {
            return new();
        }

        // iterate once and add the elements that pass the predicate
        var included = Create<T>(capacity: collection.Capacity);
        foreach(var element in collection)
        {
            if(predicate?.Invoke(element) == true)
            {
                included.Add(element);
            }
        }

        // iterate once more to add filtered elements now that the actual capacity is known
        var filtered = Create<T>(capacity: included.Length);
        foreach(var element in included)
        {
            filtered.Add(element);
        }
        return filtered;
    }

    /// <summary>
    /// Filters a stack collection for any that return <see langword="true"/> from <paramref name="predicate"/>.
    /// </summary>
    /// <typeparam name="T">The type of stack collection element.</typeparam>
    /// <param name="collection">The stack collection to filter.</param>
    /// <param name="predicate">Returns <see langword="true"/> when the element should be included.</param>
    /// <returns>A <see langword="new"/> stack collection with filtered elements.</returns>
    public static StackCollection<L> Select<T, L>(this StackCollection<T> collection, Func<T, L?> predicate)
    {
        // if there is no predicate, there is nothing to do, return an empty collection
        if (predicate == null)
        {
            return new();
        }

        // iterate once to create the new elements
        var selected = Create<L>(capacity: collection.Capacity);
        foreach (var element in collection)
        {
            if (predicate(element) is L newElement)
            {
                selected.Add(newElement);
            }
        }

        // iterate once more to add selected elements now that the actual capacity is known
        var filtered = Create<L>(capacity: selected.Length);
        foreach (var element in selected)
        {
            filtered.Add(element);
        }
        return filtered;
    }
}
