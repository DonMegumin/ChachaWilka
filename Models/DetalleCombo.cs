using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChachaWilka.Models
{
    public class DetalleCombo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_DetalleCombo { get; set; }
        [Required]
        public int Cantidad { get; set; }
        //DetalleCombo M:1 Combo
        public int Id_Combo { get; set; }
        public Combo Combos { get; set; } = null!;
        //DetalleCombo M:1 Producto
        public int Id_Producto { get; set; }
        public Producto Productos { get; set; } = null!;
    }
}
