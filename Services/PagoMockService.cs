using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// Datos de prueba: una entrega de ARV ya subsidiada al 100% (RN-02,
/// queda en RD$ 0.00 y "Pagada" automáticamente, igual que se ve del
/// lado de Caja) y una consulta especializada arancelada pendiente de
/// pago — el mismo escenario "Consulta Técnica Especializada RD$250.00"
/// que aparece en las transacciones recientes de la pantalla de Caja.
/// </summary>
public class PagoMockService : IPagoService
{
    private readonly object _bloqueo = new();
    private int _siguienteRecibo = 9403;

    private readonly List<FacturaDto> _facturas = new()
    {
        new FacturaDto
        {
            Id = 1,
            PacienteId = "1",
            Concepto = "Dispensación Medicamentos ARV",
            Fecha = DateTime.Today.AddDays(-3),
            Monto = 0m,
            EsSubsidiada = true,
            Estado = "Pagada",
            NumeroRecibo = "REC-9402",
            NumeroFactura = "FAC-9402",
            FechaPago = DateTime.Today.AddDays(-3)
        },
        new FacturaDto
        {
            Id = 2,
            PacienteId = "1",
            Concepto = "Consulta Técnica Especializada",
            Fecha = DateTime.Today.AddDays(-1),
            Monto = 250m,
            EsSubsidiada = false,
            Estado = "Pendiente"
        }
    };

    public Task<List<FacturaDto>> ObtenerFacturasPacienteAsync(string pacienteId)
    {
        lock (_bloqueo)
        {
            var facturas = _facturas
                .Where(f => f.PacienteId == pacienteId)
                .OrderByDescending(f => f.Fecha)
                .ToList();
            return Task.FromResult(facturas);
        }
    }

    public Task<FacturaDto?> ObtenerFacturaAsync(string pacienteId, int facturaId)
    {
        lock (_bloqueo)
        {
            var factura = _facturas.FirstOrDefault(f => f.Id == facturaId && f.PacienteId == pacienteId);
            return Task.FromResult(factura);
        }
    }

    public Task<ResultadoPago> PagarFacturaAsync(string pacienteId, int facturaId, string referenciaPasarela)
    {
        lock (_bloqueo)
        {
            var factura = _facturas.FirstOrDefault(f => f.Id == facturaId && f.PacienteId == pacienteId);

            if (factura is null)
            {
                return Task.FromResult(ResultadoPago.Fallo("La factura no existe."));
            }

            if (factura.Estado == "Pagada")
            {
                return Task.FromResult(ResultadoPago.Fallo("Esta factura ya fue pagada."));
            }

            // RN-05: ninguna transacción se considera completada sin
            // recibo Y factura generados juntos.
            var numero = _siguienteRecibo++;
            factura.NumeroRecibo = $"REC-{numero}";
            factura.NumeroFactura = $"FAC-{numero}";
            factura.Estado = "Pagada";
            factura.FechaPago = DateTime.Now;

            return Task.FromResult(ResultadoPago.Ok(factura));
        }
    }
}
