using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace StackCollection;

/// <summary>
/// A stack-allocated <see langword="readonly"/> collection of elements.
/// </summary>
[DebuggerDisplay("{ToString(),raw}")]
[DebuggerTypeProxy(typeof(StackCollectionDebugView<>))]
public ref struct StackCollection<T> where T : allows ref struct
{
    private int _length;
    private int _capacity;
    private readonly ref byte _startAddress;

    /// <summary>
    /// Creates a new empty <see cref="StackCollection{T}"/> with no capacity.
    /// <para>
    /// Serves as <see langword="default"/>, and more than likely not the desired behavior.
    /// </para>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StackCollection()
    {
        _length = 0;
        _capacity = 0;
        _startAddress = ref Unsafe.NullRef<byte>();
    }

    /// <summary>
    /// Creates a new <see cref="StackCollection{T}"/> using the provided <paramref name="reference"/>
    /// to guarantee no heap allocation.
    /// </summary>
    /// <param name="reference">The starting element to use as a reference.</param>
    /// <param name="capacity">The maximum number of elements the stack collection can hold.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public StackCollection(ref T reference, int capacity = 0, int length = 0)
    {
        if (length == 0 && capacity == 0)
        {
            this = default;
            return;
        }

        _length = length;
        _capacity = length > capacity ? length : capacity;
        _startAddress = ref Unsafe.As<T, byte>(ref reference);
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
    public ref T this[int index]
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
    /// Trims the stack collection <see cref="Capacity"/> to match the <see cref="Length"/>.
    /// </summary>
    public void Trim() => _capacity = _length;

    /// <summary>
    /// Sets the <see cref="Length"/> to <c>0</c>, so the next <see cref="Add(T)"/> will
    /// overwrite the first element.
    /// </summary>
    public void Clear()
    {
        // zero out memory by using Span<byte>, which is safe even if T is a ref struct
        ref byte startByte = ref _startAddress;
        int totalBytesToClear = _length * Unsafe.SizeOf<T>();

        // the cast to Span<T> is restricted, but Span<byte> is not
        Span<byte> activeBytes = MemoryMarshal.CreateSpan(ref startByte, totalBytesToClear);
        activeBytes.Clear();

        // reset the length
        _length = 0;
    }

    /// <summary>
    /// Copies the active elements of the current collection to the destination collection.
    /// <para>
    /// Will <see langword="throw"/> an <see cref="InvalidOperationException"/> if the destination capacity is insufficient
    /// or the destination buffer is uninitialized.
    /// </para>
    /// </summary>
    /// <param name="destination">The destination stack collection to copy elements into, which has it's length updated.</param>
    /// <exception cref="InvalidOperationException"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(ref StackCollection<T> destination)
    {
        // ensure destination is empty
        if (_length == 0)
        {
            destination.Clear();
            return;
        }

        if (Unsafe.IsNullRef(ref destination._startAddress))
        {
            throw new InvalidOperationException("Destination collection buffer is uninitialized.");
        }

        if (destination._capacity < _length)
        {
            throw new InvalidOperationException($"Destination capacity ({destination._capacity}) is too small to hold the source length ({_length}).");
        }

        // calculate the total number of bytes to copy
        int byteCount = _length * Unsafe.SizeOf<T>();

        // fast, low-level memory copy (memcpy).
        Unsafe.CopyBlock(ref Unsafe.As<T, byte>(ref destination.First), ref Unsafe.As<T, byte>(ref First), (uint)byteCount);

        // update the destination's length to match the copied count
        destination._length = _length;
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

    /// <summary>
    /// Returns a <see cref="string"/> with the name of the type and the number of elements.
    /// </summary>
    public override readonly string ToString()
        => $"System.ReadOnlySpan<{typeof(T).Name}>[{_length}]";
}