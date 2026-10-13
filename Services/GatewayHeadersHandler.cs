namespace PortalCitas.Services;

/// <summary>
/// Se ejecuta en CADA llamada que el servidor web hace al Gateway
/// (HttpClient del lado del servidor, nunca desde el navegador).
///
///  - Agrega X-Paciente-Id con el paciente de la sesión actual. El
///    valor sale de la sesión del servidor, no de nada que mande el
///    navegador.
///  - Agrega la llave del Gateway si está configurada. Vive en la
///    configuración del servidor (appsettings / user-secrets /
///    variable de entorno) y jamás se envía al navegador.
/// </summary>
public class GatewayHeadersHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _accessor;
    private readonly IConfiguration _config;

    public GatewayHeadersHandler(IHttpContextAccessor accessor, IConfiguration config)
    {
        _accessor = accessor;
        _config = config;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var pacienteId = _accessor.HttpContext?.Session.GetString("PacienteId");
        if (!string.IsNullOrEmpty(pacienteId))
        {
            request.Headers.Remove("X-Paciente-Id");
            request.Headers.Add("X-Paciente-Id", pacienteId);
        }

        var apiKey = _config["IntegracionApiKey"];
        if (!string.IsNullOrEmpty(apiKey))
        {
            var nombreHeader = _config["IntegracionApiKeyHeader"] ?? "X-Api-Key";
            request.Headers.Remove(nombreHeader);
            request.Headers.Add(nombreHeader, apiKey);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
