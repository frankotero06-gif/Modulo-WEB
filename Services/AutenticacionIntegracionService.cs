using System.Net;
using System.Net.Http.Json;
using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// TODO cuando Ricardo confirme el contrato real:
///   1. Ajustar las rutas "auth/login" y "auth/registro".
///   2. Ajustar los nombres de "RespuestaAuthApi".
///   3. Poner la URL real en appsettings.json → "IntegracionApiBaseUrl".
/// </summary>
public class AutenticacionIntegracionService : IAutenticacionService
{
    private readonly HttpClient _httpClient;

    public AutenticacionIntegracionService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<ResultadoLogin> IniciarSesionAsync(string correo, string clave)
    {
        try
        {
            var respuesta = await _httpClient.PostAsJsonAsync("auth/login", new { correo, clave });
            if (!respuesta.IsSuccessStatusCode)
            {
                return ResultadoLogin.Fallo("Correo o contraseña incorrectos.");
            }

            var datos = await respuesta.Content.ReadFromJsonAsync<RespuestaAuthApi>();
            if (datos is null)
            {
                return ResultadoLogin.Fallo("Respuesta inesperada del servidor.");
            }

            return ResultadoLogin.Ok(datos.PacienteId, datos.NombrePaciente);
        }
        catch (HttpRequestException)
        {
            return ResultadoLogin.Fallo("No se pudo conectar con el sistema. Intenta de nuevo en unos minutos.");
        }
    }

    /// <summary>
    /// Contrato asumido (pendiente de confirmar con Ricardo):
    ///   POST auth/registro
    ///   { nombres, apellidos, cedula, fechaNacimiento, telefono, correo, clave }
    ///   201 → { pacienteId, nombrePaciente }
    ///   409 → { error }  (correo o cédula ya registrados)
    /// La clave viaja por HTTPS al Gateway y es el Gateway/CORE quien la
    /// guarda con hash salado; la web nunca la almacena.
    /// </summary>
    public async Task<ResultadoRegistro> RegistrarPacienteAsync(RegistroViewModel datos)
    {
        var cuerpo = new
        {
            nombres = datos.Nombres.Trim(),
            apellidos = datos.Apellidos.Trim(),
            cedula = datos.CedulaNormalizada,
            fechaNacimiento = datos.FechaNacimiento?.ToString("yyyy-MM-dd"),
            telefono = datos.TelefonoNormalizado,
            correo = datos.Correo.Trim(),
            clave = datos.Clave
        };

        try
        {
            var respuesta = await _httpClient.PostAsJsonAsync("auth/registro", cuerpo);

            if (respuesta.StatusCode == HttpStatusCode.Conflict)
            {
                var mensaje = await ApiErrores.LeerMensajeAsync(respuesta, "Ya existe una cuenta con ese correo o cédula.");
                return ResultadoRegistro.Fallo(mensaje);
            }

            if (!respuesta.IsSuccessStatusCode)
            {
                var mensaje = await ApiErrores.LeerMensajeAsync(respuesta, "No se pudo crear la cuenta. Revisa los datos e intenta de nuevo.");
                return ResultadoRegistro.Fallo(mensaje);
            }

            var datosApi = await respuesta.Content.ReadFromJsonAsync<RespuestaAuthApi>();
            if (datosApi is null || string.IsNullOrEmpty(datosApi.PacienteId))
            {
                return ResultadoRegistro.Fallo("Respuesta inesperada del servidor.");
            }

            return ResultadoRegistro.Ok(datosApi.PacienteId, datosApi.NombrePaciente);
        }
        catch (HttpRequestException)
        {
            return ResultadoRegistro.Fallo("No se pudo conectar con el sistema. Intenta de nuevo en unos minutos.");
        }
    }

    private class RespuestaAuthApi
    {
        public string PacienteId { get; set; } = string.Empty;
        public string NombrePaciente { get; set; } = string.Empty;
    }
}
