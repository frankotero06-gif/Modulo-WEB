namespace PortalCitas.Services;

/// <summary>
/// Se ejecuta en CADA llamada que el servidor web hace al Gateway
/// (HttpClient del lado del servidor, nunca desde el navegador).
///
///  - Agrega "Authorization: Bearer {token}" con el token que el Gateway
///    entregó al iniciar sesión o registrarse. El token vive en la sesión
///    del servidor; nunca se envía al navegador.
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
        var token = _accessor.HttpContext?.Session.GetString("Token");
        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
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
