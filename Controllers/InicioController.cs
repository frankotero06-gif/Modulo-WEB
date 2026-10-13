using Microsoft.AspNetCore.Mvc;
using PortalCitas.Models;
using PortalCitas.Services;

namespace PortalCitas.Controllers;

/// <summary>
/// Pantalla de bienvenida: junta en un solo lugar la próxima cita, lo que
/// hay por pagar y cuántos resultados están listos. Solo lee de los mismos
/// servicios que ya usan Citas, Facturas y Resultados.
/// </summary>
public class InicioController : PacienteControllerBase
{
    private readonly IAgendaService _agendaService;
    private readonly IPagoService _pagoService;
    private readonly IResultadosService _resultadosService;

    public InicioController(IAgendaService agendaService, IPagoService pagoService, IResultadosService resultadosService)
    {
        _agendaService = agendaService;
        _pagoService = pagoService;
        _resultadosService = resultadosService;
    }

    // GET /Inicio
    public async Task<IActionResult> Index()
    {
        var citas = await _agendaService.ObtenerCitasPacienteAsync(PacienteId);
        var facturas = await _pagoService.ObtenerFacturasPacienteAsync(PacienteId);
        var resultados = await _resultadosService.ObtenerResultadosPacienteAsync(PacienteId);

        var citasActivas = citas
            .Where(c => c.Estado != "Cancelada" && c.Fecha.Date >= DateTime.Today)
            .OrderBy(c => c.Fecha)
            .ThenBy(c => c.Hora)
            .ToList();

        var modelo = new InicioViewModel
        {
            NombrePaciente = NombrePaciente,
            ProximaCita = citasActivas.FirstOrDefault(),
            CitasProximas = citasActivas.Count,
            FacturasPendientes = facturas.Where(f => f.Estado != "Pagada").ToList(),
            ResultadosListos = resultados.Count(r => r.Estado == "Listo"),
            ResultadosPendientes = resultados.Count(r => r.Estado != "Listo"),
        };

        ViewBag.Mensaje = TempData["Mensaje"];
        return View(modelo);
    }
}
