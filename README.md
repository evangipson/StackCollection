# StackCollection
A very fast, dynamically-allocated collection which never leaves the stack that supports `ref struct` generic members.

## Benchmarks

### Creation
The following benchmarks capture memory and speed information for creating a new collection with one hundred `int` elements:

| Method            | Mean       | StdDev    | Allocated |
|------------------ |-----------:|----------:|----------:|
| `StackCollection` |  1.0008 ns | 0.0145 ns |         - |
| `ReadOnlySpan`    |  0.0003 ns | 0.0005 ns |         - |
| `Span`            | 13.5635 ns | 0.1860 ns |     424 B |
| `List`            | 15.7198 ns | 0.1829 ns |     456 B |
| `Array`           | 13.1794 ns | 0.1358 ns |     424 B |

Notice that both `StackCollection` and `ReadOnlySpan` have no allocated heap size - they are purely living on the stack! The difference of course being `StackCollection` can accept `ref struct` members.

### Querying
The following benchmarks capture memory and speed information for querying a `StackCollection` and a `List` (no need to include `ReadOnlySpan` or `Span` in these benchmarks, as they don't support querying), using the same queries, over one thousand `int` elements:

| Method                              | Mean     | StdDev  | Allocated |
|------------------------------------ |---------:|--------:|----------:|
| StackCollection_AnyQuery            | 243.2 ns | 0.70 ns |         - |
| StackCollection_SelectQuery         | 370.3 ns | 0.36 ns |         - |
| StackCollection_WhereQuery          | 371.5 ns | 0.71 ns |         - |
| StackCollection_WhereSelectQuery    | 606.9 ns | 1.10 ns |         - |
| StackCollection_SelectWhereAnyQuery | 273.7 ns | 1.39 ns |         - |
| List_AnyQuery                       | 305.2 ns | 0.90 ns |    4056 B |
| List_SelectQuery                    | 414.9 ns | 2.46 ns |    8184 B |
| List_WhereQuery                     | 680.9 ns | 3.58 ns |    6184 B |
| List_WhereSelectQuery               | 714.9 ns | 2.23 ns |    6264 B |
| List_WhereSelectAnyQuery            | 527.4 ns | 7.76 ns |    4208 B |

Notice that `StackCollection` not only out-performs `List` for each query, it also has no allocated heap size while performing those queries.

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

    // chain queries with deferred execution:
    var filter = collection.Where(x => x > 10)
        .Select(x => x + 1)
        .Any(x => x < 10)
}
```