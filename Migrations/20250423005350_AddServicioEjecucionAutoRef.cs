using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPOrchestratorAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddServicioEjecucionAutoRef : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
{
    // -- Solo crear la tabla ServicioEjecucion (si aún no existe) --
    migrationBuilder.CreateTable(
        name: "ServicioEjecucion",
        columns: table => new
        {
            Id                             = table.Column<int>(type: "int", nullable: false)
                                                  .Annotation("SqlServer:Identity", "1, 1"),
            ServicioId                     = table.Column<int>(type: "int", nullable: false),
            ServicioConfiguracionId        = table.Column<int>(type: "int", nullable: false),
            ServicioDesencadenadorId       = table.Column<int>(type: "int", nullable: true),
            FechaEjecucion                 = table.Column<DateTime>(type: "datetime2", nullable: false),
            Duracion                       = table.Column<double>(type: "float", nullable: false),
            Estado                         = table.Column<bool>(type: "bit", nullable: false),
            MensajeError                   = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Parametros                     = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Resultado                      = table.Column<string>(type: "nvarchar(max)", nullable: true),
            CamposExtra                    = table.Column<string>(type: "nvarchar(max)", nullable: true),
            ServicioEjecucionDesencadenadorId = table.Column<int>(type: "int", nullable: true),
            // Campos de auditoría heredados...
            CreatedAt                      = table.Column<DateTime>(type: "datetime2", nullable: false),
            CreatedBy                      = table.Column<string>(type: "nvarchar(max)", nullable: false),
            UpdatedAt                      = table.Column<DateTime>(type: "datetime2", nullable: true),
            UpdatedBy                      = table.Column<string>(type: "nvarchar(max)", nullable: true),
            DeletedAt                      = table.Column<DateTime>(type: "datetime2", nullable: true),
            DeletedBy                      = table.Column<string>(type: "nvarchar(max)", nullable: true),
            Deleted                         = table.Column<bool>(type: "bit", nullable: false)
        },
        constraints: table =>
        {
            table.PrimaryKey("PK_ServicioEjecucion", x => x.Id);
            table.ForeignKey(
                name: "FK_ServicioEjecucion_ServicioEjecucion_ServicioEjecucionDesencadenadorId",
                column: x => x.ServicioEjecucionDesencadenadorId,
                principalTable: "ServicioEjecucion",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        });

    migrationBuilder.CreateIndex(
        name: "IX_ServicioEjecucion_ServicioEjecucionDesencadenadorId",
        table: "ServicioEjecucion",
        column: "ServicioEjecucionDesencadenadorId");
}

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ServicioEjecucion");
        }
    }
}
