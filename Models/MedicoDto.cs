using System.Text.Json.Serialization;

namespace PortalCitas.Models;

public class MedicoDto
{
    public int Id { get; set; }

    /// <summary>El CORE lo envía como "nombreCompleto".</summary>
    [JsonPropertyName("nombreCompleto")]
    public string Nombre { get; set; } = string.Empty;

    public string Especialidad { get; set; } = string.Empty;

    /// <summary>El CORE incluye médicos inactivos; la web solo ofrece los activos.</summary>
    public bool Activo { get; set; } = true;
}
