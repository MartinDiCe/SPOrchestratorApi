using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SPOrchestratorAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddGuardarRegistrosAndContinuarCon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "GuardarRegistros",
                table: "ServicioConfiguracion",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "ContinuarCon",
                table: "ServicioConfiguracion",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GuardarRegistros",
                table: "ServicioConfiguracion");

            migrationBuilder.DropColumn(
                name: "ContinuarCon",
                table: "ServicioConfiguracion");
        }
    }
}
