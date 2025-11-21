# StackCollection
A very fast, dynamically-allocated collection which never leaves the stack that supports `ref struct` generic members.

## Basic Usage
You can create a `StackCollection` of by using the `Create` extension method:

```csharp
using StackCollection;

public void Main()
{
    var collection = StackCollection.Create<int>(capacity: 3);

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
    var collection = StackCollection.Create<SomeStruct>(capacity: 3);

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
    var collection = StackCollection.Create<string>(capacity: 3);

    collection.Add("Hello");
    collection.Add("World");
    collection.Add("from the stack!");

    Console.WriteLine($"{collection.Length} elements in the stack collection.");
    Console.WriteLine($"Total allocated bytes of each stack collection element is {collection.ElementBytes}");
    Console.WriteLine($"Total allocated bytes of the entire stack collection is {collection.TotalBytes}");

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
    // create the collection to LINQ up
    var collection = StackCollection.Create<int>(capacity: 4);
    collection.Add(1);
    collection.Add(10);
    collection.Add(100);
    collection.Add(1000);

    // will contain 1, 10, and 100
    var filtered = collection.Where(x => x < 500);
    Console.WriteLine($"{filtered.Length} elements in the stack collection.");
    foreach(var element in filtered)
    {
        Console.WriteLine($"'{element}' is less than 500.");
    }

    // will contain all elements from collection as SomeStructs
    var transformed = collection.Select(x => new SomeStruct(x));
    Console.WriteLine($"{transformed.Length} elements in the stack collection.");
    foreach(var element in transformed)
    {
        Console.WriteLine($"Found {nameof(SomeStruct)} element: '{element}'");
    }
}
```