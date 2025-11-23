using BenchmarkDotNet.Attributes;

namespace StackCollection.Benchmarks;

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
