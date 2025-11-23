namespace StackCollection.UnitTests;

public class StackCollectionTests
{
    [Fact]
    public void StackCollection_ShouldCreateEmptyStackCollection()
    {
        StackCollection<int> collection = new();

        Assert.Equal(0, collection.Length);
        Assert.Equal(0, collection.Capacity);
    }

    [Fact]
    public void StackCollection_ShouldThrowIndexOutOfRangeException_WhenAddingBeyondCapacity()
    {
        Assert.Throws<IndexOutOfRangeException>(() => new StackCollection<int>().Add(1));
    }

    [Fact]
    public void StackCollection_ShouldThrowIndexOutOfRangeException_WhenIndexingOutOfRange()
    {
        Assert.Throws<IndexOutOfRangeException>(() => new StackCollection<int>()[1]);
    }

    [Fact]
    public void StackCollection_ShouldCreateFromPrimitiveParams()
    {
        int reference = 0;
        StackCollection<int> collection = new(ref reference, capacity: 3);
        collection.Add(1);
        collection.Add(2);
        collection.Add(3);

        Assert.Equal(3, collection.Length);
        Assert.Equal(3, collection.Capacity);
        Assert.Equal(1, collection[0]);
        Assert.Equal(2, collection[1]);
        Assert.Equal(3, collection[2]);
    }

    [Fact]
    public void StackCollection_ShouldCreateFromStructParams()
    {
        var now = DateTime.UtcNow;
        var yesterday = now.AddDays(-1);

        StackCollection<DateTime> collection = new(ref now, capacity: 2);
        collection.Add(now);
        collection.Add(yesterday);

        Assert.Equal(2, collection.Length);
        Assert.Equal(2, collection.Capacity);
        Assert.Equal(now, collection[0]);
        Assert.Equal(yesterday, collection[1]);
    }

    [Fact]
    public void StackCollection_ShouldCreateSingleElementCollection_UsingRefStruct()
    {
        SomeStruct reference = new();
        StackCollection<SomeStruct> collection = new(ref reference, capacity: 1);
        collection.Add(new(1));

        Assert.Equal(1, collection.Length);
        Assert.Equal(1, collection.Capacity);
        Assert.Equal(1, collection[0].Number);
    }

    [Fact]
    public void StackCollection_ShouldCreateMultipleElementCollection_UsingRefStruct()
    {
        SomeStruct reference = new();
        StackCollection<SomeStruct> collection = new(ref reference, capacity: 3);
        collection.Add(new(1));
        collection.Add(new(2));
        collection.Add(new(3));

        Assert.Equal(3, collection.Length);
        Assert.Equal(3, collection.Capacity);
        Assert.Equal(1, collection[0].Number);
        Assert.Equal(2, collection[1].Number);
        Assert.Equal(3, collection[2].Number);
    }
}
