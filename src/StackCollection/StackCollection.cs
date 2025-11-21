using System.Runtime.CompilerServices;

namespace StackCollection;

/// <summary>
/// A stack-allocated <see langword="readonly"/> collection of elements.
/// </summary>
public ref struct StackCollection<T> where T : allows ref struct
{
    private ref byte _startAddress;
    private readonly int _capacity;
    private int _length;
    private readonly int _elementBytes = Unsafe.SizeOf<T>();

    // constructor requires the buffer reference immediately
    public StackCollection(ref T bufferStart, int capacity = StackCollectionConstants.DefaultBufferSize)
    {
        // capture the start of the CONTIGUOUS block here
        _startAddress = ref Unsafe.As<T, byte>(ref bufferStart);
        _capacity = capacity;
        _length = 0;
    }

    public readonly int Length => _length;

    public readonly int ElementBytes => _elementBytes;

    public readonly int TotalBytes => ElementBytes * Length;

    public void Add(T item)
    {
        if (_length >= _capacity)
        {
            throw new IndexOutOfRangeException("Stack buffer full");
        }

        // Calculate offset based on the SAFE, contiguous starting point
        ref T start = ref Unsafe.As<byte, T>(ref _startAddress);
        ref T slot = ref Unsafe.Add(ref start, _length);

        slot = item;
        _length++;
    }

    public ref T this[int index]
    {
        get
        {
            if ((uint)index >= (uint)_length) throw new IndexOutOfRangeException();
            ref T start = ref Unsafe.As<byte, T>(ref _startAddress);
            return ref Unsafe.Add(ref start, index);
        }
    }

    public Enumerator GetEnumerator() => new(ref _startAddress, _length);

    public ref struct Enumerator
    {
        private ref byte _currentAddress;
        private readonly int _count;
        private int _index;

        public Enumerator(ref byte startAddress, int count)
        {
            _currentAddress = ref startAddress;
            _count = count;
            _index = -1;
        }

        public bool MoveNext()
        {
            _index++;
            return _index < _count;
        }

        public ref T Current
        {
            get
            {
                // recover ref T
                ref T start = ref Unsafe.As<byte, T>(ref _currentAddress);
                return ref Unsafe.Add(ref start, _index);
            }
        }
    }
}
