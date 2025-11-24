using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using StackCollection.Iterators;
using StackCollection.Queries;

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
            => MemoryMarshal.CreateReadOnlySpan(ref stackCollection.First, stackCollection.Length);

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
        /// Creates a <see cref="StackCollection{T}"/>, intended to be used as results for a query.
        /// </summary>
        /// <typeparam name="T">The type of element to put in the stack collection.</typeparam>
        /// <param name="span">The <typeparamref name="T"/> elements to put in the stack collection.</param>
        /// <returns>
        /// A <see langword="new"/> results <see cref="StackCollection{T}"/>.
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public StackCollection<T> CreateResults(ref T reference)
            => new(ref reference, capacity: stackCollection.Capacity);

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
            // if there is no elements in the collection or predicate, there is nothing to find
            if (stackCollection.Length == 0 || predicate == null)
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
        /// Filters a stack collection using the provided <paramref name="predicate"/>, returning a deferred query.
        /// Execution is deferred until <see cref="Query{T, T, TQuery}.ToStackCollection"/> is called.
        /// </summary>
        /// <param name="predicate">Returns <see langword="true"/> when the element should be included.</param>
        /// <returns>A deferred <see cref="Query{T, T, TQuery}"/> for chaining.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Query<T, T, T, WhereIterator<T, SourceIterator<T>>> Where(Func<T, bool> predicate)
        {
            SourceIterator<T> sourceIterator = new(stackCollection);
            WhereIterator<T, SourceIterator<T>> whereIterator = new(ref Unsafe.AsRef(in sourceIterator), predicate);
            return new(whereIterator);
        }

        /// <summary>
        /// Selects elements in a stack collection using <paramref name="selector"/>, returning a deferred query.
        /// Execution is deferred until <see cref="Query{T, TDest, TQuery}.ToStackCollection"/> is called.
        /// </summary>
        /// <typeparam name="TDest">The type of elements in the output stack collection.</typeparam>
        /// <param name="selector">
        /// Returns <typeparamref name="TDest"/> using the incoming <typeparamref name="T"/> element.
        /// </param>
        /// <returns>A deferred <see cref="Query{T, TDest, TQuery}"/> for chaining.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Query<T, TDest, TDest, SelectIterator<T, TDest, SourceIterator<T>>> Select<TDest>(Func<T, TDest?> selector)
            where TDest : allows ref struct
        {
            SourceIterator<T> sourceIterator = new(stackCollection);
            SelectIterator<T, TDest, SourceIterator<T>> selectIterator = new(ref Unsafe.AsRef(in sourceIterator), selector);
            return new(selectIterator);
        }
    }

    /// <summary>
    /// A <see langword="static"/> collection of methods to extend functionality of <see langword="class"/>
    /// member stack collections.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elements in the stack collection, all must be a <see langword="class"/>.
    /// </typeparam>
    extension<T>(StackCollection<T> stackCollection) where T : class
    {
        /// <summary>
        /// Gets all the <typeparamref name="T"/> elements of a stack collection as <typeparamref name="TDest"/> elements.
        /// <para>
        /// All the <typeparamref name="TDest"/> elements are guaranteed to not be <see langword="null"/>.
        /// </para>
        /// </summary>
        /// <param name="results">The consumer-allocated stack collection where the results are stored.</param>
        /// <returns>The modified <paramref name="results"/> collection.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref StackCollection<TDest> OfType<TDest>(ref StackCollection<TDest> results) where TDest : class
        {
            // clear any results in case of fluent chaining
            results.Clear();

            // if there is no elements or results capacity, there is nothing to do
            if (stackCollection.Length == 0 || results.Capacity == 0)
            {
                return ref results;
            }

            // filter the collection down to non-null TDest elements
            foreach (var element in stackCollection)
            {
                if (element is TDest newElement)
                {
                    results.Add(newElement);
                }
            }

            return ref results;
        }
    }

    /// <summary>
    /// Creates a <see cref="StackCollection{T}"/> from a <see cref="ReadOnlySpan{T}"/>, intended to be
    /// used as results for a query.
    /// </summary>
    /// <param name="span"><see cref="ReadOnlySpan{T}"/> containing at least one <typeparamref name="T"/>.</param>
    /// <returns>A <see langword="new"/> results <see cref="StackCollection{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StackCollection<T> CreateResults<T>(this StackCollection<T> collection, Span<T> span)
        => new(ref span[0], capacity: collection.Capacity);

    /// <summary>
    /// Creates a <see cref="StackCollection{T}"/> from a <see cref="ReadOnlySpan{T}"/>, intended to be
    /// used as results for a query that will give back a different type than it's input.
    /// </summary>
    /// <param name="span"><see cref="ReadOnlySpan{T}"/> containing at least one <typeparamref name="T"/>.</param>
    /// <returns>A <see langword="new"/> results <see cref="StackCollection{T}"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static StackCollection<TDest> CreateResultsOf<T, TDest>(this StackCollection<T> collection, Span<TDest> span)
        => new(ref span[0], capacity: collection.Capacity);

    /// <summary>
    /// Creates a dynamically-sized <see cref="StackCollection{T}"/> from a <see cref="ReadOnlySpan{T}"/>.
    /// <para>
    /// Will not work with <see langword="ref"/> <see langword="struct"/> elements, because they cannot be
    /// <see cref="ReadOnlySpan{T}"/> members due to generic argument constraints.
    /// </para>
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
