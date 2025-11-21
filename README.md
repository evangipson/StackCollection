# StackCollection
A very fast, dynamically-allocated collection which never leaves the stack that supports `ref struct` generic members.

## Basic Usage
You can create a `StackCollection` using either constructor it provides:

```csharp
using StackCollection;

// a simple ref struct for illustrative purposes
public ref struct SomeStruct(int number = 0)
{
    public int Number => number;
}

public void Main()
{
    // using a Span<object> collection expression for  primitives, structs, or objects:
    StackCollection<int> numbers = new([1, 2, 3]);

    // using the Create extension method for ref structs:
    var someStructs = StackCollection.Create<SomeStruct>(capacity: 3);
    someStructs.Add(new(1));
    someStructs.Add(new(2));
    someStructs.Add(new(3));

    // using the Create extension method with delegate:
    var otherStructs = StackCollection.Create<SomeStruct>(capacity: 3, sc =>
    {
        sc.Add(new(4));
        sc.Add(new(5));
        sc.Add(new(6));
        return sc;
    });
}
```

`StackCollection` supports basic enumerators and collection properties:

```csharp
using StackCollection;

public void Main()
{
    StackCollection<string> collection = new(["Hello", "world", "from the stack!"]);

    Console.WriteLine($"{collection.Length} elements in the stack collection.");
    Console.WriteLine($"Total allocated bytes of each element is {collection.ElementBytes}");
    Console.WriteLine($"Total allocated collection bytes is {collection.TotalBytes}");

    foreach(var element in collection)
    {
        Console.WriteLine($"Found element: '{element}'");
    }
}
```

`StackCollection` also supports some LINQ-style extension methods:

```csharp
using StackCollection;

public void Main()
{
    StackCollection<int> collection = new([1, 10, 100, 1000]);

    foreach(var element in collection.Where(x => x < 500))
    {
        Console.WriteLine($"'{element}' is less than 500.");
    }

    foreach(var element in collection.Select(x => new SomeStruct(x)))
    {
        Console.WriteLine($"Found {nameof(SomeStruct)} element: '{element}'");
    }
}
```