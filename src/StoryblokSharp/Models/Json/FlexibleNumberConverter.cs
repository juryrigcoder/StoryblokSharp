using System.Text.Json;
using System.Text.Json.Serialization;

namespace StoryblokSharp.Models.Json;

/// <summary>
/// Converts between string and number values for JSON serialization/deserialization
/// </summary>
public class FlexibleNumberConverter : JsonConverter<long>
{
    public override long Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return reader.GetInt64();
            case JsonTokenType.String:
                if (long.TryParse(reader.GetString(), out long result))
                    return result;
                throw new JsonException("Could not parse string to number");
            case JsonTokenType.Null:
                return 0;
            default:
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, long value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value);
    }
}

/// <summary>
/// Converts between string and nullable number values for JSON serialization/deserialization
/// </summary>
public class FlexibleNullableNumberConverter : JsonConverter<long?>
{
    public override long? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Number:
                return reader.GetInt64();
            case JsonTokenType.String:
                if (string.IsNullOrEmpty(reader.GetString()))
                    return null;
                if (long.TryParse(reader.GetString(), out long result))
                    return result;
                return null;
            case JsonTokenType.Null:
                return null;
            default:
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, long? value, JsonSerializerOptions options)
    {
        if (value.HasValue)
            writer.WriteNumberValue(value.Value);
        else
            writer.WriteNullValue();
    }
}