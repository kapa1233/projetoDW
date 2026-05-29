using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebServicos.Models;

namespace WebServicos.Data
{
    /// <summary>
    /// Contexto principal da base de dados da aplicação WebServicos.
    /// Herda de IdentityDbContext para integrar autenticação ASP.NET Core Identity
    /// com as tabelas de utilizadores e roles.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        /// <summary>
        /// Construtor que recebe as opções de configuração (connection string, provider, etc.).
        /// </summary>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ── Tabelas da aplicação (mapeadas para tabelas SQLite) ──

        /// <summary>Tabela de serviços oferecidos pela empresa.</summary>
        public DbSet<Servico> Servicos { get; set; }

        /// <summary>Tabela de categorias de serviços.</summary>
        public DbSet<CategoriaServico> CategoriasServico { get; set; }

        /// <summary>Tabela de pedidos submetidos pelos clientes.</summary>
        public DbSet<Pedido> Pedidos { get; set; }

        /// <summary>Tabela de junção muitos-para-muitos entre Pedido e Servico.</summary>
        public DbSet<PedidoServico> PedidoServicos { get; set; }

        /// <summary>Tabela de mensagens de chat por pedido.</summary>
        public DbSet<Mensagem> Mensagens { get; set; }

        /// <summary>Tabela de propostas de alteração submetidas por clientes.</summary>
        public DbSet<PedidoAlteracao> PedidoAlteracoes { get; set; }

        /// <summary>
        /// Configura as relações entre entidades e os dados de seed iniciais.
        /// Chamado automaticamente pelo EF Core ao criar/migrar a base de dados.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração base do Identity (tabelas de utilizadores, roles, claims, etc.)
            base.OnModelCreating(modelBuilder);

            // ── Chave composta para a tabela de junção PedidoServico ──
            // Garante unicidade por combinação PedidoId + ServicoId
            modelBuilder.Entity<PedidoServico>()
                .HasKey(ps => new { ps.PedidoId, ps.ServicoId });

            // ── Relação Pedido -> PedidoServico (um-para-muitos) ──
            modelBuilder.Entity<PedidoServico>()
                .HasOne(ps => ps.Pedido)
                .WithMany(p => p.PedidoServicos)
                .HasForeignKey(ps => ps.PedidoId);

            // ── Relação Servico -> PedidoServico (um-para-muitos) ──
            modelBuilder.Entity<PedidoServico>()
                .HasOne(ps => ps.Servico)
                .WithMany(s => s.PedidoServicos)
                .HasForeignKey(ps => ps.ServicoId);

            // ── Relação Servico -> CategoriaServico (muitos-para-um) ──
            // OnDelete SetNull: ao eliminar categoria, os serviços ficam sem categoria
            modelBuilder.Entity<Servico>()
                .HasOne(s => s.CategoriaServico)
                .WithMany(c => c.Servicos)
                .HasForeignKey(s => s.CategoriaServicoId)
                .OnDelete(DeleteBehavior.SetNull);

            // ── Relação Pedido -> ApplicationUser (muitos-para-um) ──
            // OnDelete Restrict: não permite eliminar cliente com pedidos associados
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Seed de Categorias (dados iniciais inseridos via migração) ──
            modelBuilder.Entity<CategoriaServico>().HasData(
                new CategoriaServico { Id = 1, Nome = "Websites", Descricao = "Criação e desenvolvimento de websites" },
                new CategoriaServico { Id = 2, Nome = "E-commerce", Descricao = "Lojas online e plataformas de venda" },
                new CategoriaServico { Id = 3, Nome = "SEO & Marketing", Descricao = "Otimização para motores de busca e marketing digital" },
                new CategoriaServico { Id = 4, Nome = "Manutenção", Descricao = "Suporte e manutenção de websites existentes" },
                new CategoriaServico { Id = 5, Nome = "Hardware & Software", Descricao = "Recuperação de dados, restauro de sistemas e assistência técnica de hardware" }
            );

            // ── Seed de Serviços (6 serviços pré-configurados com preços e ícones) ──
            modelBuilder.Entity<Servico>().HasData(
                new Servico
                {
                    Id = 1, Nome = "Website Institucional", Descricao = "Website profissional para a sua empresa com até 5 páginas, design responsivo e SEO básico.",
                    PrecoBase = 799.00m, Ativo = true, Icone = "bi-building", DataCriacao = new DateTime(2024, 1, 1), CategoriaServicoId = 1
                },
                new Servico
                {
                    Id = 2, Nome = "Loja Online", Descricao = "Plataforma de e-commerce completa com carrinho de compras, pagamentos e gestão de stock.",
                    PrecoBase = 1499.00m, Ativo = true, Icone = "bi-shop", DataCriacao = new DateTime(2024, 1, 1), CategoriaServicoId = 2
                },
                new Servico
                {
                    Id = 3, Nome = "Landing Page", Descricao = "Página de conversão otimizada para campanhas de marketing.",
                    PrecoBase = 349.00m, Ativo = true, Icone = "bi-layout-text-window", DataCriacao = new DateTime(2024, 1, 1), CategoriaServicoId = 1
                },
                new Servico
                {
                    Id = 4, Nome = "Otimização SEO", Descricao = "Auditoria e otimização completa do seu website para motores de busca.",
                    PrecoBase = 299.00m, Ativo = true, Icone = "bi-search", DataCriacao = new DateTime(2024, 1, 1), CategoriaServicoId = 3
                },
                new Servico
                {
                    Id = 5, Nome = "Manutenção Mensal", Descricao = "Suporte técnico, atualizações de segurança e backups regulares.",
                    PrecoBase = 89.00m, Ativo = true, Icone = "bi-tools", DataCriacao = new DateTime(2024, 1, 1), CategoriaServicoId = 4
                },
                new Servico
                {
                    Id = 6, Nome = "Recuperação de Software & Hardware", Descricao = "Restauro completo de sistemas operativos, recuperação de dados e reparação de hardware.",
                    PrecoBase = 3000.00m, Ativo = true, Icone = "bi-pc-display-horizontal", DataCriacao = new DateTime(2024, 1, 1), CategoriaServicoId = 5
                }
            );
        }
    }
}
