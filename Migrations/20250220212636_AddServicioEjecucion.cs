using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPOrchestratorAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddServicioEjecucion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ServicioEjecucion",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServicioId = table.Column<int>(type: "int", nullable: false),
                    ServicioConfiguracionId = table.Column<int>(type: "int", nullable: false),
                    ServicioDesencadenadorId = table.Column<int>(type: "int", nullable: true),
                    FechaEjecucion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Duracion = table.Column<double>(type: "float", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    MensajeError = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Parametros = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Resultado = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CamposExtra = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Deleted = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table => { table.PrimaryKey("PK_ServicioEjecucion", x => x.Id); });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ServicioEjecucion");
            
        }
    }
}
