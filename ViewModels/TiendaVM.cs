using System.ComponentModel.DataAnnotations;
using ChachaWilka.Models;
using ChachaWilka.Services;

namespace ChachaWilka.ViewModels;

public sealed record PortadaVM(IReadOnlyList<Producto> Productos, AjustesTienda Ajustes, string? Categoria, bool Disponible);
public sealed record ComboVM(string Categoria, IReadOnlyList<Producto> Productos, decimal Precio, bool Repeticiones);
public sealed record CestaVM(CarritoCalculado Carrito, AjustesTienda Ajustes);
public sealed record PedidoResumen(int Id, string? Codigo, DateOnly Fecha, decimal Total, EstadoPedido Estado, string? Cliente);
public sealed record IntentoPago(int Id, string? Referencia, DateOnly Fecha, decimal Total, EstadoPago Estado, bool Evidencia, string? Motivo);
public sealed record DetallePedidoVM(Pedido Pedido, AjustesTienda Ajustes, IReadOnlyList<IntentoPago> Pagos, bool Personal);

public sealed class PagoVM
{
    [StringLength(40), RegularExpression(@"[A-Za-z0-9-]+", ErrorMessage = "Usa letras, números o guiones.")] public string? Referencia { get; set; }
    [Required] public DateOnly? Fecha { get; set; }
    [Required, Range(typeof(decimal), "0.01", "1000000000", ParseLimitsInInvariantCulture = true)] public decimal? Monto { get; set; }
    public IFormFile? Evidencia { get; set; }
}

public sealed class ProductoVM
{
    [Required, StringLength(50)] public string Nombre { get; set; } = "";
    [Required, StringLength(200)] public string Descripcion { get; set; } = "";
    [Range(1, int.MaxValue)] public int CategoriaId { get; set; }
    [Range(typeof(decimal), "0.01", "99999", ParseLimitsInInvariantCulture = true)] public decimal Precio { get; set; }
    public bool Activo { get; set; } = true;
    [StringLength(140)] public string? Imagen { get; set; }
    public string? Version { get; set; }
}

public sealed class CuentaPersonalVM
{
    [Required, StringLength(50)] public string Nombre { get; set; } = "";
    [Required, EmailAddress, StringLength(100)] public string Correo { get; set; } = "";
    [Required, StringLength(128, MinimumLength = 12)] public string Clave { get; set; } = "";
    [Required] public string Rol { get; set; } = RolesTienda.Empleado;
}

public sealed record ReportesVM(DateOnly Desde, DateOnly Hasta, decimal Cobrado, decimal Vendido,
    IReadOnlyList<ConteoEstado> Estados, IReadOnlyList<Producto> Inventario, IReadOnlyList<MovimientoInventario> Movimientos);
public sealed record ConteoEstado(EstadoPedido Estado, int Cantidad);
public sealed record CuentaVM(string Nombre, string Correo, string Telefono, bool DeclaracionPendiente);
