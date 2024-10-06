using Sharpify.Collections;

namespace Sharpify.Tests.Collections;

public class StringBuffersTests {
    [Fact]
    public void AllocatedStringBuffer_NoCapacity_Throws() {
        // Arrange
        Action act = () => {
            var buffer = new StringBuffer();
            buffer.Append('a');
        };

        // Act & Assert
        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void AllocatedStringBuffer_AppendLine_OnElement() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[20]);

        // Act
        buffer.AppendLine("Hello");
        buffer.Append("World");

        var expected = string.Create(null, stackalloc char[20], $"Hello{Environment.NewLine}World");

        // Assert
        buffer.Allocate(true).Should().Be(expected);
    }

    [Fact]
    public void AllocatedStringBuffer_AppendLine_NoParams() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[20]);

        // Act
        buffer.Append("Hello");
        buffer.AppendLine();
        buffer.Append("World");

        var expected = string.Create(null, stackalloc char[20], $"Hello{Environment.NewLine}World");

        // Assert
        buffer.Allocate(true).Should().Be(expected);
    }

    [Fact]
    public void AllocatedStringBuffer_AppendLine_NoParams_Builder() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[20]);

        // Act
        buffer.Append("Hello")
              .AppendLine()
              .Append("World");

        var expected = string.Create(null, stackalloc char[20], $"Hello{Environment.NewLine}World");

        // Assert
        buffer.Allocate(true).Should().Be(expected);
    }

    [Fact]
    public void AllocatedStringBuffer_NoTrimming_ReturnFullString() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[5]);

        // Act
        buffer.Append('a');
        buffer.Append('b');
        buffer.Append('c');
        buffer.Append('d');

        // Assert
        buffer.Allocate(false).Should().Be("abcd\0");
    }

    [Fact]
    public void AllocatedStringBuffer_WithTrimming_ReturnTrimmedString() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[5]);

        // Act
        buffer.Append('a');
        buffer.Append('b');
        buffer.Append('c');
        buffer.Append('d');

        // Assert
        buffer.Allocate(true).Should().Be("abcd");
    }

    [Fact]
    public void AllocatedStringBuffer_WithWhiteSpaceTrimming_ReturnTrimmedString() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[5]);

        // Act
        buffer.Append('a');
        buffer.Append('b');
        buffer.Append('c');
        buffer.Append('d');
        buffer.Append(' ');

        // Assert
        buffer.Allocate(true, true).Should().Be("abcd");
    }

    [Fact]
    public void AllocatedStringBuffer_ImplicitOperatorString() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[10]);

        // Act
        buffer.Append('a');
        buffer.Append('b');
        buffer.Append('c');
        buffer.Append('d');

        // Assert
        string str = buffer.Allocate();
        str.Should().Be("abcd");
    }

    [Fact]
    public void AllocatedStringBuffer_ImplicitOperatorReadOnlySpan() {
        // Arrange
        var buffer = StringBuffer.Create(stackalloc char[10]);

        // Act
        buffer.Append('a');
        buffer.Append('b');
        buffer.Append('c');
        buffer.Append('d');

        // Assert
        (buffer.WrittenSpan is "abcd").Should().Be(true);
    }
}