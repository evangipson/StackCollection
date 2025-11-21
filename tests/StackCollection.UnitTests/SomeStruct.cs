namespace StackCollection.UnitTests;

internal readonly ref struct SomeStruct(int number = 0)
{
    public int Number => number;
}
