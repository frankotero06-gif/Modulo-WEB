using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PortalCitas.Models;
using PortalCitas.Services;

namespace PortalCitas.Controllers;

/// <summary>
/// Cualquier Controller que herede de esta clase exige que exista
/// una sesión de paciente iniciada (ver CuentaController.Login).
///
/// También maneja en un solo lugar los errores al leer del Gateway:
///  - Sesión vencida (401/403): cierra la sesión y manda al login con un aviso.
///  - Gateway caído o con error: muestra una pantalla con el mensaje y la
///    opción de reintentar, en vez de dejar al paciente sin explicación.
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

    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Exception != null && !context.ExceptionHandled)
        {
            var esPeticionJson = Request.Headers.Accept.ToString().Contains("application/json");

            switch (context.Exception)
            {
                case SesionVencidaException vencida:
                    HttpContext.Session.Clear();
                    TempData["Mensaje"] = vencida.Message;
                    context.Result = esPeticionJson
                        ? (IActionResult)new JsonResult(new { error = vencida.Message }) { StatusCode = StatusCodes.Status401Unauthorized }
                        : RedirectToAction("Login", "Cuenta");
                    context.ExceptionHandled = true;
                    break;

                case GatewayNoDisponibleException caido:
                    if (esPeticionJson)
                    {
                        context.Result = new JsonResult(new { error = caido.Message }) { StatusCode = StatusCodes.Status503ServiceUnavailable };
                    }
                    else
                    {
                        var vista = View("ErrorServicio", new ErrorServicioViewModel
                        {
                            Mensaje = caido.Message,
                            UrlReintento = Request.Method == "GET" ? Request.Path + Request.QueryString : null
                        });
                        vista.StatusCode = StatusCodes.Status503ServiceUnavailable;
                        context.Result = vista;
                    }
                    context.ExceptionHandled = true;
                    break;
            }
        }

        base.OnActionExecuted(context);
    }
}
