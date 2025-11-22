using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StackCollection;

/// <summary>
/// A <see langword="static"/> collection of methods to extend the functionality of a stack collection.
/// </summary>
public static class StackCollection
{
    /// <summary>
    /// A <see langword="static"/> collection of methods to extend functionality of any stack collection.
    /// </summary>
    /// <typeparam name="T">The type of elements in the stack collection.</typeparam>
    extension<T>(StackCollection<T> stackCollection)
    {
        /// <summary>
        /// Gets the elements of a stack collection as a <see cref="ReadOnlySpan{T}"/> view.
        /// </summary>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> of stack collection elements.</returns>
        public ReadOnlySpan<T> AsReadOnlySpan()
            => MemoryMarshal.CreateSpan(ref stackCollection.First, stackCollection.Length);

        /// <summary>
        /// Gets the elements of a stack collection as a <see cref="Span{T}"/>.
        /// </summary>
        /// <returns>A <see cref="Span{T}"/> of stack collection elements.</returns>
        public Span<T> AsSpan()
            => MemoryMarshal.CreateSpan(ref stackCollection.First, stackCollection.Length);
    }

    /// <summary>
    /// A <see langword="static"/> collection of methods to extend functionality of <see langword="ref"/>
    /// <see langword="struct"/> member stack collections.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elements in the stack collection, potentially <see langword="ref"/> <see langword="struct"/>.
    /// </typeparam>
    extension<T>(StackCollection<T> stackCollection) where T : allows ref struct
    {
        /// <summary>
        /// Checks if any element of a stack collection passes the <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">Returns <see langword="true"/> when the element passes.</param>
        /// <returns>
        /// <see langword="true"/> if any element of the stack collection passes the <paramref name="predicate"/>,
        /// <see langword="false"/> otherwise.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Any(Func<T, bool>? predicate = null)
        {
            // if there is no predicate, there is nothing to find
            if (predicate == null)
            {
                return false;
            }

            // if the predicate yields a successful result, any succeeds
            foreach (var element in stackCollection)
            {
                if (predicate(element))
                {
                    return true;
                }
            }

            // if the predicate never yields a successful result, any fails
            return false;
        }

        /// <summary>
        /// Filters a stack collection for any that return <see langword="true"/> from <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">Returns <see langword="true"/> when the element should be included.</param>
        /// <returns>A <see langword="new"/> stack collection with filtered elements.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackCollection<T> Where(Func<T, bool>? predicate = null)
        {
            // if there is no predicate, there is nothing to do, return an empty collection
            if (predicate == null)
            {
                return new();
            }

            // iterate once and add the elements that pass the predicate
            StackCollection<T> included = new(ref stackCollection.First, capacity: stackCollection.Capacity);
            foreach (var element in stackCollection)
            {
                if (predicate(element))
                {
                    included.Add(element);
                }
            }

            // if no elements passed the predicate, return an empty stack collection
            if (included.Length == 0)
            {
                return new();
            }

            // iterate once more to add filtered elements now that the actual capacity is known
            StackCollection<T> filtered = new(ref included.First, capacity: included.Length);
            foreach (var element in included)
            {
                filtered.Add(element);
            }

            return filtered;
        }

        /// <summary>
        /// Selects elements in a stack collection using <paramref name="predicate"/>.
        /// </summary>
        /// <param name="predicate">
        /// Returns <typeparamref name="T"/> using the incoming <typeparamref name="T"/> element.
        /// </param>
        /// <returns>A <see langword="new"/> stack collection with selected elements.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackCollection<T> Select(Func<T, T?>? predicate = null) => predicate == null
            ? stackCollection
            : stackCollection.Select<T, T>(predicate);

        /// <summary>
        /// Selects elements in a stack collection using <paramref name="predicate"/>.
        /// </summary>
        /// <typeparam name="TDest">The type of elements in the output stack collection.</typeparam>
        /// <param name="predicate">
        /// Returns <typeparamref name="TDest"/> using the incoming <typeparamref name="TSource"/> element.
        /// </param>
        /// <returns>A <see langword="new"/> stack collection with selected elements.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackCollection<TDest> Select<TDest>(Func<T, TDest?>? predicate = null) where TDest : allows ref struct
        {
            // if there is no predicate there is nothing to do, return an empty stack collection
            if (predicate == null)
            {
                return new();
            }

            // iterate once to create the new elements
            StackCollection<TDest> selected = new(capacity: stackCollection.Capacity);
            foreach (var element in stackCollection)
            {
                if (predicate(element) is TDest newElement)
                {
                    selected.Add(newElement);
                }
            }

            // if no elements were selected return an empty stack collection
            if (selected.Length == 0)
            {
                return new();
            }

            // iterate once more to add selected elements now that the actual capacity is known
            StackCollection<TDest> filtered = new(capacity: selected.Length);
            foreach (var element in selected)
            {
                filtered.Add(element);
            }
            return filtered;
        }
    }

    /// <summary>
    /// Creates a dynamically-sized <see cref="StackCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of element to put in the stack collection.</typeparam>
    /// <param name="span">The <typeparamref name="T"/> elements to put in the stack collection.</param>
    /// <returns>
    /// A <see langword="new"/> dynamically-sized <see cref="StackCollection{T}"/>.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StackCollection<T> Create<T>(ReadOnlySpan<T> span)
    {
        StackCollection<T> collection = new(ref Unsafe.AsRef(in span[0]), length: span.Length);
        span.CopyTo(collection.AsSpan());
        return collection;
    }
}
