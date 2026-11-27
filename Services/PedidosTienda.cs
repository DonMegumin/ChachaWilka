using ChachaWilka.Data;
using ChachaWilka.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace ChachaWilka.Services;

public sealed class PedidosTienda(AppDbContext db, CarritoTienda carrito)
{
    public async Task<int?> ConfirmadoAsync(int actorId, Guid clave) => await db.pedidos.AsNoTracking()
        .Where(p => p.Id_Usuario == actorId && p.ClaveConfirmacion == clave)
        .Select(p => (int?)p.Id_Pedido).SingleOrDefaultAsync();

    public async Task<int> CrearAsync(SeleccionCarrito seleccion, int clienteId, decimal totalVisto)
    {
        if (await ConfirmadoAsync(clienteId, seleccion.Confirmacion) is { } anterior)
            return anterior;

        await using var transaccion = await db.Database.BeginTransactionAsync();
        try
        {
            if (!await db.usuarios.AnyAsync(u => u.Id_Usuario == clienteId && u.Activo && u.MayorDeEdad && u.Consentimiento != null))
                throw new ReglaTiendaException("Tu cuenta necesita validar la mayoría de edad y el consentimiento.");
            if (!await db.Ajustes.AnyAsync(a => a.Yape != ""))
                throw new ReglaTiendaException("La tienda aún no tiene un número de Yape configurado.");

            var cotizacion = await carrito.CalcularAsync(seleccion, seguimiento: true);
            if (cotizacion.Lineas.Count == 0 || cotizacion.Errores.Count > 0)
                throw new ReglaTiendaException(cotizacion.Errores.FirstOrDefault() ?? "Tu cesta está vacía.");
            if (cotizacion.Total != totalVisto)
                throw new ReglaTiendaException("El precio cambió. Revisa el total actualizado antes de confirmar.");

            var pedido = new Pedido
            {
                Id_Usuario = clienteId, Fecha = RelojTienda.Hoy, Total = cotizacion.Total,
                Situacion = EstadoPedido.PendientePago, ClaveConfirmacion = seleccion.Confirmacion,
                Codigo = "CW-" + Guid.NewGuid().ToString("N")[..12].ToUpperInvariant(),
                DetallePedidos = cotizacion.Lineas.Select(l => new DetallePedido
                {
                    Id_Producto = l.Producto.Id_Producto, NombreHistorico = l.Producto.Nombre, GrupoCombo = l.Combo,
                    Cantidad = l.Cantidad, Precio = l.Precio, Subtotal = l.Subtotal
                }).ToList()
            };
            Evento(pedido, clienteId, "Pedido creado. Unidades reservadas.");
            db.pedidos.Add(pedido);
            foreach (var grupo in cotizacion.Lineas.GroupBy(l => l.Producto))
                grupo.Key.Reservado += grupo.Sum(l => l.Cantidad);

            await db.SaveChangesAsync();
            foreach (var grupo in cotizacion.Lineas.GroupBy(l => l.Producto))
                Movimiento(grupo.Key.Id_Producto, pedido.Id_Pedido, clienteId, 0, grupo.Sum(l => l.Cantidad), "Reserva por pedido");

            await db.SaveChangesAsync();
            await transaccion.CommitAsync();
            return pedido.Id_Pedido;
        }
        catch (DbUpdateException ex) when (ex is DbUpdateConcurrencyException || ex.InnerException is SqlException { Number: 2601 or 2627 or 1205 })
        {
            await transaccion.RollbackAsync();
            db.ChangeTracker.Clear();
            if (await ConfirmadoAsync(clienteId, seleccion.Confirmacion) is { } repetido)
                return repetido;
            throw new ReglaTiendaException("Cambió la disponibilidad. Vuelve a revisar tu cesta.");
        }
    }

    public async Task PresentarPagoAsync(int id, int clienteId, string? referencia, DateOnly fecha, decimal monto, byte[]? archivo, string? tipo)
    {
        var pedido = await db.pedidos.SingleOrDefaultAsync(p => p.Id_Pedido == id && p.Id_Usuario == clienteId)
            ?? throw new KeyNotFoundException();
        if (pedido.Situacion != EstadoPedido.PendientePago)
            throw new ReglaTiendaException("El pedido no admite otro pago en este momento.");

        referencia = string.IsNullOrWhiteSpace(referencia) ? null : referencia.Trim().ToUpperInvariant();
        if (referencia is null && archivo is null)
            throw new ReglaTiendaException("Ingresa el código de operación o adjunta el comprobante.");
        if (fecha < pedido.Fecha || fecha > RelojTienda.Hoy)
            throw new ReglaTiendaException("La fecha del pago debe estar entre la fecha del pedido y hoy.");
        if (referencia is not null && await db.pagos.AnyAsync(p => p.Referencia == referencia))
            throw new ReglaTiendaException("Ese código de operación ya está registrado.");

        db.pagos.Add(new Pago
        {
            Id_Pedido = id, Id_Usuario = clienteId, Monto = monto, FechaPago = fecha,
            MetodoPago = "Yape", Referencia = referencia, TipoArchivo = tipo, Evidencia = archivo,
            Situacion = EstadoPago.PendienteRevision
        });
        pedido.Situacion = EstadoPedido.EnRevision;
        Evento(pedido, clienteId, "Pago enviado a revisión.");
        await GuardarAsync();
    }

    public async Task RevisarPagoAsync(int id, int pagoId, int actorId, bool aprobar, string? motivo)
    {
        var pedido = await db.pedidos.SingleOrDefaultAsync(p => p.Id_Pedido == id) ?? throw new KeyNotFoundException();
        var pago = await db.pagos.SingleOrDefaultAsync(p => p.Id_Pago == pagoId && p.Id_Pedido == id) ?? throw new KeyNotFoundException();
        if (pedido.Situacion != EstadoPedido.EnRevision || pago.Situacion != EstadoPago.PendienteRevision)
            throw new ReglaTiendaException("Este pago ya fue atendido.");
        if (aprobar && pago.Monto != pedido.Total)
            throw new ReglaTiendaException("El importe informado no coincide con el pedido. Recházalo e indica el motivo.");
        if (!aprobar && string.IsNullOrWhiteSpace(motivo))
            throw new ReglaTiendaException("Indica por qué se rechazó el pago.");

        pago.Situacion = aprobar ? EstadoPago.Verificado : EstadoPago.Rechazado;
        pago.EstadoPago = aprobar;
        pago.Motivo = aprobar ? null : motivo?.Trim();
        pago.RevisorId = actorId;
        pago.Revisado = DateTimeOffset.UtcNow;
        pedido.Situacion = aprobar ? EstadoPedido.Confirmado : EstadoPedido.PendientePago;
        Evento(pedido, actorId, aprobar ? "Pago verificado en Yape." : "Pago rechazado. Puedes enviar un nuevo comprobante.");
        await GuardarAsync();
    }

    public async Task CambiarEstadoAsync(int id, int actorId, EstadoPedido destino, bool identidadVerificada, string? motivo)
    {
        var pedido = await db.pedidos.Include(p => p.DetallePedidos).ThenInclude(l => l.Productos)
            .SingleOrDefaultAsync(p => p.Id_Pedido == id) ?? throw new KeyNotFoundException();
        var origen = pedido.Situacion;
        var permitido = (origen, destino) switch
        {
            (EstadoPedido.Confirmado, EstadoPedido.EnPreparacion) => true,
            (EstadoPedido.EnPreparacion, EstadoPedido.ListoRecojo) => true,
            (EstadoPedido.ListoRecojo, EstadoPedido.Retirado) => identidadVerificada,
            (EstadoPedido.PendientePago or EstadoPedido.EnRevision, EstadoPedido.Cancelado) => !string.IsNullOrWhiteSpace(motivo),
            _ => false
        };
        if (!permitido)
            throw new ReglaTiendaException("No se puede aplicar ese cambio. Para el recojo confirma la identidad y mayoría de edad. Para cancelar indica el motivo.");

        if (destino is EstadoPedido.Retirado or EstadoPedido.Cancelado)
        {
            var retiro = destino == EstadoPedido.Retirado;
            foreach (var grupo in pedido.DetallePedidos.GroupBy(l => l.Productos))
            {
                var unidades = grupo.Sum(l => l.Cantidad);
                var producto = grupo.Key;
                if (producto.Reservado < unidades || (retiro && producto.Stock < unidades))
                    throw new ReglaTiendaException("El saldo reservado no coincide con el pedido. Revisa el inventario.");
                producto.Reservado -= unidades;
                if (retiro)
                    producto.Stock -= unidades;
                Movimiento(producto.Id_Producto, id, actorId, retiro ? -unidades : 0, -unidades, retiro ? "Recojo" : "Liberación por cancelación");
            }
            if (retiro)
            {
                pedido.Retirado = DateTimeOffset.UtcNow;
                pedido.Estado = true;
            }
            else
            {
                var pendientes = await db.pagos.Where(p => p.Id_Pedido == id && p.Situacion == EstadoPago.PendienteRevision).ToListAsync();
                foreach (var pago in pendientes)
                {
                    pago.Situacion = EstadoPago.Rechazado;
                    pago.RevisorId = actorId;
                    pago.Revisado = DateTimeOffset.UtcNow;
                    pago.Motivo = "Pedido cancelado por el personal.";
                }
            }
        }

        pedido.Situacion = destino;
        Evento(pedido, actorId, destino == EstadoPedido.Cancelado ? "Pedido cancelado. " + motivo!.Trim() : destino.Etiqueta());
        await GuardarAsync();
    }

    public void Movimiento(int producto, int? pedido, int actor, int existencia, int reserva, string motivo) => db.Movimientos.Add(new()
    {
        ProductoId = producto, PedidoId = pedido, ActorId = actor, ExistenciaDelta = existencia, ReservaDelta = reserva, Motivo = motivo
    });

    private static void Evento(Pedido pedido, int actor, string descripcion) =>
        pedido.Historial.Add(new EventoPedido { ActorId = actor, Descripcion = descripcion });

    private async Task GuardarAsync()
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ReglaTiendaException("La información cambió mientras se procesaba. Actualiza la página y vuelve a revisar.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new ReglaTiendaException("La operación ya fue registrada. Revisa el pedido antes de volver a enviarla.");
        }
    }
}
