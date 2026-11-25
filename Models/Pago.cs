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
        //Pago M:1 Usuario
        public int Id_Usuario { get; set; }
        public Usuario Usuarios { get; set; }
        //Pago M:1 Pedido
        public int Id_Pedido { get; set; }
        public Pedido Pedidos { get; set; }
    }
}
