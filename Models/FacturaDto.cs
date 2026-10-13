namespace PortalCitas.Models;

public class FacturaDto
{
    public int Id { get; set; }
    public string PacienteId { get; set; } = string.Empty;
    public string Concepto { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }

    /// <summary>true si RN-02 aplica (ARV subsidiado al 100%, monto = 0).</summary>
    public bool EsSubsidiada { get; set; }

    /// <summary>"Pendiente" o "Pagada".</summary>
    public string Estado { get; set; } = "Pendiente";

    /// <summary>Se generan juntos al completar el pago (RN-05: comprobante obligatorio).</summary>
    public string? NumeroRecibo { get; set; }
    public string? NumeroFactura { get; set; }
    public DateTime? FechaPago { get; set; }
}
