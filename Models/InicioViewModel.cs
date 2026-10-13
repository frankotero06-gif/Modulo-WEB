namespace PortalCitas.Models;

/// <summary>Resumen que ve el paciente al entrar al portal.</summary>
public class InicioViewModel
{
    public string NombrePaciente { get; set; } = string.Empty;

    /// <summary>La cita confirmada o pendiente más cercana (hoy o después).</summary>
    public CitaDto? ProximaCita { get; set; }

    /// <summary>Cuántas citas activas tiene por delante (incluye la próxima).</summary>
    public int CitasProximas { get; set; }

    public List<FacturaDto> FacturasPendientes { get; set; } = new();
    public decimal TotalPendiente => FacturasPendientes.Sum(f => f.Monto);

    public int ResultadosListos { get; set; }
    public int ResultadosPendientes { get; set; }
}
