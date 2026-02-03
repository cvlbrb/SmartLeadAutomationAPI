using SmartLeadAutomation.Models.Enums;

namespace SmartLeadAutomation.DTOs
{
    /// <summary>
    /// DTO para resposta com dados completos de um lead.
    /// Utilizado em endpoints de consulta e listagem.
    /// </summary>
    public class LeadResponseDto
    {
        /// <summary>
        /// Identificador único do lead
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Identificador externo para integração
        /// </summary>
        public string? ExternalId { get; set; }

        /// <summary>
        /// Nome completo do lead
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Endereço de email do lead
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Número de telefone para contato
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// Nome da empresa do lead
        /// </summary>
        public string? Company { get; set; }

        /// <summary>
        /// Cargo do lead na empresa
        /// </summary>
        public string? JobTitle { get; set; }

        /// <summary>
        /// Nível de prioridade (Alta, Média, Baixa)
        /// </summary>
        public string Priority { get; set; } = string.Empty;

        /// <summary>
        /// Pontuação de qualidade (0-100)
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Status atual no funil de vendas
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Canal de origem do lead
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// Valor estimado do negócio formatado
        /// </summary>
        public string? EstimatedValue { get; set; }

        /// <summary>
        /// Valor numérico estimado
        /// </summary>
        public decimal? EstimatedValueNumeric { get; set; }

        /// <summary>
        /// Probabilidade de conversão (%)
        /// </summary>
        public int? ConversionProbability { get; set; }

        /// <summary>
        /// Indica se possui consentimento de marketing
        /// </summary>
        public bool MarketingConsent { get; set; }

        /// <summary>
        /// Indica se o lead respondeu às comunicações
        /// </summary>
        public bool HasResponded { get; set; }

        /// <summary>
        /// Data do último contato
        /// </summary>
        public DateTime? LastContactDate { get; set; }

        /// <summary>
        /// Número de interações realizadas
        /// </summary>
        public int InteractionCount { get; set; }

        /// <summary>
        /// Data de criação do lead
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Data da última atualização
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Observações sobre o lead
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// Tags do lead
        /// </summary>
        public string? Tags { get; set; }

        /// <summary>
        /// Indica se o lead está ativo
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// Cor associada à prioridade (para UI)
        /// </summary>
        public string PriorityColor { get; set; } = string.Empty;
    }
}
