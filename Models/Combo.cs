using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChachaWilka.Models
{
    public class Combo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Combo { get; set; }
        [Required]
        [StringLength(50)]
        public string Nombre_Combo { get; set; } = string.Empty;
        [Required]
        [StringLength(200)]
        public string Descripcion_Combo { get; set; } = string.Empty;
        [Required]
        public decimal Precio_Combo { get; set; }
        [Required]
        public bool Estado_Combo { get; set; }
        //Combo 1:M DetalleCombo
        public ICollection<DetalleCombo> DetalleCombos { get; set; }
    }
}
