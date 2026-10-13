using System.ComponentModel.DataAnnotations;

namespace PortalCitas.Models;

public class DatosPagoInput
{
    [Required(ErrorMessage = "Ingresa el nombre del titular.")]
    [Display(Name = "Nombre del titular")]
    public string NombreTitular { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa el número de tarjeta.")]
    [RegularExpression(@"^[0-9\s]{13,19}$", ErrorMessage = "Número de tarjeta inválido.")]
    [Display(Name = "Número de tarjeta")]
    public string NumeroTarjeta { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa la fecha de vencimiento.")]
    [RegularExpression(@"^(0[1-9]|1[0-2])\/[0-9]{2}$", ErrorMessage = "Formato MM/AA.")]
    [Display(Name = "Vencimiento (MM/AA)")]
    public string Vencimiento { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresa el CVV.")]
    [RegularExpression(@"^[0-9]{3,4}$", ErrorMessage = "CVV inválido.")]
    [Display(Name = "CVV")]
    public string Cvv { get; set; } = string.Empty;
}
