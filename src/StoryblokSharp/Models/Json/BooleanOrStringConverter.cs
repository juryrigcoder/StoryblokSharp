using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Handles conversion between boolean and string values for the isExternal property
/// </summary>

namespace StoryblokSharp.Models.Json;
public class BooleanOrStringConverter : JsonConverter<string?>
{
    public override string? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.True:
                return "true";
            case JsonTokenType.False:
                return "false";
            case JsonTokenType.String:
                return reader.GetString();
            case JsonTokenType.Null:
                return null;
            default:
                throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, string? value, JsonSerializerOptions options)
    {
        if (value == null)
            writer.WriteNullValue();
        else if (bool.TryParse(value, out bool boolValue))
            writer.WriteBooleanValue(boolValue);
        else
            writer.WriteStringValue(value);
    }
}