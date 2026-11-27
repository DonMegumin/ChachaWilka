using System.ComponentModel.DataAnnotations;

namespace ChachaWilka.Models;

public static class RolesTienda
{
    public const string Cliente = "Cliente";
    public const string Empleado = "Empleado";
    public const string Administrador = "Administrador";
    public const string Personal = Empleado + "," + Administrador;
}

public enum EstadoPedido
{
    Historico, PendientePago, EnRevision, Confirmado, EnPreparacion, ListoRecojo, Retirado, Cancelado
}

public enum EstadoPago
{
    Historico, PendienteRevision, Verificado, Rechazado
}

public static class EtiquetasTienda
{
    public static string Etiqueta(this EstadoPedido estado) => estado switch
    {
        EstadoPedido.Historico => "Histórico",
        EstadoPedido.PendientePago => "Pendiente de pago",
        EstadoPedido.EnRevision => "Pago en revisión",
        EstadoPedido.EnPreparacion => "En preparación",
        EstadoPedido.ListoRecojo => "Listo para recoger",
        _ => estado.ToString()
    };

    public static string Etiqueta(this EstadoPago estado) => estado switch
    {
        EstadoPago.Historico => "Histórico",
        EstadoPago.PendienteRevision => "Pendiente de revisión",
        _ => estado.ToString()
    };
}

public sealed class AjustesTienda
{
    public int Id { get; set; } = 1;
    [StringLength(20)] public string Yape { get; set; } = "994040462";
    [StringLength(80)] public string TitularYape { get; set; } = "";
    [StringLength(120)] public string Horario { get; set; } = "Lun–sáb, 10:00 a. m.–6:00 p. m. (por confirmar)";
    [StringLength(150)] public string Direccion { get; set; } = "Jr. Recreo 437, Chachapoyas";
    [StringLength(20)] public string WhatsApp { get; set; } = "51989878957";
    [Range(typeof(decimal), "0", "10000", ParseLimitsInInvariantCulture = true)] public decimal PrecioCombo { get; set; } = 100;
    [Range(typeof(decimal), "0", "10000", ParseLimitsInInvariantCulture = true)] public decimal PrecioComboCremas { get; set; } = 120;
    public bool PermitirRepeticiones { get; set; }
    [Timestamp] public byte[] Version { get; set; } = [];
}

public sealed class EventoPedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public int ActorId { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    [StringLength(300)] public string Descripcion { get; set; } = "";
}

public sealed class MovimientoInventario
{
    public int Id { get; set; }
    public int ProductoId { get; set; }
    public Producto Producto { get; set; } = null!;
    public int? PedidoId { get; set; }
    public int ActorId { get; set; }
    public int ExistenciaDelta { get; set; }
    public int ReservaDelta { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    [StringLength(200)] public string Motivo { get; set; } = "";
}

public sealed class ConsultaPedido
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public Pedido Pedido { get; set; } = null!;
    [StringLength(1000)] public string Pregunta { get; set; } = "";
    [StringLength(1000)] public string? Respuesta { get; set; }
    public DateTimeOffset Creada { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? Respondida { get; set; }
    public int? ResponsableId { get; set; }
    [Timestamp] public byte[] Version { get; set; } = [];
}

public sealed class CambioCuenta
{
    public int Id { get; set; }
    public int ActorId { get; set; }
    public int CuentaId { get; set; }
    public DateTimeOffset Fecha { get; set; } = DateTimeOffset.UtcNow;
    [StringLength(150)] public string Accion { get; set; } = "";
}
