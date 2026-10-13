using System.ComponentModel.DataAnnotations;

namespace PortalCitas.Models;

public class NuevaCitaInput
{
    // int nunca llega "vacío" (vale 0), por eso además del Required se
    // valida el rango: así sí aparece el error si no se eligió médico.
    [Required(ErrorMessage = "Selecciona un médico.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un médico.")]
    [Display(Name = "Médico / Especialidad")]
    public int MedicoId { get; set; }

    [Required(ErrorMessage = "Selecciona una fecha.")]
    [DataType(DataType.Date)]
    [Display(Name = "Fecha")]
    public DateTime Fecha { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Selecciona un horario disponible.")]
    [Display(Name = "Horario")]
    public string Hora { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cuéntanos brevemente el motivo de la cita.")]
    [StringLength(300, ErrorMessage = "Máximo 300 caracteres.")]
    [Display(Name = "Motivo de la consulta")]
    public string Motivo { get; set; } = string.Empty;
}

/// <summary>ViewModel combinado que arma el Controller para la vista Agendar.</summary>
public class AgendarCitaViewModel
{
    public NuevaCitaInput Form { get; set; } = new();
    public List<MedicoDto> Medicos { get; set; } = new();
}
