using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChachaWilka.Migrations
{
    /// <inheritdoc />
    public partial class migracion1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    Id_Categoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.Id_Categoria);
                });

            migrationBuilder.CreateTable(
                name: "combos",
                columns: table => new
                {
                    Id_Combo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Combo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion_Combo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Precio_Combo = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado_Combo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_combos", x => x.Id_Combo);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    Id_Rol = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre_Rol = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.Id_Rol);
                });

            migrationBuilder.CreateTable(
                name: "productos",
                columns: table => new
                {
                    Id_Producto = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    Id_Categoria = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_productos", x => x.Id_Producto);
                    table.ForeignKey(
                        name: "FK_productos_categorias_Id_Categoria",
                        column: x => x.Id_Categoria,
                        principalTable: "categorias",
                        principalColumn: "Id_Categoria",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id_Usuario = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Contraseña = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Id_Rol = table.Column<int>(type: "int", maxLength: 30, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id_Usuario);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_Id_Rol",
                        column: x => x.Id_Rol,
                        principalTable: "roles",
                        principalColumn: "Id_Rol",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "detalleCombos",
                columns: table => new
                {
                    Id_DetalleCombo = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Id_Combo = table.Column<int>(type: "int", nullable: false),
                    Id_Producto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detalleCombos", x => x.Id_DetalleCombo);
                    table.ForeignKey(
                        name: "FK_detalleCombos_combos_Id_Combo",
                        column: x => x.Id_Combo,
                        principalTable: "combos",
                        principalColumn: "Id_Combo",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detalleCombos_productos_Id_Producto",
                        column: x => x.Id_Producto,
                        principalTable: "productos",
                        principalColumn: "Id_Producto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pedidos",
                columns: table => new
                {
                    Id_Pedido = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    Id_Usuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pedidos", x => x.Id_Pedido);
                    table.ForeignKey(
                        name: "FK_pedidos_usuarios_Id_Usuario",
                        column: x => x.Id_Usuario,
                        principalTable: "usuarios",
                        principalColumn: "Id_Usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reportes",
                columns: table => new
                {
                    Id_Reporte = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Fecha_Reporte = table.Column<DateOnly>(type: "date", nullable: false),
                    Id_Usuario = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reportes", x => x.Id_Reporte);
                    table.ForeignKey(
                        name: "FK_reportes_usuarios_Id_Usuario",
                        column: x => x.Id_Usuario,
                        principalTable: "usuarios",
                        principalColumn: "Id_Usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "detallePedidos",
                columns: table => new
                {
                    Id_DetallePedido = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Id_Pedido = table.Column<int>(type: "int", nullable: false),
                    Id_Producto = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_detallePedidos", x => x.Id_DetallePedido);
                    table.ForeignKey(
                        name: "FK_detallePedidos_pedidos_Id_Pedido",
                        column: x => x.Id_Pedido,
                        principalTable: "pedidos",
                        principalColumn: "Id_Pedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_detallePedidos_productos_Id_Producto",
                        column: x => x.Id_Producto,
                        principalTable: "productos",
                        principalColumn: "Id_Producto",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pagos",
                columns: table => new
                {
                    Id_Pago = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Monto = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    MetodoPago = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaPago = table.Column<DateOnly>(type: "date", nullable: false),
                    Comprobante = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EstadoPago = table.Column<bool>(type: "bit", nullable: false),
                    Id_Usuario = table.Column<int>(type: "int", nullable: false),
                    Id_Pedido = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagos", x => x.Id_Pago);
                    table.ForeignKey(
                        name: "FK_pagos_pedidos_Id_Pedido",
                        column: x => x.Id_Pedido,
                        principalTable: "pedidos",
                        principalColumn: "Id_Pedido",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pagos_usuarios_Id_Usuario",
                        column: x => x.Id_Usuario,
                        principalTable: "usuarios",
                        principalColumn: "Id_Usuario");
                });

            migrationBuilder.CreateIndex(
                name: "IX_detalleCombos_Id_Combo",
                table: "detalleCombos",
                column: "Id_Combo");

            migrationBuilder.CreateIndex(
                name: "IX_detalleCombos_Id_Producto",
                table: "detalleCombos",
                column: "Id_Producto");

            migrationBuilder.CreateIndex(
                name: "IX_detallePedidos_Id_Pedido",
                table: "detallePedidos",
                column: "Id_Pedido");

            migrationBuilder.CreateIndex(
                name: "IX_detallePedidos_Id_Producto",
                table: "detallePedidos",
                column: "Id_Producto");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_Id_Pedido",
                table: "pagos",
                column: "Id_Pedido");

            migrationBuilder.CreateIndex(
                name: "IX_pagos_Id_Usuario",
                table: "pagos",
                column: "Id_Usuario");

            migrationBuilder.CreateIndex(
                name: "IX_pedidos_Id_Usuario",
                table: "pedidos",
                column: "Id_Usuario");

            migrationBuilder.CreateIndex(
                name: "IX_productos_Id_Categoria",
                table: "productos",
                column: "Id_Categoria");

            migrationBuilder.CreateIndex(
                name: "IX_reportes_Id_Usuario",
                table: "reportes",
                column: "Id_Usuario");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_Id_Rol",
                table: "usuarios",
                column: "Id_Rol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "detalleCombos");

            migrationBuilder.DropTable(
                name: "detallePedidos");

            migrationBuilder.DropTable(
                name: "pagos");

            migrationBuilder.DropTable(
                name: "reportes");

            migrationBuilder.DropTable(
                name: "combos");

            migrationBuilder.DropTable(
                name: "productos");

            migrationBuilder.DropTable(
                name: "pedidos");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
