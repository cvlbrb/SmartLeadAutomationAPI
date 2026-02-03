using System.ComponentModel.DataAnnotations;
using SmartLeadAutomation.Models.Enums;

namespace SmartLeadAutomation.DTOs
{
    /// <summary>
    /// DTO para criação de um novo lead.
    /// Contém apenas os campos necessários para cadastro inicial.
    /// </summary>
    public class CreateLeadDto
    {
        /// <summary>
        /// Nome completo do lead (obrigatório)
        /// </summary>
        [Required(ErrorMessage = "O nome é obrigatório")]
        [MinLength(2, ErrorMessage = "O nome deve ter pelo menos 2 caracteres")]
        [MaxLength(200, ErrorMessage = "O nome deve ter no máximo 200 caracteres")]
        [Display(Name = "Nome Completo")]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Endereço de email do lead (obrigatório, deve ser único)
        /// </summary>
        [Required(ErrorMessage = "O email é obrigatório")]
        [EmailAddress(ErrorMessage = "Formato de email inválido")]
        [MaxLength(255, ErrorMessage = "O email deve ter no máximo 255 caracteres")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Número de telefone para contato (opcional)
        /// </summary>
        [Phone(ErrorMessage = "Formato de telefone inválido")]
        [MaxLength(20, ErrorMessage = "O telefone deve ter no máximo 20 caracteres")]
        [Display(Name = "Telefone")]
        public string? Phone { get; set; }

        /// <summary>
        /// Nome da empresa do lead (opcional)
        /// </summary>
        [MaxLength(200, ErrorMessage = "O nome da empresa deve ter no máximo 200 caracteres")]
        [Display(Name = "Empresa")]
        public string? Company { get; set; }

        /// <summary>
        /// Cargo do lead na empresa (opcional)
        /// </summary>
        [MaxLength(100, ErrorMessage = "O cargo deve ter no máximo 100 caracteres")]
        [Display(Name = "Cargo")]
        public string? JobTitle { get; set; }

        /// <summary>
        /// Canal de origem do lead (obrigatório)
        /// </summary>
        [Required(ErrorMessage = "A origem é obrigatória")]
        [EnumDataType(typeof(LeadSource), ErrorMessage = "Origem inválida")]
        [Display(Name = "Origem")]
        public LeadSource Source { get; set; } = LeadSource.Website;

        /// <summary>
        /// Valor estimado do negócio (opcional)
        /// </summary>
        [Range(0, 999999999.99, ErrorMessage = "O valor estimado deve ser positivo")]
        [Display(Name = "Valor Estimado")]
        public decimal? EstimatedValue { get; set; }

        /// <summary>
        /// Consentimento para marketing (padrão: falso)
        /// </summary>
        [Display(Name = "Consentimento de Marketing")]
        public bool MarketingConsent { get; set; } = false;

        /// <summary>
        /// Observações sobre o lead (opcional)
        /// </summary>
        [MaxLength(2000, ErrorMessage = "As notas devem ter no máximo 2000 caracteres")]
        [Display(Name = "Observações")]
        public string? Notes { get; set; }

        /// <summary>
        /// Tags separadas por vírgula (opcional)
        /// </summary>
        [MaxLength(500, ErrorMessage = "As tags devem ter no máximo 500 caracteres")]
        [Display(Name = "Tags")]
        public string? Tags { get; set; }

        /// <summary>
        /// Identificador externo para integração (opcional)
        /// </summary>
        [MaxLength(100, ErrorMessage = "O ID externo deve ter no máximo 100 caracteres")]
        [Display(Name = "ID Externo")]
        public string? ExternalId { get; set; }
    }
}
