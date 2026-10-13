using System.ComponentModel.DataAnnotations;

namespace PortalCitas.Models;

/// <summary>
/// Datos que el paciente llena para crear su cuenta en el portal.
///
/// A propósito NO se pide ningún dato clínico (diagnóstico, código
/// serológico, tratamiento): según RN-01 y RF-CORE-02 eso lo registra
/// el personal médico en la aplicación de empleados del Core, y la
/// identidad civil queda separada del expediente.
/// </summary>
public class RegistroViewModel
{
    [Required(ErrorMessage = "Escribe tu nombre.")]
    [StringLength(60, ErrorMessage = "Máximo 60 caracteres.")]
    [Display(Name = "Nombre(s)")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe tus apellidos.")]
    [StringLength(60, ErrorMessage = "Máximo 60 caracteres.")]
    [Display(Name = "Apellidos")]
    public string Apellidos { get; set; } = string.Empty;

    /// <summary>Cédula dominicana: 11 dígitos, con o sin guiones (000-0000000-0).</summary>
    [Required(ErrorMessage = "Escribe tu cédula.")]
    [RegularExpression(@"^\d{3}-?\d{7}-?\d$", ErrorMessage = "La cédula debe tener 11 dígitos (ej. 001-1234567-8).")]
    [Display(Name = "Cédula")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "Indica tu fecha de nacimiento.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha de nacimiento")]
    public DateTime? FechaNacimiento { get; set; }

    /// <summary>Teléfono dominicano: 809, 829 u 849 + 7 dígitos.</summary>
    [Required(ErrorMessage = "Escribe tu teléfono.")]
    [RegularExpression(@"^\(?(809|829|849)\)?[\s-]?\d{3}[\s-]?\d{4}$", ErrorMessage = "Usa un número dominicano, ej. 809-555-1234.")]
    [Display(Name = "Teléfono")]
    public string Telefono { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe tu correo.")]
    [EmailAddress(ErrorMessage = "Escribe un correo válido.")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Crea una contraseña.")]
    [StringLength(64, MinimumLength = 8, ErrorMessage = "La contraseña debe tener entre 8 y 64 caracteres.")]
    [RegularExpression(@"^(?=.*[A-Za-zÁÉÍÓÚÑáéíóúñ])(?=.*\d).+$", ErrorMessage = "Incluye al menos una letra y un número.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Clave { get; set; } = string.Empty;

    [Required(ErrorMessage = "Repite tu contraseña.")]
    [Compare(nameof(Clave), ErrorMessage = "Las contraseñas no coinciden.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar contraseña")]
    public string ConfirmarClave { get; set; } = string.Empty;

    /// <summary>Consentimiento para el tratamiento de datos personales (Ley 172-13).</summary>
    [Range(typeof(bool), "true", "true", ErrorMessage = "Debes aceptar el uso de tus datos para crear la cuenta.")]
    [Display(Name = "Acepto el tratamiento de mis datos")]
    public bool AceptaPrivacidad { get; set; }

    /// <summary>Cédula sin guiones ni espacios, como se guarda y se envía al Gateway.</summary>
    public string CedulaNormalizada => new(Cedula.Where(char.IsDigit).ToArray());

    /// <summary>Teléfono solo con dígitos (10).</summary>
    public string TelefonoNormalizado => new(Telefono.Where(char.IsDigit).ToArray());
}
