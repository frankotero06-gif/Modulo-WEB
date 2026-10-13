using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// Implementación de prueba con datos en memoria. Se registra como
/// Singleton en Program.cs para que las citas agendadas durante la
/// demo persistan mientras la app esté corriendo (se pierden al
/// reiniciar — es solo para no depender de Ricardo todavía).
/// </summary>
public class AgendaMockService : IAgendaService
{
    private readonly object _bloqueo = new();

    private readonly List<MedicoDto> _medicos = new()
    {
        new MedicoDto { Id = 1, Nombre = "Dra. Ramírez", Especialidad = "Infectología" },
        new MedicoDto { Id = 2, Nombre = "Dr. Ferreira", Especialidad = "Medicina General" },
        new MedicoDto { Id = 3, Nombre = "Lic. Objio", Especialidad = "Psicología" },
    };

    private readonly List<string> _horariosBase = new()
    {
        "08:00", "09:00", "10:00", "11:00", "14:00", "15:00", "16:00"
    };

    private readonly List<CitaDto> _citas = new()
    {
        new CitaDto
        {
            Id = 1001,
            PacienteId = "1",
            MedicoId = 1,
            MedicoNombre = "Dra. Ramírez",
            Especialidad = "Infectología",
            Fecha = DateTime.Today.AddDays(5),
            Hora = "09:00",
            Motivo = "Control trimestral",
            Estado = "Confirmada"
        }
    };

    private int _siguienteId = 1002;

    public Task<List<MedicoDto>> ObtenerMedicosAsync() => Task.FromResult(_medicos.ToList());

    public Task<List<CitaDto>> ObtenerCitasPacienteAsync(string pacienteId)
    {
        lock (_bloqueo)
        {
            var citas = _citas
                .Where(c => c.PacienteId == pacienteId)
                .OrderBy(c => c.Fecha)
                .ThenBy(c => c.Hora)
                .ToList();
            return Task.FromResult(citas);
        }
    }

    public Task<List<string>> ObtenerHorariosDisponiblesAsync(int medicoId, DateTime fecha)
    {
        lock (_bloqueo)
        {
            var ocupados = _citas
                .Where(c => c.MedicoId == medicoId && c.Fecha.Date == fecha.Date && c.Estado != "Cancelada")
                .Select(c => c.Hora)
                .ToHashSet();

            var disponibles = _horariosBase.Where(h => !ocupados.Contains(h)).ToList();
            return Task.FromResult(disponibles);
        }
    }

    public Task<ResultadoAgendar> AgendarCitaAsync(string pacienteId, int medicoId, DateTime fecha, string hora, string motivo)
    {
        lock (_bloqueo)
        {
            var medico = _medicos.FirstOrDefault(m => m.Id == medicoId);
            if (medico is null)
            {
                return Task.FromResult(ResultadoAgendar.Fallo("El médico seleccionado no existe."));
            }

            var yaOcupado = _citas.Any(c =>
                c.MedicoId == medicoId && c.Fecha.Date == fecha.Date && c.Hora == hora && c.Estado != "Cancelada");

            if (yaOcupado)
            {
                return Task.FromResult(ResultadoAgendar.Fallo("Ese horario ya no está disponible. Elige otro."));
            }

            var nuevaCita = new CitaDto
            {
                Id = _siguienteId++,
                PacienteId = pacienteId,
                MedicoId = medico.Id,
                MedicoNombre = medico.Nombre,
                Especialidad = medico.Especialidad,
                Fecha = fecha.Date,
                Hora = hora,
                Motivo = motivo,
                Estado = "Confirmada"
            };

            _citas.Add(nuevaCita);
            return Task.FromResult(ResultadoAgendar.Ok(nuevaCita));
        }
    }

    public Task<bool> CancelarCitaAsync(string pacienteId, int citaId)
    {
        lock (_bloqueo)
        {
            var cita = _citas.FirstOrDefault(c => c.Id == citaId && c.PacienteId == pacienteId);
            if (cita is null)
            {
                return Task.FromResult(false);
            }

            cita.Estado = "Cancelada";
            return Task.FromResult(true);
        }
    }
}
