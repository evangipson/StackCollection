# StackCollection
A very fast, dynamically-allocated collection which never leaves the stack that supports `ref struct` generic members.

## Basic Usage
You can create a `StackCollection` using a collection expression:

```csharp
using StackCollection;

public void Main()
{
    // let the compiler heap-allocate the List<int>:
    List<int> numbers = [1, 2, 3];

    // use a ReadOnlySpan<T> collection expression to entirely avoid heap allocation:
    var collection = StackCollection.Create([.. numbers]);
}
```

You can also create a `StackCollection` using `ref struct` members, but it must be created using the constructor (because `ReadOnlySpan<T>` and `Span<T>` do not support `ref struct` generics):

```csharp
using StackCollection;

// a simple ref struct for illustrative purposes
public ref struct SomeStruct(int number = 0)
{
    public int Number => number;
}

public void Main()
{
    // create an initial ref struct to serve as the reference for the stack collection:
    SomeStruct startingElement = new();

    // use the constructor for ref structs (as they cannot be generic ReadOnlySpan<T> arguments):
    StackCollection<SomeStruct> collection = new(ref startingElement, capacity: 3);
    
    // add ref struct members to the stack collection
    collection.Add(new(1));
    collection.Add(new(2));
    collection.Add(new(3));
}
```

`StackCollection` supports basic enumerators and collection properties:

```csharp
using StackCollection;

public void Main()
{
    List<string> greetings = ["Hello", "world", "from the stack!"];
    var collection = StackCollection.Create([.. greetings]);

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
    List<int> numbers = [1, 10, 100, 1000];
    var collection = StackCollection.Create([.. numbers]);

    // check for any member that matches a predicate:
    var isAnyNumberSmall = collection.Any(x => x < 10);

    // filter down to numbers less than 500:
    foreach(var smallNumber in collection.Where(x => x < 500))
    {
        Console.WriteLine($"'{smallNumber}' is less than 500.");
    }

    // select a currency-style string of each number:
    foreach(var currencyString in collection.Select(x => $"{x:c}"))
    {
        Console.WriteLine($"Found {currencyString} in the wallet.");
    }
}
```

## Okay, but why?
Well, stack allocation is really fast. Like, _really fast_. And I was tired of not being able to have collections with `ref struct` elements.

Also, generally the concept of a collection that is guaranteed to never leave the stack interests me, and `Span<T>`/`ReadOnlySpan<T>` was not meeting my goals.

## Benchmarks
The following benchmarks capture memory and speed information for creating a new collection with one hundred `int` elements:

| Method            | Mean       | StdDev    | Median     | Allocated |
|------------------ |-----------:|----------:|-----------:|----------:|
| `StackCollection` |  1.0008 ns | 0.0145 ns |  1.0034 ns |         - |
| `ReadOnlySpan`    |  0.0003 ns | 0.0005 ns |  0.0000 ns |         - |
| `Span`            | 13.5635 ns | 0.1860 ns | 13.5806 ns |     424 B |
| `List`            | 15.7198 ns | 0.1829 ns | 15.7173 ns |     456 B |
| `Array`           | 13.1794 ns | 0.1358 ns | 13.2120 ns |     424 B |

Notice that both `StackCollection` and `ReadOnlySpan` have no allocated heap size - they are purely living on the stack! The difference of course being `StackCollection` can accept `ref struct` members.