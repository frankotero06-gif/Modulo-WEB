using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// Constancias en memoria (Singleton en Program.cs) mientras Integración no
/// exponga el endpoint real. Trae de ejemplo las constancias del paciente
/// de prueba ("1"), coherentes con los datos de los otros mocks.
/// </summary>
public class ConstanciaMockService : IConstanciaService
{
    private readonly object _bloqueo = new();
    private readonly List<ConstanciaDto> _constancias = new();
    private int _siguienteId = 1;

    public ConstanciaMockService()
    {
        var registro = new ConstanciaDto
        {
            PacienteId = "1",
            NombrePaciente = "Juan Pérez",
            Tipo = TiposConstancia.RegistroCuenta,
            Titulo = "Constancia de registro",
            Descripcion = "Creaste tu cuenta en el Portal del paciente",
            Detalles = new()
            {
                new("Cédula", "001-*****67-8"),
                new("Correo de acceso", "paciente@correo.com"),
                new("Teléfono", "809-555-1234"),
                new("Consentimiento Ley 172-13", "Aceptado"),
            }
        };
        Guardar(registro, DateTime.Today.AddDays(-30).AddHours(10));

        var pagoArv = FabricaConstancias.PagoRealizado("1", "Juan Pérez", new FacturaDto
        {
            Id = 1,
            Concepto = "Dispensación Medicamentos ARV",
            Monto = 0m,
            EsSubsidiada = true,
            NumeroRecibo = "REC-9402",
            NumeroFactura = "FAC-9402",
        });
        Guardar(pagoArv, DateTime.Today.AddDays(-3).AddHours(11));

        var cita = FabricaConstancias.CitaAgendada("1", "Juan Pérez", new CitaDto
        {
            Id = 1001,
            MedicoNombre = "Dra. Ramírez",
            Especialidad = "Infectología",
            Fecha = DateTime.Today.AddDays(5),
            Hora = "09:00",
            Motivo = "Control trimestral",
        });
        Guardar(cita, DateTime.Today.AddDays(-2).AddHours(16));
    }

    public Task<ConstanciaDto?> RegistrarAsync(ConstanciaDto nueva)
    {
        lock (_bloqueo)
        {
            return Task.FromResult<ConstanciaDto?>(Guardar(nueva, DateTime.Now));
        }
    }

    public Task<List<ConstanciaDto>> ObtenerPorPacienteAsync(string pacienteId)
    {
        lock (_bloqueo)
        {
            var lista = _constancias
                .Where(c => c.PacienteId == pacienteId)
                .OrderByDescending(c => c.FechaEmision)
                .ToList();
            return Task.FromResult(lista);
        }
    }

    public Task<ConstanciaDto?> ObtenerAsync(string pacienteId, int constanciaId)
    {
        lock (_bloqueo)
        {
            var constancia = _constancias.FirstOrDefault(c => c.Id == constanciaId && c.PacienteId == pacienteId);
            return Task.FromResult(constancia);
        }
    }

    private ConstanciaDto Guardar(ConstanciaDto constancia, DateTime fecha)
    {
        constancia.Id = _siguienteId++;
        constancia.FechaEmision = fecha;
        constancia.Numero = $"CON-{fecha:yyyyMMdd}-{constancia.Id:D4}";
        _constancias.Add(constancia);
        return constancia;
    }
}
