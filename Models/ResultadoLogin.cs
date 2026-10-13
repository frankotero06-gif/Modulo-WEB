namespace PortalCitas.Models;

public class ResultadoLogin
{
    public bool Exitoso { get; set; }
    public string? PacienteId { get; set; }
    public string? NombrePaciente { get; set; }

    /// <summary>Token JWT que devuelve el Gateway. Se guarda en la sesión del servidor.</summary>
    public string? Token { get; set; }
    public string? MensajeError { get; set; }

    public static ResultadoLogin Ok(string pacienteId, string nombrePaciente, string? token = null) => new()
    {
        Exitoso = true,
        PacienteId = pacienteId,
        NombrePaciente = nombrePaciente,
        Token = token
    };

    public static ResultadoLogin Fallo(string mensaje) => new()
    {
        Exitoso = false,
        MensajeError = mensaje
    };
}
