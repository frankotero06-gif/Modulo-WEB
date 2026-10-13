using System.Net.Http.Json;
using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// Contrato acordado con el CORE (Emilia):
///   GET  pacientes/{pacienteId}/facturas
///   GET  pacientes/{pacienteId}/facturas/{facturaId}
///   POST pacientes/{pacienteId}/facturas/{facturaId}/pagar
///        cuerpo { "formaPago": "Tarjeta"|"Efectivo"|"Transferencia", "referenciaPasarela": "..." }
///        con Tarjeta la referencia es obligatoria (máx. 100 caracteres);
///        si la factura es subsidiada, la forma de pago se ignora.
///        -> 200 con la factura ya pagada (numeroRecibo, numeroFactura, fechaPago)
///        -> 409 { "error": "..." }
/// El CORE nunca recibe el número de tarjeta: solo la referencia de la pasarela.
/// </summary>
public class PagoIntegracionService : IPagoService
{
    private const int MaxReferencia = 100;
    private readonly HttpClient _httpClient;

    public PagoIntegracionService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<List<FacturaDto>> ObtenerFacturasPacienteAsync(string pacienteId)
    {
        var resultado = await _httpClient.GetFromJsonAsync<List<FacturaDto>>($"pacientes/{pacienteId}/facturas");
        return resultado ?? new List<FacturaDto>();
    }

    public async Task<FacturaDto?> ObtenerFacturaAsync(string pacienteId, int facturaId)
    {
        return await _httpClient.GetFromJsonAsync<FacturaDto>($"pacientes/{pacienteId}/facturas/{facturaId}");
    }

    public async Task<ResultadoPago> PagarFacturaAsync(string pacienteId, int facturaId, string referenciaPasarela)
    {
        if (string.IsNullOrWhiteSpace(referenciaPasarela) || referenciaPasarela.Length > MaxReferencia)
        {
            return ResultadoPago.Fallo("La referencia de la pasarela no es válida.");
        }

        try
        {
            var respuesta = await _httpClient.PostAsJsonAsync($"pacientes/{pacienteId}/facturas/{facturaId}/pagar", new
            {
                formaPago = "Tarjeta",
                referenciaPasarela
            });

            if (!respuesta.IsSuccessStatusCode)
            {
                // 409 = p. ej. la factura ya estaba pagada (mensaje en { "error": "..." }).
                var mensaje = await ApiErrores.LeerMensajeAsync(respuesta, "El pago no pudo registrarse. Intenta de nuevo.");
                return ResultadoPago.Fallo(mensaje);
            }

            var factura = await respuesta.Content.ReadFromJsonAsync<FacturaDto>();
            return factura is not null
                ? ResultadoPago.Ok(factura)
                : ResultadoPago.Fallo("Respuesta inesperada del servidor.");
        }
        catch (HttpRequestException)
        {
            return ResultadoPago.Fallo("No se pudo conectar con el sistema. Intenta de nuevo en unos minutos.");
        }
    }
}
