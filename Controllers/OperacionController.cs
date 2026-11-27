using System.Security.Claims;
using ChachaWilka.Data;
using ChachaWilka.Models;
using ChachaWilka.Services;
using ChachaWilka.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChachaWilka.Controllers;

[Authorize(Roles = RolesTienda.Personal)]
public class OperacionController(AppDbContext db, PedidosTienda pedidos) : Controller
{
    private int Actor => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public async Task<IActionResult> Index(EstadoPedido? estado)
    {
        var consulta = db.pedidos.AsNoTracking();
        if (estado != null)
            consulta = consulta.Where(p => p.Situacion == estado);
        ViewData["Estado"] = estado;
        ViewData["Consultas"] = await db.Consultas.AsNoTracking().Where(c => c.Respondida == null).OrderBy(c => c.Creada)
            .Take(20).Select(c => new { c.PedidoId, c.Pedido.Codigo }).Select(c => new KeyValuePair<int, string>(c.PedidoId, c.Codigo ?? "Pedido")).ToListAsync();
        return View(await consulta.OrderByDescending(p => p.Id_Pedido).Take(100)
            .Select(p => new PedidoResumen(p.Id_Pedido, p.Codigo, p.Fecha, p.Total, p.Situacion, p.Usuarios.Nombre)).ToListAsync());
    }

    [HttpPost]
    public Task<IActionResult> Revisar(int id, int pago, bool aprobar, bool recepcionConfirmada, string? motivo) =>
        Cambiar(id, () =>
        {
            if (aprobar && !recepcionConfirmada)
                throw new ReglaTiendaException("Confirma que contrastaste el importe y la operación directamente en Yape.");
            return pedidos.RevisarPagoAsync(id, pago, Actor, aprobar, TextoMotivo(motivo));
        });

    [HttpPost]
    public Task<IActionResult> Avanzar(int id, EstadoPedido estado, bool identidadVerificada, string? motivo) =>
        Cambiar(id, () => pedidos.CambiarEstadoAsync(id, Actor, estado, identidadVerificada, TextoMotivo(motivo)));

    [HttpPost]
    public Task<IActionResult> Responder(int id, int consulta, string? respuesta) => Cambiar(id, async () =>
    {
        if (string.IsNullOrWhiteSpace(respuesta) || respuesta.Trim().Length > 1000)
            throw new ReglaTiendaException("Escribe una respuesta de hasta 1000 caracteres.");
        var entrada = await db.Consultas.SingleOrDefaultAsync(c => c.Id == consulta && c.PedidoId == id)
            ?? throw new KeyNotFoundException();
        if (entrada.Respondida != null)
            throw new ReglaTiendaException("La consulta ya fue atendida.");
        entrada.Respuesta = respuesta.Trim();
        entrada.Respondida = DateTimeOffset.UtcNow;
        entrada.ResponsableId = Actor;
        await db.SaveChangesAsync();
    });

    private static string? TextoMotivo(string? motivo)
    {
        if (motivo?.Trim().Length > 200)
            throw new ReglaTiendaException("El motivo admite hasta 200 caracteres.");
        return motivo;
    }

    private async Task<IActionResult> Cambiar(int id, Func<Task> operacion)
    {
        try
        {
            await operacion();
            TempData["Aviso"] = "Cambio registrado.";
        }
        catch (KeyNotFoundException) { return NotFound(); }
        catch (ReglaTiendaException ex) { TempData["Error"] = ex.Message; }
        catch (DbUpdateConcurrencyException) { TempData["Error"] = "Otro usuario actualizó la información. Revisa el estado actual."; }
        return RedirectToAction("Detalle", "Pedidos", new { id });
    }
}
