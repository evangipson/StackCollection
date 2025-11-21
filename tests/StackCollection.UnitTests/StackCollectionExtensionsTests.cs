namespace StackCollection.UnitTests;

public class StackCollectionExtensionsTests
{
    [Fact]
    public void Create_ShouldCreatePrimitiveDynamicallySizedStackCollection()
    {
        var collection = StackCollection.Create<int>(capacity: 20);
        collection.Add(1);
        collection.Add(2);

        Assert.Equal(2, collection.Length);
        Assert.Equal(20, collection.Capacity);
    }

    [Fact]
    public void Create_ShouldCreatePrimitiveDynamicallySizedStackCollection_WithDelegate()
    {
        var collection = StackCollection.Create<int>(capacity: 20, collection =>
        {
            collection.Add(1);
            collection.Add(2);
            return collection;
        });

        Assert.Equal(2, collection.Length);
        Assert.Equal(20, collection.Capacity);
    }

    [Fact]
    public void Create_ShouldCreateRefStructDynamicallySizedStackCollection()
    {
        var collection = StackCollection.Create<SomeStruct>(capacity: 20);
        collection.Add(new(1));
        collection.Add(new(2));

        Assert.Equal(2, collection.Length);
        Assert.Equal(20, collection.Capacity);
    }

    [Fact]
    public void Create_ShouldCreateRefStructDynamicallySizedStackCollection_WithDelegate()
    {
        var collection = StackCollection.Create<SomeStruct>(capacity: 20, collection =>
        {
            collection.Add(new(1));
            collection.Add(new(2));
            return collection;
        });

        Assert.Equal(2, collection.Length);
        Assert.Equal(20, collection.Capacity);
    }
}
