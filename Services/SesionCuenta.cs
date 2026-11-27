using System.Security.Claims;
using ChachaWilka.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ChachaWilka.Services;

public sealed class SesionCuenta(AppDbContext db, ILogger<SesionCuenta> logger) : CookieAuthenticationEvents
{
    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var id = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (int.TryParse(id, out var cuentaId))
        {
            try
            {
                var cuenta = await db.usuarios.AsNoTracking().Where(u => u.Id_Usuario == cuentaId)
                    .Select(u => new { u.Activo, u.TieneHash, u.Version, Rol = u.Rol.Nombre_Rol })
                    .SingleOrDefaultAsync(context.HttpContext.RequestAborted);
                if (cuenta is { Activo: true, TieneHash: true } &&
                    cuenta.Rol == context.Principal?.FindFirstValue(ClaimTypes.Role) &&
                    Convert.ToBase64String(cuenta.Version) == context.Principal?.FindFirstValue("VersionCuenta"))
                    return;
            }
            catch (SqlException ex)
            {
                logger.LogWarning("No se pudo validar la sesión ({Codigo}).", ex.Number);
                context.RejectPrincipal();
                return;
            }
        }

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync();
    }
}
