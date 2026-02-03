using System.ComponentModel.DataAnnotations;
using SmartLeadAutomation.Models.Enums;

namespace SmartLeadAutomation.DTOs
{
    /// <summary>
    /// DTO para atualização de um lead existente.
    /// Permite modificar dados de contato, status e classificação.
    /// </summary>
    public class UpdateLeadDto
    {
        /// <summary>
        /// Nome completo do lead
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório")]
        [MinLength(2, ErrorMessage = "O nome deve ter pelo menos 2 caracteres")]
        [MaxLength(200, ErrorMessage = "O nome deve ter no máximo 200 caracteres")]
        [Display(Name = "Nome Completo")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Número de telefone para contato
        /// </summary>
        [Phone(ErrorMessage = "Formato de telefone inválido")]
        [MaxLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres")]
        [Display(Name = "Telefone")]
        public string? Phone { get; set; }

        /// <summary>
        /// Nome da empresa do lead
        /// </summary>
        [MaxLength(200, ErrorMessage = "O nome da empresa deve ter no máximo 200 caracteres")]
        [Display(Name = "Empresa")]
        public string? Company { get; set; }

        /// <summary>
        /// Cargo do lead na empresa
        /// </summary>
        [MaxLength(100, ErrorMessage = "O cargo deve ter no máximo 100 caracteres")]
        [Display(Name = "Cargo")]
        public string? JobTitle { get; set; }

        /// <summary>
        /// Status atual do lead no funil de vendas
        /// </summary>
        [Required(ErrorMessage = "O status é obrigatório")]
        [EnumDataType(typeof(LeadStatus), ErrorMessage = "Status inválido")]
        [Display(Name = "Status")]
        public LeadStatus Status { get; set; } = LeadStatus.Novo;

        /// <summary>
        /// Canal de origem do lead
        /// </summary>
        [Required(ErrorMessage = "A origem é obrigatória")]
        [EnumDataType(typeof(LeadSource), ErrorMessage = "Origem inválida")]
        [Display(Name = "Origem")]
        public LeadSource Source { get; set; } = LeadSource.Outro;

        /// <summary>
        /// Valor estimado do negócio
        /// </summary>
        [Range(0, 999999999.99, ErrorMessage = "O valor estimado deve ser positivo")]
        [Display(Name = "Valor Estimado")]
        public decimal? EstimatedValue { get; set; }

        /// <summary>
        /// Probabilidade de conversão (0-100%)
        /// </summary>
        [Range(0, 100, ErrorMessage = "A probabilidade deve estar entre 0 e 100")]
        [Display(Name = "Probabilidade de Conversão")]
        public int? ConversionProbability { get; set; }

        /// <summary>
        /// Consentimento para marketing
        /// </summary>
        [Display(Name = "Consentimento de Marketing")]
        public bool MarketingConsent { get; set; } = false;

        /// <summary>
        /// Observações sobre o lead
        /// </summary>
        [MaxLength(2000, ErrorMessage = "As notas devem ter no máximo 2000 caracteres")]
        [Display(Name = "Observações")]
        public string? Notes { get; set; }

        /// <summary>
        /// Tags separadas por vírgula
        /// </summary>
        [MaxLength(500, ErrorMessage = "As tags devem ter no máximo 500 caracteres")]
        [Display(Name = "Tags")]
        public string? Tags { get; set; }

        /// <summary>
        /// Indica se o lead respondeu às comunicações
        /// </summary>
        [Display(Name = "Respondeu")]
        public bool HasResponded { get; set; } = false;
    }
}
