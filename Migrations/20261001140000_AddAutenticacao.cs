using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
namespace ControleSalas.Migrations
{
    public partial class AddAutenticacao : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(name: "EmailNormalizado", table: "Usuarios", type: "TEXT", nullable: true);
            migrationBuilder.AddColumn<string>(name: "SenhaHash", table: "Usuarios", type: "TEXT", nullable: false, defaultValue: "");
            migrationBuilder.AddColumn<string>(name: "Perfil", table: "Usuarios", type: "TEXT", nullable: false, defaultValue: "Usuario");
            migrationBuilder.CreateIndex(name: "IX_Usuarios_EmailNormalizado", table: "Usuarios", column: "EmailNormalizado", unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(name: "IX_Usuarios_EmailNormalizado", table: "Usuarios");
            migrationBuilder.DropColumn(name: "EmailNormalizado", table: "Usuarios");
            migrationBuilder.DropColumn(name: "SenhaHash", table: "Usuarios");
            migrationBuilder.DropColumn(name: "Perfil", table: "Usuarios");
        }
    }
}
