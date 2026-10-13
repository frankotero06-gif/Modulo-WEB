namespace PortalCitas.Models;

public class ResultadoRegistro
{
    public bool Exitoso { get; set; }
    public string? PacienteId { get; set; }
    public string? NombrePaciente { get; set; }

    /// <summary>Token JWT que devuelve el Gateway. Se guarda en la sesión del servidor.</summary>
    public string? Token { get; set; }
    public string? MensajeError { get; set; }

    /// <summary>
    /// Nombre del campo del formulario al que pertenece el error (ej. "Correo"
    /// si el correo ya está registrado), para mostrarlo junto a ese campo.
    /// </summary>
    public string? CampoError { get; set; }

    public static ResultadoRegistro Ok(string pacienteId, string nombrePaciente, string? token = null) => new()
    {
        Exitoso = true,
        PacienteId = pacienteId,
        NombrePaciente = nombrePaciente,
        Token = token
    };

    public static ResultadoRegistro Fallo(string mensaje, string? campo = null) => new()
    {
        Exitoso = false,
        MensajeError = mensaje,
        CampoError = campo
    };
}
