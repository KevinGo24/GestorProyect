using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GestorProyect.api.Serialization;

public sealed class FechaLocalJsonConverter : JsonConverter<DateOnly>
{
    private const string Formato = "dd-MM-yy";

    public override DateOnly Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        var value = reader.GetString();
        if (value is null || !DateOnly.TryParseExact(
                value,
                Formato,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var fecha))
        {
            throw new JsonException($"La fecha debe tener el formato {Formato}.");
        }

        return fecha;
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value.ToString(Formato, CultureInfo.InvariantCulture));
}
