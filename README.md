# StackCollection
A very fast, dynamically-allocated collection which never leaves the stack that supports `ref struct` generic members.

- [Basic usage](#basic-usage)
    - [Creating](#creating-a-stackcollection)
    - [Enumerating](#enumerating-a-stackcollection)
    - [Querying](#querying-a-stackcollection)
- [Benchmarks](#benchmarks)
    - [Creation](#creation)
    - [Any Query](#any-query)
    - [Select Query](#select-query)
    - [Where Query](#where-query)
    - [Chained Queries](#chained-queries)
    - [Deferred Execution Queries](#deferred-execution-queries)

## Basic Usage
### Creating a StackCollection
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

### Enumerating a StackCollection
`StackCollection` supports basic enumerators and collection properties:

```csharp
using StackCollection;

public void Main()
{
    List<string> greetings = ["Hello", "world", "from the stack!"];
    var collection = StackCollection.Create([.. greetings]);

    Console.WriteLine($"{collection.Length} elements in the stack collection.");
    foreach(var element in collection)
    {
        Console.WriteLine($"Found element: '{element}'");
    }
}
```

### Querying a StackCollection
`StackCollection` also supports some LINQ-style methods with deferred execution:

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
    var filter = collection.Where(x => x > 1)
        .Select(x => x + 1)
        .Any(x => x < 10);

    // get the results into a new stack collection by first allocating
    // a new collection on the stack, then passing a reference to it
    // when executing the query:
    var queryResults = collection.CreateResults([0]);
    var newCollection = collection.Where(x => x > 10)
        .Select(x => x < 1000)
        .ToStackCollection(ref queryResults);
    foreach(var element in newCollection)
    {
        Console.WriteLine($"Found {element} in the new collection.");
    }
}
```

## Benchmarks
### Creation
The following benchmarks capture memory and speed information for creating a new collection with one thousand `int` elements:

```csharp
[MemoryDiagnoser]
public class StackCollectionBenchmarks
{
    private readonly int[] _numbers = [.. Enumerable.Range(1, 1000)];

    private ReadOnlySpan<int> Numbers => _numbers;

    [Benchmark]
    public void CreateIntStackCollection()
    {
        _ = StackCollection.Create(Numbers);
    }

    [Benchmark]
    public void CreateIntReadOnlySpan()
    {
        ReadOnlySpan<int> _ = [.. _numbers];
    }

    [Benchmark]
    public void CreateIntSpan()
    {
        Span<int> _ = [.. _numbers];
    }

    [Benchmark]
    public void CreateIntList()
    {
        List<int> _ = [.. _numbers];
    }

    [Benchmark]
    public void CreateIntArray()
    {
        int[] _ = [.. _numbers];
    }
}
```

Running these benchmarks produces the following results:

| Method            | Mean       | StdDev    | Allocated |
|------------------ |-----------:|----------:|----------:|
| `StackCollection` |  1.0008 ns | 0.0145 ns |         - |
| `ReadOnlySpan`    |  0.0003 ns | 0.0005 ns |         - |
| `Span`            | 13.5635 ns | 0.1860 ns |     424 B |
| `List`            | 15.7198 ns | 0.1829 ns |     456 B |
| `Array`           | 13.1794 ns | 0.1358 ns |     424 B |

Notice that both `StackCollection` and `ReadOnlySpan` have no allocated heap size - they are purely living on the stack! The difference of course being `StackCollection` can accept `ref struct` members.

### Any Query
Consider the following benchmarks:

```csharp
[MemoryDiagnoser]
public class AnyBenchmarks
{
    private readonly int[] _numbers = [.. Enumerable.Range(1, 1000)];
    private ReadOnlySpan<int> Numbers => _numbers;
    private StackCollection<int> Collection => StackCollection.Create(Numbers);
    private List<int> List => [..Numbers];

    [Benchmark]
    public bool StackCollection_AnyQuery() => Collection.Any(x => x > 999);

    [Benchmark]
    public bool List_AnyQuery() => List.Any(x => x > 999);
}
```

Running this benchmark produces the following results:

| Method                              | Mean     | StdDev  | Allocated |
|------------------------------------ |---------:|--------:|----------:|
| StackCollection_AnyQuery            | 243.2 ns | 0.70 ns |         - |
| List_AnyQuery                       | 305.2 ns | 0.90 ns |    4056 B |

### Select Query
Consider the following benchmarks:

```csharp
[MemoryDiagnoser]
public class SelectBenchmarks
{
    private readonly int[] _numbers = [.. Enumerable.Range(1, 1000)];
    private ReadOnlySpan<int> Numbers => _numbers;
    private StackCollection<int> Collection => StackCollection.Create(Numbers);
    private List<int> List => [..Numbers];

    [Benchmark]
    public StackCollection<int> StackCollection_SelectQuery()
    {
        var selectResults = Collection.CreateResults(Number);
        return Collection.Select(x => x + 1)
            .ToStackCollection(ref selectResults)
    }

    [Benchmark]
    public List<int> List_SelectQuery() => [..List.Select(x => x + 1)];
}
```

Running this benchmark produces the following results:

| Method                              | Mean     | StdDev  | Allocated |
|------------------------------------ |---------:|--------:|----------:|
| StackCollection_SelectQuery         | 370.3 ns | 0.36 ns |         - |
| List_SelectQuery                    | 414.9 ns | 2.46 ns |    8184 B |

### Where Query
Consider the following benchmarks:

```csharp
[MemoryDiagnoser]
public class WhereBenchmarks
{
    private readonly int[] _numbers = [.. Enumerable.Range(1, 1000)];
    private ReadOnlySpan<int> Numbers => _numbers;
    private StackCollection<int> Collection => StackCollection.Create(Numbers);
    private List<int> List => [..Numbers];

    [Benchmark]
    public StackCollection<int> StackCollection_WhereQuery()
    {
        var selectResults = Collection.CreateResults(Number);
        return Collection.Where(x => x < 999)
            .ToStackCollection(ref selectResults)
    }

    [Benchmark]
    public List<int> List_WhereQuery() => [..List.Where(x => x < 999)];
}
```

Running this benchmark produces the following results:

| Method                              | Mean     | StdDev  | Allocated |
|------------------------------------ |---------:|--------:|----------:|
| StackCollection_WhereQuery          | 371.5 ns | 0.71 ns |         - |
| List_WhereQuery                     | 680.9 ns | 3.58 ns |    6184 B |

### Chained Queries
Consider the following benchmarks:

```csharp
[MemoryDiagnoser]
public class ChainedQueryBenchmarks
{
    private readonly int[] _numbers = [.. Enumerable.Range(1, 1000)];
    private ReadOnlySpan<int> Numbers => _numbers;
    private StackCollection<int> Collection => StackCollection.Create(Numbers);
    private List<int> List => [..Numbers];

    [Benchmark]
    public StackCollection<int> StackCollection_WhereSelectQuery()
    {
        var selectResults = Collection.CreateResults(Number);
        return Collection.Where(x => x < 999)
            .Select(x => x + 1)
            .ToStackCollection(ref selectResults)
    }

    [Benchmark]
    public List<int> List_WhereSelectQuery()
        => [..List.Where(x => x < 999).Select(x => x + 1)];
}
```

Running this benchmark produces the following results:

| Method                              | Mean     | StdDev  | Allocated |
|------------------------------------ |---------:|--------:|----------:|
| StackCollection_WhereSelectQuery    | 606.9 ns | 1.10 ns |         - |
| List_WhereSelectQuery               | 714.9 ns | 2.23 ns |    6264 B |

### Deferred Execution Queries

Deferred execution can even be accomplished without using `ToStackCollection`:

```csharp
[MemoryDiagnoser]
public class DeferredExecutionBenchmarks
{
    private readonly int[] _numbers = [.. Enumerable.Range(1, 1000)];
    private ReadOnlySpan<int> Numbers => _numbers;
    private StackCollection<int> Collection => StackCollection.Create(Numbers);
    private List<int> List => [..Numbers];

    [Benchmark]
    public StackCollection<int> StackCollection_WhereSelectAnyQuery()
    {
        return Collection.Where(x => x > 500)
            .Select(x => x + 1)
            .Any(x => x > 499);
    }

    [Benchmark]
    public List<int> List_WhereSelectQuery()
    {
        return List.Where(x => x > 500)
            .Select(x => x + 1)
            .Any(x => x > 499);
    }
}
```

Running this benchmark produces the following results:

| Method                              | Mean     | StdDev  | Allocated |
|------------------------------------ |---------:|--------:|----------:|
| StackCollection_SelectWhereAnyQuery | 273.7 ns | 1.39 ns |         - |
| List_WhereSelectAnyQuery            | 527.4 ns | 7.76 ns |    4208 B |

Notice that `StackCollection` not only out-performs `List` for each query, it also has no allocated heap size while performing those queries.