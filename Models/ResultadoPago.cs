namespace PortalCitas.Models;

public class ResultadoPago
{
    public bool Exitoso { get; set; }
    public string? MensajeError { get; set; }
    public FacturaDto? Factura { get; set; }

    public static ResultadoPago Ok(FacturaDto factura) => new() { Exitoso = true, Factura = factura };
    public static ResultadoPago Fallo(string mensaje) => new() { Exitoso = false, MensajeError = mensaje };
}
