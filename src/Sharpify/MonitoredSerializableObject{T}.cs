using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

namespace Sharpify;

/// <summary>
/// Represents a <see cref="SerializableObject{T}"/> that is monitored for changes from the file system.
/// </summary>
/// <typeparam name="T">The type of the value stored in the object.</typeparam>
/// <remarks>
/// This class provides functionality to serialize and deserialize the object to/from a file,
/// and raises an event whenever the file or the object is modified.
/// </remarks>
public class MonitoredSerializableObject<T> : SerializableObject<T> {
    private readonly FileSystemWatcher _watcher;

    /// <summary>
    /// Represents a serializable object that is monitored for changes in a specified file path.
    /// </summary>
    /// <param name="path">The path to the file. validated on creation</param>
    /// <param name="jsonTypeInfo">The json type info that can be used to serialize T without reflection</param>
    /// <exception cref="IOException">Thrown when the directory of the path does not exist or when the filename is invalid.</exception>
    public MonitoredSerializableObject(string path, JsonTypeInfo<T> jsonTypeInfo) : this(path, default!, jsonTypeInfo) { }

    /// <summary>
    /// Represents a serializable object that is monitored for changes in a specified file path.
    /// </summary>
    /// <param name="path">The path to the file. validated on creation</param>
    /// <param name="defaultValue">the default value of T, will be used if the file doesn't exist or can't be deserialized</param>
    /// <param name="jsonTypeInfo">The json type info that can be used to serialize T without reflection</param>
    /// <exception cref="IOException">Thrown when the directory of the path does not exist or when the filename is invalid.</exception>
    public MonitoredSerializableObject(string path, T defaultValue, JsonTypeInfo<T> jsonTypeInfo) : base(path, defaultValue, jsonTypeInfo) {
        _watcher = new FileSystemWatcher(_segmentedPath.Directory, _segmentedPath.FileName) {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true
        };

        _watcher.Changed += OnFileChanged;
    }

    private async void OnFileChanged(object sender, FileSystemEventArgs e) {
        if (e.ChangeType is not WatcherChangeTypes.Changed) {
            return;
        }
        if (!File.Exists(_path)) {
            return;
        }
        try {
            _lock.EnterWriteLock();
            var res = await ReadFromFileAsync();
            if (res.IsFail) {
                return;
            }
            _value = res.Value!;
            InvokeOnChangedEvent(_value);
        } finally {
            _lock.ExitWriteLock();
        }
    }

    private async Task<Result<T>> ReadFromFileAsync() {
        int retries = 5;
        do {
            try {
                using var file = File.Open(_path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                T? deserialized = JsonSerializer.Deserialize(file, _jsonTypeInfo);
                return deserialized is null ? Result.Fail() : Result.Ok(deserialized);
            } catch (JsonException) { // Handles invalid files
                return Result.Fail();
            } catch (IOException) {
                await Task.Delay(100);
                continue;
            }
        } while (Interlocked.Decrement(ref retries) >= 0);

        return Result.Fail();
    }

    /// <inheritdoc/>
    public override void Modify(Func<T, T> modifier) {
        try {
            _lock.EnterWriteLock();
            _value = modifier(_value);
            _watcher.EnableRaisingEvents = false;
            using var file = File.Open(_path, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            JsonSerializer.Serialize(file, _value, _jsonTypeInfo);
            InvokeOnChangedEvent(_value);
        } finally {
            _lock.ExitWriteLock();
            _watcher.EnableRaisingEvents = true;
        }
    }

    /// <inheritdoc/>
    public override void Dispose() {
        if (_disposed) {
            return;
        }
        _watcher?.Dispose();
        _lock?.Dispose();
        _disposed = true;
    }
}