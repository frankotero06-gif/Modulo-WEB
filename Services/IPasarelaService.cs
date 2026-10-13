using PortalCitas.Models;

namespace PortalCitas.Services;

public class ResultadoPasarela
{
    public bool Exitoso { get; set; }
    public string? Referencia { get; set; }
    public string? MensajeError { get; set; }

    public static ResultadoPasarela Ok(string referencia) => new() { Exitoso = true, Referencia = referencia };
    public static ResultadoPasarela Fallo(string mensaje) => new() { Exitoso = false, MensajeError = mensaje };
}

/// <summary>
/// Representa a la pasarela de pago (entidad externa). Recibe los datos
/// de la tarjeta, autoriza el cobro y devuelve una referencia. Esa
/// referencia es lo ÚNICO que se envía al CORE.
/// </summary>
public interface IPasarelaService
{
    Task<ResultadoPasarela> AutorizarAsync(DatosPagoInput datosPago, decimal monto);
}
