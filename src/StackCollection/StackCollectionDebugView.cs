using System.Diagnostics;

namespace StackCollection;

/// <summary>
/// A debug view for <see cref="StackCollection{T}"/> that shows all
/// elements of the stack collection and hides the root properties.
/// </summary>
/// <typeparam name="T">The type of elements in the debug view.</typeparam>
internal sealed class StackCollectionDebugView<T> where T : new()
{
    private readonly T[] _items;

    public StackCollectionDebugView(StackCollection<T> collection)
    {
        _items = collection.AsReadOnlySpan().ToArray();
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public T[] Items => _items;
}