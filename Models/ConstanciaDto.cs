namespace PortalCitas.Models;

/// <summary>
/// Constancia de una acción que el paciente hizo en el portal (crear la
/// cuenta, agendar o cancelar una cita, pagar una factura). El profesor
/// pidió que toda solicitud o cambio hecho por la web le deje al usuario
/// un documento que pueda guardar o descargar como prueba de lo que hizo.
///
/// Nunca incluye valores clínicos (RN-01).
/// </summary>
public class ConstanciaDto
{
    public int Id { get; set; }

    /// <summary>Número visible, ej. "CON-20261013-0004".</summary>
    public string Numero { get; set; } = string.Empty;

    public string PacienteId { get; set; } = string.Empty;
    public string NombrePaciente { get; set; } = string.Empty;

    /// <summary>Uno de los valores de <see cref="TiposConstancia"/>.</summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>Ej. "Constancia de cita agendada".</summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>Resumen de una línea, ej. "Dra. Ramírez, martes 13 de octubre a las 10:00".</summary>
    public string Descripcion { get; set; } = string.Empty;

    public DateTime FechaEmision { get; set; }

    /// <summary>Id de la cita o factura a la que se refiere, si aplica.</summary>
    public int? ReferenciaId { get; set; }

    public List<DetalleConstancia> Detalles { get; set; } = new();
}

public class DetalleConstancia
{
    public string Etiqueta { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;

    public DetalleConstancia() { }

    public DetalleConstancia(string etiqueta, string valor)
    {
        Etiqueta = etiqueta;
        Valor = valor;
    }
}

public static class TiposConstancia
{
    public const string RegistroCuenta = "RegistroCuenta";
    public const string CitaAgendada = "CitaAgendada";
    public const string CitaCancelada = "CitaCancelada";
    public const string PagoRealizado = "PagoRealizado";
}
