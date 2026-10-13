namespace PortalCitas.Models;

/// <summary>
/// A propósito, este DTO NO incluye el valor clínico (carga viral,
/// conteo CD4, etc.). Según RN-01 del SRS, esos datos únicamente
/// pueden ser leídos por personal médico acreditado, a través de la
/// aplicación de empleados del Core — no por el paciente vía Web.
/// Aquí solo exponemos si la prueba fue solicitada y si ya está
/// lista, para que el paciente sepa cuándo consultar con su médico.
/// </summary>
public class ResultadoLabDto
{
    public int Id { get; set; }
    public string PacienteId { get; set; } = string.Empty;
    public string Prueba { get; set; } = string.Empty;
    /// <summary>El CORE la envía como "fechaSolicitud".</summary>
    public DateTime FechaSolicitud { get; set; }

    /// <summary>"Listo" o "Pendiente".</summary>
    public string Estado { get; set; } = "Pendiente";

    /// <summary>Nota no clínica, ej. "Disponible en tu próxima consulta".</summary>
    public string? Nota { get; set; }
}
