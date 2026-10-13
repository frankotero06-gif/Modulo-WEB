using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace PortalCitas.Services;

/// <summary>
/// El CORE responde los conflictos de negocio (horario ocupado, factura
/// ya pagada, etc.) con 409 y un cuerpo { "error": "..." }. Esto lee ese
/// mensaje para mostrárselo al paciente; si no hay cuerpo usable, usa
/// un texto por defecto.
/// </summary>
internal static class ApiErrores
{
    public static async Task<string> LeerMensajeAsync(HttpResponseMessage respuesta, string porDefecto)
    {
        if (respuesta.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return "Tu sesión no está autorizada para esta operación. Inicia sesión de nuevo.";
        }

        return await LeerErrorDelCuerpoAsync(respuesta) ?? porDefecto;
    }

    /// <summary>
    /// Solo el texto de { "error": "..." } si la respuesta lo trae; si no, null.
    /// Sirve cuando el código HTTP significa algo distinto según la ruta
    /// (en el login, 401 es "clave incorrecta", no "sesión vencida").
    /// </summary>
    public static async Task<string?> LeerErrorDelCuerpoAsync(HttpResponseMessage respuesta)
    {
        try
        {
            var cuerpo = await respuesta.Content.ReadFromJsonAsync<ErrorApi>();
            if (!string.IsNullOrWhiteSpace(cuerpo?.Error))
            {
                return cuerpo.Error;
            }
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            // Sin cuerpo JSON: quien llama decide el mensaje.
        }

        return null;
    }

    private class ErrorApi
    {
        public string? Error { get; set; }
    }
}
