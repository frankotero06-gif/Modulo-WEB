using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// Pasarela simulada, local a la web. Sirve mientras el equipo decide
/// quién simula la pasarela (esta web o el Gateway de Ricardo). No
/// guarda ni registra los datos de la tarjeta.
/// Truco de demo: una tarjeta que termine en 0000 es rechazada.
/// </summary>
public class PasarelaMockService : IPasarelaService
{
    public async Task<ResultadoPasarela> AutorizarAsync(DatosPagoInput datosPago, decimal monto)
    {
        await Task.Delay(300);

        var digitos = new string(datosPago.NumeroTarjeta.Where(char.IsDigit).ToArray());
        if (digitos.EndsWith("0000"))
        {
            return ResultadoPasarela.Fallo("La tarjeta fue rechazada por el emisor.");
        }

        var referencia = $"PAS-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        return ResultadoPasarela.Ok(referencia);
    }
}
