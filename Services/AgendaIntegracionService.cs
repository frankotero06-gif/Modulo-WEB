using System.Net.Http.Json;
using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// Contrato acordado con el CORE (Emilia):
///   GET  medicos                                  -> [{ id, nombreCompleto, especialidad, activo }]
///   GET  medicos/{id}/horarios?fecha=yyyy-MM-dd   -> ["08:00", ...]  (solo 08,09,10,11,14,15,16)
///   GET  pacientes/{pacienteId}/citas
///   POST citas                                    -> 201 con la cita | 409 { error }
///   POST citas/{id}/cancelar?pacienteId={id}      -> 200 sin cuerpo | 404 si no es de ese paciente
/// Todo el JSON va en camelCase, fechas ISO 8601. Sin el token del paciente
/// el Gateway responde 401; si no coincide con el paciente de la ruta o del
/// cuerpo, 403 (el header "Authorization: Bearer" lo agrega GatewayHeadersHandler).
/// </summary>
public class AgendaIntegracionService : IAgendaService
{
    private readonly HttpClient _httpClient;

    public AgendaIntegracionService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<List<MedicoDto>> ObtenerMedicosAsync()
    {
        var resultado = await LecturaGateway.ObtenerAsync<List<MedicoDto>>(_httpClient, "medicos");
        // El CORE también devuelve médicos inactivos; el paciente no debe poder elegirlos.
        return (resultado ?? new List<MedicoDto>()).Where(m => m.Activo).ToList();
    }

    public async Task<List<CitaDto>> ObtenerCitasPacienteAsync(string pacienteId)
    {
        var resultado = await LecturaGateway.ObtenerAsync<List<CitaDto>>(_httpClient, $"pacientes/{pacienteId}/citas");
        return resultado ?? new List<CitaDto>();
    }

    public async Task<List<string>> ObtenerHorariosDisponiblesAsync(int medicoId, DateTime fecha)
    {
        var fechaTexto = fecha.ToString("yyyy-MM-dd");
        var resultado = await LecturaGateway.ObtenerAsync<List<string>>(_httpClient, $"medicos/{medicoId}/horarios?fecha={fechaTexto}");
        return resultado ?? new List<string>();
    }

    public async Task<ResultadoAgendar> AgendarCitaAsync(string pacienteId, int medicoId, DateTime fecha, string hora, string motivo)
    {
        try
        {
            var respuesta = await _httpClient.PostAsJsonAsync("citas", new
            {
                pacienteId,
                medicoId,
                fecha = fecha.ToString("yyyy-MM-dd"),
                hora,
                motivo
            });

            if (!respuesta.IsSuccessStatusCode)
            {
                // 409 = el horario ya se ocupó (mensaje viene en { "error": "..." }).
                var mensaje = await ApiErrores.LeerMensajeAsync(respuesta, "No se pudo agendar la cita.");
                return ResultadoAgendar.Fallo(mensaje);
            }

            var cita = await respuesta.Content.ReadFromJsonAsync<CitaDto>();
            return cita is not null
                ? ResultadoAgendar.Ok(cita)
                : ResultadoAgendar.Fallo("Respuesta inesperada del servidor.");
        }
        catch (HttpRequestException)
        {
            return ResultadoAgendar.Fallo("No se pudo conectar con el sistema. Intenta de nuevo en unos minutos.");
        }
    }

    public async Task<bool> CancelarCitaAsync(string pacienteId, int citaId)
    {
        try
        {
            var url = $"citas/{citaId}/cancelar?pacienteId={Uri.EscapeDataString(pacienteId)}";
            var respuesta = await _httpClient.PostAsync(url, null);
            return respuesta.IsSuccessStatusCode; // 404 = la cita no es de este paciente
        }
        catch (HttpRequestException)
        {
            return false;
        }
    }
}
