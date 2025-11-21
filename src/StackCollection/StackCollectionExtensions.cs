using System.Runtime.CompilerServices;

namespace StackCollection;

/// <summary>
/// A <see langword="static"/> collection of methods to extend the
/// functionality of a stack collection.
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
    /// Creates a dynamically-sized buffer for a <see cref="StackCollection{T}"/>.
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
        ref byte memoryStart = ref System.Runtime.InteropServices.MemoryMarshal.GetReference(rawMemory);

        // create the collection wrapper and collection
        ref T typedStart = ref Unsafe.As<byte, T>(ref memoryStart);
        var collection = new StackCollection<T>(ref typedStart, capacity);

        // execute the delegate if one is provided
        return action == null
            ? collection
            : action.Invoke(collection);
    }
}
