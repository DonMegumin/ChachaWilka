using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChachaWilka.Models
{
    public class Rol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Rol { get; set; }
        [Required]
        [StringLength(50)]
        public string Nombre_Rol { get; set; } = string.Empty;
        //1:M
        public ICollection<Usuario> Usuarios { get; set; } = [];
    }
}
