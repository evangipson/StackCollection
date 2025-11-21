using System.Runtime.CompilerServices;

namespace StackCollection;

/// <summary>
/// A stack-allocated <see langword="readonly"/> collection of elements.
/// <para>
/// Must provide <see cref="StackBuffer{T}.Instance"/> as a <see langword="ref"/>
/// parameter when creating a <see langword="new"/> stack collection.
/// </para>
/// </summary>
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public ref struct StackCollection<T>(ref T bufferStart, int capacity = StackCollectionConstants.DefaultBufferSize)
    where T : allows ref struct
{
    private ref byte _startAddress = ref Unsafe.As<T, byte>(ref bufferStart);
    private readonly int _capacity = capacity;
    private int _length = 0;
    private readonly int _elementBytes = Unsafe.SizeOf<T>();

    /// <summary>
    /// The amount of elements in the stack collection.
    /// </summary>
    public readonly int Length => _length;

    /// <summary>
    /// The total allocated bytes of a single element in the stack collection.
    /// </summary>
    public readonly int ElementBytes => _elementBytes;

    /// <summary>
    /// The total allocated bytes of the stack collection.
    /// </summary>
    public readonly int TotalBytes => ElementBytes * Length;

    /// <summary>
    /// The maximum number of elements for the stack collection.
    /// </summary>
    public readonly int Capacity => _capacity;

    /// <summary>
    /// Adds an element to the stack collection.
    /// <para>
    /// Will <see langword="throw"/> an <see cref="IndexOutOfRangeException"/> if
    /// the number of elements is already at <see cref="Capacity"/>.
    /// </para>
    /// </summary>
    /// <param name="item">The element to add to the stack collection.</param>
    /// <exception cref="IndexOutOfRangeException"/>
    public void Add(T item)
    {
        // throw if the length will equal or exceed capacity.
        if (_length >= _capacity)
        {
            throw new IndexOutOfRangeException("Unable to add an element to the stack collection because the buffer is full.");
        }

        // calculate offset based on the SAFE, contiguous starting point
        ref T start = ref Unsafe.As<byte, T>(ref _startAddress);
        ref T slot = ref Unsafe.Add(ref start, _length);

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
            if ((uint)index >= (uint)_length)
            {
                throw new IndexOutOfRangeException($"Unable to index {index} element of {Length} for the stack collection.");
            }

            // get the start of the contiguous buffer
            ref T start = ref Unsafe.As<byte, T>(ref _startAddress);

            // increment the pointer of the buffer and return it
            return ref Unsafe.Add(ref start, index);
        }
    }

    /// <summary>
    /// Gets an enumerator for the stack collection.
    /// </summary>
    /// <returns>A stack collection enumerator.</returns>
    public Enumerator GetEnumerator() => new(ref _startAddress, _length);

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
