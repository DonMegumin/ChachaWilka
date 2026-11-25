using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChachaWilka.Models
{
    public class DetallePedido
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_DetallePedido { get; set; }
        [Required]
        public int Cantidad { get; set; }
        [Required]
        public decimal Precio { get; set; }
        [Required]
        public decimal Subtotal { get; set; }
        //DetallePedido M:1 Pedido
        public int Id_Pedido { get; set; }
        public Pedido Pedidos { get; set; }
        //DetallePedido M:1 Producto
        public int Id_Producto { get; set; }
        public Producto Productos { get; set; }
    }
}
