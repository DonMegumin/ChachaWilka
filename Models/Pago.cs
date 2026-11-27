using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChachaWilka.Models
{
    public class Pago
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Pago { get; set; }
        [Required]
        public decimal Monto { get; set; }
        [Required]
        [StringLength(50)]
        public string MetodoPago { get; set; } = string.Empty;
        [Required]
        public DateOnly FechaPago { get; set; }
        [Required]
        [StringLength(100)]
        public string Comprobante { get; set; } = string.Empty;
        [Required]
        public bool EstadoPago { get; set; }
        public EstadoPago Situacion { get; set; }
        [StringLength(40)] public string? Referencia { get; set; }
        [StringLength(30)] public string? TipoArchivo { get; set; }
        public byte[]? Evidencia { get; set; }
        public int? RevisorId { get; set; }
        public DateTimeOffset? Revisado { get; set; }
        [StringLength(300)] public string? Motivo { get; set; }
        [Timestamp] public byte[] Version { get; set; } = [];
        //Pago M:1 Usuario
        public int Id_Usuario { get; set; }
        public Usuario Usuarios { get; set; } = null!;
        //Pago M:1 Pedido
        public int Id_Pedido { get; set; }
        public Pedido Pedidos { get; set; } = null!;
    }
}
