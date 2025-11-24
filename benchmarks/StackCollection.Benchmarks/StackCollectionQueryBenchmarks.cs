using BenchmarkDotNet.Attributes;

namespace StackCollection.Benchmarks;

[MemoryDiagnoser]
public class StackCollectionQueryBenchmarks
{
    private readonly int[] _numbers = [.. Enumerable.Range(1, 1000)];

    private ReadOnlySpan<int> Numbers => _numbers;

    private Span<int> Number => _numbers.AsSpan()[0..1];

    private StackCollection<int> Collection => StackCollection.Create(Numbers);

    private List<int> List => [.. Numbers];

    [Benchmark]
    public StackCollection<int> StackCollection_SelectQuery()
    {
        var selectResults = Collection.CreateResults(Number);
        return Collection.Select(x => x + 1)
            .ToStackCollection(ref selectResults);
    }

    [Benchmark]
    public StackCollection<int> StackCollection_WhereQuery()
    {
        var whereResults = Collection.CreateResults(Number);
        return Collection.Where(x => x > 500)
            .ToStackCollection(ref whereResults);
    }

    [Benchmark]
    public StackCollection<int> StackCollection_WhereSelectWhereQuery()
    {
        var chainResults = Collection.CreateResults(Number);
        return Collection.Where(x => x > 500)
            .Select(x => x + 1)
            .ToStackCollection(ref chainResults);
    }

    [Benchmark]
    public List<int> List_SelectQuery()
    {
        return [.. List.Select(x => x + 1)];
    }

    [Benchmark]
    public List<int> List_WhereQuery()
    {
        return [.. List.Where(x => x > 500)];
    }

    [Benchmark]
    public List<int> List_WhereSelectQuery()
    {
        return [.. List.Where(x => x > 500).Select(x => x + 1)];
    }
}
