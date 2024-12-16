using System.Text.Json;
using System.Text.Json.Serialization;

namespace StoryblokSharp.Models.Json;

/// <summary>
/// Converts between dynamic objects and inferred types for Storyblok API
/// </summary>
public class ObjectToInferredTypesConverter : JsonConverter<object>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out long l))
                    return l;
                return reader.GetDouble();
            case JsonTokenType.String:
                string? str = reader.GetString();
                // Try to parse as DateTimeOffset if it looks like a date
                if (str != null && str.Contains("T") && DateTimeOffset.TryParse(str, out var dt))
                    return dt;
                return str;
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.StartObject:
                var dictionary = new Dictionary<string, object?>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        throw new JsonException("Expected PropertyName");

                    var propertyName = reader.GetString();
                    reader.Read();
                    dictionary[propertyName!] = Read(ref reader, typeof(object), options);
                }
                return dictionary;
            case JsonTokenType.StartArray:
                var list = new List<object?>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndArray)
                        break;

                    list.Add(Read(ref reader, typeof(object), options));
                }
                return list.ToArray();
            default:
                throw new JsonException($"Unsupported token type: {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        JsonSerializer.Serialize(writer, value, value.GetType(), options);
    }
}