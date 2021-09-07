using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace M4.Infrastructure.Migrations.MagicFormulaDb
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmailSolicitacao",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Titulo = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    Mensagem = table.Column<string>(type: "VARCHAR(1024)", nullable: false),
                    Destinatarios = table.Column<string>(type: "VARCHAR(100)", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "DATETIME", nullable: false),
                    Enviado = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmailSolicitacao", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmailSolicitacao");
        }
    }
}
