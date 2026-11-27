using System.Text.Json;
using ChachaWilka.Data;
using ChachaWilka.Models;
using Microsoft.EntityFrameworkCore;

namespace ChachaWilka.Services;

public sealed class SeleccionCarrito
{
    public Guid Confirmacion { get; set; } = Guid.NewGuid();
    public Dictionary<int, int> Botellas { get; set; } = [];
    public List<SeleccionCombo> Combos { get; set; } = [];
}

public sealed record SeleccionCombo(Guid Id, string Categoria, int[] Sabores);
public sealed record LineaCarrito(Producto Producto, int Cantidad, decimal Precio, string? Combo = null)
{
    public decimal Subtotal => Precio * Cantidad;
}

public sealed record CarritoCalculado(SeleccionCarrito Seleccion, List<LineaCarrito> Lineas, List<string> Errores)
{
    public decimal Total => Lineas.Sum(l => l.Subtotal);
    public int Cantidad => Seleccion.Botellas.Values.Sum() + Seleccion.Combos.Count * 4;
}

public sealed class ReglaTiendaException(string mensaje) : Exception(mensaje);

public sealed class CarritoTienda(AppDbContext db)
{
    private const string Clave = "ChachaWillka.Carrito";

    public SeleccionCarrito Leer(ISession sesion) => sesion.GetString(Clave) is { } json
        ? JsonSerializer.Deserialize<SeleccionCarrito>(json) ?? new() : new();

    public void Guardar(ISession sesion, SeleccionCarrito seleccion)
    {
        seleccion.Confirmacion = Guid.NewGuid();
        sesion.SetString(Clave, JsonSerializer.Serialize(seleccion));
    }

    public void Vaciar(ISession sesion) => sesion.Remove(Clave);

    public async Task<CarritoCalculado> CalcularAsync(SeleccionCarrito seleccion, bool seguimiento = false)
    {
        var ids = seleccion.Botellas.Keys.Concat(seleccion.Combos.SelectMany(c => c.Sabores)).Distinct().ToArray();
        var resultados = new CarritoCalculado(seleccion, [], []);
        if (ids.Length == 0)
            return resultados;

        var consulta = seguimiento ? db.productos.AsTracking() : db.productos.AsNoTracking();
        var productos = await consulta.Include(p => p.Categorias).Where(p => ids.Contains(p.Id_Producto))
            .ToDictionaryAsync(p => p.Id_Producto);
        var ajustes = seleccion.Combos.Count > 0 ? await db.Ajustes.AsNoTracking().SingleOrDefaultAsync() ?? new() : null;

        foreach (var (id, cantidad) in seleccion.Botellas)
        {
            if (productos.TryGetValue(id, out var producto) && producto.Activo && producto.Categorias.Activa && producto.Precio > 0)
                resultados.Lineas.Add(new LineaCarrito(producto, cantidad, producto.Precio));
            else
                resultados.Errores.Add("Un producto de tu cesta ya no está a la venta. Retíralo para continuar.");
        }

        foreach (var combo in seleccion.Combos)
        {
            var precio = PrecioCombo(ajustes!, combo.Categoria);
            var sabores = combo.Sabores.Select(id => productos.GetValueOrDefault(id)).ToArray();
            if (precio <= 0 || sabores.Length != 4 ||
                (!ajustes!.PermitirRepeticiones && combo.Sabores.Distinct().Count() != 4) ||
                sabores.Any(p => p is null || !p.Activo || !p.Categorias.Activa || p.Precio <= 0 ||
                    p.Categorias.Nombre_Categoria != combo.Categoria))
            {
                resultados.Errores.Add("Una caja ya no cumple las condiciones de venta. Retírala y vuelve a armarla.");
                continue;
            }

            var costoUnitario = decimal.Floor(precio * 25) / 100;
            for (var i = 0; i < sabores.Length; i++)
                resultados.Lineas.Add(new LineaCarrito(sabores[i]!, 1, i == 3 ? precio - costoUnitario * 3 : costoUnitario,
                    "Caja de " + combo.Categoria.ToLowerInvariant()));
        }

        foreach (var grupo in resultados.Lineas.GroupBy(l => l.Producto))
        {
            if (grupo.Sum(l => l.Cantidad) > grupo.Key.Disponible)
                resultados.Errores.Add($"Solo quedan {Math.Max(0, grupo.Key.Disponible)} unidades de {grupo.Key.Nombre}.");
        }

        return resultados;
    }

    public static decimal PrecioCombo(AjustesTienda ajustes, string categoria) => categoria switch
    {
        CatalogoMarca.Licores => ajustes.PrecioCombo,
        CatalogoMarca.Cremas => ajustes.PrecioComboCremas,
        _ => 0
    };
}
