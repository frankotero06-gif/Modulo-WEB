using Microsoft.AspNetCore.Mvc;
using PortalCitas.Models;
using PortalCitas.Services;

namespace PortalCitas.Controllers;

public class CuentaController : Controller
{
    private readonly IAutenticacionService _autenticacionService;
    private readonly IConstanciaService _constancias;

    public CuentaController(IAutenticacionService autenticacionService, IConstanciaService constancias)
    {
        _autenticacionService = autenticacionService;
        _constancias = constancias;
    }

    private bool HaySesion => !string.IsNullOrEmpty(HttpContext.Session.GetString("PacienteId"));

    [HttpGet]
    public IActionResult Login()
    {
        if (HaySesion)
        {
            return RedirectToAction("Index", "Inicio");
        }

        ViewBag.Mensaje = TempData["Mensaje"];
        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _autenticacionService.IniciarSesionAsync(modelo.Correo, modelo.Clave);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.MensajeError ?? "Correo o contraseña incorrectos.");
            return View(modelo);
        }

        IniciarSesion(resultado.PacienteId, resultado.NombrePaciente);
        return RedirectToAction("Index", "Inicio");
    }

    // GET /Cuenta/Registro
    [HttpGet]
    public IActionResult Registro()
    {
        if (HaySesion)
        {
            return RedirectToAction("Index", "Inicio");
        }

        return View(new RegistroViewModel());
    }

    // POST /Cuenta/Registro
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registro(RegistroViewModel modelo)
    {
        if (modelo.FechaNacimiento is DateTime nacimiento)
        {
            if (nacimiento.Date >= DateTime.Today)
            {
                ModelState.AddModelError(nameof(modelo.FechaNacimiento), "La fecha de nacimiento debe ser anterior a hoy.");
            }
            else if (nacimiento.Date < DateTime.Today.AddYears(-120))
            {
                ModelState.AddModelError(nameof(modelo.FechaNacimiento), "Revisa el año de nacimiento.");
            }
        }

        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var resultado = await _autenticacionService.RegistrarPacienteAsync(modelo);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(resultado.CampoError ?? string.Empty, resultado.MensajeError ?? "No se pudo crear la cuenta.");
            return View(modelo);
        }

        IniciarSesion(resultado.PacienteId, resultado.NombrePaciente);
        TempData["Mensaje"] = $"Tu cuenta está lista, {modelo.Nombres.Trim()}. Guarda tu constancia de registro.";

        // Constancia del registro, para que el paciente tenga prueba de lo que hizo.
        var constancia = await _constancias.RegistrarAsync(
            FabricaConstancias.RegistroCuenta(resultado.PacienteId ?? string.Empty, modelo));
        if (constancia != null)
        {
            return RedirectToAction("Ver", "Constancias", new { id = constancia.Id });
        }

        TempData["Mensaje"] = $"Tu cuenta está lista, {modelo.Nombres.Trim()}. Ya puedes agendar tu primera cita.";
        return RedirectToAction("Index", "Inicio");
    }

    public IActionResult Salir()
    {
        HttpContext.Session.Clear();
        TempData["Mensaje"] = "Cerraste sesión.";
        return RedirectToAction("Login");
    }

    private void IniciarSesion(string? pacienteId, string? nombrePaciente)
    {
        HttpContext.Session.SetString("PacienteId", pacienteId ?? string.Empty);
        HttpContext.Session.SetString("NombrePaciente", nombrePaciente ?? string.Empty);
    }
}
