using MemoryPack;

namespace Sharpify.Data;

/// <summary>
/// A serializer for a database without encryption and case sensitive keys
/// </summary>
internal class DisabledIgnoreCaseSerializer : DisabledSerializer {
    internal DisabledIgnoreCaseSerializer(string path, StringEncoding encoding = StringEncoding.Utf8) : base(path, encoding) {
    }

    /// <inheritdoc />
    internal override Dictionary<string, byte[]?> Deserialize(int estimatedSize) => new Dictionary<string, byte[]?>(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    internal override ValueTask<Dictionary<string, byte[]?>> DeserializeAsync(int estimatedSize, CancellationToken cancellationToken = default) => ValueTask.FromResult(new Dictionary<string, byte[]?>(StringComparer.OrdinalIgnoreCase));
}