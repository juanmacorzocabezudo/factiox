using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FactioX.Migrations
{
    /// <inheritdoc />
    public partial class AddModuloTPV : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TicketTpvId",
                table: "LineasDocumento",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncluirAnioEnSerieDevolucionTicket",
                table: "ConfiguracionEmpresa",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IncluirAnioEnSerieTicket",
                table: "ConfiguracionEmpresa",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "LongitudNumeroDevolucionTicket",
                table: "ConfiguracionEmpresa",
                type: "int",
                nullable: false,
                defaultValue: 6);

            migrationBuilder.AddColumn<int>(
                name: "LongitudNumeroTicket",
                table: "ConfiguracionEmpresa",
                type: "int",
                nullable: false,
                defaultValue: 6);

            migrationBuilder.AddColumn<int>(
                name: "NumeroDevolucionTicketActual",
                table: "ConfiguracionEmpresa",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NumeroTicketActual",
                table: "ConfiguracionEmpresa",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SerieDevolucionTicket",
                table: "ConfiguracionEmpresa",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "TD")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "SerieTicket",
                table: "ConfiguracionEmpresa",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "T")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "TpvDescontarStock",
                table: "ConfiguracionEmpresa",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "TpvPermitirStockNegativo",
                table: "ConfiguracionEmpresa",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "TpvTextoPieTicket",
                table: "ConfiguracionEmpresa",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "TpvPreciosIncluyenIVA",
                table: "ConfiguracionEmpresa",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.CreateTable(
                name: "EmpresaModulos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    EmpresaId = table.Column<int>(type: "int", nullable: false),
                    Modulo = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    FechaActivacion = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaDesactivacion = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UsuarioActivacionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaModulos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmpresaModulos_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TpvTerminales",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Codigo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    SerieTicket = table.Column<string>(type: "varchar(10)", maxLength: 10, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NumeroTicketActual = table.Column<int>(type: "int", nullable: false),
                    ClienteGenericoId = table.Column<int>(type: "int", nullable: true),
                    FormaPagoPorDefectoId = table.Column<int>(type: "int", nullable: true),
                    ImpresoraAnchoMm = table.Column<int>(type: "int", nullable: false),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TpvTerminales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TpvTerminales_Clientes_ClienteGenericoId",
                        column: x => x.ClienteGenericoId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TpvTerminales_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TpvTerminales_FormasPagoPersonalizadas_FormaPagoPorDefectoId",
                        column: x => x.FormaPagoPorDefectoId,
                        principalTable: "FormasPagoPersonalizadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TpvSesionesCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TerminalId = table.Column<int>(type: "int", nullable: false),
                    UsuarioAperturaId = table.Column<int>(type: "int", nullable: false),
                    FechaApertura = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FondoInicial = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UsuarioCierreId = table.Column<int>(type: "int", nullable: true),
                    FechaCierre = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    EfectivoContado = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    EfectivoEsperado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descuadre = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Estado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observaciones = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TpvSesionesCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TpvSesionesCaja_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TpvSesionesCaja_TpvTerminales_TerminalId",
                        column: x => x.TerminalId,
                        principalTable: "TpvTerminales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TicketsTpv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Numero = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaCobro = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    TerminalId = table.Column<int>(type: "int", nullable: false),
                    SesionCajaId = table.Column<int>(type: "int", nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    Estado = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoTicket = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TicketOriginalId = table.Column<int>(type: "int", nullable: true),
                    PresupuestoOrigenId = table.Column<int>(type: "int", nullable: true),
                    FacturaId = table.Column<int>(type: "int", nullable: true),
                    DescuentoGlobal = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    BaseImponible = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TotalIVA = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Total = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Entregado = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Cambio = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Notas = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Huella = table.Column<string>(type: "varchar(64)", maxLength: 64, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmpresaId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketsTpv", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketsTpv_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketsTpv_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketsTpv_Facturas_FacturaId",
                        column: x => x.FacturaId,
                        principalTable: "Facturas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketsTpv_Presupuestos_PresupuestoOrigenId",
                        column: x => x.PresupuestoOrigenId,
                        principalTable: "Presupuestos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketsTpv_TicketsTpv_TicketOriginalId",
                        column: x => x.TicketOriginalId,
                        principalTable: "TicketsTpv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketsTpv_TpvSesionesCaja_SesionCajaId",
                        column: x => x.SesionCajaId,
                        principalTable: "TpvSesionesCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketsTpv_TpvTerminales_TerminalId",
                        column: x => x.TerminalId,
                        principalTable: "TpvTerminales",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketsTpv_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TpvMovimientosCaja",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    SesionCajaId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Concepto = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TpvMovimientosCaja", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TpvMovimientosCaja_TpvSesionesCaja_SesionCajaId",
                        column: x => x.SesionCajaId,
                        principalTable: "TpvSesionesCaja",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PagosTicketTpv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    TicketTpvId = table.Column<int>(type: "int", nullable: false),
                    FormaPagoPersonalizadaId = table.Column<int>(type: "int", nullable: true),
                    EsEfectivo = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Referencia = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PagosTicketTpv", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PagosTicketTpv_FormasPagoPersonalizadas_FormaPagoPersonaliza~",
                        column: x => x.FormaPagoPersonalizadaId,
                        principalTable: "FormasPagoPersonalizadas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_PagosTicketTpv_TicketsTpv_TicketTpvId",
                        column: x => x.TicketTpvId,
                        principalTable: "TicketsTpv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_LineasDocumento_TicketTpvId",
                table: "LineasDocumento",
                column: "TicketTpvId");

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaModulos_EmpresaId_Modulo",
                table: "EmpresaModulos",
                columns: new[] { "EmpresaId", "Modulo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PagosTicketTpv_FormaPagoPersonalizadaId",
                table: "PagosTicketTpv",
                column: "FormaPagoPersonalizadaId");

            migrationBuilder.CreateIndex(
                name: "IX_PagosTicketTpv_TicketTpvId",
                table: "PagosTicketTpv",
                column: "TicketTpvId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsTpv_ClienteId",
                table: "TicketsTpv",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsTpv_EmpresaId_Estado",
                table: "TicketsTpv",
                columns: new[] { "EmpresaId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketsTpv_EmpresaId_Fecha",
                table: "TicketsTpv",
                columns: new[] { "EmpresaId", "Fecha" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketsTpv_EmpresaId_Numero",
                table: "TicketsTpv",
                columns: new[] { "EmpresaId", "Numero" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TicketsTpv_FacturaId",
                table: "TicketsTpv",
                column: "FacturaId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsTpv_PresupuestoOrigenId",
                table: "TicketsTpv",
                column: "PresupuestoOrigenId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsTpv_SesionCajaId",
                table: "TicketsTpv",
                column: "SesionCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsTpv_TerminalId",
                table: "TicketsTpv",
                column: "TerminalId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsTpv_TicketOriginalId",
                table: "TicketsTpv",
                column: "TicketOriginalId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketsTpv_UsuarioId",
                table: "TicketsTpv",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_TpvMovimientosCaja_SesionCajaId",
                table: "TpvMovimientosCaja",
                column: "SesionCajaId");

            migrationBuilder.CreateIndex(
                name: "IX_TpvSesionesCaja_EmpresaId",
                table: "TpvSesionesCaja",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_TpvSesionesCaja_TerminalId_Estado",
                table: "TpvSesionesCaja",
                columns: new[] { "TerminalId", "Estado" });

            migrationBuilder.CreateIndex(
                name: "IX_TpvTerminales_ClienteGenericoId",
                table: "TpvTerminales",
                column: "ClienteGenericoId");

            migrationBuilder.CreateIndex(
                name: "IX_TpvTerminales_EmpresaId_Codigo",
                table: "TpvTerminales",
                columns: new[] { "EmpresaId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TpvTerminales_FormaPagoPorDefectoId",
                table: "TpvTerminales",
                column: "FormaPagoPorDefectoId");

            migrationBuilder.AddForeignKey(
                name: "FK_LineasDocumento_TicketsTpv_TicketTpvId",
                table: "LineasDocumento",
                column: "TicketTpvId",
                principalTable: "TicketsTpv",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LineasDocumento_TicketsTpv_TicketTpvId",
                table: "LineasDocumento");

            migrationBuilder.DropTable(
                name: "EmpresaModulos");

            migrationBuilder.DropTable(
                name: "PagosTicketTpv");

            migrationBuilder.DropTable(
                name: "TpvMovimientosCaja");

            migrationBuilder.DropTable(
                name: "TicketsTpv");

            migrationBuilder.DropTable(
                name: "TpvSesionesCaja");

            migrationBuilder.DropTable(
                name: "TpvTerminales");

            migrationBuilder.DropIndex(
                name: "IX_LineasDocumento_TicketTpvId",
                table: "LineasDocumento");

            migrationBuilder.DropColumn(
                name: "TicketTpvId",
                table: "LineasDocumento");

            migrationBuilder.DropColumn(
                name: "IncluirAnioEnSerieDevolucionTicket",
                table: "ConfiguracionEmpresa");

            migrationBuilder.DropColumn(
                name: "IncluirAnioEnSerieTicket",
                table: "ConfiguracionEmpresa");

            migrationBuilder.DropColumn(
                name: "LongitudNumeroDevolucionTicket",
                table: "ConfiguracionEmpresa");

            migrationBuilder.DropColumn(
                name: "LongitudNumeroTicket",
                table: "ConfiguracionEmpresa");

            migrationBuilder.DropColumn(
                name: "NumeroDevolucionTicketActual",
                table: "ConfiguracionEmpresa");

            migrationBuilder.DropColumn(
                name: "NumeroTicketActual",
                table: "ConfiguracionEmpresa");

            migrationBuilder.DropColumn(
                name: "SerieDevolucionTicket",
                table: "ConfiguracionEmpresa");

            migrationBuilder.DropColumn(
                name: "SerieTicket",
                table: "ConfiguracionEmpresa");

            migrationBuilder.DropColumn(
                name: "TpvDescontarStock",
                table: "ConfiguracionEmpresa");

            migrationBuilder.DropColumn(
                name: "TpvPermitirStockNegativo",
                table: "ConfiguracionEmpresa");

            migrationBuilder.DropColumn(
                name: "TpvTextoPieTicket",
                table: "ConfiguracionEmpresa");

            migrationBuilder.DropColumn(
                name: "TpvPreciosIncluyenIVA",
                table: "ConfiguracionEmpresa");
        }
    }
}
