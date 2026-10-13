using Microsoft.AspNetCore.Mvc;
using PortalCitas.Services;

namespace PortalCitas.Controllers;

public class ResultadosController : PacienteControllerBase
{
    private readonly IResultadosService _resultadosService;

    public ResultadosController(IResultadosService resultadosService)
    {
        _resultadosService = resultadosService;
    }

    public async Task<IActionResult> Index()
    {
        var resultados = await _resultadosService.ObtenerResultadosPacienteAsync(PacienteId);
        return View(resultados);
    }
}
