using System.Globalization;
using Microsoft.AspNetCore.Localization;
using PortalCitas.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<GatewayHeadersHandler>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
});

// ------------------------------------------------------------
// Mismo patrón en los dos servicios: un switch en appsettings.json
// decide si usamos datos falsos o la API real de Integración.
// ------------------------------------------------------------
var usarMockAuth = builder.Configuration.GetValue<bool>("UsarMockAutenticacion");
var usarMockAgenda = builder.Configuration.GetValue<bool>("UsarMockAgenda");
var usarMockPago = builder.Configuration.GetValue<bool>("UsarMockPago");
var usarMockResultados = builder.Configuration.GetValue<bool>("UsarMockResultados");
var usarMockConstancias = builder.Configuration.GetValue<bool>("UsarMockConstancias");
var integracionBaseUrl = builder.Configuration["IntegracionApiBaseUrl"]
    ?? "https://TODO-reemplazar-con-url-de-ricardo.com/api";

// HttpClient descarta el último segmento de la URL base si no termina en "/"
// (".../api" + "medicos" daría ".../medicos"). Por eso se fuerza el "/" final
// y las rutas en Services/ no empiezan con "/".
if (!integracionBaseUrl.EndsWith("/")) integracionBaseUrl += "/";

if (usarMockAuth)
{
    // Singleton: así las cuentas creadas con "Crear cuenta" siguen
    // existiendo mientras la app esté corriendo.
    builder.Services.AddSingleton<IAutenticacionService, AutenticacionMockService>();
}
else
{
    builder.Services.AddHttpClient<IAutenticacionService, AutenticacionIntegracionService>(c => c.BaseAddress = new Uri(integracionBaseUrl)).AddHttpMessageHandler<GatewayHeadersHandler>();
}

if (usarMockAgenda)
{
    // Singleton a propósito: así las citas agendadas en la demo
    // persisten mientras la app siga corriendo.
    builder.Services.AddSingleton<IAgendaService, AgendaMockService>();
}
else
{
    builder.Services.AddHttpClient<IAgendaService, AgendaIntegracionService>(c => c.BaseAddress = new Uri(integracionBaseUrl)).AddHttpMessageHandler<GatewayHeadersHandler>();
}

if (usarMockPago)
{
    // Singleton, igual que el mock de agenda: para que un pago hecho
    // durante la demo se refleje de inmediato en el comprobante.
    builder.Services.AddSingleton<IPagoService, PagoMockService>();
}
else
{
    builder.Services.AddHttpClient<IPagoService, PagoIntegracionService>(c => c.BaseAddress = new Uri(integracionBaseUrl)).AddHttpMessageHandler<GatewayHeadersHandler>();
}

if (usarMockResultados)
{
    builder.Services.AddScoped<IResultadosService, ResultadosMockService>();
}
else
{
    builder.Services.AddHttpClient<IResultadosService, ResultadosIntegracionService>(c => c.BaseAddress = new Uri(integracionBaseUrl)).AddHttpMessageHandler<GatewayHeadersHandler>();
}

if (usarMockConstancias)
{
    // Singleton: las constancias generadas en la demo se conservan
    // mientras la app esté corriendo.
    builder.Services.AddSingleton<IConstanciaService, ConstanciaMockService>();
}
else
{
    builder.Services.AddHttpClient<IConstanciaService, ConstanciaIntegracionService>(c => c.BaseAddress = new Uri(integracionBaseUrl)).AddHttpMessageHandler<GatewayHeadersHandler>();
}

// Pasarela de pago (simulada). Si Ricardo la expone en el Gateway, aquí se
// registra su implementación real en vez de esta.
builder.Services.AddScoped<IPasarelaService, PasarelaMockService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Cuenta/Login");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// Fechas y montos en español dominicano ("viernes 17 de octubre", RD$ 250.00),
// sin importar el idioma del servidor donde se ejecute.
var culturaDominicana = new CultureInfo("es-DO");
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture(culturaDominicana),
    SupportedCultures = new[] { culturaDominicana },
    SupportedUICultures = new[] { culturaDominicana },
});
app.UseRouting();
app.UseSession();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Principal}/{action=Index}/{id?}");

app.Run();
