using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebServicos.Migrations
{
    /// <inheritdoc />
    public partial class AddChatAndUrlToPedidos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EnderecoHttp",
                table: "Pedidos",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Mensagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PedidoId = table.Column<int>(type: "INTEGER", nullable: false),
                    RemetenteId = table.Column<string>(type: "TEXT", nullable: false),
                    Conteudo = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    DataHora = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Lida = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mensagens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Mensagens_AspNetUsers_RemetenteId",
                        column: x => x.RemetenteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Mensagens_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_PedidoId",
                table: "Mensagens",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_RemetenteId",
                table: "Mensagens",
                column: "RemetenteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mensagens");

            migrationBuilder.DropColumn(
                name: "EnderecoHttp",
                table: "Pedidos");
        }
    }
}
