using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// Cubre RF-WEB-02 del SRS: flujo normal de consulta, pago y
/// visualización del comprobante. Igual que IAgendaService, el
/// Controller solo conoce esta interfaz.
/// </summary>
public interface IPagoService
{
    Task<List<FacturaDto>> ObtenerFacturasPacienteAsync(string pacienteId);

    Task<FacturaDto?> ObtenerFacturaAsync(string pacienteId, int facturaId);

    /// <summary>
    /// El CORE nunca recibe el número de tarjeta: solo la referencia que
    /// devuelve la pasarela al autorizar el cobro (ver IPasarelaService).
    /// </summary>
    Task<ResultadoPago> PagarFacturaAsync(string pacienteId, int facturaId, string referenciaPasarela);
}
