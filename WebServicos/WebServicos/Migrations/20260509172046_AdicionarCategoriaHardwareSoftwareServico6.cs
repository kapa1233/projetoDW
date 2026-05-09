using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebServicos.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCategoriaHardwareSoftwareServico6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servicos_CategoriasServico_CategoriaServicoId",
                table: "Servicos");

            migrationBuilder.CreateTable(
                name: "PedidoAlteracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PedidoId = table.Column<int>(type: "INTEGER", nullable: false),
                    TituloProjetoProposto = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    DescricaoProposta = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    ObservacoesProposta = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    DataPropostas = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    MotivoRejeicao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    DataDecisao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DecididoPorId = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoAlteracoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoAlteracoes_AspNetUsers_DecididoPorId",
                        column: x => x.DecididoPorId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_PedidoAlteracoes_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CategoriasServico",
                columns: new[] { "Id", "Descricao", "Nome" },
                values: new object[] { 5, "Recuperação de dados, restauro de sistemas e assistência técnica de hardware", "Hardware & Software" });

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 1,
                column: "CategoriaServicoId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 2,
                column: "CategoriaServicoId",
                value: 2);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 3,
                column: "CategoriaServicoId",
                value: 1);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 4,
                column: "CategoriaServicoId",
                value: 3);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 5,
                column: "CategoriaServicoId",
                value: 4);

            migrationBuilder.InsertData(
                table: "Servicos",
                columns: new[] { "Id", "Ativo", "CategoriaServicoId", "DataCriacao", "Descricao", "Icone", "Nome", "PrecoBase" },
                values: new object[] { 6, true, 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Restauro completo de sistemas operativos, recuperação de dados e reparação de hardware.", "bi-pc-display-horizontal", "Recuperação de Software & Hardware", 3000.00m });

            migrationBuilder.CreateIndex(
                name: "IX_PedidoAlteracoes_DecididoPorId",
                table: "PedidoAlteracoes",
                column: "DecididoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoAlteracoes_PedidoId",
                table: "PedidoAlteracoes",
                column: "PedidoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Servicos_CategoriasServico_CategoriaServicoId",
                table: "Servicos",
                column: "CategoriaServicoId",
                principalTable: "CategoriasServico",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servicos_CategoriasServico_CategoriaServicoId",
                table: "Servicos");

            migrationBuilder.DropTable(
                name: "PedidoAlteracoes");

            migrationBuilder.DeleteData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "CategoriasServico",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 1,
                column: "CategoriaServicoId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 2,
                column: "CategoriaServicoId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 3,
                column: "CategoriaServicoId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 4,
                column: "CategoriaServicoId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Servicos",
                keyColumn: "Id",
                keyValue: 5,
                column: "CategoriaServicoId",
                value: null);

            migrationBuilder.AddForeignKey(
                name: "FK_Servicos_CategoriasServico_CategoriaServicoId",
                table: "Servicos",
                column: "CategoriaServicoId",
                principalTable: "CategoriasServico",
                principalColumn: "Id");
        }
    }
}
