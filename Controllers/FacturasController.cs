using Microsoft.AspNetCore.Mvc;
using PortalCitas.Models;
using PortalCitas.Services;

namespace PortalCitas.Controllers;

public class FacturasController : PacienteControllerBase
{
    private readonly IPagoService _pagoService;
    private readonly IPasarelaService _pasarela;
    private readonly IConstanciaService _constancias;

    public FacturasController(IPagoService pagoService, IPasarelaService pasarela, IConstanciaService constancias)
    {
        _pagoService = pagoService;
        _pasarela = pasarela;
        _constancias = constancias;
    }

    // GET /Facturas
    public async Task<IActionResult> Index()
    {
        var facturas = await _pagoService.ObtenerFacturasPacienteAsync(PacienteId);
        ViewBag.Mensaje = TempData["Mensaje"];
        return View(facturas);
    }

    // GET /Facturas/Pagar/5
    public async Task<IActionResult> Pagar(int id)
    {
        var factura = await _pagoService.ObtenerFacturaAsync(PacienteId, id);
        if (factura is null)
        {
            return NotFound();
        }

        if (factura.Estado == "Pagada")
        {
            return RedirectToAction(nameof(Comprobante), new { id });
        }

        return View(new PagarFacturaViewModel { Factura = factura });
    }

    // POST /Facturas/Pagar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pagar(int id, DatosPagoInput datosPago)
    {
        var factura = await _pagoService.ObtenerFacturaAsync(PacienteId, id);
        if (factura is null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(new PagarFacturaViewModel { Factura = factura, DatosPago = datosPago });
        }

        // 1) La pasarela autoriza el cobro con los datos de la tarjeta.
        //    Esos datos mueren aquí: no se guardan, no se registran en logs
        //    y no se reenvían al CORE.
        var autorizacion = await _pasarela.AutorizarAsync(datosPago, factura.Monto);
        if (!autorizacion.Exitoso)
        {
            ModelState.AddModelError(string.Empty, autorizacion.MensajeError ?? "La pasarela rechazó el pago.");
            return View(new PagarFacturaViewModel { Factura = factura, DatosPago = new DatosPagoInput() });
        }

        // 2) Al CORE solo le llega la referencia de la pasarela.
        var resultado = await _pagoService.PagarFacturaAsync(PacienteId, id, autorizacion.Referencia!);

        if (!resultado.Exitoso)
        {
            ModelState.AddModelError(string.Empty, resultado.MensajeError ?? "No se pudo procesar el pago.");
            return View(new PagarFacturaViewModel { Factura = factura, DatosPago = datosPago });
        }

        // La transacción ya tiene su recibo y factura (RN-05); además queda
        // registrada en "Mis constancias" junto con lo demás que hizo el paciente.
        if (resultado.Factura != null)
        {
            await _constancias.RegistrarAsync(
                FabricaConstancias.PagoRealizado(PacienteId, NombrePaciente, resultado.Factura));
        }

        TempData["Mensaje"] = "Pago procesado. Tu recibo y factura ya están disponibles.";
        return RedirectToAction(nameof(Comprobante), new { id });
    }

    // GET /Facturas/Comprobante/5
    public async Task<IActionResult> Comprobante(int id)
    {
        var factura = await _pagoService.ObtenerFacturaAsync(PacienteId, id);
        if (factura is null || factura.Estado != "Pagada")
        {
            return NotFound();
        }

        return View(factura);
    }

    // GET /Facturas/ComprobantePdf/5  → descarga el recibo y factura en PDF
    public async Task<IActionResult> ComprobantePdf(int id)
    {
        var factura = await _pagoService.ObtenerFacturaAsync(PacienteId, id);
        if (factura is null || factura.Estado != "Pagada")
        {
            return NotFound();
        }

        var bytes = FabricaConstancias.PdfComprobante(factura, NombrePaciente, PacienteId);
        return File(bytes, "application/pdf", $"Comprobante-{factura.NumeroRecibo}.pdf");
    }
}
