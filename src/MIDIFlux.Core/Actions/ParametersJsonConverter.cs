using System.Text.Json;
using System.Text.Json.Serialization;
using MIDIFlux.Core.Actions.Parameters;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// Custom JSON converter for the Parameters dictionary that handles all parameter type deserialization
/// and ensures proper polymorphic serialization of ActionBase objects.
/// </summary>
public class ParametersJsonConverter : JsonConverter<Dictionary<string, object?>>
{
    /// <summary>
    /// Reads parameters from JSON with full type conversion handling
    /// </summary>
    public override Dictionary<string, object?> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // First deserialize as raw dictionary
        var rawDict = JsonSerializer.Deserialize<Dictionary<string, object?>>(ref reader, options) ?? new Dictionary<string, object?>();

        // Return the raw dictionary - ActionBase will handle the type conversion using the parameter definitions
        return rawDict;
    }

    /// <summary>
    /// Converts a JsonElement to the appropriate type based on the parameter type
    /// </summary>
    /// <param name="jsonElement">The JsonElement to convert</param>
    /// <param name="parameterType">The target parameter type</param>
    /// <returns>The converted value</returns>
    public static object? ConvertJsonElementToParameterType(JsonElement jsonElement, ParameterType parameterType)
    {
        return parameterType switch
        {
            ParameterType.Integer => jsonElement.ValueKind switch
            {
                JsonValueKind.Number => jsonElement.GetInt32(),
                JsonValueKind.String when int.TryParse(jsonElement.GetString(), out var intValue) => intValue,
                _ => throw new JsonException($"Cannot convert JsonElement of kind {jsonElement.ValueKind} to integer")
            },
            ParameterType.String => jsonElement.ValueKind switch
            {
                JsonValueKind.String => jsonElement.GetString(),
                JsonValueKind.Number => jsonElement.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => null,
                _ => jsonElement.GetRawText()
            },
            ParameterType.Boolean => jsonElement.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String when bool.TryParse(jsonElement.GetString(), out var boolValue) => boolValue,
                _ => throw new JsonException($"Cannot convert JsonElement of kind {jsonElement.ValueKind} to boolean")
            },
            ParameterType.Enum => jsonElement.ValueKind switch
            {
                JsonValueKind.Number => jsonElement.GetInt32(),
                JsonValueKind.String => jsonElement.GetString(), // Return string value for enum name parsing
                _ => throw new JsonException($"Cannot convert JsonElement of kind {jsonElement.ValueKind} to enum")
            },
            ParameterType.ByteArray => jsonElement.ValueKind switch
            {
                JsonValueKind.Array => jsonElement.EnumerateArray().Select(e => e.GetByte()).ToArray(),
                JsonValueKind.String => Convert.FromBase64String(jsonElement.GetString() ?? ""),
                _ => throw new JsonException($"Cannot convert JsonElement of kind {jsonElement.ValueKind} to byte array")
            },
            ParameterType.SubAction => DeserializeSubAction(jsonElement),
            ParameterType.SubActionList => DeserializeSubActionList(jsonElement),
            ParameterType.ValueConditionList => DeserializeValueConditionList(jsonElement),
            _ => throw new JsonException($"Unsupported parameter type: {parameterType}")
        };
    }

    /// <summary>
    /// Deserializes a single SubAction from JsonElement
    /// </summary>
    private static ActionBase? DeserializeSubAction(JsonElement jsonElement)
    {
        var actionJson = jsonElement.GetRawText();
        return JsonSerializer.Deserialize<ActionBase>(actionJson);
    }

    /// <summary>
    /// Deserializes a SubActionList from JsonElement
    /// </summary>
    private static List<ActionBase> DeserializeSubActionList(JsonElement jsonElement)
    {
        var actionList = new List<ActionBase>();
        if (jsonElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var actionElement in jsonElement.EnumerateArray())
            {
                var actionJson = actionElement.GetRawText();
                var action = JsonSerializer.Deserialize<ActionBase>(actionJson);
                if (action != null)
                {
                    actionList.Add(action);
                }
            }
        }
        return actionList;
    }

    /// <summary>
    /// Deserializes a ValueConditionList from JsonElement
    /// </summary>
    private static List<Parameters.ValueCondition> DeserializeValueConditionList(JsonElement jsonElement)
    {
        var conditionList = new List<Parameters.ValueCondition>();
        if (jsonElement.ValueKind == JsonValueKind.Array)
        {
            foreach (var conditionElement in jsonElement.EnumerateArray())
            {
                var conditionJson = conditionElement.GetRawText();
                var condition = JsonSerializer.Deserialize<Parameters.ValueCondition>(conditionJson);
                if (condition != null)
                {
                    conditionList.Add(condition);
                }
            }
        }
        return conditionList;
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
