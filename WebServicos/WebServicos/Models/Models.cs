using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WebServicos.Models
{
    // ──────────────────────────────────────────────────────────
    // Utilizador (extende o IdentityUser do ASP.NET Core Identity)
    // ──────────────────────────────────────────────────────────
    public class ApplicationUser : IdentityUser
    {
        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        [StringLength(150, ErrorMessage = "O nome não pode exceder 150 caracteres.")]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; } = string.Empty;

        [Display(Name = "Data de Registo")]
        public DateTime DataRegisto { get; set; } = DateTime.UtcNow;

        // Relação: um utilizador pode ter muitos pedidos (cliente)
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }

    // ──────────────────────────────────────────────────────────
    // Serviço oferecido pela empresa
    // Relação muitos-para-muitos com Pedido (via PedidoServico)
    // ──────────────────────────────────────────────────────────
    public class Servico
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do serviço é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        [Display(Name = "Nome do Serviço")]
        public string Nome { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(1000, ErrorMessage = "A descrição não pode exceder 1000 caracteres.")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; } = string.Empty;

        [Required(ErrorMessage = "O preço base é obrigatório.")]
        [Range(0, 99999.99, ErrorMessage = "O preço deve estar entre 0 e 99999,99 €.")]
        [Display(Name = "Preço Base (€)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecoBase { get; set; }

        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        // Ícone Bootstrap Icons (ex: "bi-globe", "bi-shop")
        [StringLength(50)]
        [Display(Name = "Ícone")]
        public string Icone { get; set; } = "bi-globe";

        // FK para Categoria (muitos-para-um, opcional)
        public int? CategoriaServicoId { get; set; }

        [ForeignKey(nameof(CategoriaServicoId))]
        [Display(Name = "Categoria")]
        public CategoriaServico? CategoriaServico { get; set; }

        // Relação muitos-para-muitos com Pedido
        public ICollection<PedidoServico> PedidoServicos { get; set; } = new List<PedidoServico>();
    }

    // ──────────────────────────────────────────────────────────
    // Categoria de serviço (relação muitos-para-um com Servico)
    // ──────────────────────────────────────────────────────────
    public class CategoriaServico
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
        [StringLength(80, ErrorMessage = "O nome não pode exceder 80 caracteres.")]
        [Display(Name = "Categoria")]
        public string Nome { get; set; } = string.Empty;

        [StringLength(300)]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        // Relação: uma categoria tem muitos serviços (muitos-para-um)
        public ICollection<Servico> Servicos { get; set; } = new List<Servico>();
    }

    // ──────────────────────────────────────────────────────────
    // Pedido de um cliente
    // Relação muitos-para-um com ApplicationUser
    // Relação muitos-para-muitos com Servico (via PedidoServico)
    // ──────────────────────────────────────────────────────────
    public class Pedido
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O título do projeto é obrigatório.")]
        [StringLength(200, ErrorMessage = "O título não pode exceder 200 caracteres.")]
        [Display(Name = "Título do Projeto")]
        public string TituloProjeto { get; set; } = string.Empty;

        [Required(ErrorMessage = "A descrição do pedido é obrigatória.")]
        [StringLength(2000, ErrorMessage = "A descrição não pode exceder 2000 caracteres.")]
        [Display(Name = "Descrição do Pedido")]
        public string Descricao { get; set; } = string.Empty;

        [Display(Name = "Data do Pedido")]
        public DateTime DataPedido { get; set; } = DateTime.UtcNow;

        [Display(Name = "Prazo Estimado")]
        public DateTime? PrazoEstimado { get; set; }

        [Display(Name = "Estado")]
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendente;

        [Display(Name = "Orçamento Total (€)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? OrcamentoTotal { get; set; }

        [StringLength(1000)]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        [Url(ErrorMessage = "O endereço deve ser uma URL válida.")]
        [StringLength(500)]
        [Display(Name = "Endereço do Projeto (URL)")]
        public string? EnderecoHttp { get; set; }

        // FK para o cliente (muitos-para-um)
        [Required]
        public string ClienteId { get; set; } = string.Empty;

        [ForeignKey(nameof(ClienteId))]
        [Display(Name = "Cliente")]
        public ApplicationUser? Cliente { get; set; }

        // Relação muitos-para-muitos com Servico
        public ICollection<PedidoServico> PedidoServicos { get; set; } = new List<PedidoServico>();

        // Relação um-para-muitos com Mensagem
        public ICollection<Mensagem> Mensagens { get; set; } = new List<Mensagem>();
    }

    // ──────────────────────────────────────────────────────────
    // Tabela de junção: muitos-para-muitos entre Pedido e Servico
    // ──────────────────────────────────────────────────────────
    public class PedidoServico
    {
        public int PedidoId { get; set; }
        public Pedido? Pedido { get; set; }

        public int ServicoId { get; set; }
        public Servico? Servico { get; set; }

        [Display(Name = "Quantidade")]
        [Range(1, 100)]
        public int Quantidade { get; set; } = 1;

        [Display(Name = "Preço Acordado (€)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecoAcordado { get; set; }

        [StringLength(500)]
        [Display(Name = "Notas Específicas")]
        public string? Notas { get; set; }
    }

    // ──────────────────────────────────────────────────────────
    // Enumeração dos estados possíveis de um pedido
    // ──────────────────────────────────────────────────────────
    public enum EstadoPedido
    {
        [Display(Name = "Pendente")]
        Pendente = 0,

        [Display(Name = "Em Análise")]
        EmAnalise = 1,

        [Display(Name = "Aprovado")]
        Aprovado = 2,

        [Display(Name = "Em Desenvolvimento")]
        EmDesenvolvimento = 3,

        [Display(Name = "Em Revisão")]
        EmRevisao = 4,

        [Display(Name = "Concluído")]
        Concluido = 5,

        [Display(Name = "Cancelado")]
        Cancelado = 6
    }

    // ──────────────────────────────────────────────────────────
    // Mensagem de chat entre Cliente e Administrador
    // ──────────────────────────────────────────────────────────
    public class Mensagem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [ForeignKey(nameof(PedidoId))]
        [Display(Name = "Pedido")]
        public Pedido? Pedido { get; set; }

        [Required]
        public string RemetenteId { get; set; } = string.Empty;

        [ForeignKey(nameof(RemetenteId))]
        [Display(Name = "Remetente")]
        public ApplicationUser? Remetente { get; set; }

        [Required(ErrorMessage = "A mensagem é obrigatória.")]
        [StringLength(2000, ErrorMessage = "A mensagem não pode exceder 2000 caracteres.")]
        [Display(Name = "Mensagem")]
        public string Conteudo { get; set; } = string.Empty;

        [Display(Name = "Data/Hora")]
        public DateTime DataHora { get; set; } = DateTime.UtcNow;

        [Display(Name = "Lida")]
        public bool Lida { get; set; } = false;
    }

    // ──────────────────────────────────────────────────────────
    // Alterações Propostas pelo Cliente (Pendentes de Aprovação)
    // ──────────────────────────────────────────────────────────
    public class PedidoAlteracao
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [ForeignKey(nameof(PedidoId))]
        [Display(Name = "Pedido")]
        public Pedido? Pedido { get; set; }

        [StringLength(200)]
        [Display(Name = "Título Proposto")]
        public string? TituloProjetoProposto { get; set; }

        [StringLength(2000)]
        [Display(Name = "Descrição Proposta")]
        public string? DescricaoProposta { get; set; }

        [StringLength(1000)]
        [Display(Name = "Observações Propostas")]
        public string? ObservacoesProposta { get; set; }

        [Display(Name = "Data da Proposta")]
        public DateTime DataPropostas { get; set; } = DateTime.UtcNow;

        [Display(Name = "Estado")]
        public EstadoAlteracao Estado { get; set; } = EstadoAlteracao.Pendente;

        [StringLength(500)]
        [Display(Name = "Motivo da Rejeição")]
        public string? MotivoRejeicao { get; set; }

        [Display(Name = "Data da Decisão")]
        public DateTime? DataDecisao { get; set; }

        [Display(Name = "Decidido por")]
        public string? DecididoPorId { get; set; }

        [ForeignKey(nameof(DecididoPorId))]
        public ApplicationUser? DecididoPor { get; set; }
    }

    // ──────────────────────────────────────────────────────────
    // Estados das Alterações Propostas
    // ──────────────────────────────────────────────────────────
    public enum EstadoAlteracao
    {
        [Display(Name = "Pendente")]
        Pendente = 0,

        [Display(Name = "Aprovado")]
        Aprovado = 1,

        [Display(Name = "Rejeitado")]
        Rejeitado = 2
    }
}


