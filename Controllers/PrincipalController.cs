using Microsoft.AspNetCore.Mvc;

namespace PortalCitas.Controllers;

/// <summary>
/// Página principal pública (sin sesión): quiénes somos, misión, visión y
/// qué se puede hacer en el portal. Desde aquí se va a Iniciar sesión o a
/// Crear cuenta. Es la primera pantalla al abrir el sitio ("/").
/// </summary>
public class PrincipalController : Controller
{
    private readonly IWebHostEnvironment _entorno;

    public PrincipalController(IWebHostEnvironment entorno)
    {
        _entorno = entorno;
    }

    public IActionResult Index()
    {
        var nombre = HttpContext.Session.GetString("NombrePaciente");
        ViewBag.HaySesion = !string.IsNullOrEmpty(HttpContext.Session.GetString("PacienteId"));
        ViewBag.NombrePaciente = string.IsNullOrEmpty(nombre) ? null : nombre;

        // Si el equipo coloca una foto del edificio en wwwroot/img/edificio.jpg,
        // se muestra en la portada; si no, se usa el lazo como imagen principal.
        var rutaFoto = Path.Combine(_entorno.WebRootPath ?? string.Empty, "img", "edificio.jpg");
        ViewBag.FotoEdificio = System.IO.File.Exists(rutaFoto) ? "/img/edificio.jpg" : null;

        return View();
    }
}
