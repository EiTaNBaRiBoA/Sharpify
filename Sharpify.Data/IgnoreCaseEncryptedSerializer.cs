using System.Security.Cryptography;

namespace Sharpify.Data;

/// <summary>
/// A serializer for a database encryption and case sensitive keys
/// </summary>
internal class IgnoreCaseEncryptedSerializer : EncryptedSerializer {
    internal IgnoreCaseEncryptedSerializer(string path, string key) : base(path, key) {
    }

/// <inheritdoc />
    internal override Dictionary<string, ReadOnlyMemory<byte>> Deserialize(int estimatedSize) {
        if (estimatedSize is 0) {
            return new Dictionary<string, ReadOnlyMemory<byte>>(StringComparer.OrdinalIgnoreCase);
        }
        using var buffer = new RentedBufferWriter<byte>(estimatedSize);
        using var file = new FileStream(_path, FileMode.Open);
        using var transform = Helper.Instance.GetDecryptor(_key);
        using var cryptoStream = new CryptoStream(file, transform, CryptoStreamMode.Read);
        var numRead = cryptoStream.Read(buffer.Buffer, 0, estimatedSize - AesProvider.ReservedBufferSize);
        buffer.Advance(numRead);
        var dict = IgnoreCaseSerializer.FromSpan(buffer.WrittenSpan);
        return dict;
    }

/// <inheritdoc />
    internal override async ValueTask<Dictionary<string, ReadOnlyMemory<byte>>> DeserializeAsync(int estimatedSize, CancellationToken cancellationToken = default) {
        if (estimatedSize is 0) {
            return new Dictionary<string, ReadOnlyMemory<byte>>(StringComparer.OrdinalIgnoreCase);
        }
        using var buffer = new RentedBufferWriter<byte>(estimatedSize);
        using var file = new FileStream(_path, FileMode.Open);
        using var transform = Helper.Instance.GetDecryptor(_key);
        using var cryptoStream = new CryptoStream(file, transform, CryptoStreamMode.Read);
        var numRead = await cryptoStream.ReadAsync(buffer.Buffer, 0, estimatedSize, cancellationToken);
        buffer.Advance(numRead);
        var dict = IgnoreCaseSerializer.FromSpan(buffer.WrittenSpan);
        return dict;
    }
}