using System.Text.Json;
using System.Text.Json.Serialization;
using MIDIFlux.Core.Actions.Parameters;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Custom JSON converter for the Parameters dictionary that ensures proper polymorphic serialization
/// of ActionBase objects while preserving type information through the ActionJsonConverter.
/// </summary>
public class ParametersJsonConverter : JsonConverter<Dictionary<string, object?>>
{
    /// <summary>
    /// Reads parameters from JSON
    /// </summary>
    public override Dictionary<string, object?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Use default deserialization - the ActionBase.JsonParameters setter handles the complex logic
        return JsonSerializer.Deserialize<Dictionary<string, object?>>(ref reader, options) ?? new Dictionary<string, object?>();
    }

    /// <summary>
    /// Writes parameters to JSON with proper polymorphic handling for ActionBase objects
    /// </summary>
    public override void Write(Utf8JsonWriter writer, Dictionary<string, object?> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);

            if (kvp.Value is ActionBase actionBase)
            {
                // Serialize ActionBase with proper polymorphic type information
                JsonSerializer.Serialize(writer, actionBase, typeof(ActionBase), options);
            }
            else if (kvp.Value is ActionBase[] actionArray)
            {
                // Serialize ActionBase array with proper polymorphic type information for each element
                writer.WriteStartArray();
                foreach (var action in actionArray)
                {
                    JsonSerializer.Serialize(writer, action, typeof(ActionBase), options);
                }
                writer.WriteEndArray();
            }
            else if (kvp.Value is Enum enumValue)
            {
                // Serialize enum as string name instead of numeric value
                writer.WriteStringValue(enumValue.ToString());
            }
            else
            {
                // Use default serialization for other types
                JsonSerializer.Serialize(writer, kvp.Value, options);
            }
        }

        writer.WriteEndObject();
    }
}
