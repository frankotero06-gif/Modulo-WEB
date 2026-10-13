using System.Globalization;
using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// Arma el contenido de cada tipo de constancia y su versión en PDF, para
/// que los Controllers no repitan esta lógica.
/// </summary>
public static class FabricaConstancias
{
    private static readonly CultureInfo Es = new("es-DO");

    private const string NotaLegal =
        "Esta constancia sirve como prueba de la solicitud realizada en el Portal del paciente. " +
        "Conforme a la Ley No. 135-11 sobre VIH/SIDA y la Ley No. 172-13 de protección de datos personales, " +
        "la información de este documento es confidencial. No contiene información clínica. " +
        "CONAVIHSIDA, República Dominicana.";

    public static ConstanciaDto RegistroCuenta(string pacienteId, RegistroViewModel datos)
    {
        var nombre = $"{datos.Nombres.Trim()} {datos.Apellidos.Trim()}";
        return new ConstanciaDto
        {
            PacienteId = pacienteId,
            NombrePaciente = nombre,
            Tipo = TiposConstancia.RegistroCuenta,
            Titulo = "Constancia de registro",
            Descripcion = "Creaste tu cuenta en el Portal del paciente",
            Detalles = new()
            {
                new("Cédula", OcultarCedula(datos.CedulaNormalizada)),
                new("Correo de acceso", datos.Correo.Trim()),
                new("Teléfono", datos.Telefono.Trim()),
                new("Consentimiento Ley 172-13", "Aceptado"),
            }
        };
    }

    public static ConstanciaDto CitaAgendada(string pacienteId, string nombrePaciente, CitaDto cita) => new()
    {
        PacienteId = pacienteId,
        NombrePaciente = nombrePaciente,
        Tipo = TiposConstancia.CitaAgendada,
        Titulo = "Constancia de cita agendada",
        Descripcion = $"{cita.MedicoNombre}, {FechaLarga(cita.Fecha)} a las {cita.Hora}",
        ReferenciaId = cita.Id,
        Detalles = DetallesCita(cita, "Confirmada")
    };

    public static ConstanciaDto CitaCancelada(string pacienteId, string nombrePaciente, CitaDto cita) => new()
    {
        PacienteId = pacienteId,
        NombrePaciente = nombrePaciente,
        Tipo = TiposConstancia.CitaCancelada,
        Titulo = "Constancia de cancelación de cita",
        Descripcion = $"{cita.MedicoNombre}, {FechaLarga(cita.Fecha)} a las {cita.Hora}",
        ReferenciaId = cita.Id,
        Detalles = DetallesCita(cita, "Cancelada por el paciente")
    };

    public static ConstanciaDto PagoRealizado(string pacienteId, string nombrePaciente, FacturaDto factura) => new()
    {
        PacienteId = pacienteId,
        NombrePaciente = nombrePaciente,
        Tipo = TiposConstancia.PagoRealizado,
        Titulo = "Constancia de pago",
        Descripcion = $"{factura.Concepto}, RD$ {factura.Monto.ToString("N2", Es)}",
        ReferenciaId = factura.Id,
        Detalles = new()
        {
            new("Concepto", factura.Concepto),
            new("Monto pagado", $"RD$ {factura.Monto.ToString("N2", Es)}"),
            new("Número de recibo", factura.NumeroRecibo ?? "-"),
            new("Número de factura", factura.NumeroFactura ?? "-"),
            new("Forma de pago", factura.EsSubsidiada ? "Subsidio estatal 100%" : "Tarjeta (en línea)"),
        }
    };

    /// <summary>PDF de una constancia.</summary>
    public static byte[] Pdf(ConstanciaDto constancia) => GeneradorPdf.Generar(new DocumentoPdf
    {
        Titulo = constancia.Titulo,
        Subtitulo = constancia.Descripcion,
        Numero = constancia.Numero,
        Fecha = FechaHora(constancia.FechaEmision),
        Secciones = new()
        {
            new SeccionPdf("Paciente", new[]
            {
                new DetalleConstancia("Nombre", constancia.NombrePaciente),
                new DetalleConstancia("Número de paciente", constancia.PacienteId),
            }),
            new SeccionPdf("Detalle", constancia.Detalles),
        },
        Nota = NotaLegal
    });

    /// <summary>PDF del recibo y factura de un pago (RN-05).</summary>
    public static byte[] PdfComprobante(FacturaDto factura, string nombrePaciente, string pacienteId) => GeneradorPdf.Generar(new DocumentoPdf
    {
        Titulo = "Recibo y factura",
        Subtitulo = "Comprobante de pago generado por el Portal del paciente.",
        EtiquetaNumero = "Número de recibo",
        Numero = factura.NumeroRecibo ?? "-",
        EtiquetaFecha = "Pagado el",
        Fecha = factura.FechaPago is DateTime pagado ? FechaHora(pagado) : "-",
        Secciones = new()
        {
            new SeccionPdf("Factura", new[]
            {
                new DetalleConstancia("Número de factura", factura.NumeroFactura ?? "-"),
                new DetalleConstancia("Concepto", factura.Concepto),
                new DetalleConstancia("Fecha del servicio", factura.Fecha.ToString("d 'de' MMMM 'de' yyyy", Es)),
                new DetalleConstancia("Cobertura", factura.EsSubsidiada ? "Subsidio estatal 100%" : "Pago del paciente"),
                new DetalleConstancia("Total pagado", $"RD$ {factura.Monto.ToString("N2", Es)}"),
            }),
            new SeccionPdf("Paciente", new[]
            {
                new DetalleConstancia("Nombre", nombrePaciente),
                new DetalleConstancia("Número de paciente", pacienteId),
            }),
        },
        Nota = "Este documento sirve como recibo y factura del servicio descrito, conforme a la Ley No. 135-11 " +
               "y la Ley No. 172-13 sobre Protección de Datos Personales. CONAVIHSIDA, República Dominicana."
    });

    public static string FechaLarga(DateTime fecha) => fecha.ToString("dddd d 'de' MMMM 'de' yyyy", Es);

    public static string FechaHora(DateTime fecha) => fecha.ToString("d 'de' MMMM 'de' yyyy, h:mm tt", Es);

    private static List<DetalleConstancia> DetallesCita(CitaDto cita, string estado) => new()
    {
        new("Número de cita", cita.Id.ToString(CultureInfo.InvariantCulture)),
        new("Médico", cita.MedicoNombre),
        new("Especialidad", cita.Especialidad),
        new("Fecha", FechaLarga(cita.Fecha)),
        new("Hora", cita.Hora),
        new("Motivo", string.IsNullOrWhiteSpace(cita.Motivo) ? "-" : cita.Motivo),
        new("Estado", estado),
    };

    /// <summary>"00112345678" → "001-*****78-9": basta para reconocerla sin exponerla completa.</summary>
    private static string OcultarCedula(string cedula) =>
        cedula.Length == 11 ? $"{cedula[..3]}-*****{cedula.Substring(8, 2)}-{cedula[10]}" : "***";
}
