using System.Runtime.CompilerServices;

namespace StackCollection;

/// <summary>
/// A continguous stack-allocated block of memory for <typeparamref name="T"/>.
/// <para>
/// Used when creating a <see langword="new"/> <see cref="StackCollection{T}"/>.
/// </para>
/// </summary>
/// <typeparam name="T">The type of element to allocate memory for.</typeparam>
[InlineArray(StackCollectionConstants.DefaultBufferSize)]
public ref struct StackBuffer<T>() where T : allows ref struct
{
    /// <summary>
    /// The first element of the contiguous block of stack-allocated memory.
    /// </summary>
    public T? Instance;
}
