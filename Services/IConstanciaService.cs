using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// Guarda y consulta las constancias del paciente. Igual que los demás
/// servicios, el Controller solo conoce esta interfaz (mock o real).
/// </summary>
public interface IConstanciaService
{
    /// <summary>
    /// Registra una constancia nueva y la devuelve con su Id, Numero y
    /// FechaEmision. Devuelve null si no se pudo guardar: la acción del
    /// paciente (cita, pago...) ya se hizo, así que el portal sigue sin ella.
    /// </summary>
    Task<ConstanciaDto?> RegistrarAsync(ConstanciaDto nueva);

    Task<List<ConstanciaDto>> ObtenerPorPacienteAsync(string pacienteId);

    Task<ConstanciaDto?> ObtenerAsync(string pacienteId, int constanciaId);
}
