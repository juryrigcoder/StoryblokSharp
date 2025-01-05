using System.Text.Json;
using System.Text.Json.Serialization;

namespace StoryblokSharp.Models.Json;

/// <summary>
/// Converts between string/object values for image source fields
/// </summary>
public class FlexibleImageSourceConverter : JsonConverter<object?>
{
    public override object? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.StartObject:
                // Return the entire object as a dictionary
                var dictionary = new Dictionary<string, object?>();
                while (reader.Read())
                {
                    if (reader.TokenType == JsonTokenType.EndObject)
                        break;

                    if (reader.TokenType != JsonTokenType.PropertyName)
                        throw new JsonException("Expected PropertyName");

                    var propertyName = reader.GetString();
                    reader.Read();
                    dictionary[propertyName!] = GetValue(ref reader);
                }
                return dictionary;
            case JsonTokenType.Null:
                return null;
            default:
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }
    }

    private object? GetValue(ref Utf8JsonReader reader)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Number:
                if (reader.TryGetInt64(out long longValue))
                    return longValue;
                return reader.GetDouble();
            case JsonTokenType.True:
                return true;
            case JsonTokenType.False:
                return false;
            case JsonTokenType.Null:
                return null;
            default:
                throw new JsonException($"Unexpected value token type: {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, object? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        switch (value)
        {
            case string stringValue:
                writer.WriteStringValue(stringValue);
                break;
            case Dictionary<string, object?> dictValue:
                writer.WriteStartObject();
                foreach (var kvp in dictValue)
                {
                    writer.WritePropertyName(kvp.Key);
                    JsonSerializer.Serialize(writer, kvp.Value, options);
                }
                writer.WriteEndObject();
                break;
            default:
                JsonSerializer.Serialize(writer, value, options);
                break;
        }
    }
}