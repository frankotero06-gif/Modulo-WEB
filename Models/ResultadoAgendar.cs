namespace PortalCitas.Models;

public class ResultadoAgendar
{
    public bool Exitoso { get; set; }
    public string? MensajeError { get; set; }
    public CitaDto? Cita { get; set; }

    public static ResultadoAgendar Ok(CitaDto cita) => new() { Exitoso = true, Cita = cita };
    public static ResultadoAgendar Fallo(string mensaje) => new() { Exitoso = false, MensajeError = mensaje };
}
