using System.Collections;
using System.Diagnostics;

namespace StackCollection;

/// <summary>
/// A debug view for <see cref="StackCollection{T}"/> that shows all
/// elements of the stack collection and hides the root properties.
/// </summary>
/// <typeparam name="T">The type of elements in the debug view.</typeparam>
internal sealed class StackCollectionDebugView<T>
{
    private readonly ArrayList _items;

    public StackCollectionDebugView(StackCollection<T> collection)
    {
        _items = new(collection.Capacity);
        foreach(var item in collection)
        {
            _items.Add(item);
        }
    }

    [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
    public ArrayList Items => _items;
}