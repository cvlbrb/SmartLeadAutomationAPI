using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SmartLeadAutomation.Models.Enums;

namespace SmartLeadAutomation.Models.Entities
{
    /// <summary>
    /// Entidade principal que representa um lead no sistema.
    /// Contém todas as informações de contato, classificação e histórico do lead.
    /// </summary>
    public class Lead
    {
        #region Propriedades de Identificação

        /// <summary>
        /// Identificador único do lead (chave primária)
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// Identificador externo para integração com outros sistemas (CRM, etc.)
        /// </summary>
        [MaxLength(100)]
        public string? ExternalId { get; set; }

        #endregion

        #region Propriedades de Contato

        /// <summary>
        /// Nome completo do lead
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório")]
        [MaxLength(200, ErrorMessage = "O nome deve ter no máximo 200 caracteres")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Endereço de email do lead (deve ser único no sistema)
        /// </summary>
        [Required(ErrorMessage = "O email é obrigatório")]
        [MaxLength(255, ErrorMessage = "O email deve ter no máximo 255 caracteres")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Número de telefone para contato
        /// </summary>
        [MaxLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres")]
        [Phone(ErrorMessage = "Formato de telefone inválido")]
        public string? Phone { get; set; }

        /// <summary>
        /// Nome da empresa ou organização do lead
        /// </summary>
        [MaxLength(200, ErrorMessage = "O nome da empresa deve ter no máximo 200 caracteres")]
        public string? Company { get; set; }

        /// <summary>
        /// Cargo ou posição do lead na empresa
        /// </summary>
        [MaxLength(100, ErrorMessage = "O cargo deve ter no máximo 100 caracteres")]
        public string? JobTitle { get; set; }

        #endregion

        #region Propriedades de Classificação

        /// <summary>
        /// Nível de prioridade calculado automaticamente (Alta, Média, Baixa)
        /// </summary>
        [Required]
        public PriorityLevel Priority { get; set; } = PriorityLevel.Baixa;

        /// <summary>
        /// Pontuação numérica de qualidade do lead (0-100)
        /// </summary>
        [Range(0, 100, ErrorMessage = "A pontuação deve estar entre 0 e 100")]
        public int Score { get; set; } = 0;

        /// <summary>
        /// Status atual do lead no funil de vendas
        /// </summary>
        [Required]
        public LeadStatus Status { get; set; } = LeadStatus.Novo;

        /// <summary>
        /// Canal de origem do lead
        /// </summary>
        [Required]
        public LeadSource Source { get; set; } = LeadSource.Outro;

        #endregion

        #region Propriedades de Valor e Negócio

        /// <summary>
        /// Valor estimado do negócio em reais
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 999999999.99, ErrorMessage = "O valor estimado deve ser positivo")]
        public decimal? EstimatedValue { get; set; }

        /// <summary>
        /// Probabilidade de conversão (0-100%)
        /// </summary>
        [Range(0, 100, ErrorMessage = "A probabilidade deve estar entre 0 e 100")]
        public int? ConversionProbability { get; set; }

        #endregion

        #region Propriedades de Engajamento

        /// <summary>
        /// Indica se o lead optou por receber comunicações de marketing
        /// </summary>
        [Required]
        public bool MarketingConsent { get; set; } = false;

        /// <summary>
        /// Data do último contato ou interação com o lead
        /// </summary>
        public DateTime? LastContactDate { get; set; }

        /// <summary>
        /// Número de interações realizadas com o lead
        /// </summary>
        [Range(0, int.MaxValue)]
        public int InteractionCount { get; set; } = 0;

        /// <summary>
        /// Indica se o lead respondeu às comunicações
        /// </summary>
        [Required]
        public bool HasResponded { get; set; } = false;

        #endregion

        #region Propriedades de Auditoria

        /// <summary>
        /// Data de criação do registro (preenchida automaticamente)
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Data da última atualização do registro
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Data de exclusão lógica (null se ativo)
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// Indica se o registro está ativo (exclusão lógica)
        /// </summary>
        [Required]
        public bool IsActive { get; set; } = true;

        #endregion

        #region Propriedades de Metadados

        /// <summary>
        /// Observações ou notas sobre o lead
        /// </summary>
        [MaxLength(2000, ErrorMessage = "As notas devem ter no máximo 2000 caracteres")]
        public string? Notes { get; set; }

        /// <summary>
        /// Tags ou categorias adicionais (formato JSON)
        /// </summary>
        [MaxLength(500)]
        public string? Tags { get; set; }

        /// <summary>
        /// Dados adicionais específicos da origem (formato JSON)
        /// </summary>
        [MaxLength(2000)]
        public string? SourceData { get; set; }

        /// <summary>
        /// IP de origem da captura do lead
        /// </summary>
        [MaxLength(45)]
        public string? IpAddress { get; set; }

        /// <summary>
        /// User-Agent do navegador na captura
        /// </summary>
        [MaxLength(500)]
        public string? UserAgent { get; set; }

        #endregion

        #region Métodos de Negócio

        /// <summary>
        /// Atualiza a data de modificação e recalcula métricas
        /// </summary>
        public void UpdateModifiedDate()
        {
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Realiza exclusão lógica do lead
        /// </summary>
        public void SoftDelete()
        {
            IsActive = false;
            DeletedAt = DateTime.UtcNow;
            UpdateModifiedDate();
        }

        /// <summary>
        /// Restaura um lead excluído logicamente
        /// </summary>
        public void Restore()
        {
            IsActive = true;
            DeletedAt = null;
            UpdateModifiedDate();
        }

        /// <summary>
        /// Incrementa o contador de interações
        /// </summary>
        public void IncrementInteraction()
        {
            InteractionCount++;
            LastContactDate = DateTime.UtcNow;
            UpdateModifiedDate();
        }

        /// <summary>
        /// Marca o lead como respondido
        /// </summary>
        public void MarkAsResponded()
        {
            HasResponded = true;
            IncrementInteraction();
        }

        #endregion
    }
}
