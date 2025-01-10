using System.Text.Json;
using System.Text.Json.Serialization;


namespace StoryblokSharp.Models.Json;
public class DownloadImageSrcConverter : JsonConverter<object>
{
    public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            return reader.GetString() ?? string.Empty;
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            return JsonSerializer.Deserialize<JsonElement>(ref reader, options);
        }
        else
        {
            throw new JsonException($"Unexpected token type: {reader.TokenType}");
        }
    }

    public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
    {
        if (value is string stringValue)
        {
            writer.WriteStringValue(stringValue);
        }
        else if (value is JsonElement jsonElement)
        {
            jsonElement.WriteTo(writer);
        }
        else
        {
            throw new JsonException($"Unexpected value type: {value.GetType()}");
        }
    }
}