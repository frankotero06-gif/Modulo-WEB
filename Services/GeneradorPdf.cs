using System.Globalization;
using System.Text;
using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// Genera un PDF sencillo (tamaño A4) para constancias y comprobantes, sin
/// librerías externas: escribe directamente el formato PDF 1.4 con las
/// fuentes estándar Helvetica / Helvetica-Bold, que todo lector de PDF trae.
///
/// Limitación conocida: esas fuentes usan la codificación WinAnsi (Latin-1),
/// que cubre el español (á, é, ñ, ¿, ¡...). Cualquier otro carácter se
/// reemplaza por uno equivalente o por "?".
/// </summary>
public static class GeneradorPdf
{
    private static readonly double[] Azul = { 0.0, 0.184, 0.424 };
    private static readonly double[] Rojo = { 0.808, 0.067, 0.149 };
    private static readonly double[] Tinta = { 0.071, 0.125, 0.227 };
    private static readonly double[] Gris = { 0.275, 0.329, 0.427 };
    private static readonly double[] Linea = { 0.867, 0.894, 0.933 };
    private static readonly double[] FondoNumero = { 0.929, 0.949, 0.976 };
    private static readonly double[] Blanco = { 1.0, 1.0, 1.0 };
    private static readonly double[] AzulClaro = { 0.86, 0.9, 0.96 };

    public static byte[] Generar(DocumentoPdf doc)
    {
        var lienzo = new Lienzo();
        lienzo.NuevaPagina();

        // Título y subtítulo
        lienzo.Texto(56, lienzo.Y, "F2", 20, Tinta, doc.Titulo);
        lienzo.Y -= 20;
        foreach (var linea in Partir(doc.Subtitulo, 90))
        {
            lienzo.Texto(56, lienzo.Y, "F1", 10.5, Gris, linea);
            lienzo.Y -= 14;
        }
        lienzo.Y -= 8;

        // Recuadro con número y fecha
        lienzo.Color(FondoNumero, trazo: false);
        lienzo.Agregar($"56 {N(lienzo.Y - 38)} 483 44 re f");
        lienzo.Texto(70, lienzo.Y - 12, "F1", 8.5, Gris, doc.EtiquetaNumero);
        lienzo.Texto(70, lienzo.Y - 28, "F2", 13, Azul, doc.Numero);
        lienzo.Texto(330, lienzo.Y - 12, "F1", 8.5, Gris, doc.EtiquetaFecha);
        lienzo.Texto(330, lienzo.Y - 28, "F2", 11, Tinta, doc.Fecha);
        lienzo.Y -= 62;

        // Secciones con filas "etiqueta: valor"
        foreach (var seccion in doc.Secciones)
        {
            lienzo.Asegurar(40);
            lienzo.Texto(56, lienzo.Y, "F2", 11.5, Azul, seccion.Titulo);
            lienzo.Y -= 10;
            lienzo.Color(Linea, trazo: true);
            lienzo.Agregar($"0.8 w 56 {N(lienzo.Y)} m 539 {N(lienzo.Y)} l S");
            lienzo.Y -= 16;

            foreach (var fila in seccion.Filas)
            {
                var lineas = Partir(fila.Valor, 52);
                lienzo.Asegurar(14 * lineas.Count + 10);
                lienzo.Texto(56, lienzo.Y, "F1", 10, Gris, fila.Etiqueta);
                for (var i = 0; i < lineas.Count; i++)
                {
                    lienzo.Texto(230, lienzo.Y - 14 * i, "F2", 10, Tinta, lineas[i]);
                }
                lienzo.Y -= 14 * lineas.Count + 6;
                lienzo.Color(Linea, trazo: true);
                lienzo.Agregar($"0.5 w 56 {N(lienzo.Y + 2)} m 539 {N(lienzo.Y + 2)} l S");
                lienzo.Y -= 10;
            }
            lienzo.Y -= 10;
        }

        // Nota legal
        if (!string.IsNullOrWhiteSpace(doc.Nota))
        {
            var lineas = Partir(doc.Nota, 105);
            lienzo.Asegurar(12 * lineas.Count + 10);
            foreach (var linea in lineas)
            {
                lienzo.Texto(56, lienzo.Y, "F1", 8.5, Gris, linea);
                lienzo.Y -= 12;
            }
        }

        var paginas = lienzo.Terminar();
        return Ensamblar(paginas);
    }

    // ------------------------------------------------------------------
    // Armado del archivo PDF (objetos, tabla xref y trailer)
    // ------------------------------------------------------------------
    private static byte[] Ensamblar(List<string> paginas)
    {
        var total = paginas.Count;
        var latin1 = Encoding.Latin1;
        using var salida = new MemoryStream();
        var posiciones = new Dictionary<int, long>();

        void Escribir(string s)
        {
            var bytes = latin1.GetBytes(s);
            salida.Write(bytes, 0, bytes.Length);
        }

        void Objeto(int numero, string cuerpo)
        {
            posiciones[numero] = salida.Position;
            Escribir($"{numero} 0 obj\n{cuerpo}\nendobj\n");
        }

        Escribir("%PDF-1.4\n");
        salida.Write(new byte[] { 0x25, 0xE2, 0xE3, 0xCF, 0xD3, 0x0A }, 0, 6);

        // Objetos: 1 catálogo, 2 páginas, 3-4 fuentes, y por cada página
        // un objeto Page (5, 7, 9...) y su contenido (6, 8, 10...).
        var hijos = string.Join(" ", Enumerable.Range(0, total).Select(i => $"{5 + 2 * i} 0 R"));
        Objeto(1, "<< /Type /Catalog /Pages 2 0 R >>");
        Objeto(2, $"<< /Type /Pages /Kids [{hijos}] /Count {total} >>");
        Objeto(3, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica /Encoding /WinAnsiEncoding >>");
        Objeto(4, "<< /Type /Font /Subtype /Type1 /BaseFont /Helvetica-Bold /Encoding /WinAnsiEncoding >>");

        for (var i = 0; i < total; i++)
        {
            var numeroPagina = 5 + 2 * i;
            var numeroContenido = numeroPagina + 1;
            Objeto(numeroPagina,
                $"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 595 842] " +
                $"/Resources << /Font << /F1 3 0 R /F2 4 0 R >> >> /Contents {numeroContenido} 0 R >>");

            var datos = latin1.GetBytes(paginas[i]);
            posiciones[numeroContenido] = salida.Position;
            Escribir($"{numeroContenido} 0 obj\n<< /Length {datos.Length} >>\nstream\n");
            salida.Write(datos, 0, datos.Length);
            Escribir("\nendstream\nendobj\n");
        }

        var inicioXref = salida.Position;
        var totalObjetos = 4 + 2 * total;
        var xref = new StringBuilder();
        xref.Append($"xref\n0 {totalObjetos + 1}\n0000000000 65535 f \n");
        for (var k = 1; k <= totalObjetos; k++)
        {
            xref.Append(posiciones[k].ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
        }
        xref.Append($"trailer\n<< /Size {totalObjetos + 1} /Root 1 0 R >>\nstartxref\n{inicioXref}\n%%EOF\n");
        Escribir(xref.ToString());

        return salida.ToArray();
    }

    // ------------------------------------------------------------------
    // Utilidades de texto
    // ------------------------------------------------------------------

    /// <summary>Número con punto decimal, sin importar la cultura del servidor.</summary>
    private static string N(double valor) => valor.ToString("0.###", CultureInfo.InvariantCulture);

    /// <summary>Reemplaza caracteres que WinAnsi no tiene.</summary>
    private static string Limpiar(string texto)
    {
        var sb = new StringBuilder(texto.Length);
        foreach (var c in texto)
        {
            switch (c)
            {
                case '—': case '–': case '•': sb.Append('-'); break;
                case '“': case '”': sb.Append('"'); break;
                case '‘': case '’': sb.Append('\''); break;
                case '…': sb.Append("..."); break;
                case ' ': case ' ': sb.Append(' '); break;
                case '\r': break;
                default: sb.Append(c < 256 ? c : '?'); break;
            }
        }
        return sb.ToString();
    }

    private static string Escapar(string texto) =>
        Limpiar(texto).Replace("\\", "\\\\").Replace("(", "\\(").Replace(")", "\\)");

    /// <summary>Corta el texto en líneas de como máximo <paramref name="maximo"/> caracteres, por palabras.</summary>
    private static List<string> Partir(string texto, int maximo)
    {
        var lineas = new List<string>();
        foreach (var parrafo in Limpiar(texto ?? string.Empty).Split('\n'))
        {
            var actual = string.Empty;
            foreach (var palabra in parrafo.Split(' '))
            {
                if (actual.Length == 0)
                {
                    actual = palabra;
                }
                else if (actual.Length + 1 + palabra.Length <= maximo)
                {
                    actual += " " + palabra;
                }
                else
                {
                    lineas.Add(actual);
                    actual = palabra;
                }
            }
            lineas.Add(actual);
        }
        return lineas;
    }

    // ------------------------------------------------------------------
    // Lienzo: acumula las instrucciones de dibujo de cada página
    // ------------------------------------------------------------------
    private sealed class Lienzo
    {
        private readonly List<string> _paginas = new();
        private StringBuilder? _actual;

        /// <summary>Posición vertical actual (en PDF, 0 es el borde de abajo).</summary>
        public double Y { get; set; }

        public void Agregar(string instruccion) => _actual!.Append(instruccion).Append('\n');

        public void Color(double[] c, bool trazo) =>
            Agregar($"{N(c[0])} {N(c[1])} {N(c[2])} {(trazo ? "RG" : "rg")}");

        public void Texto(double x, double y, string fuente, double tamano, double[] color, string texto)
        {
            Color(color, trazo: false);
            Agregar($"BT /{fuente} {N(tamano)} Tf {N(x)} {N(y)} Td ({Escapar(texto)}) Tj ET");
        }

        public void NuevaPagina()
        {
            if (_actual != null)
            {
                _paginas.Add(_actual.ToString());
            }
            _actual = new StringBuilder();
            Encabezado();
        }

        /// <summary>Si no cabe un bloque de esta altura, pasa a una página nueva.</summary>
        public void Asegurar(double alto)
        {
            if (Y - alto < 70)
            {
                NuevaPagina();
            }
        }

        /// <summary>Cierra la última página y agrega el pie con "Página X de Y".</summary>
        public List<string> Terminar()
        {
            if (_actual != null)
            {
                _paginas.Add(_actual.ToString());
                _actual = null;
            }

            var total = _paginas.Count;
            var pie = Escapar("Documento generado por el Portal del paciente de CONAVIHSIDA. Proyecto académico.");
            var resultado = new List<string>();
            for (var i = 0; i < total; i++)
            {
                var numeracion = Escapar("Página " + (i + 1) + " de " + total);
                resultado.Add(
                    _paginas[i] +
                    "0.435 0.49 0.576 rg\n" +
                    "BT /F1 8 Tf 56 40 Td (" + pie + ") Tj ET\n" +
                    "BT /F1 8 Tf 500 40 Td (" + numeracion + ") Tj ET\n");
            }
            return resultado;
        }

        private void Encabezado()
        {
            // Banda azul
            Color(Azul, trazo: false);
            Agregar("0 762 595 80 re f");

            // Lazo rojo (mismo trazo que el ícono del portal, escalado)
            static (double X, double Y) P(double x, double y) => (56 + x * 0.24, 828 - y * 0.24);
            var a = P(24, 134);
            var b = P(56, 72);
            var c1 = P(64, 55); var c2 = P(70, 40); var c3 = P(64, 26);
            var d1 = P(58, 10); var d2 = P(42, 10); var d3 = P(36, 26);
            var e1 = P(30, 40); var e2 = P(36, 55); var e3 = P(44, 72);
            var f = P(76, 134);
            Color(Rojo, trazo: true);
            Agregar("3.2 w 1 j");
            Agregar(
                $"{N(a.X)} {N(a.Y)} m {N(b.X)} {N(b.Y)} l " +
                $"{N(c1.X)} {N(c1.Y)} {N(c2.X)} {N(c2.Y)} {N(c3.X)} {N(c3.Y)} c " +
                $"{N(d1.X)} {N(d1.Y)} {N(d2.X)} {N(d2.Y)} {N(d3.X)} {N(d3.Y)} c " +
                $"{N(e1.X)} {N(e1.Y)} {N(e2.X)} {N(e2.Y)} {N(e3.X)} {N(e3.Y)} c " +
                $"{N(f.X)} {N(f.Y)} l S");

            Texto(88, 806, "F2", 16, Blanco, "CONAVIHSIDA");
            Texto(88, 789, "F1", 9.5, AzulClaro, "Consejo Nacional para el VIH y el SIDA");
            Texto(88, 776, "F1", 9.5, AzulClaro, "Portal del paciente");
            Y = 720;
        }
    }
}
