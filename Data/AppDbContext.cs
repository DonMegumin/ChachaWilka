using ChachaWilka.Models;
using Microsoft.EntityFrameworkCore;

namespace ChachaWilka.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Categoria> categorias => Set<Categoria>();
    public DbSet<Combo> combos => Set<Combo>();
    public DbSet<DetalleCombo> detalleCombos => Set<DetalleCombo>();
    public DbSet<DetallePedido> detallePedidos => Set<DetallePedido>();
    public DbSet<Pago> pagos => Set<Pago>();
    public DbSet<Pedido> pedidos => Set<Pedido>();
    public DbSet<Producto> productos => Set<Producto>();
    public DbSet<Reporte> reportes => Set<Reporte>();
    public DbSet<Rol> roles => Set<Rol>();
    public DbSet<Usuario> usuarios => Set<Usuario>();
    public DbSet<AjustesTienda> Ajustes => Set<AjustesTienda>();
    public DbSet<ConsultaPedido> Consultas => Set<ConsultaPedido>();
    public DbSet<EventoPedido> Historial => Set<EventoPedido>();
    public DbSet<MovimientoInventario> Movimientos => Set<MovimientoInventario>();
    public DbSet<CambioCuenta> CambiosCuenta => Set<CambioCuenta>();

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.Properties<decimal>().HavePrecision(18, 2);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Usuario>().HasOne(u => u.Rol).WithMany(r => r.Usuarios).HasForeignKey(u => u.Id_Rol);
        modelBuilder.Entity<Usuario>().HasIndex(u => u.CorreoNormalizado).IsUnique().HasFilter("[TieneHash] = 1");
        modelBuilder.Entity<Usuario>().Property(u => u.Activo).HasDefaultValue(true);
        modelBuilder.Entity<Pedido>().HasOne(p => p.Usuarios).WithMany(u => u.Pedidos).HasForeignKey(p => p.Id_Usuario);
        modelBuilder.Entity<Pedido>().Property(p => p.Situacion).HasConversion<string>().HasMaxLength(25).HasDefaultValue(EstadoPedido.Historico);
        modelBuilder.Entity<Pedido>().HasIndex(p => p.Codigo).IsUnique();
        modelBuilder.Entity<Pedido>().HasIndex(p => new { p.Id_Usuario, p.ClaveConfirmacion }).IsUnique();
        modelBuilder.Entity<Pedido>().HasIndex(p => new { p.Situacion, p.Creado });
        modelBuilder.Entity<Pedido>().HasMany(p => p.Historial).WithOne().HasForeignKey(e => e.PedidoId);
        modelBuilder.Entity<Pago>().HasOne(p => p.Usuarios).WithMany(u => u.Pagos).HasForeignKey(p => p.Id_Usuario).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<Pago>().HasOne(p => p.Pedidos).WithMany(o => o.Pagos).HasForeignKey(p => p.Id_Pedido);
        modelBuilder.Entity<Pago>().Property(p => p.Situacion).HasConversion<string>().HasMaxLength(25).HasDefaultValue(EstadoPago.Historico);
        modelBuilder.Entity<Pago>().HasIndex(p => p.Referencia).IsUnique();
        modelBuilder.Entity<Pago>().HasIndex(p => p.Id_Pedido, "IX_pagos_Pedido_Verificado").IsUnique().HasFilter("[Situacion] = N'Verificado'");
        modelBuilder.Entity<Pago>().ToTable(t => t.HasCheckConstraint("CK_pagos_Evidencia", "[Situacion] = N'Historico' OR [Referencia] IS NOT NULL OR [Evidencia] IS NOT NULL"));
        modelBuilder.Entity<Producto>().HasOne(p => p.Categorias).WithMany(c => c.Productos).HasForeignKey(p => p.Id_Categoria);
        modelBuilder.Entity<Producto>().HasIndex(p => p.ReferenciaCatalogo).IsUnique();
        modelBuilder.Entity<Producto>().Property(p => p.Activo).HasDefaultValue(true);
        modelBuilder.Entity<Producto>().ToTable(t => t.HasCheckConstraint("CK_productos_Stock", "[Stock] >= 0 AND [Reservado] >= 0 AND [Reservado] <= [Stock]"));
        modelBuilder.Entity<Categoria>().Property(c => c.Activa).HasDefaultValue(true);
        modelBuilder.Entity<DetallePedido>().HasOne(d => d.Pedidos).WithMany(p => p.DetallePedidos).HasForeignKey(d => d.Id_Pedido);
        modelBuilder.Entity<DetallePedido>().HasOne(d => d.Productos).WithMany(p => p.DetallePedidos).HasForeignKey(d => d.Id_Producto);
        modelBuilder.Entity<DetalleCombo>().HasOne(d => d.Combos).WithMany(c => c.DetalleCombos).HasForeignKey(d => d.Id_Combo);
        modelBuilder.Entity<DetalleCombo>().HasOne(d => d.Productos).WithMany(p => p.DetalleCombos).HasForeignKey(d => d.Id_Producto);
        modelBuilder.Entity<Reporte>().HasOne(r => r.Usuarios).WithMany(u => u.Reportes).HasForeignKey(r => r.Id_Usuario);
        modelBuilder.Entity<AjustesTienda>().Property(a => a.Id).ValueGeneratedNever();
        modelBuilder.Entity<MovimientoInventario>().HasOne(m => m.Producto).WithMany().HasForeignKey(m => m.ProductoId).OnDelete(DeleteBehavior.NoAction);
        modelBuilder.Entity<MovimientoInventario>().HasIndex(m => new { m.ProductoId, m.Fecha });
    }
}
