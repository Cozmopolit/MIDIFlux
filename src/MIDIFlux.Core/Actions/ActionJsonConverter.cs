using System.Text.Json;
using System.Text.Json.Serialization;

namespace MIDIFlux.Core.Actions;

/// <summary>
/// JSON converter for polymorphic action deserialization.
/// Handles type-safe conversion from JSON to strongly-typed action instances.
/// Replaces the old ActionConfigJsonConverter for the new unified system.
/// </summary>
public class ActionJsonConverter : JsonConverter<ActionBase>
{
    /// <summary>
    /// Reads action from JSON with polymorphic type resolution
    /// </summary>
    public override ActionBase? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Read the JSON object into a JsonDocument for inspection
        using var doc = JsonDocument.ParseValue(ref reader);
        var root = doc.RootElement;

        // Look for the $type property to determine the concrete type
        if (!root.TryGetProperty("$type", out var typeProperty))
        {
            throw new JsonException("Missing $type property in action JSON");
        }

        var typeName = typeProperty.GetString();
        if (string.IsNullOrEmpty(typeName))
        {
            throw new JsonException("Empty $type property in action JSON");
        }

        // Use reflection-based action type discovery instead of massive switch statement
        var actionType = ActionTypeRegistry.Instance.GetActionType(typeName);
        if (actionType == null)
        {
            throw new JsonException($"Unknown action type: {typeName}");
        }

        // Create a new JsonSerializerOptions without this converter to avoid recursion
        var innerOptions = new JsonSerializerOptions(options);
        innerOptions.Converters.Clear();

        // Add back other converters except this one
        foreach (var converter in options.Converters)
        {
            if (converter.GetType() != typeof(ActionJsonConverter))
            {
                innerOptions.Converters.Add(converter);
            }
        }

        // Deserialize to the concrete type
        var rawText = root.GetRawText();
        var result = JsonSerializer.Deserialize(rawText, actionType, innerOptions) as ActionBase;

        if (result == null)
        {
            throw new JsonException($"Failed to deserialize action of type {typeName}");
        }

        return result;
    }

    /// <summary>
    /// Writes action to JSON with type information
    /// </summary>
    public override void Write(Utf8JsonWriter writer, ActionBase value, JsonSerializerOptions options)
    {
        // Get the concrete type name
        var typeName = value.GetType().Name;

        writer.WriteStartObject();

        // Write the $type property first
        writer.WriteString("$type", typeName);

        // Create a new JsonSerializerOptions without this converter to avoid recursion
        var innerOptions = new JsonSerializerOptions(options);
        innerOptions.Converters.Clear();

        // Add back other converters except this one
        foreach (var converter in options.Converters)
        {
            if (converter.GetType() != typeof(ActionJsonConverter))
            {
                innerOptions.Converters.Add(converter);
            }
        }

        // Serialize the rest of the object
        var json = JsonSerializer.Serialize(value, value.GetType(), innerOptions);
        using var doc = JsonDocument.Parse(json);

        foreach (var property in doc.RootElement.EnumerateObject())
        {
            if (property.Name != "$type") // Skip $type as we already wrote it
            {
                property.WriteTo(writer);
            }
        }

        writer.WriteEndObject();
    }
}
