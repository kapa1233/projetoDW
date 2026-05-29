using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace WebServicos.Models
{
    /// <summary>
    /// Utilizador da aplicação. Estende o IdentityUser do ASP.NET Core Identity
    /// com campos adicionais como nome completo e data de registo.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        /// <summary>Nome completo do utilizador, obrigatório no registo.</summary>
        [Required(ErrorMessage = "O nome completo é obrigatório.")]
        [StringLength(150, ErrorMessage = "O nome não pode exceder 150 caracteres.")]
        [Display(Name = "Nome Completo")]
        public string NomeCompleto { get; set; } = string.Empty;

        /// <summary>Data e hora UTC em que o utilizador se registou.</summary>
        [Display(Name = "Data de Registo")]
        public DateTime DataRegisto { get; set; } = DateTime.UtcNow;

        /// <summary>Lista de pedidos submetidos por este cliente.</summary>
        public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
    }

    /// <summary>
    /// Serviço oferecido pela empresa (ex: Website, Loja Online, SEO).
    /// Relaciona-se com Pedido de forma muitos-para-muitos através de PedidoServico.
    /// </summary>
    public class Servico
    {
        /// <summary>Identificador único do serviço (chave primária).</summary>
        [Key]
        public int Id { get; set; }

        /// <summary>Nome do serviço, ex: "Website Institucional".</summary>
        [Required(ErrorMessage = "O nome do serviço é obrigatório.")]
        [StringLength(100, ErrorMessage = "O nome não pode exceder 100 caracteres.")]
        [Display(Name = "Nome do Serviço")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>Descrição detalhada do que o serviço inclui.</summary>
        [Required(ErrorMessage = "A descrição é obrigatória.")]
        [StringLength(1000, ErrorMessage = "A descrição não pode exceder 1000 caracteres.")]
        [Display(Name = "Descrição")]
        public string Descricao { get; set; } = string.Empty;

        /// <summary>Preço base do serviço em euros, sem IVA.</summary>
        [Required(ErrorMessage = "O preço base é obrigatório.")]
        [Range(0, 99999.99, ErrorMessage = "O preço deve estar entre 0 e 99999,99 €.")]
        [Display(Name = "Preço Base (€)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal PrecoBase { get; set; }

        /// <summary>Indica se o serviço está disponível para novos pedidos.</summary>
        [Display(Name = "Ativo")]
        public bool Ativo { get; set; } = true;

        /// <summary>Data UTC em que o serviço foi criado no sistema.</summary>
        [Display(Name = "Data de Criação")]
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        /// <summary>Classe do ícone Bootstrap Icons, ex: "bi-globe" ou "bi-shop".</summary>
        [StringLength(50)]
        [Display(Name = "Ícone")]
        public string Icone { get; set; } = "bi-globe";

        /// <summary>Chave estrangeira para a categoria do serviço (opcional).</summary>
        public int? CategoriaServicoId { get; set; }

        /// <summary>Categoria a que este serviço pertence (navegação).</summary>
        [ForeignKey(nameof(CategoriaServicoId))]
        [Display(Name = "Categoria")]
        public CategoriaServico? CategoriaServico { get; set; }

        /// <summary>Pedidos que incluem este serviço (tabela de junção).</summary>
        public ICollection<PedidoServico> PedidoServicos { get; set; } = new List<PedidoServico>();
    }

    /// <summary>
    /// Categoria que agrupa serviços relacionados (ex: Websites, E-commerce, SEO).
    /// Relação um-para-muitos com Servico.
    /// </summary>
    public class CategoriaServico
    {
        /// <summary>Identificador único da categoria (chave primária).</summary>
        [Key]
        public int Id { get; set; }

        /// <summary>Nome da categoria, ex: "Websites" ou "E-commerce".</summary>
        [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
        [StringLength(80, ErrorMessage = "O nome não pode exceder 80 caracteres.")]
        [Display(Name = "Categoria")]
        public string Nome { get; set; } = string.Empty;

        /// <summary>Descrição opcional da categoria.</summary>
        [StringLength(300)]
        [Display(Name = "Descrição")]
        public string? Descricao { get; set; }

        /// <summary>Serviços pertencentes a esta categoria.</summary>
        public ICollection<Servico> Servicos { get; set; } = new List<Servico>();
    }

    /// <summary>
    /// Pedido submetido por um cliente para um ou mais serviços.
    /// Relação muitos-para-um com ApplicationUser (cliente).
    /// Relação muitos-para-muitos com Servico (via PedidoServico).
    /// </summary>
    public class Pedido
    {
        /// <summary>Identificador único do pedido (chave primária).</summary>
        [Key]
        public int Id { get; set; }

        /// <summary>Nome do projeto que o cliente quer desenvolver.</summary>
        [Required(ErrorMessage = "O título do projeto é obrigatório.")]
        [StringLength(200, ErrorMessage = "O título não pode exceder 200 caracteres.")]
        [Display(Name = "Título do Projeto")]
        public string TituloProjeto { get; set; } = string.Empty;

        /// <summary>Descrição detalhada dos requisitos do projeto.</summary>
        [Required(ErrorMessage = "A descrição do pedido é obrigatória.")]
        [StringLength(2000, ErrorMessage = "A descrição não pode exceder 2000 caracteres.")]
        [Display(Name = "Descrição do Pedido")]
        public string Descricao { get; set; } = string.Empty;

        /// <summary>Data e hora UTC em que o pedido foi submetido.</summary>
        [Display(Name = "Data do Pedido")]
        public DateTime DataPedido { get; set; } = DateTime.UtcNow;

        /// <summary>Prazo desejado pelo cliente para conclusão (opcional).</summary>
        [Display(Name = "Prazo Estimado")]
        public DateTime? PrazoEstimado { get; set; }

        /// <summary>Estado atual do pedido no fluxo de trabalho.</summary>
        [Display(Name = "Estado")]
        public EstadoPedido Estado { get; set; } = EstadoPedido.Pendente;

        /// <summary>Soma dos preços base dos serviços selecionados, calculada ao criar o pedido.</summary>
        [Display(Name = "Orçamento Total (€)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? OrcamentoTotal { get; set; }

        /// <summary>Notas adicionais do cliente sobre o projeto.</summary>
        [StringLength(1000)]
        [Display(Name = "Observações")]
        public string? Observacoes { get; set; }

        /// <summary>URL do projeto após conclusão, preenchido pelo administrador.</summary>
        [Url(ErrorMessage = "O endereço deve ser uma URL válida.")]
        [StringLength(500)]
        [Display(Name = "Endereço do Projeto (URL)")]
        public string? EnderecoHttp { get; set; }

        /// <summary>Chave estrangeira para o utilizador que submeteu o pedido.</summary>
        [Required]
        public string ClienteId { get; set; } = string.Empty;

        /// <summary>Cliente que submeteu o pedido (navegação).</summary>
        [ForeignKey(nameof(ClienteId))]
        [Display(Name = "Cliente")]
        public ApplicationUser? Cliente { get; set; }

        /// <summary>Serviços incluídos neste pedido (tabela de junção).</summary>
        public ICollection<PedidoServico> PedidoServicos { get; set; } = new List<PedidoServico>();

        /// <summary>Mensagens de chat trocadas entre o cliente e o administrador.</summary>
        public ICollection<Mensagem> Mensagens { get; set; } = new List<Mensagem>();
    }

    /// <summary>
    /// Tabela de junção para a relação muitos-para-muitos entre Pedido e Servico.
    /// Armazena também o preço acordado e notas específicas para cada serviço do pedido.
    /// </summary>
    public class PedidoServico
    {
        /// <summary>Chave estrangeira para o pedido (parte da chave composta).</summary>
        public int PedidoId { get; set; }

        /// <summary>Pedido associado (navegação).</summary>
        public Pedido? Pedido { get; set; }

        /// <summary>Chave estrangeira para o serviço (parte da chave composta).</summary>
        public int ServicoId { get; set; }

        /// <summary>Serviço associado (navegação).</summary>
        public Servico? Servico { get; set; }

        /// <summary>Quantidade de unidades do serviço contratadas.</summary>
        [Display(Name = "Quantidade")]
        [Range(1, 100)]
        public int Quantidade { get; set; } = 1;

        /// <summary>Preço final acordado para este serviço neste pedido (pode diferir do PrecoBase).</summary>
        [Display(Name = "Preço Acordado (€)")]
        [Column(TypeName = "decimal(10,2)")]
        public decimal? PrecoAcordado { get; set; }

        /// <summary>Requisitos ou notas específicas para este serviço dentro do pedido.</summary>
        [StringLength(500)]
        [Display(Name = "Notas Específicas")]
        public string? Notas { get; set; }
    }

    /// <summary>
    /// Enumeração dos estados possíveis de um pedido ao longo do seu ciclo de vida.
    /// </summary>
    public enum EstadoPedido
    {
        /// <summary>Pedido recebido, ainda não analisado pelo administrador.</summary>
        [Display(Name = "Pendente")]
        Pendente = 0,

        /// <summary>Administrador está a analisar os requisitos do pedido.</summary>
        [Display(Name = "Em Análise")]
        EmAnalise = 1,

        /// <summary>Pedido aprovado e orçamento aceite, aguarda início do desenvolvimento.</summary>
        [Display(Name = "Aprovado")]
        Aprovado = 2,

        /// <summary>Projeto em fase de desenvolvimento activo.</summary>
        [Display(Name = "Em Desenvolvimento")]
        EmDesenvolvimento = 3,

        /// <summary>Desenvolvimento concluído, aguarda revisão e aprovação do cliente.</summary>
        [Display(Name = "Em Revisão")]
        EmRevisao = 4,

        /// <summary>Projeto entregue e aceite pelo cliente.</summary>
        [Display(Name = "Concluído")]
        Concluido = 5,

        /// <summary>Pedido cancelado pelo cliente ou pelo administrador.</summary>
        [Display(Name = "Cancelado")]
        Cancelado = 6
    }

    /// <summary>
    /// Mensagem de chat enviada no contexto de um pedido.
    /// Permite comunicação direta entre o cliente e o administrador.
    /// </summary>
    public class Mensagem
    {
        /// <summary>Identificador único da mensagem (chave primária).</summary>
        [Key]
        public int Id { get; set; }

        /// <summary>Chave estrangeira para o pedido a que a mensagem pertence.</summary>
        [Required]
        public int PedidoId { get; set; }

        /// <summary>Pedido associado a esta mensagem (navegação).</summary>
        [ForeignKey(nameof(PedidoId))]
        [Display(Name = "Pedido")]
        public Pedido? Pedido { get; set; }

        /// <summary>Chave estrangeira para o utilizador que enviou a mensagem.</summary>
        [Required]
        public string RemetenteId { get; set; } = string.Empty;

        /// <summary>Utilizador que enviou a mensagem (navegação).</summary>
        [ForeignKey(nameof(RemetenteId))]
        [Display(Name = "Remetente")]
        public ApplicationUser? Remetente { get; set; }

        /// <summary>Texto da mensagem enviada.</summary>
        [Required(ErrorMessage = "A mensagem é obrigatória.")]
        [StringLength(2000, ErrorMessage = "A mensagem não pode exceder 2000 caracteres.")]
        [Display(Name = "Mensagem")]
        public string Conteudo { get; set; } = string.Empty;

        /// <summary>Data e hora UTC em que a mensagem foi enviada.</summary>
        [Display(Name = "Data/Hora")]
        public DateTime DataHora { get; set; } = DateTime.UtcNow;

        /// <summary>Indica se a mensagem foi lida pelo destinatário.</summary>
        [Display(Name = "Lida")]
        public bool Lida { get; set; } = false;
    }

    /// <summary>
    /// Proposta de alteração submetida pelo cliente a um pedido existente.
    /// Requer aprovação do administrador antes de ser aplicada ao pedido original.
    /// </summary>
    public class PedidoAlteracao
    {
        /// <summary>Identificador único da alteração proposta (chave primária).</summary>
        [Key]
        public int Id { get; set; }

        /// <summary>Chave estrangeira para o pedido que se pretende alterar.</summary>
        [Required]
        public int PedidoId { get; set; }

        /// <summary>Pedido original que será modificado se aprovado (navegação).</summary>
        [ForeignKey(nameof(PedidoId))]
        [Display(Name = "Pedido")]
        public Pedido? Pedido { get; set; }

        /// <summary>Novo título proposto para o projeto (null = sem alteração).</summary>
        [StringLength(200)]
        [Display(Name = "Título Proposto")]
        public string? TituloProjetoProposto { get; set; }

        /// <summary>Nova descrição proposta (null = sem alteração).</summary>
        [StringLength(2000)]
        [Display(Name = "Descrição Proposta")]
        public string? DescricaoProposta { get; set; }

        /// <summary>Novas observações propostas (null = sem alteração).</summary>
        [StringLength(1000)]
        [Display(Name = "Observações Propostas")]
        public string? ObservacoesProposta { get; set; }

        /// <summary>Data UTC em que o cliente submeteu a proposta de alteração.</summary>
        [Display(Name = "Data da Proposta")]
        public DateTime DataPropostas { get; set; } = DateTime.UtcNow;

        /// <summary>Estado atual da proposta (Pendente, Aprovado ou Rejeitado).</summary>
        [Display(Name = "Estado")]
        public EstadoAlteracao Estado { get; set; } = EstadoAlteracao.Pendente;

        /// <summary>Motivo da rejeição, preenchido pelo administrador ao rejeitar.</summary>
        [StringLength(500)]
        [Display(Name = "Motivo da Rejeição")]
        public string? MotivoRejeicao { get; set; }

        /// <summary>Data UTC em que o administrador aprovou ou rejeitou a proposta.</summary>
        [Display(Name = "Data da Decisão")]
        public DateTime? DataDecisao { get; set; }

        /// <summary>ID do administrador que tomou a decisão.</summary>
        [Display(Name = "Decidido por")]
        public string? DecididoPorId { get; set; }

        /// <summary>Administrador que tomou a decisão (navegação).</summary>
        [ForeignKey(nameof(DecididoPorId))]
        public ApplicationUser? DecididoPor { get; set; }
    }

    /// <summary>
    /// Estados possíveis de uma proposta de alteração a um pedido.
    /// </summary>
    public enum EstadoAlteracao
    {
        /// <summary>Proposta submetida, aguarda decisão do administrador.</summary>
        [Display(Name = "Pendente")]
        Pendente = 0,

        /// <summary>Administrador aprovou e aplicou as alterações ao pedido.</summary>
        [Display(Name = "Aprovado")]
        Aprovado = 1,

        /// <summary>Administrador rejeitou a proposta de alteração.</summary>
        [Display(Name = "Rejeitado")]
        Rejeitado = 2
    }
}
