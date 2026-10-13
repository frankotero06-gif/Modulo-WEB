using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PortalCitas.Services;

/// <summary>
/// La sesión del paciente ya no es válida en el Gateway (token vencido o
/// rechazado: 401/403). PacienteControllerBase la atrapa, cierra la sesión
/// y manda al login con un mensaje.
/// </summary>
public class SesionVencidaException : Exception
{
    public SesionVencidaException()
        : base("Tu sesión venció. Inicia sesión de nuevo para continuar.") { }
}

/// <summary>
/// El Gateway no respondió o respondió con error. PacienteControllerBase la
/// atrapa y muestra una pantalla con el mensaje y la opción de reintentar.
/// </summary>
public class GatewayNoDisponibleException : Exception
{
    public GatewayNoDisponibleException(string mensaje, Exception? causa = null)
        : base(mensaje, causa) { }
}

/// <summary>
/// Lectura (GET) de datos del Gateway con manejo de errores en un solo
/// lugar, para médicos, horarios, citas, facturas, resultados y constancias.
/// Antes se usaba GetFromJsonAsync sin try/catch: si el Gateway caía o el
/// token vencía, el paciente terminaba en el login sin ningún mensaje.
/// </summary>
internal static class LecturaGateway
{
    /// <param name="nullSiNoExiste">Si es true, un 404 devuelve null en vez de error (ej. una factura que no existe).</param>
    public static async Task<T?> ObtenerAsync<T>(HttpClient http, string ruta, bool nullSiNoExiste = false)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await http.GetAsync(ruta);
        }
        catch (HttpRequestException ex)
        {
            throw new GatewayNoDisponibleException(
                "No pudimos conectar con el sistema de CONAVIHSIDA. Intenta de nuevo en unos minutos.", ex);
        }
        catch (TaskCanceledException ex)
        {
            throw new GatewayNoDisponibleException(
                "El sistema tardó demasiado en responder. Intenta de nuevo en unos minutos.", ex);
        }

        using (respuesta)
        {
            if (respuesta.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                throw new SesionVencidaException();
            }

            if (respuesta.StatusCode == HttpStatusCode.NotFound && nullSiNoExiste)
            {
                return default;
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                var mensaje = await ApiErrores.LeerMensajeAsync(
                    respuesta, "El sistema no pudo responder en este momento. Intenta de nuevo en unos minutos.");
                throw new GatewayNoDisponibleException(mensaje);
            }

            try
            {
                return await respuesta.Content.ReadFromJsonAsync<T>();
            }
            catch (JsonException ex)
            {
                throw new GatewayNoDisponibleException(
                    "El sistema respondió con datos que el portal no reconoce. Avísale al equipo de soporte.", ex);
            }
        }
    }
}
