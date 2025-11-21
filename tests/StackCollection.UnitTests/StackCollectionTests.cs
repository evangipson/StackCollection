using System.Runtime.CompilerServices;

namespace StackCollection.UnitTests;

public class StackCollectionTests
{
    [Fact]
    public void StackCollection_ShouldCreateNewPrimitiveCollection()
    {
        StackBuffer<int> buffer = new();

        StackCollection<int> collection = new(ref buffer.Instance);
        collection.Add(1);
        collection.Add(2);
        collection.Add(3);

        Assert.Equal(3, collection.Length);
    }

    [Fact]
    public void StackCollection_ShouldReturnCorrectPrimitiveSize()
    {
        var expected = 4 * 3; // int is 4 bytes, there are 3 elements
        StackBuffer<int> buffer = new();

        StackCollection<int> collection = new(ref buffer.Instance);
        collection.Add(1);
        collection.Add(2);
        collection.Add(3);

        Assert.Equal(expected, collection.TotalBytes);
    }

    [Fact]
    public void StackCollection_ShouldAllowPrimitiveIndexAccess()
    {
        StackBuffer<int> buffer = new();

        StackCollection<int> collection = new(ref buffer.Instance);
        collection.Add(1);
        collection.Add(2);
        collection.Add(3);

        Assert.Equal(1, collection[0]);
        Assert.Equal(2, collection[1]);
        Assert.Equal(3, collection[2]);
    }

    [Fact]
    public void StackCollection_ShouldCreateNewRefStructCollection()
    {
        StackBuffer<SomeStruct> buffer = new();

        StackCollection<SomeStruct> collection = new(ref buffer.Instance);
        collection.Add(new(1));
        collection.Add(new(2));
        collection.Add(new(3));

        Assert.Equal(3, collection.Length);
    }

    [Fact]
    public void StackCollection_ShouldReturnCorrectRefStructSize()
    {
        var expected = Unsafe.SizeOf<SomeStruct>() * 3;
        StackBuffer<SomeStruct> buffer = new();

        StackCollection<SomeStruct> collection = new(ref buffer.Instance);
        collection.Add(new(1));
        collection.Add(new(2));
        collection.Add(new(3));

        Assert.Equal(expected, collection.TotalBytes);
    }

    [Fact]
    public void StackCollection_ShouldAllowRefStructIndexAccess()
    {
        StackBuffer<SomeStruct> buffer = new();
        SomeStruct firstElement = new(1);
        SomeStruct secondElement = new(1);
        SomeStruct thirdElement = new(1);

        StackCollection<SomeStruct> collection = new(ref buffer.Instance);
        collection.Add(firstElement);
        collection.Add(secondElement);
        collection.Add(thirdElement);

        Assert.Equal(firstElement.Number, collection[0].Number);
        Assert.Equal(secondElement.Number, collection[1].Number);
        Assert.Equal(thirdElement.Number, collection[2].Number);
    }

    [Fact]
    public void StackCollection_ShouldComputeCorrectProperties()
    {
        StackBuffer<string> buffer = new();
        StackCollection<string> collection = new(ref buffer.Instance);

        collection.Add("Hello");
        collection.Add("World");
        collection.Add("from the stack!");

        Assert.Equal(Unsafe.SizeOf<string>(), collection.ElementBytes);
        Assert.Equal(Unsafe.SizeOf<string>() * 3, collection.TotalBytes);
    }
}
