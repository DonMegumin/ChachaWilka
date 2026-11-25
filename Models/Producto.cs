using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChachaWilka.Models
{
    public class Producto
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Producto { get; set; }
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        [StringLength(200)]
        public string Descripcion { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]
        public string Categoria { get; set; } = string.Empty;
        [Required]
        public decimal Precio { get; set; }
        [Required]
        public int Stock { get; set; }
        //Producto M:1 Categoria
        public int Id_Categoria { get; set; }
        public Categoria Categorias { get; set; }
        //Producto 1:M DetallePedido
        public ICollection<DetallePedido> DetallePedidos { get; set; }
        //Producto 1:M DetalleCombo
        public ICollection<DetalleCombo> DetalleCombos { get; set; }
    }
}
