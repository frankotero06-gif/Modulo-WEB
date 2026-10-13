using System.Net;
using System.Net.Http.Json;
using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// Contrato asumido (pendiente de confirmar con Ricardo / Emilia). Lo
/// lógico es que el CORE guarde las constancias, porque la web no tiene
/// base de datos propia (RNF-03):
///   POST pacientes/{pacienteId}/constancias        → 201 con la constancia (id, numero, fechaEmision)
///   GET  pacientes/{pacienteId}/constancias        → lista
///   GET  pacientes/{pacienteId}/constancias/{id}   → una, o 404
/// </summary>
public class ConstanciaIntegracionService : IConstanciaService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ConstanciaIntegracionService> _logger;

    public ConstanciaIntegracionService(HttpClient httpClient, ILogger<ConstanciaIntegracionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ConstanciaDto?> RegistrarAsync(ConstanciaDto nueva)
    {
        try
        {
            var ruta = $"pacientes/{Uri.EscapeDataString(nueva.PacienteId)}/constancias";
            var respuesta = await _httpClient.PostAsJsonAsync(ruta, nueva);
            if (!respuesta.IsSuccessStatusCode)
            {
                _logger.LogWarning("No se pudo registrar la constancia ({Codigo}).", (int)respuesta.StatusCode);
                return null;
            }
            return await respuesta.Content.ReadFromJsonAsync<ConstanciaDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "No se pudo conectar para registrar la constancia.");
            return null;
        }
    }

    public async Task<List<ConstanciaDto>> ObtenerPorPacienteAsync(string pacienteId)
    {
        try
        {
            var lista = await _httpClient.GetFromJsonAsync<List<ConstanciaDto>>(
                $"pacientes/{Uri.EscapeDataString(pacienteId)}/constancias");
            return lista ?? new List<ConstanciaDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "No se pudieron cargar las constancias.");
            return new List<ConstanciaDto>();
        }
    }

    public async Task<ConstanciaDto?> ObtenerAsync(string pacienteId, int constanciaId)
    {
        try
        {
            var respuesta = await _httpClient.GetAsync(
                $"pacientes/{Uri.EscapeDataString(pacienteId)}/constancias/{constanciaId}");
            if (respuesta.StatusCode == HttpStatusCode.NotFound || !respuesta.IsSuccessStatusCode)
            {
                return null;
            }
            return await respuesta.Content.ReadFromJsonAsync<ConstanciaDto>();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "No se pudo cargar la constancia {Id}.", constanciaId);
            return null;
        }
    }
}
