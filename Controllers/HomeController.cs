using System.Diagnostics;
using ChachaWilka.Data;
using ChachaWilka.Models;
using ChachaWilka.Services;
using ChachaWilka.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ChachaWilka.Controllers;

public class HomeController(AppDbContext db, ILogger<HomeController> logger) : Controller
{
    public async Task<IActionResult> Index(string? categoria, bool soloColeccion = false)
    {
        categoria = categoria is CatalogoMarca.Cremas or CatalogoMarca.Licores ? categoria : null;
        PortadaVM portada;
        try
        {
            var productos = db.productos.AsNoTracking().Include(p => p.Categorias).Where(p => p.Activo && p.Categorias.Activa);
            if (categoria != null)
                productos = productos.Where(p => p.Categorias.Nombre_Categoria == categoria);
            portada = new PortadaVM(await productos.OrderBy(p => p.Id_Producto).ToListAsync(),
                await db.Ajustes.AsNoTracking().SingleOrDefaultAsync() ?? new(), categoria, true);
        }
        catch (SqlException ex)
        {
            logger.LogWarning("Catálogo de consulta activo por falta de SQL Server ({Codigo}).", ex.Number);
            var catalogo = CatalogoMarca.Productos().Where(p => categoria == null || p.Categoria == categoria).ToList();
            portada = new PortadaVM(catalogo, new(), categoria, false);
        }
        return soloColeccion ? PartialView("_Coleccion", portada) : View(portada);
    }

    public IActionResult Privacy() => View();

    public IActionResult Estado(int codigo)
    {
        ViewData["Codigo"] = codigo;
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
