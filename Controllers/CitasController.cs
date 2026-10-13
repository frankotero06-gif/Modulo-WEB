using Microsoft.AspNetCore.Mvc;
using PortalCitas.Models;
using PortalCitas.Services;

namespace PortalCitas.Controllers;

public class CitasController : PacienteControllerBase
{
    private readonly IAgendaService _agendaService;
    private readonly IConstanciaService _constancias;

    public CitasController(IAgendaService agendaService, IConstanciaService constancias)
    {
        _agendaService = agendaService;
        _constancias = constancias;
    }

    // GET /Citas
    public async Task<IActionResult> Index()
    {
        var citas = await _agendaService.ObtenerCitasPacienteAsync(PacienteId);
        ViewBag.Mensaje = TempData["Mensaje"];
        return View(citas);
    }

    // GET /Citas/Agendar
    public async Task<IActionResult> Agendar()
    {
        var modelo = new AgendarCitaViewModel
        {
            Medicos = await _agendaService.ObtenerMedicosAsync(),
        };
        return View(modelo);
    }

    // POST /Citas/Agendar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Agendar(NuevaCitaInput form)
    {
        if (!ModelState.IsValid)
        {
            var modeloConError = new AgendarCitaViewModel
            {
                Form = form,
                Medicos = await _agendaService.ObtenerMedicosAsync(),
            };
            return View(modeloConError);
        }

        var resultado = await _agendaService.AgendarCitaAsync(
            PacienteId, form.MedicoId, form.Fecha, form.Hora, form.Motivo);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.MensajeError ?? "No se pudo agendar la cita.");
            var modeloConError = new AgendarCitaViewModel
            {
                Form = form,
                Medicos = await _agendaService.ObtenerMedicosAsync(),
            };
            return View(modeloConError);
        }

        TempData["Mensaje"] = $"Cita agendada para el {resultado.Cita!.Fecha:dd/MM/yyyy} a las {resultado.Cita.Hora}.";

        // Constancia de la solicitud: el paciente la puede guardar o descargar.
        var constancia = await _constancias.RegistrarAsync(
            FabricaConstancias.CitaAgendada(PacienteId, NombrePaciente, resultado.Cita));
        if (constancia != null)
        {
            return RedirectToAction("Ver", "Constancias", new { id = constancia.Id });
        }
        return RedirectToAction(nameof(Index));
    }

    // GET /Citas/HorariosDisponibles?medicoId=1&fecha=2026-09-25
    // Endpoint AJAX que consume wwwroot/js/agendar.js para pintar los
    // botones de horario disponibles sin recargar la página.
    [HttpGet]
    public async Task<IActionResult> HorariosDisponibles(int medicoId, DateTime fecha)
    {
        var horarios = await _agendaService.ObtenerHorariosDisponiblesAsync(medicoId, fecha);
        return Json(horarios);
    }

    // POST /Citas/Cancelar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancelar(int id)
    {
        // Se busca la cita antes de cancelarla para poder describirla en la constancia.
        var cita = (await _agendaService.ObtenerCitasPacienteAsync(PacienteId)).FirstOrDefault(c => c.Id == id);

        var ok = await _agendaService.CancelarCitaAsync(PacienteId, id);
        if (!ok)
        {
            TempData["Mensaje"] = "No se pudo cancelar la cita.";
            return RedirectToAction(nameof(Index));
        }

        TempData["Mensaje"] = "Cita cancelada. El horario quedó libre.";
        if (cita != null)
        {
            var constancia = await _constancias.RegistrarAsync(
                FabricaConstancias.CitaCancelada(PacienteId, NombrePaciente, cita));
            if (constancia != null)
            {
                return RedirectToAction("Ver", "Constancias", new { id = constancia.Id });
            }
        }
        return RedirectToAction(nameof(Index));
    }
}
