using System.Net.Http.Json;
using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// TODO cuando Ricardo confirme el contrato real: ajustar la ruta.
///
/// Importante para el equipo (no solo para Ricardo): el endpoint que
/// respalda esto NO debe devolver carga viral ni CD4 en el JSON, ni
/// siquiera si el frontend luego no los pinta. Según RN-01, ese dato
/// no debería salir del Core hacia un cliente que no sea la
/// aplicación de empleados con rol médico.
/// </summary>
public class ResultadosIntegracionService : IResultadosService
{
    private readonly HttpClient _httpClient;

    public ResultadosIntegracionService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<List<ResultadoLabDto>> ObtenerResultadosPacienteAsync(string pacienteId)
    {
        // GET pacientes/{id}/resultados (vista resumida, sin valores clínicos)
        var resultado = await LecturaGateway.ObtenerAsync<List<ResultadoLabDto>>(_httpClient, $"pacientes/{pacienteId}/resultados");
        return resultado ?? new List<ResultadoLabDto>();
    }
}
