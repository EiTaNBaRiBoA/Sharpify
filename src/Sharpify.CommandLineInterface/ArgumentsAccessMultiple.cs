using System.Globalization;

namespace Sharpify.CommandLineInterface;

public sealed partial class Arguments {
    /// <summary>
    /// Tries to retrieve the value of a positional argument.
    /// </summary>
    /// <param name="position">The key to check.</param>
    /// <param name="separator"></param>
    /// <param name="values">The values of the argument or an empty array if they don't exist.</param>
    /// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetValues(int position, string? separator, out string[] values)
        => TryGetValues(position.ToString(), separator, out values);

    /// <summary>
    /// Tries to retrieve the value of a specified key in the arguments.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <param name="separator"></param>
    /// <param name="values">The values of the argument or an empty array if don't exist.</param>
    /// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetValues(string key, string? separator, out string[] values) {
        if (!_arguments.TryGetValue(key, out var res)) {
            values = [];
            return false;
        }
        values = res.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return true;
    }

    /// <summary>
    /// Tries to retrieve a values of one of a specified key's aliases in the arguments.
    /// </summary>
    /// <param name="keys">A collection of aliases for a parameter name</param>
    /// <param name="separator"></param>
    /// <param name="values">The values of the argument or an empty array if don't exist.</param>
    /// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetValues(ReadOnlySpan<string> keys, string? separator, out string[] values) {
        if (!_arguments.TryGetValue(keys, out var res)) {
            values = [];
            return false;
        }
        values = res.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        return true;
    }

    /// <summary>
    /// Tries to retrieve the values of a specified key in the arguments.
    /// </summary>
    /// <param name="position">The positional argument to check.</param>
    /// <param name="separator"></param>
    /// <param name="values">The values of the argument or an empty array if don't exist.</param>
    /// <remarks>
    /// <para>
    /// If the key doesn't exist or any of the values can't be parsed, an empty array will be used in the out parameter.
    /// </para>
    /// </remarks>
    /// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetValues<T>(int position, string? separator, out T[] values) where T : IParsable<T>
        => TryGetValues(position.ToString(), separator, out values);

    /// <summary>
    /// Tries to retrieve the values of a specified key in the arguments.
    /// </summary>
    /// <param name="key">The key to check.</param>
    /// <param name="separator"></param>
    /// <param name="values">The values of the argument or an empty array if don't exist.</param>
    /// <remarks>
    /// <para>
    /// If the key doesn't exist or any of the values can't be parsed, an empty array will be used in the out parameter.
    /// </para>
    /// </remarks>
    /// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetValues<T>(string key, string? separator, out T[] values) where T : IParsable<T> {
        if (!TryGetValue(key, out string val)) {
            values = [];
            return false;
        }

        var parts = val.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var result = new T[parts.Length];
        int i = 0;

        foreach (var part in parts) {
            if (!T.TryParse(part, CultureInfo.CurrentCulture, out T? parsed)) {
                values = [];
                return false;
            }
            result[i++] = parsed;
        }

        values = result;
        return true;
    }

    /// <summary>
    /// Tries to retrieve a values of one of a specified key's aliases in the arguments.
    /// </summary>
    /// <param name="keys">A collection of aliases for a parameter name</param>
    /// <param name="separator"></param>
    /// <param name="values">The values of the argument or an empty array if don't exist.</param>
    /// <remarks>
    /// <para>
    /// If the key doesn't exist or any of the values can't be parsed, an empty array will be used in the out parameter.
    /// </para>
    /// </remarks>
    /// <returns>true if the key exists, false otherwise.</returns>
    public bool TryGetValues<T>(ReadOnlySpan<string> keys, string? separator, out T[] values) where T : IParsable<T> {
        if (!TryGetValue(keys, out string val)) {
            values = [];
            return false;
        }

        var parts = val.Split(separator, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        var result = new T[parts.Length];
        int i = 0;

        foreach (var part in parts) {
            if (!T.TryParse(part, CultureInfo.CurrentCulture, out T? parsed)) {
                values = [];
                return false;
            }
            result[i++] = parsed;
        }

        values = result;
        return true;
    }
}
