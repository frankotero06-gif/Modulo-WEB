using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PortalCitas.Models;

/// <summary>
/// Lee un campo como texto aunque el JSON lo traiga como número.
///
/// Se usa en <c>pacienteId</c>: el Gateway (Ricardo) lo devuelve como texto
/// ("15") y el CORE (Emilia) como número (15). Con este convertidor la web
/// acepta las dos formas y siempre lo maneja como texto, así no falla
/// ReadFromJsonAsync si alguna de las dos capas cambia.
/// Al enviar, lo escribe como texto.
/// </summary>
public sealed class TextoFlexibleConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                return reader.GetString() ?? string.Empty;
            case JsonTokenType.Number:
                return reader.TryGetInt64(out var entero)
                    ? entero.ToString(CultureInfo.InvariantCulture)
                    : reader.GetDecimal().ToString(CultureInfo.InvariantCulture);
            default:
                throw new JsonException($"Se esperaba texto o número y llegó {reader.TokenType}.");
        }
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options) =>
        writer.WriteStringValue(value);
}
