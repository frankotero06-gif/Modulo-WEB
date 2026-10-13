namespace PortalCitas.Models;

public class PagarFacturaViewModel
{
    public FacturaDto Factura { get; set; } = new();
    public DatosPagoInput DatosPago { get; set; } = new();
}
