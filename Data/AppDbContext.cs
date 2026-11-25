using ChachaWilka.Models;
using Microsoft.EntityFrameworkCore;

namespace ChachaWilka.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<Categoria> categorias { get; set; }
        public DbSet<Combo> combos { get; set; }
        public DbSet<DetalleCombo> detalleCombos { get; set; }
        public DbSet<DetallePedido> detallePedidos { get; set; }
        public DbSet<Pago> pagos { get; set; }
        public DbSet<Pedido> pedidos { get; set; }
        public DbSet<Producto> productos { get; set; }
        public DbSet<Reporte> reportes { get; set; }
        public DbSet<Rol> roles { get; set; }
        public DbSet<Usuario> usuarios { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Rol - Usuario
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Usuario>().HasOne(r => r.Rol).WithMany(u => u.Usuarios).HasForeignKey(u => u.Id_Rol);
            //Usuario - Pedido
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Pedido>().HasOne(us => us.Usuarios).WithMany(pe => pe.Pedidos).HasForeignKey(us => us.Id_Usuario);
            //Usuario - Pago
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Pago>().HasOne(us => us.Usuarios).WithMany(pa => pa.Pagos).HasForeignKey(us => us.Id_Usuario).OnDelete(DeleteBehavior.NoAction);
            //Categoria - Producto
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Producto>().HasOne(ca => ca.Categorias).WithMany(pr => pr.Productos).HasForeignKey(ca => ca.Id_Categoria);
            //Pedido - DetallePedido
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DetallePedido>().HasOne(pe => pe.Pedidos).WithMany(dpe => dpe.DetallePedidos).HasForeignKey(pe => pe.Id_Pedido);
            //Producto – DetallePedido
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DetallePedido>().HasOne(pr => pr.Productos).WithMany(dpe => dpe.DetallePedidos).HasForeignKey(pr => pr.Id_Producto);
            //Pedido – Pago
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Pago>().HasOne(p => p.Pedidos).WithMany(pa => pa.Pagos).HasForeignKey(p => p.Id_Pedido);
            //Combo – DetalleCombo
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DetalleCombo>().HasOne(co => co.Combos).WithMany(dc => dc.DetalleCombos).HasForeignKey(co => co.Id_Combo);
            //Producto – DetalleCombo
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DetalleCombo>().HasOne(pr => pr.Productos).WithMany(dc => dc.DetalleCombos).HasForeignKey(pr => pr.Id_Producto);
            //Usuario – Reporte
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Reporte>().HasOne(us => us.Usuarios).WithMany(re => re.Reportes).HasForeignKey(us => us.Id_Usuario);
        }
    }
}
