using System.Security.Claims;
using ChachaWilka.Data;
using ChachaWilka.Models;
using ChachaWilka.Services;
using ChachaWilka.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ChachaWilka.Controllers;

public class CestaController(AppDbContext db, CarritoTienda carrito, PedidosTienda pedidos) : Controller
{
    public async Task<IActionResult> Index()
    {
        var seleccion = carrito.Leer(HttpContext.Session);
        return View(new CestaVM(await carrito.CalcularAsync(seleccion),
            await db.Ajustes.AsNoTracking().SingleOrDefaultAsync() ?? new()));
    }

    public async Task<IActionResult> Combo(string? categoria)
    {
        categoria = categoria == CatalogoMarca.Cremas ? categoria : CatalogoMarca.Licores;
        var ajustes = await db.Ajustes.AsNoTracking().SingleOrDefaultAsync() ?? new();
        var opcion = await db.productos.AsNoTracking().Where(p => p.Activo && p.Categorias.Activa &&
            p.Categorias.Nombre_Categoria == categoria && p.Stock > p.Reservado && p.Precio > 0).OrderBy(p => p.Nombre).ToListAsync();
        return View(new ComboVM(categoria, opcion, CarritoTienda.PrecioCombo(ajustes, categoria), ajustes.PermitirRepeticiones));
    }

    [HttpPost]
    public async Task<IActionResult> Agregar(int id, string? categoria)
    {
        var seleccion = carrito.Leer(HttpContext.Session);
        var nuevaCantidad = seleccion.Botellas.GetValueOrDefault(id) + 1;
        if (nuevaCantidad > 99 || seleccion.Botellas.Count >= 40 && !seleccion.Botellas.ContainsKey(id))
            return Aviso("Llegaste al límite de unidades de la cesta.", () => Coleccion(categoria));
        seleccion.Botellas[id] = nuevaCantidad;
        var calculado = await carrito.CalcularAsync(seleccion);
        if (calculado.Errores.Count > 0)
            return Aviso(calculado.Errores[0], () => Coleccion(categoria));
        carrito.Guardar(HttpContext.Session, seleccion);
        TempData["Aviso"] = "Añadido a tu cesta.";
        return Coleccion(categoria);
    }

    [HttpPost]
    public async Task<IActionResult> AgregarCombo(string categoria, int[] seleccion)
    {
        var cesta = carrito.Leer(HttpContext.Session);
        if (seleccion.Length != 4 || cesta.Combos.Count >= 10 || categoria is not (CatalogoMarca.Cremas or CatalogoMarca.Licores))
            return Aviso("Selecciona cuatro sabores de la misma colección.", () => RedirectToAction(nameof(Combo), new { categoria }));
        cesta.Combos.Add(new SeleccionCombo(Guid.NewGuid(), categoria, seleccion));
        var calculado = await carrito.CalcularAsync(cesta);
        if (calculado.Errores.Count > 0)
            return Aviso(calculado.Errores[0], () => RedirectToAction(nameof(Combo), new { categoria }));
        carrito.Guardar(HttpContext.Session, cesta);
        TempData["Aviso"] = "La caja está en tu cesta.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Cambiar(int id, int cantidad)
    {
        var seleccion = carrito.Leer(HttpContext.Session);
        if (!seleccion.Botellas.ContainsKey(id) || cantidad is < 1 or > 99)
            return Aviso("Ingresa una cantidad entre 1 y 99.", () => RedirectToAction(nameof(Index)));
        var producto = await db.productos.FindAsync(id);
        if (producto is null || cantidad > producto.Disponible)
            return Aviso("Esa cantidad supera el stock disponible.", () => RedirectToAction(nameof(Index)));
        seleccion.Botellas[id] = cantidad;
        carrito.Guardar(HttpContext.Session, seleccion);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult Quitar(int? id, Guid? combo)
    {
        var seleccion = carrito.Leer(HttpContext.Session);
        if (id is int producto)
            seleccion.Botellas.Remove(producto);
        if (combo is Guid paquete)
            seleccion.Combos.RemoveAll(c => c.Id == paquete);
        carrito.Guardar(HttpContext.Session, seleccion);
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = RolesTienda.Cliente), HttpPost]
    public async Task<IActionResult> Confirmar(Guid clave, long totalCentimos)
    {
        var cliente = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        if (await pedidos.ConfirmadoAsync(cliente, clave) is { } anterior)
            return RedirectToAction("Detalle", "Pedidos", new { id = anterior });

        var seleccion = carrito.Leer(HttpContext.Session);
        if (seleccion.Confirmacion != clave)
            return Aviso("Tu cesta cambió. Revisa el contenido antes de confirmar.", () => RedirectToAction(nameof(Index)));
        try
        {
            var id = await pedidos.CrearAsync(seleccion, cliente, totalCentimos / 100m);
            carrito.Vaciar(HttpContext.Session);
            return RedirectToAction("Detalle", "Pedidos", new { id });
        }
        catch (ReglaTiendaException ex)
        {
            return Aviso(ex.Message, () => RedirectToAction(nameof(Index)));
        }
    }

    private IActionResult Coleccion(string? categoria) =>
        RedirectToAction("Index", "Home", new { categoria }, "coleccion");

    private IActionResult Aviso(string mensaje, Func<IActionResult> destino)
    {
        TempData["Error"] = mensaje;
        return destino();
    }
}
