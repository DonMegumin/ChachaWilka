using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChachaWilka.Models
{
    public class Pedido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Pedido { get; set; }
        [Required]
        public DateOnly Fecha { get; set; }
        [Required]
        public decimal Total { get; set; }
        [Required]
        public bool Estado { get; set; }
        public EstadoPedido Situacion { get; set; }
        [StringLength(24)] public string? Codigo { get; set; }
        public Guid? ClaveConfirmacion { get; set; }
        public DateTimeOffset Creado { get; set; } = DateTimeOffset.UtcNow;
        public DateTimeOffset? Retirado { get; set; }
        [Timestamp] public byte[] Version { get; set; } = [];
        public ICollection<EventoPedido> Historial { get; set; } = [];
        public ICollection<ConsultaPedido> Consultas { get; set; } = [];
        //Pedido M:1 Usuario
        public int Id_Usuario { get; set; }
        public Usuario Usuarios { get; set; } = null!;
        //Pedido 1:M DetallePedido
        public ICollection<DetallePedido> DetallePedidos { get; set; } = [];
        //Pedido 1:M Pago
        public ICollection<Pago> Pagos { get; set; } = [];
    }
}
