using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// Contrato único para todo lo relacionado a agendar citas. El
/// Controller y las Vistas solo conocen esta interfaz — nunca la
/// implementación concreta (mock o real).
/// </summary>
public interface IAgendaService
{
    Task<List<MedicoDto>> ObtenerMedicosAsync();

    Task<List<CitaDto>> ObtenerCitasPacienteAsync(string pacienteId);

    Task<List<string>> ObtenerHorariosDisponiblesAsync(int medicoId, DateTime fecha);

    Task<ResultadoAgendar> AgendarCitaAsync(string pacienteId, int medicoId, DateTime fecha, string hora, string motivo);

    Task<bool> CancelarCitaAsync(string pacienteId, int citaId);
}
