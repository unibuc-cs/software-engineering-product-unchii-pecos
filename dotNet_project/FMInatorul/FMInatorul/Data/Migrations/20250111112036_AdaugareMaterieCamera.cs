using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FMInatorul.Migrations
{
    public partial class AdaugareMaterieCamera : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaterieID",
                table: "Rooms",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_MaterieID",
                table: "Rooms",
                column: "MaterieID");

            migrationBuilder.AddForeignKey(
                name: "FK_Rooms_Materii_MaterieID",
                table: "Rooms",
                column: "MaterieID",
                principalTable: "Materii",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Rooms_Materii_MaterieID",
                table: "Rooms");

            migrationBuilder.DropIndex(
                name: "IX_Rooms_MaterieID",
                table: "Rooms");

            migrationBuilder.DropColumn(
                name: "MaterieID",
                table: "Rooms");
        }
    }
}
