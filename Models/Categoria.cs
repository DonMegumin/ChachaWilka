using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChachaWilka.Models
{
    public class Categoria
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Categoria { get; set; }
        [Required]
        [StringLength(50)]
        public string Nombre_Categoria { get; set; } = string.Empty;
        //Categoria 1:M Producto
        public ICollection<Producto> Productos { get; set; }
    }
}
