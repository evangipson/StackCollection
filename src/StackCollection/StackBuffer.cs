using System.Runtime.CompilerServices;

namespace StackCollection;

[InlineArray(StackCollectionConstants.DefaultBufferSize)]
public ref struct StackBuffer<T>() where T : allows ref struct
{
    public T Instance;
}
