using Microsoft.AspNetCore.Mvc;
using PortalCitas.Services;

namespace PortalCitas.Controllers;

/// <summary>
/// "Mis constancias": todo lo que el paciente ha hecho en el portal, con la
/// opción de ver, imprimir o descargar en PDF cada constancia.
/// </summary>
public class ConstanciasController : PacienteControllerBase
{
    private readonly IConstanciaService _constancias;

    public ConstanciasController(IConstanciaService constancias)
    {
        _constancias = constancias;
    }

    // GET /Constancias
    public async Task<IActionResult> Index()
    {
        var lista = await _constancias.ObtenerPorPacienteAsync(PacienteId);
        ViewBag.Mensaje = TempData["Mensaje"];
        return View(lista);
    }

    // GET /Constancias/Ver/5
    public async Task<IActionResult> Ver(int id)
    {
        var constancia = await _constancias.ObtenerAsync(PacienteId, id);
        if (constancia is null)
        {
            return NotFound();
        }

        ViewBag.Mensaje = TempData["Mensaje"];
        return View(constancia);
    }

    // GET /Constancias/Pdf/5  → descarga el archivo
    public async Task<IActionResult> Pdf(int id)
    {
        var constancia = await _constancias.ObtenerAsync(PacienteId, id);
        if (constancia is null)
        {
            return NotFound();
        }

        var bytes = FabricaConstancias.Pdf(constancia);
        return File(bytes, "application/pdf", $"Constancia-{constancia.Numero}.pdf");
    }
}
