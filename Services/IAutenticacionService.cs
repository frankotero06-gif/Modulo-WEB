using PortalCitas.Models;

namespace PortalCitas.Services;

public interface IAutenticacionService
{
    Task<ResultadoLogin> IniciarSesionAsync(string correo, string clave);

    /// <summary>
    /// Crea la cuenta del paciente en el portal. Si sale bien, devuelve el
    /// pacienteId para iniciar sesión de inmediato.
    /// </summary>
    Task<ResultadoRegistro> RegistrarPacienteAsync(RegistroViewModel datos);
}
