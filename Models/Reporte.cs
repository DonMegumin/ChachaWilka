using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChachaWilka.Models
{
    public class Reporte
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Reporte { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public DateOnly Fecha_Reporte { get; set; }
        //Reporte M:1 Usuario
        public int Id_Usuario { get; set; }
        public Usuario Usuarios { get; set; }
    }
}
