namespace PortalCitas.Models;

public class ResultadoLogin
{
    public bool Exitoso { get; set; }
    public string? PacienteId { get; set; }
    public string? NombrePaciente { get; set; }
    public string? MensajeError { get; set; }

    public static ResultadoLogin Ok(string pacienteId, string nombrePaciente) => new()
    {
        Exitoso = true,
        PacienteId = pacienteId,
        NombrePaciente = nombrePaciente
    };

    public static ResultadoLogin Fallo(string mensaje) => new()
    {
        Exitoso = false,
        MensajeError = mensaje
    };
}
