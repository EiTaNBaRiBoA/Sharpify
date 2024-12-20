using MemoryPack;

using Sharpify.Collections;

namespace Sharpify.Data.Serializers;

/// <summary>
/// A serializer for a database without encryption and case sensitive keys
/// </summary>
internal class Serializer : AbstractSerializer {
    internal Serializer(string path, StringEncoding encoding = StringEncoding.Utf8) : base(path, encoding) {
    }

/// <inheritdoc />
    internal override Dictionary<string, byte[]?> Deserialize(int estimatedSize) {
        if (estimatedSize is 0) {
            return new Dictionary<string, byte[]?>();
        }
        using var buffer = new RentedBufferWriter<byte>(estimatedSize);
        using var file = new FileStream(_path, FileMode.Open);
        int numRead = file.Read(buffer.Buffer, 0, estimatedSize);
        buffer.Advance(numRead);
        Dictionary<string, byte[]?> dict =
            MemoryPackSerializer.Deserialize<Dictionary<string, byte[]?>>(buffer.WrittenSpan, SerializerOptions)
         ?? new Dictionary<string, byte[]?>();
        return dict;
    }

/// <inheritdoc />
    internal override async ValueTask<Dictionary<string, byte[]?>> DeserializeAsync(int estimatedSize, CancellationToken cancellationToken = default) {
        if (estimatedSize is 0) {
            return new Dictionary<string, byte[]?>();
        }
        using var file = new FileStream(_path, FileMode.Open);
        var dict = await MemoryPackSerializer.DeserializeAsync<Dictionary<string, byte[]?>>(file, SerializerOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
        return dict ?? new Dictionary<string, byte[]?>();
    }

/// <inheritdoc />
    internal override void Serialize(Dictionary<string, byte[]?> dict, int estimatedSize) {
        using var file = new FileStream(_path, FileMode.Create);
        using var buffer = new RentedBufferWriter<byte>(estimatedSize);
        MemoryPackSerializer.Serialize(in buffer, in dict, SerializerOptions);
        file.Write(buffer.WrittenSpan);
    }

/// <inheritdoc />
    internal override async ValueTask SerializeAsync(Dictionary<string, byte[]?> dict, CancellationToken cancellationToken = default) {
        using var file = new FileStream(_path, FileMode.Create);
        await MemoryPackSerializer.SerializeAsync(file, dict, SerializerOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
    }
}