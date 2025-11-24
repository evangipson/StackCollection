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
    public void Where_ShouldFilterPrimitiveStackCollection()
    {
        var collection = StackCollection.Create([1, 2, 3, 4, 5]);
        var whereResults = collection.CreateResults([0]);

        var result = collection.Where(x => x < 3).ToStackCollection(ref whereResults);

        Assert.Equal(2, result.Length);
        Assert.True(result[0] < 3);
        Assert.True(result[1] < 3);
    }

    [Fact]
    public void Select_ShouldCreateNewPrimitiveStackCollection()
    {
        var collection = StackCollection.Create([1, 2, 3, 4, 5]);
        var selectResults = collection.CreateResultsOf([false]);

        var result = collection.Select(x => x < 3).ToStackCollection(ref selectResults);

        Assert.Equal(5, result.Length);
    }

    [Fact]
    public void ChainedQueries_ShouldCreateNewPrimitiveStackCollection()
    {
        var collection = StackCollection.Create([1, 2, 3, 4, 5]);
        var chainedResults = collection.CreateResultsOf(["yo"]);

        var result = collection.Where(x => x < 4)
            .Select(x => $"{x:c}")
            .ToStackCollection(ref chainedResults);

        Assert.Equal(3, result.Length);
        Assert.Equal("$1.00", result[0]);
        Assert.Equal("$2.00", result[1]);
        Assert.Equal("$3.00", result[2]);
    }

    [Fact]
    public void OfType_ShouldRemoveNullAndCastClassyStackCollection()
    {
        var collection = StackCollection.Create<SomeExtendedClass>([new(1), new(2), new(3)]);
        var resultCollection = collection.CreateResultsOf<SomeExtendedClass, SomeClass>([new(0)]);

        var result = collection.OfType(ref resultCollection);

        Assert.Equal(3, result.Length);
        Assert.Equal(3, result.Capacity);
        Assert.Equal(1, result[0].Number);
        Assert.Equal(2, result[1].Number);
        Assert.Equal(3, result[2].Number);
    }
}
