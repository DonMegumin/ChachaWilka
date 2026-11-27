using System.Security.Claims;
using ChachaWilka.Data;
using ChachaWilka.Models;
using ChachaWilka.Services;
using ChachaWilka.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChachaWilka.Controllers;

[Authorize]
public class PedidosController(AppDbContext db, PedidosTienda pedidos) : Controller
{
    private int Actor => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    private bool Personal => User.IsInRole(RolesTienda.Empleado) || User.IsInRole(RolesTienda.Administrador);

    [Authorize(Roles = RolesTienda.Cliente)]
    public async Task<IActionResult> Index() => View(await db.pedidos.AsNoTracking().Where(p => p.Id_Usuario == Actor)
        .OrderByDescending(p => p.Id_Pedido).Select(p => new PedidoResumen(p.Id_Pedido, p.Codigo, p.Fecha, p.Total, p.Situacion, null)).ToListAsync());

    [HttpGet]
    public async Task<IActionResult> Detalle(int id)
    {
        var pedido = await db.pedidos.AsNoTracking().Where(p => p.Id_Pedido == id && (Personal || p.Id_Usuario == Actor))
            .Include(p => p.Usuarios).Include(p => p.DetallePedidos).ThenInclude(d => d.Productos)
            .Include(p => p.Historial).Include(p => p.Consultas).AsSplitQuery().SingleOrDefaultAsync();
        if (pedido is null)
            return NotFound();
        var pagos = await db.pagos.AsNoTracking().Where(p => p.Id_Pedido == id).OrderByDescending(p => p.Id_Pago)
            .Select(p => new IntentoPago(p.Id_Pago, p.Referencia, p.FechaPago, p.Monto, p.Situacion, p.Evidencia != null, p.Motivo)).ToListAsync();
        return View(new DetallePedidoVM(pedido, await db.Ajustes.AsNoTracking().SingleOrDefaultAsync() ?? new(), pagos, Personal));
    }

    [Authorize(Roles = RolesTienda.Cliente), HttpPost]
    [RequestSizeLimit(ArchivosPago.Maximo + 65_536)]
    public async Task<IActionResult> Pago(int id, PagoVM modelo)
    {
        if (!ModelState.IsValid || modelo.Monto is not decimal monto || decimal.Round(monto, 2) != monto)
            return ErrorPedido(id, "Revisa el importe, la fecha y el código ingresados.");
        try
        {
            var (archivo, tipo) = await ArchivosPago.LeerAsync(modelo.Evidencia, HttpContext.RequestAborted);
            await pedidos.PresentarPagoAsync(id, Actor, modelo.Referencia, modelo.Fecha!.Value, monto, archivo, tipo);
            TempData["Aviso"] = "Comprobante enviado. Te avisaremos del resultado en este pedido.";
            return RedirectToAction(nameof(Detalle), new { id });
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ReglaTiendaException ex) { return ErrorPedido(id, ex.Message); }
    }

    [HttpGet]
    public async Task<IActionResult> Evidencia(int id)
    {
        var pago = await db.pagos.AsNoTracking().Where(p => p.Id_Pago == id && (Personal || p.Pedidos.Id_Usuario == Actor))
            .Select(p => new { p.Evidencia, p.TipoArchivo }).SingleOrDefaultAsync();
        if (pago?.Evidencia is null)
            return NotFound();
        var extension = pago.TipoArchivo switch { "image/png" => "png", "image/jpeg" => "jpg", "application/pdf" => "pdf", _ => "bin" };
        Response.Headers.CacheControl = "no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(pago.Evidencia, "application/octet-stream", "comprobante-" + id + "." + extension);
    }

    [Authorize(Roles = RolesTienda.Cliente), HttpPost]
    public async Task<IActionResult> Consultar(int id, string? pregunta)
    {
        if (!await db.pedidos.AnyAsync(p => p.Id_Pedido == id && p.Id_Usuario == Actor))
            return NotFound();
        if (string.IsNullOrWhiteSpace(pregunta) || pregunta.Trim().Length > 1000)
            return ErrorPedido(id, "Escribe una consulta de hasta 1000 caracteres.");
        db.Consultas.Add(new ConsultaPedido { PedidoId = id, Pregunta = pregunta.Trim() });
        await db.SaveChangesAsync();
        TempData["Aviso"] = "Consulta enviada.";
        return RedirectToAction(nameof(Detalle), null, new { id }, "consultas");
    }

    private IActionResult ErrorPedido(int id, string mensaje)
    {
        TempData["Error"] = mensaje;
        return RedirectToAction(nameof(Detalle), new { id });
    }
}
