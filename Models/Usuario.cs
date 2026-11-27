using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChachaWilka.Models
{
    public class Usuario
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_Usuario { get; set; }
        [Required]
        [StringLength(50)]
        public string Nombre { get; set; } = string.Empty;
        [Required]
        [StringLength(100)]

        public string Correo { get; set; } = string.Empty;
        [Required]
        [StringLength(512)]
        public string Contraseña { get; set; } = string.Empty;
        [StringLength(100)] public string CorreoNormalizado { get; set; } = "";
        [StringLength(20)] public string Telefono { get; set; } = "";
        public bool MayorDeEdad { get; set; }
        public DateTimeOffset? Consentimiento { get; set; }
        public bool Activo { get; set; } = true;
        public bool TieneHash { get; set; }
        [Timestamp] public byte[] Version { get; set; } = [];
        [Required]
        [StringLength(30)]
        //Usuario M:1 Rol
        public int Id_Rol { get; set; }
        public Rol Rol { get; set; } = null!;
        //usuario 1:M Pedido
        public ICollection<Pedido> Pedidos { get; set; } = [];
        //usuario 1:M Pago
        public ICollection<Pago> Pagos { get; set; } = [];
        //usuario 1:M Reporte
        public ICollection<Reporte> Reportes { get; set; } = [];
    }
}
