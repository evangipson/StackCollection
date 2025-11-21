namespace StackCollection.UnitTests;

public class StackCollectionTests
{
    [Fact]
    public void StackCollection_ShouldCreateEmptyStackCollection()
    {
        StackCollection<int> collection = new();

        Assert.Equal(0, collection.Length);
        Assert.Equal(0, collection.Capacity);
        Assert.Equal(0, collection.ElementBytes);
        Assert.Equal(0, collection.TotalBytes);
    }

    [Fact]
    public void StackCollection_ShouldCreateFromPrimitiveParams()
    {
        StackCollection<int> collection = new([1, 2, 3]);

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

        StackCollection<DateTime> collection = new([now, yesterday]);

        Assert.Equal(2, collection.Length);
        Assert.Equal(2, collection.Capacity);
        Assert.Equal(now, collection[0]);
        Assert.Equal(yesterday, collection[1]);
    }
}
