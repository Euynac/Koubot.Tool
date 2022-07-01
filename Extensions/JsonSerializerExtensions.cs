using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Koubot.Tool.Extensions;

public static class JsonSerializerExtensions
{
    /// <summary>
    /// Using <see cref="JsonSerializer"/> to deserialize json string to <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json">Support <see langword="null"/>, <see cref="string.Empty"/> and whitespace.</param>
    /// <param name="options"></param>
    /// <returns>return <see langword="null"/> or <see langword="default"/> if <paramref name="json"/> is <see langword="null"/>, <see cref="string.Empty"/> or whitespace.</returns>
    public static T? DeserializeJson<T>(this string? json, JsonSerializerOptions? options = default) => string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.Deserialize<T>(json, options);

    /// <summary>
    /// Using <see cref="JsonSerializer"/> to deserialize json string to <typeparamref name="T"/> async.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="json">Support <see langword="null"/>, <see cref="string.Empty"/> and whitespace.</param>
    /// <param name="options"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>return <see langword="null"/> or <see langword="default"/> if <paramref name="json"/> is <see langword="null"/>, <see cref="string.Empty"/> or whitespace.</returns>
    public static ValueTask<T?> DeserializeJsonAsync<T>(this string? json, JsonSerializerOptions? options = default, CancellationToken cancellationToken = default) => string.IsNullOrWhiteSpace(json) ? default : JsonSerializer.DeserializeAsync<T>(json.ConvertToStream(), options, cancellationToken);

    /// <summary>
    /// Use future populate method instead #29538
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="target"></param>
    /// <param name="jsonSource"></param>
    public static void PopulateObject<T>(T target, string jsonSource) where T : class =>
        PopulateObject(target, jsonSource, typeof(T));

    public static void OverwriteProperty<T>(T target, JsonProperty updatedProperty) where T : class =>
        OverwriteProperty(target, updatedProperty, typeof(T));

    public static void PopulateObject(object target, string jsonSource, Type type)
    {
        var json = JsonDocument.Parse(jsonSource).RootElement;
        foreach (var property in json.EnumerateObject())
        {
            OverwriteProperty(target, property, type);
        }
    }

    public static void OverwriteProperty(object target, JsonProperty updatedProperty, Type type)
    {
        var propertyInfo = type.GetProperty(updatedProperty.Name);

        if (propertyInfo == null)
        {
            return;
        }

        var propertyType = propertyInfo.PropertyType;
        object parsedValue;

        if (propertyType.IsValueType || propertyType == typeof(string))
        {
            parsedValue = JsonSerializer.Deserialize(
                updatedProperty.Value.GetRawText(),
                propertyType)!;
        }
        else
        {
            parsedValue = propertyInfo.GetValue(target);
            PopulateObject(
                parsedValue,
                updatedProperty.Value.GetRawText(),
                propertyType);
        }

        propertyInfo.SetValue(target, parsedValue);
    }

    public static T? DeserializeAnonymousType<T>(string? json, T anonymousTypeObject, JsonSerializerOptions? options = default)
        => json.DeserializeJson<T>(options);

    public static ValueTask<TValue?> DeserializeAnonymousTypeAsync<TValue>(string? json, TValue anonymousTypeObject,
        JsonSerializerOptions? options = default, CancellationToken cancellationToken = default)
        => DeserializeJsonAsync<TValue?>(json, options, cancellationToken);
    public static ValueTask<TValue?> DeserializeAnonymousTypeAsync<TValue>(Stream stream, TValue anonymousTypeObject, JsonSerializerOptions? options = default, CancellationToken cancellationToken = default)
        => JsonSerializer.DeserializeAsync<TValue>(stream, options, cancellationToken); // Method to deserialize from a stream added for completeness

    
}