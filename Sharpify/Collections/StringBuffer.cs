using System.Buffers;

namespace Sharpify.Collections;

/// <summary>
/// Represents a mutable string buffer that allows efficient appending of characters, strings and other <see cref="ISpanFormattable"/> implementations.
/// </summary>
public ref struct StringBuffer {
    private readonly char[] _buffer;
    private readonly int _length;
    private int _position;

    /// <summary>
    /// Creates a mutable string buffer with the specified capacity.
    /// </summary>
    /// <param name="capacity">The capacity</param>
    /// <param name="clearBuffer">Whether clearing the buffer. Has a slight performance hit</param>
    public StringBuffer(int capacity, bool clearBuffer = false) {
        _length = capacity;
        _buffer = ArrayPool<char>.Shared.Rent(_length);
        if (clearBuffer) {
            Array.Clear(_buffer);
        }
        _position = 0;
    }

    /// <summary>
    /// Creates a mutable string buffer of length 0 (empty)
    /// </summary>
    /// <remarks>
    /// It will throw if you try to append anything to it. use <see cref="StringBuffer(int, bool)"/> instead.
    /// </remarks>
    public StringBuffer() : this(0, false) { }

    /// <summary>
    /// Appends a character to the string buffer.
    /// </summary>
    /// <param name="c">The character to append.</param>
    public void Append(char c) {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThan<int>(_position + 1, _length);
#elif NET7_0
        if (_position + 1 >= _length) {
            throw new ArgumentOutOfRangeException(nameof(_length));
        }
#endif

        _buffer[_position++] = c;
    }

    /// <summary>
    /// Appends the specified string to the buffer.
    /// </summary>
    /// <param name="str">The string to append.</param>
    public void Append(ReadOnlySpan<char> str) {
#if NET8_0_OR_GREATER
        ArgumentOutOfRangeException.ThrowIfGreaterThan<int>(_position + str.Length, _length);
#elif NET7_0
        if (_position + str.Length >= _length) {
            throw new ArgumentOutOfRangeException(nameof(_length));
        }
#endif

        str.CopyTo(_buffer.AsSpan(_position));
        _position += str.Length;
    }

    /// <summary>
    /// Appends a value to the string buffer, using the specified format and format provider.
    /// </summary>
    /// <typeparam name="T">The type of the value to append.</typeparam>
    /// <param name="value">The value to append.</param>
    /// <param name="format">The format specifier to apply to the value.</param>
    /// <param name="provider">The format provider to use.</param>
    /// <exception cref="InvalidOperationException">Thrown when the buffer is full.</exception>
    public void Append<T>(T value, ReadOnlySpan<char> format = default, IFormatProvider? provider = null) where T : ISpanFormattable {
        var span = _buffer.AsSpan(_position);
        var written = value.TryFormat(span, out var charsWritten, format, provider);
        if (!written) {
            throw new ArgumentOutOfRangeException("Buffer is full");
        }

        _position += charsWritten;
    }

    /// <summary>
    /// Allocates a string from the internal buffer.
    /// </summary>
    /// <param name="trimEnd">Indicates whether to trim the string from the end.</param>
    /// <returns>The allocated string.</returns>
    public readonly string Allocate(bool trimEnd = true) {
        ReadOnlySpan<char> span = _buffer;
        var str = trimEnd
            ? new string(span[0.._position])
            : new string(span[0.._length]);
        return str;
    }

    /// <summary>
    /// Use the allocate function with the trimEnd parameter set to true.
    /// </summary>
    /// <param name="buffer"></param>
    public static implicit operator string(StringBuffer buffer) => buffer.Allocate(true);

    /// <summary>
    /// Returns a string allocated from the StringBuffer.
    /// </summary>
    /// <remarks>It is identical to <see cref="Allocate(bool)"/></remarks>
    public override readonly string ToString() => Allocate(true);

    /// <summary>
    /// Releases the resources used by the StringBuffer.
    /// </summary>
    public readonly void Dispose() => ArrayPool<char>.Shared.Return(_buffer, false);
}