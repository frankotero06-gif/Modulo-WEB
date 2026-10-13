namespace PortalCitas.Models;

/// <summary>Contenido de un PDF simple (constancia o comprobante) para GeneradorPdf.</summary>
public class DocumentoPdf
{
    public string Titulo { get; set; } = string.Empty;
    public string Subtitulo { get; set; } = string.Empty;

    public string EtiquetaNumero { get; set; } = "Número de constancia";
    public string Numero { get; set; } = string.Empty;

    public string EtiquetaFecha { get; set; } = "Emitida el";
    public string Fecha { get; set; } = string.Empty;

    public List<SeccionPdf> Secciones { get; set; } = new();

    /// <summary>Texto legal pequeño al final del documento.</summary>
    public string? Nota { get; set; }
}

public class SeccionPdf
{
    public string Titulo { get; set; } = string.Empty;
    public List<DetalleConstancia> Filas { get; set; } = new();

    public SeccionPdf() { }

    public SeccionPdf(string titulo, IEnumerable<DetalleConstancia> filas)
    {
        Titulo = titulo;
        Filas = filas.ToList();
    }
}
