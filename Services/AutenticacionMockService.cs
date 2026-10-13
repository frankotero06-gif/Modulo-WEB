using System.Security.Cryptography;
using PortalCitas.Models;

namespace PortalCitas.Services;

/// <summary>
/// Login y registro simulados, activos mientras Integración (Ricardo) no
/// tenga listos los endpoints reales.
///
/// Las cuentas viven en memoria: se registra como Singleton en Program.cs
/// para que una cuenta creada en la demo siga existiendo mientras la app
/// esté corriendo (se pierde al reiniciar).
///
/// Las contraseñas nunca se guardan en texto plano: se guarda un hash
/// PBKDF2-SHA256 con sal aleatoria por cuenta, el mismo enfoque de
/// "hash salado" que pide RF-CORE-01 para el sistema real.
///
/// Cuenta de demo ya creada: paciente@correo.com / 123456
/// </summary>
public class AutenticacionMockService : IAutenticacionService
{
    private const int Iteraciones = 100_000;
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;

    private readonly object _bloqueo = new();
    private readonly Dictionary<string, CuentaMock> _cuentasPorCorreo = new(StringComparer.OrdinalIgnoreCase);
    private int _siguienteId = 2;

    public AutenticacionMockService()
    {
        // Paciente de ejemplo: las citas, facturas y resultados de los
        // demás mocks están cargados para el pacienteId "1".
        AgregarCuenta("1", "Juan Pérez", "paciente@correo.com", "00112345678", "123456");
    }

    public async Task<ResultadoLogin> IniciarSesionAsync(string correo, string clave)
    {
        await Task.Delay(300);

        if (string.IsNullOrWhiteSpace(correo) || string.IsNullOrWhiteSpace(clave))
        {
            return ResultadoLogin.Fallo("Correo o contraseña incorrectos.");
        }

        CuentaMock? cuenta;
        lock (_bloqueo)
        {
            _cuentasPorCorreo.TryGetValue(correo.Trim(), out cuenta);
        }

        // Mismo mensaje si el correo no existe o si la clave no coincide,
        // para no revelar qué correos tienen cuenta.
        if (cuenta is null || !ClaveCoincide(clave, cuenta))
        {
            return ResultadoLogin.Fallo("Correo o contraseña incorrectos.");
        }

        return ResultadoLogin.Ok(cuenta.PacienteId, cuenta.NombreCompleto);
    }

    public async Task<ResultadoRegistro> RegistrarPacienteAsync(RegistroViewModel datos)
    {
        await Task.Delay(300);

        var correo = datos.Correo.Trim();
        var cedula = datos.CedulaNormalizada;
        var nombre = $"{datos.Nombres.Trim()} {datos.Apellidos.Trim()}";

        lock (_bloqueo)
        {
            if (_cuentasPorCorreo.ContainsKey(correo))
            {
                return ResultadoRegistro.Fallo(
                    "Ya existe una cuenta con este correo. Inicia sesión o usa otro correo.",
                    nameof(RegistroViewModel.Correo));
            }

            if (_cuentasPorCorreo.Values.Any(c => c.Cedula == cedula))
            {
                return ResultadoRegistro.Fallo(
                    "Esta cédula ya tiene una cuenta en el portal. Si no la recuerdas, acércate a tu centro de atención.",
                    nameof(RegistroViewModel.Cedula));
            }

            var pacienteId = (_siguienteId++).ToString();
            AgregarCuenta(pacienteId, nombre, correo, cedula, datos.Clave);
            return ResultadoRegistro.Ok(pacienteId, nombre);
        }
    }

    private void AgregarCuenta(string pacienteId, string nombre, string correo, string cedula, string clave)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanoSal);
        _cuentasPorCorreo[correo] = new CuentaMock(
            pacienteId,
            nombre,
            correo,
            cedula,
            sal,
            CalcularHash(clave, sal));
    }

    private static bool ClaveCoincide(string clave, CuentaMock cuenta)
    {
        var hash = CalcularHash(clave, cuenta.Sal);
        return CryptographicOperations.FixedTimeEquals(hash, cuenta.HashClave);
    }

    private static byte[] CalcularHash(string clave, byte[] sal) =>
        Rfc2898DeriveBytes.Pbkdf2(clave, sal, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);

    private sealed record CuentaMock(
        string PacienteId,
        string NombreCompleto,
        string Correo,
        string Cedula,
        byte[] Sal,
        byte[] HashClave);
}
