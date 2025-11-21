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

    [Fact]
    public void Where_ShouldFilterPrimitiveStackCollection()
    {
        var collection = StackCollection.Create<int>(capacity: 5);
        collection.Add(1);
        collection.Add(2);
        collection.Add(3);
        collection.Add(4);
        collection.Add(5);

        var result = collection.Where(x => x < 3);

        Assert.Equal(2, result.Length);
        Assert.Equal(2, result.Capacity);
    }

    [Fact]
    public void Select_ShouldCreateNewPrimitiveStackCollection()
    {
        var collection = StackCollection.Create<int>(capacity: 5);
        collection.Add(1);
        collection.Add(2);
        collection.Add(3);
        collection.Add(4);
        collection.Add(5);

        var result = collection.Select(x => x < 3);

        Assert.Equal(5, result.Length);
        Assert.Equal(5, result.Capacity);
    }

    [Fact]
    public void SelectWhere_ShouldCreateNewFilteredPrimitiveStackCollection()
    {
        var collection = StackCollection.Create<int>(capacity: 5);
        collection.Add(1);
        collection.Add(2);
        collection.Add(3);
        collection.Add(4);
        collection.Add(5);

        var result = collection.Select(x => x < 3)
            .Where(x => x);

        // TODO: why the shit isn't this working? it's has 5 elements, but should only have 2.
        Assert.Equal(2, result.Length);
        Assert.Equal(2, result.Capacity);
    }
}
