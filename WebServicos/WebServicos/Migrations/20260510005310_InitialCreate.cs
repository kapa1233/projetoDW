using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WebServicos.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    NomeCompleto = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DataRegisto = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CategoriasServico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasServico", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    ProviderKey = table.Column<string>(type: "text", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Pedidos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TituloProjeto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DataPedido = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    PrazoEstimado = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    OrcamentoTotal = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Observacoes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    EnderecoHttp = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ClienteId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedidos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedidos_AspNetUsers_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Servicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    PrecoBase = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Icone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CategoriaServicoId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Servicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Servicos_CategoriasServico_CategoriaServicoId",
                        column: x => x.CategoriaServicoId,
                        principalTable: "CategoriasServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Mensagens",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoId = table.Column<int>(type: "integer", nullable: false),
                    RemetenteId = table.Column<string>(type: "text", nullable: false),
                    Conteudo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DataHora = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Lida = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "PedidoAlteracoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PedidoId = table.Column<int>(type: "integer", nullable: false),
                    TituloProjetoProposto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DescricaoProposta = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ObservacoesProposta = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DataPropostas = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    MotivoRejeicao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DataDecisao = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    DecididoPorId = table.Column<string>(type: "text", nullable: true)
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

            migrationBuilder.CreateTable(
                name: "PedidoServicos",
                columns: table => new
                {
                    PedidoId = table.Column<int>(type: "integer", nullable: false),
                    ServicoId = table.Column<int>(type: "integer", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    PrecoAcordado = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoServicos", x => new { x.PedidoId, x.ServicoId });
                    table.ForeignKey(
                        name: "FK_PedidoServicos_Pedidos_PedidoId",
                        column: x => x.PedidoId,
                        principalTable: "Pedidos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PedidoServicos_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "CategoriasServico",
                columns: new[] { "Id", "Descricao", "Nome" },
                values: new object[,]
                {
                    { 1, "Criação e desenvolvimento de websites", "Websites" },
                    { 2, "Lojas online e plataformas de venda", "E-commerce" },
                    { 3, "Otimização para motores de busca e marketing digital", "SEO & Marketing" },
                    { 4, "Suporte e manutenção de websites existentes", "Manutenção" },
                    { 5, "Recuperação de dados, restauro de sistemas e assistência técnica de hardware", "Hardware & Software" }
                });

            migrationBuilder.InsertData(
                table: "Servicos",
                columns: new[] { "Id", "Ativo", "CategoriaServicoId", "DataCriacao", "Descricao", "Icone", "Nome", "PrecoBase" },
                values: new object[,]
                {
                    { 1, true, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Website profissional para a sua empresa com até 5 páginas, design responsivo e SEO básico.", "bi-building", "Website Institucional", 799.00m },
                    { 2, true, 2, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Plataforma de e-commerce completa com carrinho de compras, pagamentos e gestão de stock.", "bi-shop", "Loja Online", 1499.00m },
                    { 3, true, 1, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Página de conversão otimizada para campanhas de marketing.", "bi-layout-text-window", "Landing Page", 349.00m },
                    { 4, true, 3, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Auditoria e otimização completa do seu website para motores de busca.", "bi-search", "Otimização SEO", 299.00m },
                    { 5, true, 4, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Suporte técnico, atualizações de segurança e backups regulares.", "bi-tools", "Manutenção Mensal", 89.00m },
                    { 6, true, 5, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Restauro completo de sistemas operativos, recuperação de dados e reparação de hardware.", "bi-pc-display-horizontal", "Recuperação de Software & Hardware", 3000.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_PedidoId",
                table: "Mensagens",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Mensagens_RemetenteId",
                table: "Mensagens",
                column: "RemetenteId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoAlteracoes_DecididoPorId",
                table: "PedidoAlteracoes",
                column: "DecididoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoAlteracoes_PedidoId",
                table: "PedidoAlteracoes",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ClienteId",
                table: "Pedidos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoServicos_ServicoId",
                table: "PedidoServicos",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_Servicos_CategoriaServicoId",
                table: "Servicos",
                column: "CategoriaServicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Mensagens");

            migrationBuilder.DropTable(
                name: "PedidoAlteracoes");

            migrationBuilder.DropTable(
                name: "PedidoServicos");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Pedidos");

            migrationBuilder.DropTable(
                name: "Servicos");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "CategoriasServico");
        }
    }
}
