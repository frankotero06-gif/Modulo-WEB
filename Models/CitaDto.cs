using System.Text.Json.Serialization;

namespace PortalCitas.Models;

public class CitaDto
{
    public int Id { get; set; }
    [JsonConverter(typeof(TextoFlexibleConverter))]
    public string PacienteId { get; set; } = string.Empty;
    public int MedicoId { get; set; }
    public string MedicoNombre { get; set; } = string.Empty;
    public string Especialidad { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Hora { get; set; } = string.Empty;
    public string Motivo { get; set; } = string.Empty;

    /// <summary>"Confirmada", "Pendiente" o "Cancelada".</summary>
    public string Estado { get; set; } = "Pendiente";
}
