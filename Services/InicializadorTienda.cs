using ChachaWilka.Data;
using ChachaWilka.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ChachaWilka.Services;

public sealed class InicializadorTienda(AppDbContext db, IConfiguration config, IPasswordHasher<Usuario> hasher)
{
    public async Task EjecutarAsync()
    {
        var correo = config["Tienda:Admin:Correo"]?.Trim();
        var clave = config["Tienda:Admin:Clave"];
        if (!string.IsNullOrWhiteSpace(correo) && (clave is null || clave.Length is < 12 or > 128 ||
            correo.Length > 100 || !new System.ComponentModel.DataAnnotations.EmailAddressAttribute().IsValid(correo)))
            throw new InvalidOperationException("Configura un correo válido y una contraseña de 12 a 128 caracteres para el primer administrador.");

        await db.Database.MigrateAsync();
        await using var transaccion = await db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
        var roles = await db.roles.ToListAsync();
        foreach (var nombre in new[] { RolesTienda.Cliente, RolesTienda.Empleado, RolesTienda.Administrador })
        {
            var anterior = roles.FirstOrDefault(r => string.Equals(r.Nombre_Rol, nombre, StringComparison.OrdinalIgnoreCase));
            if (anterior is null)
            {
                var rol = new Rol { Nombre_Rol = nombre };
                db.roles.Add(rol);
                roles.Add(rol);
            }
            else
                anterior.Nombre_Rol = nombre;
        }

        var categorias = await db.categorias.ToListAsync();
        var existentes = await db.productos.ToListAsync();
        foreach (var producto in CatalogoMarca.Productos())
        {
            if (existentes.Any(p => p.ReferenciaCatalogo == producto.ReferenciaCatalogo))
                continue;
            var anterior = existentes.FirstOrDefault(p => p.ReferenciaCatalogo == null &&
                string.Equals(p.Nombre, producto.Nombre, StringComparison.OrdinalIgnoreCase));
            if (anterior is not null)
            {
                anterior.ReferenciaCatalogo = producto.ReferenciaCatalogo;
                if (string.IsNullOrWhiteSpace(anterior.Imagen))
                    anterior.Imagen = producto.Imagen;
                continue;
            }
            var categoria = categorias.FirstOrDefault(c => string.Equals(c.Nombre_Categoria, producto.Categoria, StringComparison.OrdinalIgnoreCase));
            if (categoria is null)
            {
                categoria = new Categoria { Nombre_Categoria = producto.Categoria };
                db.categorias.Add(categoria);
                categorias.Add(categoria);
            }
            producto.Categorias = categoria;
            db.productos.Add(producto);
        }

        if (!await db.Ajustes.AnyAsync())
            db.Ajustes.Add(new AjustesTienda());

        if (!string.IsNullOrWhiteSpace(correo) && !await db.usuarios.AnyAsync(u => u.Activo && u.TieneHash &&
            u.Rol.Nombre_Rol == RolesTienda.Administrador))
        {
            var normalizado = correo.ToUpperInvariant();
            if (await db.usuarios.AnyAsync(u => u.TieneHash && u.CorreoNormalizado == normalizado))
                throw new InvalidOperationException("El correo de inicialización ya está en uso por otra cuenta.");
            var admin = new Usuario
            {
                Nombre = "Administración", Correo = correo, CorreoNormalizado = normalizado, TieneHash = true,
                Rol = roles.First(r => r.Nombre_Rol == RolesTienda.Administrador)
            };
            admin.Contraseña = hasher.HashPassword(admin, clave!);
            db.usuarios.Add(admin);
        }

        await db.SaveChangesAsync();
        await transaccion.CommitAsync();
        Console.WriteLine("Base de desarrollo inicializada.");
    }
}
