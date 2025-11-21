# StackCollection
A very fast, dynamically-allocated collection which never leaves the stack that supports `ref struct` generic members.

## Basic Usage
You can create a `StackCollection` of dynamic size by using the `Create` extension method:

```csharp
using StackCollection;

public void Main()
{
    var collection = StackCollection.Create<int>(capacity: 4);

    collection.Add(1);
    collection.Add(2);
    collection.Add(3);

    var firstElement = collection[0];
    var secondElement = collection[1];
    var thirdElement = collection[2];
}
```

You can also create a `StackCollection` of fixed size by manually allocating a `StackBuffer`:

```csharp
using StackCollection;

public void Main()
{
    StackBuffer<int> buffer = new();
    StackCollection<int> collection = new(ref buffer.Instance);

    collection.Add(1);
    collection.Add(2);
    collection.Add(3);

    var firstElement = collection[0];
    var secondElement = collection[1];
    var thirdElement = collection[2];
}
```

You can also create a `StackCollection` using `ref struct` members:

```csharp
using StackCollection;

public ref struct SomeStruct(int number = 0)
{
    public int Number => number;
}

public void Main()
{
    var collection = StackCollection.Create<SomeStruct>(capacity: 4);

    collection.Add(new(1));
    collection.Add(new(2));
    collection.Add(new(3));

    var firstElement = collection[0];
    var secondElement = collection[1];
    var thirdElement = collection[2];
}
```

`StackCollection` supports basic enumerators and collection properties:

```csharp
using StackCollection;

public void Main()
{
    var collection = StackCollection.Create<string>(capacity: 4);

    collection.Add("Hello");
    collection.Add("World");
    collection.Add("from the stack!");

    Console.WriteLine($"{collection.Length} elements in the stack collection.");
    Console.WriteLine($"Total allocated bytes of each stack collection element is {collection.ElementBytes}");
    Console.WriteLine($"Total allocated bytes of the entire stack collection is {collection.TotalBytes}");

    foreach(var element in collection)
    {
        Console.WriteLine($"Found stack collection element: '{element}'");
    }
}
```