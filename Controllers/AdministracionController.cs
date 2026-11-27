using System.Data;
using System.Security.Claims;
using ChachaWilka.Data;
using ChachaWilka.Models;
using ChachaWilka.Services;
using ChachaWilka.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ChachaWilka.Controllers;

[Authorize(Roles = RolesTienda.Administrador)]
public class AdministracionController(AppDbContext db, IPasswordHasher<Usuario> hasher, PedidosTienda pedidos, IWebHostEnvironment entorno) : Controller
{
    private int Actor => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    public IActionResult Index() => View();

    public async Task<IActionResult> Catalogo()
    {
        ViewData["Categorias"] = await db.categorias.AsNoTracking().OrderBy(c => c.Nombre_Categoria).ToListAsync();
        return View(await db.productos.AsNoTracking().Include(p => p.Categorias).OrderBy(p => p.Nombre).ToListAsync());
    }

    [HttpGet]
    public async Task<IActionResult> Producto(int? id)
    {
        if (id is null)
            return await VistaProducto(new ProductoVM());

        var producto = await db.productos.FindAsync(id);
        if (producto is null)
            return NotFound();
        return await VistaProducto(new ProductoVM
        {
            Nombre = producto.Nombre, Descripcion = producto.Descripcion, CategoriaId = producto.Id_Categoria,
            Precio = producto.Precio, Activo = producto.Activo, Imagen = producto.Imagen, Version = Convert.ToBase64String(producto.Version)
        }, id);
    }

    [HttpPost]
    public async Task<IActionResult> Producto(int? id, ProductoVM modelo)
    {
        if (!await db.categorias.AnyAsync(c => c.Id_Categoria == modelo.CategoriaId))
            ModelState.AddModelError(nameof(modelo.CategoriaId), "Elige una categoría válida.");
        if (decimal.Round(modelo.Precio, 2) != modelo.Precio)
            ModelState.AddModelError(nameof(modelo.Precio), "El precio admite hasta dos decimales.");
        var imagen = modelo.Imagen?.Trim() ?? "";
        if (imagen != "" && (!System.Text.RegularExpressions.Regex.IsMatch(imagen, @"^[a-z0-9-]+\.(jpg|jpeg|png|webp)$") ||
            !System.IO.File.Exists(Path.Combine(entorno.WebRootPath, "images", "chachawillka", imagen))))
            ModelState.AddModelError(nameof(modelo.Imagen), "Elige una imagen existente de la marca.");
        if (!ModelState.IsValid)
            return await VistaProducto(modelo, id);

        var producto = id is null ? new Producto() : await db.productos.FindAsync(id);
        if (producto is null)
            return NotFound();
        if (id is not null && modelo.Version != Convert.ToBase64String(producto.Version))
            return Mensaje(nameof(Catalogo), "El producto cambió. Vuelve a abrirlo antes de editar.", true);

        var categoria = await db.categorias.FindAsync(modelo.CategoriaId);
        producto.Nombre = modelo.Nombre.Trim();
        producto.Descripcion = modelo.Descripcion.Trim();
        producto.Categoria = categoria!.Nombre_Categoria;
        producto.Id_Categoria = categoria.Id_Categoria;
        producto.Precio = modelo.Precio;
        producto.Imagen = imagen;
        producto.Activo = modelo.Activo;
        if (id is null)
            db.productos.Add(producto);

        return await GuardarAsync(nameof(Catalogo), "Producto guardado.");
    }

    [HttpPost]
    public async Task<IActionResult> Categoria(int? id, string? nombre, bool activa)
    {
        if (string.IsNullOrWhiteSpace(nombre) || nombre.Trim().Length > 50)
            return Mensaje(nameof(Catalogo), "Ingresa un nombre de categoría válido.", true);
        nombre = nombre.Trim();
        if (await db.categorias.AnyAsync(c => c.Nombre_Categoria == nombre && c.Id_Categoria != id))
            return Mensaje(nameof(Catalogo), "Ya existe una categoría con ese nombre.", true);

        var categoria = id is null ? new Categoria() : await db.categorias.FindAsync(id);
        if (categoria is null)
            return NotFound();
        if (categoria.Nombre_Categoria is CatalogoMarca.Cremas or CatalogoMarca.Licores && nombre != categoria.Nombre_Categoria)
            return Mensaje(nameof(Catalogo), "Las categorías que definen las cajas conservan su nombre.", true);
        categoria.Nombre_Categoria = nombre;
        categoria.Activa = activa;
        if (id is null)
            db.categorias.Add(categoria);
        return await GuardarAsync(nameof(Catalogo), "Categoría guardada.");
    }

    [HttpPost]
    public async Task<IActionResult> Stock(int id, int cantidad, string? motivo)
    {
        if (cantidad == 0 || cantidad is < -100000 or > 100000 || string.IsNullOrWhiteSpace(motivo) || motivo.Trim().Length > 200)
            return Mensaje(nameof(Catalogo), "Indica una cantidad distinta de cero y un motivo de hasta 200 caracteres.", true);
        var producto = await db.productos.FindAsync(id);
        if (producto is null)
            return NotFound();
        if ((long)producto.Stock + cantidad < producto.Reservado || (long)producto.Stock + cantidad > 1_000_000)
            return Mensaje(nameof(Catalogo), "El ajuste debe conservar las reservas y no superar el límite de existencias.", true);
        producto.Stock += cantidad;
        pedidos.Movimiento(id, null, Actor, cantidad, 0, motivo.Trim());
        return await GuardarAsync(nameof(Catalogo), "Existencias actualizadas.");
    }

    public async Task<IActionResult> Usuarios() => View(await db.usuarios.AsNoTracking().Include(u => u.Rol)
        .OrderBy(u => u.Nombre).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> CrearPersonal(CuentaPersonalVM modelo)
    {
        if (!ModelState.IsValid || modelo.Rol is not (RolesTienda.Empleado or RolesTienda.Administrador))
            return Mensaje(nameof(Usuarios), "Revisa el nombre, el correo, el rol y que la clave tenga al menos 12 caracteres.", true);

        var correo = modelo.Correo.Trim();
        var normalizado = correo.ToUpperInvariant();
        if (await db.usuarios.AnyAsync(u => u.TieneHash && u.CorreoNormalizado == normalizado))
            return Mensaje(nameof(Usuarios), "El correo ya está registrado.", true);
        var rol = await db.roles.FirstOrDefaultAsync(r => r.Nombre_Rol == modelo.Rol);
        if (rol is null)
            return Mensaje(nameof(Usuarios), "Inicializa los roles antes de crear personal.", true);
        var cuenta = new Usuario
        {
            Nombre = modelo.Nombre.Trim(), Correo = correo, CorreoNormalizado = normalizado, Id_Rol = rol.Id_Rol, TieneHash = true
        };
        cuenta.Contraseña = hasher.HashPassword(cuenta, modelo.Clave);
        await using var transaccion = await db.Database.BeginTransactionAsync();
        try
        {
            db.usuarios.Add(cuenta);
            await db.SaveChangesAsync();
            db.CambiosCuenta.Add(new CambioCuenta { ActorId = Actor, CuentaId = cuenta.Id_Usuario, Accion = "Alta de " + rol.Nombre_Rol });
            await db.SaveChangesAsync();
            await transaccion.CommitAsync();
            return Mensaje(nameof(Usuarios), "Cuenta creada.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Mensaje(nameof(Usuarios), "El correo ya está registrado.", true);
        }
    }

    [HttpPost]
    public async Task<IActionResult> RecuperarUsuario(int id, string? clave)
    {
        if (clave is null || clave.Length is < 12 or > 128)
            return Mensaje(nameof(Usuarios), "La contraseña inicial debe tener entre 12 y 128 caracteres.", true);
        var usuario = await db.usuarios.SingleOrDefaultAsync(u => u.Id_Usuario == id && !u.TieneHash);
        if (usuario is null)
            return NotFound();
        if (await db.usuarios.AnyAsync(u => u.TieneHash && u.CorreoNormalizado == usuario.CorreoNormalizado))
            return Mensaje(nameof(Usuarios), "Ya existe una cuenta habilitada con este correo.", true);
        usuario.Id_Rol = await db.roles.Where(r => r.Nombre_Rol == RolesTienda.Cliente).Select(r => r.Id_Rol).FirstAsync();
        usuario.Contraseña = hasher.HashPassword(usuario, clave);
        usuario.Activo = true;
        usuario.TieneHash = true;
        db.CambiosCuenta.Add(new CambioCuenta { ActorId = Actor, CuentaId = id, Accion = "Recuperación de cuenta histórica como Cliente" });
        return await GuardarAsync(nameof(Usuarios), "Cuenta recuperada. El cliente puede volver a ingresar y completar sus datos.");
    }

    [HttpPost]
    public async Task<IActionResult> CambiarCuenta(int id, string rol, bool activo)
    {
        if (rol is not (RolesTienda.Cliente or RolesTienda.Empleado or RolesTienda.Administrador))
            return Mensaje(nameof(Usuarios), "Elige un rol válido.", true);
        await using var transaccion = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable);
        try
        {
            var cuenta = await db.usuarios.Include(u => u.Rol).SingleOrDefaultAsync(u => u.Id_Usuario == id);
            if (cuenta is null || !cuenta.TieneHash)
                return NotFound();
            if (cuenta.Activo && cuenta.Rol.Nombre_Rol == RolesTienda.Administrador && (!activo || rol != RolesTienda.Administrador) &&
                !await db.usuarios.AnyAsync(u => u.Id_Usuario != id && u.TieneHash && u.Activo && u.Rol.Nombre_Rol == RolesTienda.Administrador))
                return Mensaje(nameof(Usuarios), "Debe quedar al menos un administrador activo.", true);
            if (rol == RolesTienda.Cliente && (!cuenta.MayorDeEdad || cuenta.Consentimiento is null))
                return Mensaje(nameof(Usuarios), "Esa cuenta aún no tiene la declaración y el consentimiento de cliente.", true);
            var nuevoRol = await db.roles.FirstOrDefaultAsync(r => r.Nombre_Rol == rol);
            if (nuevoRol is null)
                return NotFound();
            cuenta.Id_Rol = nuevoRol.Id_Rol;
            cuenta.Activo = activo;
            db.CambiosCuenta.Add(new CambioCuenta
            {
                ActorId = Actor, CuentaId = id, Accion = rol + (activo ? " (activo)" : " (inactivo)")
            });
            await db.SaveChangesAsync();
            await transaccion.CommitAsync();
            return Mensaje(nameof(Usuarios), "Permisos guardados.");
        }
        catch (DbUpdateConcurrencyException)
        {
            return Mensaje(nameof(Usuarios), "Otra persona actualizó la cuenta. Revisa los permisos actuales.", true);
        }
        catch (Exception ex) when (ex is SqlException { Number: 1205 } || ex.InnerException is SqlException { Number: 1205 })
        {
            return Mensaje(nameof(Usuarios), "Otra persona estaba cambiando permisos. Vuelve a intentarlo.", true);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Ajustes() => View(await db.Ajustes.AsNoTracking().SingleOrDefaultAsync() ?? new AjustesTienda());

    [HttpPost]
    public async Task<IActionResult> Ajustes(AjustesTienda modelo, string? versionActual)
    {
        ModelState.Remove(nameof(AjustesTienda.Version));
        foreach (var campo in new[] { nameof(AjustesTienda.Yape), nameof(AjustesTienda.TitularYape), nameof(AjustesTienda.Horario), nameof(AjustesTienda.WhatsApp) })
        {
            if (ModelState.TryGetValue(campo, out var valor) && string.IsNullOrWhiteSpace(valor.AttemptedValue))
                ModelState.Remove(campo);
        }
        if ((!string.IsNullOrWhiteSpace(modelo.Yape) && (modelo.Yape.Length != 9 || !modelo.Yape.All(char.IsAsciiDigit))) ||
            (!string.IsNullOrWhiteSpace(modelo.WhatsApp) && (modelo.WhatsApp.Length < 8 || !modelo.WhatsApp.All(char.IsAsciiDigit))))
            ModelState.AddModelError("", "Usa 9 dígitos para Yape y solo dígitos con código de país para WhatsApp.");
        if (string.IsNullOrWhiteSpace(modelo.Direccion))
            ModelState.AddModelError(nameof(modelo.Direccion), "Ingresa la dirección de recojo.");
        if (decimal.Round(modelo.PrecioCombo, 2) != modelo.PrecioCombo || decimal.Round(modelo.PrecioComboCremas, 2) != modelo.PrecioComboCremas)
            ModelState.AddModelError("", "Los precios admiten hasta dos decimales.");
        if (!ModelState.IsValid)
        {
            ViewData["VersionActual"] = versionActual;
            return View(modelo);
        }

        var ajustes = await db.Ajustes.FindAsync(1);
        if (ajustes is null)
        {
            modelo.Id = 1;
            modelo.Yape ??= "";
            modelo.TitularYape ??= "";
            modelo.WhatsApp ??= "";
            modelo.Horario ??= "";
            db.Ajustes.Add(modelo);
        }
        else
        {
            if (versionActual != Convert.ToBase64String(ajustes.Version))
                return Mensaje(nameof(Ajustes), "Otra persona actualizó los datos. Revisa la versión actual.", true);
            ajustes.Yape = modelo.Yape ?? "";
            ajustes.TitularYape = modelo.TitularYape ?? "";
            ajustes.WhatsApp = modelo.WhatsApp ?? "";
            ajustes.Horario = modelo.Horario ?? "";
            ajustes.Direccion = modelo.Direccion.Trim();
            ajustes.PrecioCombo = modelo.PrecioCombo;
            ajustes.PrecioComboCremas = modelo.PrecioComboCremas;
            ajustes.PermitirRepeticiones = modelo.PermitirRepeticiones;
        }
        return await GuardarAsync(nameof(Ajustes), "Ajustes guardados.");
    }

    public async Task<IActionResult> Reportes(DateOnly? desde, DateOnly? hasta)
    {
        var fin = hasta ?? RelojTienda.Hoy;
        var inicio = desde ?? fin.AddDays(-29);
        if (inicio > fin || inicio.Year < 2000 || fin.Year > 2100 || fin.DayNumber - inicio.DayNumber > 366)
            return Mensaje(nameof(Reportes), "Selecciona un período válido de hasta un año.", true);
        var inicioLima = RelojTienda.Medianoche(inicio);
        var finLima = RelojTienda.Medianoche(fin.AddDays(1));
        var cobrado = await db.pagos.Where(p => p.Situacion == EstadoPago.Verificado && p.Revisado >= inicioLima && p.Revisado < finLima)
            .SumAsync(p => p.Monto);
        var vendido = await db.pedidos.Where(p => p.Situacion == EstadoPedido.Retirado && p.Retirado >= inicioLima && p.Retirado < finLima)
            .SumAsync(p => p.Total);
        var estados = await db.pedidos.Where(p => p.Fecha >= inicio && p.Fecha <= fin)
            .GroupBy(p => p.Situacion).Select(g => new ConteoEstado(g.Key, g.Count())).ToListAsync();
        var inventario = await db.productos.AsNoTracking().OrderBy(p => p.Nombre).ToListAsync();
        var movimientos = await db.Movimientos.AsNoTracking().Where(m => m.Fecha >= inicioLima && m.Fecha < finLima)
            .Include(m => m.Producto).OrderByDescending(m => m.Fecha).Take(100).ToListAsync();
        return View(new ReportesVM(inicio, fin, cobrado, vendido, estados, inventario, movimientos));
    }

    private async Task<IActionResult> VistaProducto(ProductoVM modelo, int? id = null)
    {
        ViewData["Id"] = id;
        ViewData["Categorias"] = await db.categorias.AsNoTracking().OrderBy(c => c.Nombre_Categoria).ToListAsync();
        ViewData["Imagenes"] = Directory.GetFiles(Path.Combine(entorno.WebRootPath, "images", "chachawillka"))
            .Where(ruta => Path.GetExtension(ruta) is ".jpg" or ".jpeg" or ".png" or ".webp")
            .Select(ruta => Path.GetFileName(ruta)).Order().ToList();
        return View("Producto", modelo);
    }

    private IActionResult Mensaje(string accion, string mensaje, bool error = false)
    {
        TempData[error ? "Error" : "Aviso"] = mensaje;
        return RedirectToAction(accion);
    }

    private async Task<IActionResult> GuardarAsync(string accion, string mensaje)
    {
        try
        {
            await db.SaveChangesAsync();
            return Mensaje(accion, mensaje);
        }
        catch (DbUpdateConcurrencyException)
        {
            return Mensaje(accion, "Alguien actualizó esa información. Vuelve a intentarlo.", true);
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Mensaje(accion, "Ya existe un registro con esos datos.", true);
        }
    }
}
