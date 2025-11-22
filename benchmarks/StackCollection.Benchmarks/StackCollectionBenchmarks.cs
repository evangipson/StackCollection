using BenchmarkDotNet.Attributes;

namespace StackCollection.Benchmarks;

[MemoryDiagnoser]
public class StackCollectionBenchmarks
{
    private int[] _numbers = new int[100];

    private ReadOnlySpan<int> NumberSpan => _numbers;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _numbers = [.. Enumerable.Range(1, 100)];
    }

    [Benchmark]
    public void CreateIntStackCollection()
    {
        _ = StackCollection.Create(NumberSpan);
    }

    [Benchmark]
    public void CreateIntReadOnlySpan()
    {
        ReadOnlySpan<int> _ = [.. NumberSpan];
    }

    [Benchmark]
    public void CreateIntSpan()
    {
        Span<int> _ = [.. NumberSpan];
    }

    [Benchmark]
    public void CreateIntList()
    {
        List<int> _ = [.. NumberSpan];
    }

    [Benchmark]
    public void CreateIntArray()
    {
        int[] _ = [.. NumberSpan];
    }
}
