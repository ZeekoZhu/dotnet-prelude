using System.Text.Json;
using System.Text.Json.Serialization;
using R3;

namespace BlazorWire.Shared;

/// <summary>
/// Custom JSON converter for R3.Unit type that serializes it to 0
/// </summary>
public class UnitJsonConverter : JsonConverter<Unit>
{
    /// <summary>
    /// Reads a Unit value from the JSON reader
    /// </summary>
    /// <param name="reader">The JSON reader</param>
    /// <param name="typeToConvert">The type to convert</param>
    /// <param name="options">The serializer options</param>
    /// <returns>Unit value</returns>
    public override Unit Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Skip the value (should be 0)
        reader.Skip();
        return Unit.Default;
    }

    /// <summary>
    /// Writes a Unit value to the JSON writer as 0
    /// </summary>
    /// <param name="writer">The JSON writer</param>
    /// <param name="value">The Unit value to write</param>
    /// <param name="options">The serializer options</param>
    public override void Write(Utf8JsonWriter writer, Unit value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(0);
    }
}
