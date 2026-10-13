using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// Login y registro del paciente contra el Gateway de Integración (Ricardo):
///   POST auth/paciente/login  { correo, clave }
///        → 200 { token, pacienteId, nombrePaciente }
///   POST auth/registro        { nombres, apellidos, cedula, fechaNacimiento, telefono, correo, clave }
///        → 201 { token, pacienteId, nombrePaciente }  ó  409 { error }
/// El token se guarda en la sesión del servidor y GatewayHeadersHandler lo
/// manda como "Authorization: Bearer ..." en las demás llamadas.
/// </summary>
public class AutenticacionIntegracionService : IAutenticacionService
{
    private readonly HttpClient _httpClient;

    public AutenticacionIntegracionService(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<ResultadoLogin> IniciarSesionAsync(string correo, string clave)
    {
        try
        {
            var respuesta = await _httpClient.PostAsJsonAsync("auth/paciente/login", new { correo, clave });
            if (!respuesta.IsSuccessStatusCode)
            {
                return ResultadoLogin.Fallo("Correo o contraseña incorrectos.");
            }

            var datos = await respuesta.Content.ReadFromJsonAsync<RespuestaAuthApi>();
            if (datos is null)
            {
                return ResultadoLogin.Fallo("Respuesta inesperada del servidor.");
            }

            return ResultadoLogin.Ok(datos.PacienteId, datos.NombrePaciente, datos.Token);
        }
        catch (HttpRequestException)
        {
            return ResultadoLogin.Fallo("No se pudo conectar con el sistema. Intenta de nuevo en unos minutos.");
        }
    }

    /// <summary>
    /// Contrato confirmado por Ricardo (Gateway):
    ///   POST auth/registro
    ///   { nombres, apellidos, cedula, fechaNacimiento, telefono, correo, clave }
    ///   201 → { token, pacienteId, nombrePaciente }
    ///   409 → { error }  (correo o cédula ya registrados)
    /// Ojo: en el CORE la ruta es "auth/paciente/registro"; la web llama al
    /// Gateway, y el Gateway la expone como "auth/registro".
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

            return ResultadoRegistro.Ok(datosApi.PacienteId, datosApi.NombrePaciente, datosApi.Token);
        }
        catch (HttpRequestException)
        {
            return ResultadoRegistro.Fallo("No se pudo conectar con el sistema. Intenta de nuevo en unos minutos.");
        }
    }

    private class RespuestaAuthApi
    {
        public string Token { get; set; } = string.Empty;

        [JsonConverter(typeof(TextoFlexibleConverter))]
        public string PacienteId { get; set; } = string.Empty;

        public string NombrePaciente { get; set; } = string.Empty;
    }
}
