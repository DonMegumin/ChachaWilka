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
        //Pedido M:1 Usuario
        public int Id_Usuario { get; set; }
        public Usuario Usuarios { get; set; }
        //Pedido 1:M DetallePedido
        public ICollection<DetallePedido> DetallePedidos { get; set; }
        //Pedido 1:M Pago
        public ICollection<Pago> Pagos { get; set; }
    }
}
