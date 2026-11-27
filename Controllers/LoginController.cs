using System.Security.Claims;
using ChachaWilka.Data;
using ChachaWilka.Models;
using ChachaWilka.Services;
using ChachaWilka.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ChachaWilka.Controllers;

public class LoginController(AppDbContext db, IPasswordHasher<Usuario> hasher, ILogger<LoginController> logger) : Controller
{
    [HttpGet]
    public IActionResult Registro() => View(new UsuarioVM());

    [HttpPost, EnableRateLimiting("acceso")]
    public async Task<IActionResult> Registro(UsuarioVM modelo)
    {
        var hoy = RelojTienda.Hoy;
        if (modelo.Nacimiento is { } fecha && (fecha > hoy.AddYears(-18) || fecha < hoy.AddYears(-125)))
            ModelState.AddModelError(nameof(modelo.Nacimiento), "Debes tener al menos 18 años. Revisa la fecha ingresada.");
        if (!modelo.Consentimiento)
            ModelState.AddModelError(nameof(modelo.Consentimiento), "Acepta el tratamiento de tus datos para crear la cuenta.");
        if (!ModelState.IsValid)
            return View(modelo);

        var correo = modelo.Correo.Trim();
        var normalizado = correo.ToUpperInvariant();
        try
        {
            var rol = await db.roles.FirstOrDefaultAsync(r => r.Nombre_Rol == RolesTienda.Cliente);
            if (rol is null)
                return AccesoNoDisponible(nameof(Registro), modelo);
            if (await db.usuarios.AnyAsync(u => u.CorreoNormalizado == normalizado))
            {
                ModelState.AddModelError(nameof(modelo.Correo), "Ya existe una cuenta con ese correo.");
                return View(modelo);
            }

            var usuario = new Usuario
            {
                Nombre = modelo.Nombre.Trim(), Correo = correo, CorreoNormalizado = normalizado,
                Telefono = modelo.Telefono.Trim(), MayorDeEdad = true, Consentimiento = DateTimeOffset.UtcNow,
                Id_Rol = rol.Id_Rol, TieneHash = true
            };
            usuario.Contraseña = hasher.HashPassword(usuario, modelo.Contraseña);
            db.usuarios.Add(usuario);
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            ModelState.AddModelError(nameof(modelo.Correo), "Ya existe una cuenta con ese correo.");
            return View(modelo);
        }
        catch (SqlException ex)
        {
            logger.LogWarning(ex, "No se pudo abrir el registro de clientes.");
            return AccesoNoDisponible(nameof(Registro), modelo);
        }

        TempData["Aviso"] = "Tu cuenta está lista. Ingresa para continuar.";
        return RedirectToAction(nameof(Login));
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl) => View(new LoginVM { ReturnUrl = returnUrl });

    [HttpPost, EnableRateLimiting("acceso")]
    public async Task<IActionResult> Login(LoginVM modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        Usuario? usuario;
        try
        {
            var correo = modelo.Correo.Trim().ToUpperInvariant();
            usuario = await db.usuarios.Include(u => u.Rol)
                .SingleOrDefaultAsync(u => u.TieneHash && u.Activo && u.CorreoNormalizado == correo);
        }
        catch (SqlException ex)
        {
            logger.LogWarning(ex, "No se pudo abrir el acceso a clientes.");
            return AccesoNoDisponible(nameof(Login), modelo);
        }

        if (usuario is null ||
            hasher.VerifyHashedPassword(usuario, usuario.Contraseña, modelo.Contraseña) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("", "El correo o la contraseña no coinciden.");
            return View(modelo);
        }

        var identidad = new ClaimsIdentity(
        [
            new(ClaimTypes.NameIdentifier, usuario.Id_Usuario.ToString()),
            new(ClaimTypes.Name, usuario.Nombre),
            new(ClaimTypes.Role, usuario.Rol.Nombre_Rol),
            new("VersionCuenta", Convert.ToBase64String(usuario.Version))
        ], CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(identidad));

        return usuario.Rol.Nombre_Rol switch
        {
            RolesTienda.Administrador => RedirectToAction("Index", "Administracion"),
            RolesTienda.Empleado => RedirectToAction("Index", "Operacion"),
            _ when Url.IsLocalUrl(modelo.ReturnUrl) => LocalRedirect(modelo.ReturnUrl!),
            _ => RedirectToAction("Index", "Home")
        };
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }

    [Authorize, HttpGet]
    public async Task<IActionResult> Cuenta()
    {
        var id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var cuenta = await db.usuarios.Where(u => u.Id_Usuario == id).Select(u => new CuentaVM(u.Nombre, u.Correo, u.Telefono,
            u.Rol.Nombre_Rol == RolesTienda.Cliente && (!u.MayorDeEdad || u.Consentimiento == null))).SingleAsync();
        return View(cuenta);
    }

    [Authorize(Roles = RolesTienda.Cliente), HttpPost]
    public async Task<IActionResult> CompletarCuenta(DateOnly? nacimiento, string? telefono, bool consentimiento)
    {
        var hoy = RelojTienda.Hoy;
        if (nacimiento is null || nacimiento > hoy.AddYears(-18) || nacimiento < hoy.AddYears(-125) ||
            !consentimiento || string.IsNullOrWhiteSpace(telefono) || telefono.Trim().Length > 20 ||
            !new System.ComponentModel.DataAnnotations.PhoneAttribute().IsValid(telefono))
        {
            TempData["Error"] = "Comprueba la mayoría de edad, el teléfono y el consentimiento.";
            return RedirectToAction(nameof(Cuenta));
        }
        var usuario = await db.usuarios.FindAsync(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
        if (usuario is null)
            return NotFound();
        usuario.MayorDeEdad = true;
        usuario.Telefono = telefono.Trim();
        usuario.Consentimiento = DateTimeOffset.UtcNow;
        return await GuardarCuentaAsync("Datos confirmados. Ingresa nuevamente para hacer tu pedido.");
    }

    [Authorize, HttpPost, EnableRateLimiting("acceso")]
    public async Task<IActionResult> CambiarClave(string? actual, string? nueva, string? repetida)
    {
        var usuario = await db.usuarios.FindAsync(int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!));
        if (usuario is null)
            return NotFound();
        if (nueva is null || nueva.Length is < 12 or > 128 || nueva != repetida || actual is null || actual.Length > 128 ||
            hasher.VerifyHashedPassword(usuario, usuario.Contraseña, actual) == PasswordVerificationResult.Failed)
        {
            TempData["Error"] = "Verifica tu contraseña actual y que la nueva coincida y tenga entre 12 y 128 caracteres.";
            return RedirectToAction(nameof(Cuenta));
        }
        usuario.Contraseña = hasher.HashPassword(usuario, nueva);
        return await GuardarCuentaAsync("Contraseña actualizada. Ingresa con la nueva contraseña.");
    }

    [HttpGet]
    public IActionResult Denegado()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    private IActionResult AccesoNoDisponible(string vista, object modelo)
    {
        Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        ModelState.AddModelError("", "El acceso a cuentas está temporalmente sin servicio.");
        return View(vista, modelo);
    }

    private async Task<IActionResult> GuardarCuentaAsync(string mensaje)
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            TempData["Error"] = "Tu cuenta cambió mientras la actualizabas. Vuelve a ingresar.";
            await HttpContext.SignOutAsync();
            return RedirectToAction(nameof(Login));
        }
        await HttpContext.SignOutAsync();
        TempData["Aviso"] = mensaje;
        return RedirectToAction(nameof(Login));
    }
}
