using PortalCitas.Models;

namespace PortalCitas.Services;

public class ResultadosMockService : IResultadosService
{
    private readonly List<ResultadoLabDto> _resultados = new()
    {
        new ResultadoLabDto
        {
            Id = 1,
            PacienteId = "1",
            Prueba = "Carga Viral",
            FechaSolicitud = DateTime.Today.AddDays(-10),
            Estado = "Listo",
            Nota = "Disponible — revísalo con tu médico en tu próxima consulta."
        },
        new ResultadoLabDto
        {
            Id = 2,
            PacienteId = "1",
            Prueba = "Conteo CD4",
            FechaSolicitud = DateTime.Today.AddDays(-10),
            Estado = "Listo",
            Nota = "Disponible — revísalo con tu médico en tu próxima consulta."
        },
        new ResultadoLabDto
        {
            Id = 3,
            PacienteId = "1",
            Prueba = "Hemograma Completo",
            FechaSolicitud = DateTime.Today.AddDays(-2),
            Estado = "Pendiente",
            Nota = null
        }
    };

    public Task<List<ResultadoLabDto>> ObtenerResultadosPacienteAsync(string pacienteId)
    {
        var resultados = _resultados
            .Where(r => r.PacienteId == pacienteId)
            .OrderByDescending(r => r.FechaSolicitud)
            .ToList();
        return Task.FromResult(resultados);
    }
}
