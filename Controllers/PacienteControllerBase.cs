using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PortalCitas.Controllers;

/// <summary>
/// Cualquier Controller que herede de esta clase exige que exista
/// una sesión de paciente iniciada (ver CuentaController.Login).
/// Es una versión simple mientras no se implemente el esquema de
/// autenticación completo de ASP.NET Core con el JWT de Integración.
/// </summary>
public abstract class PacienteControllerBase : Controller
{
    protected string PacienteId => HttpContext.Session.GetString("PacienteId") ?? string.Empty;
    protected string NombrePaciente => HttpContext.Session.GetString("NombrePaciente") ?? "Paciente";

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (string.IsNullOrEmpty(PacienteId))
        {
            context.Result = RedirectToAction("Login", "Cuenta");
            return;
        }

        ViewBag.NombrePaciente = NombrePaciente;
        base.OnActionExecuting(context);
    }
}
