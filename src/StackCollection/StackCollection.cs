using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StackCollection;

/// <summary>
/// A stack-allocated <see langword="readonly"/> collection of elements.
/// </summary>
public ref struct StackCollection<T> where T : allows ref struct
{
    private int _length = 0;
    private readonly int _capacity;
    private readonly ref byte _startAddress;

    /// <summary>
    /// Creates a new <see cref="StackCollection{T}"/>.
    /// </summary>
    /// <param name="startElement">The starting element to use as a reference.</param>
    /// <param name="length">The current number of elements in the stack collection.</param>
    /// <param name="capacity">The maximum number of elements the stack collection can hold.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StackCollection(ref T startElement, int length = 0, int capacity = 0)
    {
        _length = length;
        _capacity = length >= capacity
            ? length
            : capacity;
        _startAddress = ref Unsafe.As<T, byte>(ref startElement);
    }

    /// <summary>
    /// Creates a new, empty <see cref="StackCollection{T}"/>.
    /// </summary>
    /// <param name="capacity">The maximum number of elements the stack collection can hold.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StackCollection(int capacity = 0)
    {
        _capacity = capacity;

        ReadOnlySpan<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>() * capacity];
        _startAddress = ref Unsafe.AsRef(in MemoryMarshal.GetReference(buffer));
    }

    /// <summary>
    /// The amount of elements in the stack collection.
    /// </summary>
    public readonly int Length => _length;

    /// <summary>
    /// The maximum number of elements for the stack collection.
    /// </summary>
    public readonly int Capacity => _capacity;

    /// <summary>
    /// A reference to the first element of the stack collection.
    /// </summary>
    public readonly ref T First => ref Unsafe.As<byte, T>(ref _startAddress);

    /// <summary>
    /// Adds an element to the stack collection.
    /// <para>
    /// Will <see langword="throw"/> an <see cref="IndexOutOfRangeException"/> if
    /// the number of elements is already at <see cref="Capacity"/>.
    /// </para>
    /// </summary>
    /// <param name="item">The element to add to the stack collection.</param>
    /// <exception cref="IndexOutOfRangeException"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        if (_length >= _capacity)
        {
            throw new IndexOutOfRangeException("Unable to add an element to the stack collection because the buffer is full.");
        }

        ref T slot = ref Unsafe.Add(ref First, (nint)(uint)_length);
        slot = item;
        _length++;
    }

    /// <summary>
    /// Gets an element by index for the stack collection.
    /// <para>
    /// Will <see langword="throw"/> an <see cref="IndexOutOfRangeException"/> if
    /// <paramref name="index"/> meets or exceeds <see cref="Length"/>.
    /// </para>
    /// </summary>
    /// <param name="index">The index of the desired element.</param>
    /// <returns>The stack collection element.</returns>
    /// <exception cref="IndexOutOfRangeException"/>
    public ref readonly T this[int index]
    {
        get
        {
            if (index >= _capacity)
            {
                throw new IndexOutOfRangeException($"Unable to index {index} element of {Length} for the stack collection.");
            }

            return ref Unsafe.Add(ref First, (nint)(uint)index);
        }
    }

    /// <summary>
    /// Gets an enumerator for the stack collection.
    /// </summary>
    /// <returns>A stack collection enumerator.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Enumerator GetEnumerator() => new(ref _startAddress, _length);

    /// <summary>
    /// An enumerator for a stack collection.
    /// </summary>
    public ref struct Enumerator
    {
        private ref byte _currentAddress;
        private readonly int _count;
        private int _index;

        /// <summary>
        /// Creates a <see langword="new"/> stack collection enumerator.
        /// </summary>
        /// <param name="startAddress">The start of the contiguous buffer.</param>
        /// <param name="count">The number of elements in the stack collection.</param>
        public Enumerator(ref byte startAddress, int count)
        {
            _currentAddress = ref startAddress;
            _count = count;
            _index = -1;
        }

        /// <summary>
        /// The current enumerated element for the stack collection.
        /// </summary>
        public ref T Current
        {
            get
            {
                // recover ref T
                ref T start = ref Unsafe.As<byte, T>(ref _currentAddress);
                return ref Unsafe.Add(ref start, _index);
            }
        }

        /// <summary>
        /// Moves the enumerator of the stack collection to the next element.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the enumerator is within the stack collection,
        /// <see langword="false"/> otherwise.
        /// </returns>
        public bool MoveNext()
        {
            _index++;
            return _index < _count;
        }
    }
}
