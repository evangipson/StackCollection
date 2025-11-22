namespace StackCollection.UnitTests;

public class StackCollectionExtensionsTests
{
    [Fact]
    public void Create_ShouldCreatePrimitiveDynamicallySizedStackCollection()
    {
        var collection = StackCollection.Create([1, 2]);

        Assert.Equal(2, collection.Length);
        Assert.Equal(2, collection.Capacity);
        Assert.Equal(1, collection[0]);
        Assert.Equal(2, collection[1]);
    }

    [Fact]
    public void Where_ShouldFilterPrimitiveStackCollection()
    {
        var collection = StackCollection.Create([1, 2, 3, 4, 5]);

        var result = collection.Where(x => x < 3);

        Assert.Equal(2, result.Length);
        Assert.Equal(2, result.Capacity);
    }

    [Fact]
    public void Select_ShouldCreateNewPrimitiveStackCollection()
    {
        var collection = StackCollection.Create([1, 2, 3, 4, 5]);

        var result = collection.Select(x => x < 3);

        Assert.Equal(5, result.Length);
        Assert.Equal(5, result.Capacity);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(10, false)]
    public void Any_ShouldReturnTrue_IfAnyElementMatches(int filter, bool expected)
    {
        var collection = StackCollection.Create([1, 2, 3, 4, 5]);

        var result = collection.Any(x => x > filter);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void SelectWhere_ShouldCreateNewFilteredPrimitiveStackCollection()
    {
        var collection = StackCollection.Create([1, 2, 3, 4, 5]);

        var result = collection.Where(x => x < 3).Select();

        Assert.Equal(2, result.Length);
        Assert.Equal(2, result.Capacity);
    }
}
