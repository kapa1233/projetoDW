using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebServicos.Models;

namespace WebServicos.Data
{
    /// <summary>
    /// Contexto da base de dados da aplicação WebServicos.
    /// Herda de IdentityDbContext para suporte a autenticação ASP.NET Core Identity.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets — tabelas da aplicação
        public DbSet<Servico> Servicos { get; set; }
        public DbSet<CategoriaServico> CategoriasServico { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<PedidoServico> PedidoServicos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Chave composta para a tabela de junção (muitos-para-muitos) ──
            modelBuilder.Entity<PedidoServico>()
                .HasKey(ps => new { ps.PedidoId, ps.ServicoId });

            // ── Relação Pedido -> PedidoServico ──
            modelBuilder.Entity<PedidoServico>()
                .HasOne(ps => ps.Pedido)
                .WithMany(p => p.PedidoServicos)
                .HasForeignKey(ps => ps.PedidoId);

            // ── Relação Servico -> PedidoServico ──
            modelBuilder.Entity<PedidoServico>()
                .HasOne(ps => ps.Servico)
                .WithMany(s => s.PedidoServicos)
                .HasForeignKey(ps => ps.ServicoId);

            // ── Relação muitos-para-um: Pedido -> ApplicationUser (Cliente) ──
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Cliente)
                .WithMany(u => u.Pedidos)
                .HasForeignKey(p => p.ClienteId)
                .OnDelete(DeleteBehavior.Restrict);

            // ── Seed de Categorias ──
            modelBuilder.Entity<CategoriaServico>().HasData(
                new CategoriaServico { Id = 1, Nome = "Websites", Descricao = "Criação e desenvolvimento de websites" },
                new CategoriaServico { Id = 2, Nome = "E-commerce", Descricao = "Lojas online e plataformas de venda" },
                new CategoriaServico { Id = 3, Nome = "SEO & Marketing", Descricao = "Otimização para motores de busca e marketing digital" },
                new CategoriaServico { Id = 4, Nome = "Manutenção", Descricao = "Suporte e manutenção de websites existentes" }
            );

            // ── Seed de Serviços ──
            modelBuilder.Entity<Servico>().HasData(
                new Servico
                {
                    Id = 1, Nome = "Website Institucional", Descricao = "Website profissional para a sua empresa com até 5 páginas, design responsivo e SEO básico.",
                    PrecoBase = 799.00m, Ativo = true, Icone = "bi-building", DataCriacao = new DateTime(2024, 1, 1)
                },
                new Servico
                {
                    Id = 2, Nome = "Loja Online", Descricao = "Plataforma de e-commerce completa com carrinho de compras, pagamentos e gestão de stock.",
                    PrecoBase = 1499.00m, Ativo = true, Icone = "bi-shop", DataCriacao = new DateTime(2024, 1, 1)
                },
                new Servico
                {
                    Id = 3, Nome = "Landing Page", Descricao = "Página de conversão otimizada para campanhas de marketing.",
                    PrecoBase = 349.00m, Ativo = true, Icone = "bi-layout-text-window", DataCriacao = new DateTime(2024, 1, 1)
                },
                new Servico
                {
                    Id = 4, Nome = "Otimização SEO", Descricao = "Auditoria e otimização completa do seu website para motores de busca.",
                    PrecoBase = 299.00m, Ativo = true, Icone = "bi-search", DataCriacao = new DateTime(2024, 1, 1)
                },
                new Servico
                {
                    Id = 5, Nome = "Manutenção Mensal", Descricao = "Suporte técnico, atualizações de segurança e backups regulares.",
                    PrecoBase = 89.00m, Ativo = true, Icone = "bi-tools", DataCriacao = new DateTime(2024, 1, 1)
                }
            );
        }
    }
}
