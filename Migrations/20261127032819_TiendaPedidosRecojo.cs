using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChachaWilka.Migrations
{
    /// <inheritdoc />
    public partial class TiendaPedidosRecojo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_pedidos_Id_Usuario",
                table: "pedidos");

            migrationBuilder.DropIndex(
                name: "IX_pagos_Id_Pedido",
                table: "pagos");

            migrationBuilder.AlterColumn<string>(
                name: "Contraseña",
                table: "usuarios",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "usuarios",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Consentimiento",
                table: "usuarios",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CorreoNormalizado",
                table: "usuarios",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "MayorDeEdad",
                table: "usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Telefono",
                table: "usuarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "TieneHash",
                table: "usuarios",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "usuarios",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<bool>(
                name: "Activo",
                table: "productos",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Imagen",
                table: "productos",
                type: "nvarchar(140)",
                maxLength: 140,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferenciaCatalogo",
                table: "productos",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Reservado",
                table: "productos",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "productos",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<Guid>(
                name: "ClaveConfirmacion",
                table: "pedidos",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Codigo",
                table: "pedidos",
                type: "nvarchar(24)",
                maxLength: 24,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Creado",
                table: "pedidos",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Retirado",
                table: "pedidos",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Situacion",
                table: "pedidos",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "Historico");

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "pedidos",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "Evidencia",
                table: "pagos",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Motivo",
                table: "pagos",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Referencia",
                table: "pagos",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Revisado",
                table: "pagos",
                type: "datetimeoffset",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RevisorId",
                table: "pagos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Situacion",
                table: "pagos",
                type: "nvarchar(25)",
                maxLength: 25,
                nullable: false,
                defaultValue: "Historico");

            migrationBuilder.AddColumn<string>(
                name: "TipoArchivo",
                table: "pagos",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "Version",
                table: "pagos",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<string>(
                name: "GrupoCombo",
                table: "detallePedidos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreHistorico",
                table: "detallePedidos",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "Activa",
                table: "categorias",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.Sql("""
                UPDATE [usuarios]
                SET [CorreoNormalizado] = UPPER(LTRIM(RTRIM([Correo]))), [Contraseña] = N'!', [Activo] = 0
                WHERE [TieneHash] = 0;

                UPDATE [pedidos]
                SET [Creado] = TODATETIMEOFFSET(CAST([Fecha] AS datetime2), '-05:00')
                WHERE [Situacion] = N'Historico';

                UPDATE [detalle]
                SET [NombreHistorico] = [producto].[Nombre]
                FROM [detallePedidos] AS [detalle]
                INNER JOIN [productos] AS [producto] ON [producto].[Id_Producto] = [detalle].[Id_Producto];
                """);

            migrationBuilder.CreateTable(
                name: "Ajustes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Yape = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TitularYape = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Horario = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Direccion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    WhatsApp = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PrecioCombo = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PrecioComboCremas = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    PermitirRepeticiones = table.Column<bool>(type: "bit", nullable: false),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ajustes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CambiosCuenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorId = table.Column<int>(type: "int", nullable: false),
                    CuentaId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Accion = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CambiosCuenta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Consultas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoId = table.Column<int>(type: "int", nullable: false),
                    Pregunta = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Respuesta = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Creada = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Respondida = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    ResponsableId = table.Column<int>(type: "int", nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consultas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consultas_pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "pedidos",
                        principalColumn: "Id_Pedido",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Historial",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PedidoId = table.Column<int>(type: "int", nullable: false),
                    ActorId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Historial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Historial_pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "pedidos",
                        principalColumn: "Id_Pedido",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Movimientos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    PedidoId = table.Column<int>(type: "int", nullable: true),
                    ActorId = table.Column<int>(type: "int", nullable: false),
                    ExistenciaDelta = table.Column<int>(type: "int", nullable: false),
                    ReservaDelta = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movimientos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Movimientos_productos_ProductoId",
                        column: x => x.ProductoId,
                        principalTable: "productos",
                        principalColumn: "Id_Producto");
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_CorreoNormalizado",
                table: "usuarios",
                column: "CorreoNormalizado",
                unique: true,
                filter: "[TieneHash] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_productos_ReferenciaCatalogo",
                table: "productos",
                column: "ReferenciaCatalogo",
                unique: true,
                filter: "[ReferenciaCatalogo] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_productos_Stock",
                table: "productos",
                sql: "[Stock] >= 0 AND [Reservado] >= 0 AND [Reservado] <= [Stock]");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_Codigo",
                table: "pedidos",
                column: "Codigo",
                unique: true,
                filter: "[Codigo] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_Id_Usuario_ClaveConfirmacion",
                table: "pedidos",
                columns: new[] { "Id_Usuario", "ClaveConfirmacion" },
                unique: true,
                filter: "[ClaveConfirmacion] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_Situacion_Creado",
                table: "pedidos",
                columns: new[] { "Situacion", "Creado" });

            migrationBuilder.CreateIndex(
                name: "IX_pagos_Pedido_Verificado",
                table: "pagos",
                column: "Id_Pedido",
                unique: true,
                filter: "[Situacion] = N'Verificado'");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_Referencia",
                table: "pagos",
                column: "Referencia",
                unique: true,
                filter: "[Referencia] IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_pagos_Evidencia",
                table: "pagos",
                sql: "[Situacion] = N'Historico' OR [Referencia] IS NOT NULL OR [Evidencia] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Consultas_PedidoId",
                table: "Consultas",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Historial_PedidoId",
                table: "Historial",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Movimientos_ProductoId_Fecha",
                table: "Movimientos",
                columns: new[] { "ProductoId", "Fecha" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Ajustes");

            migrationBuilder.DropTable(
                name: "CambiosCuenta");

            migrationBuilder.DropTable(
                name: "Consultas");

            migrationBuilder.DropTable(
                name: "Historial");

            migrationBuilder.DropTable(
                name: "Movimientos");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_CorreoNormalizado",
                table: "usuarios");

            migrationBuilder.DropIndex(
                name: "IX_productos_ReferenciaCatalogo",
                table: "productos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_productos_Stock",
                table: "productos");

            migrationBuilder.DropIndex(
                name: "IX_pedidos_Codigo",
                table: "pedidos");

            migrationBuilder.DropIndex(
                name: "IX_pedidos_Id_Usuario_ClaveConfirmacion",
                table: "pedidos");

            migrationBuilder.DropIndex(
                name: "IX_pedidos_Situacion_Creado",
                table: "pedidos");

            migrationBuilder.DropIndex(
                name: "IX_pagos_Pedido_Verificado",
                table: "pagos");

            migrationBuilder.DropIndex(
                name: "IX_pagos_Referencia",
                table: "pagos");

            migrationBuilder.DropCheckConstraint(
                name: "CK_pagos_Evidencia",
                table: "pagos");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Consentimiento",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "CorreoNormalizado",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "MayorDeEdad",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Telefono",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "TieneHash",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "Activo",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "Imagen",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "ReferenciaCatalogo",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "Reservado",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "productos");

            migrationBuilder.DropColumn(
                name: "ClaveConfirmacion",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "Codigo",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "Creado",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "Retirado",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "Situacion",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "pedidos");

            migrationBuilder.DropColumn(
                name: "Evidencia",
                table: "pagos");

            migrationBuilder.DropColumn(
                name: "Motivo",
                table: "pagos");

            migrationBuilder.DropColumn(
                name: "Referencia",
                table: "pagos");

            migrationBuilder.DropColumn(
                name: "Revisado",
                table: "pagos");

            migrationBuilder.DropColumn(
                name: "RevisorId",
                table: "pagos");

            migrationBuilder.DropColumn(
                name: "Situacion",
                table: "pagos");

            migrationBuilder.DropColumn(
                name: "TipoArchivo",
                table: "pagos");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "pagos");

            migrationBuilder.DropColumn(
                name: "GrupoCombo",
                table: "detallePedidos");

            migrationBuilder.DropColumn(
                name: "NombreHistorico",
                table: "detallePedidos");

            migrationBuilder.DropColumn(
                name: "Activa",
                table: "categorias");

            migrationBuilder.AlterColumn<string>(
                name: "Contraseña",
                table: "usuarios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(512)",
                oldMaxLength: 512);

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_Id_Usuario",
                table: "pedidos",
                column: "Id_Usuario");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_Id_Pedido",
                table: "pagos",
                column: "Id_Pedido");
        }
    }
}
