using PortalCitas.Models;

namespace PortalCitas.Services;

public interface IResultadosService
{
    Task<List<ResultadoLabDto>> ObtenerResultadosPacienteAsync(string pacienteId);
}
