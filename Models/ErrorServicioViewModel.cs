namespace PortalCitas.Models;

/// <summary>Datos de la pantalla que se muestra cuando el Gateway no responde.</summary>
public class ErrorServicioViewModel
{
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>Dirección para "Intentar de nuevo" (solo si la página era un GET).</summary>
    public string? UrlReintento { get; set; }
}
