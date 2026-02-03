using SmartLeadAutomation.Models.Enums;

namespace SmartLeadAutomation.DTOs
{
    /// <summary>
    /// DTO para filtragem e paginação de leads.
    /// Permite busca avançada com múltiplos critérios.
    /// </summary>
    public class LeadFilterDto
    {
        /// <summary>
        /// Termo de busca para nome, email ou empresa
        /// </summary>
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Filtrar por nível de prioridade
        /// </summary>
        public PriorityLevel? Priority { get; set; }

        /// <summary>
        /// Filtrar por status
        /// </summary>
        public LeadStatus? Status { get; set; }

        /// <summary>
        /// Filtrar por origem
        /// </summary>
        public LeadSource? Source { get; set; }

        /// <summary>
        /// Filtrar leads com valor estimado mínimo
        /// </summary>
        public decimal? MinValue { get; set; }

        /// <summary>
        /// Filtrar leads com valor estimado máximo
        /// </summary>
        public decimal? MaxValue { get; set; }

        /// <summary>
        /// Filtrar leads criados a partir desta data
        /// </summary>
        public DateTime? CreatedFrom { get; set; }

        /// <summary>
        /// Filtrar leads criados até esta data
        /// </summary>
        public DateTime? CreatedTo { get; set; }

        /// <summary>
        /// Filtrar apenas leads com consentimento de marketing
        /// </summary>
        public bool? HasMarketingConsent { get; set; }

        /// <summary>
        /// Filtrar apenas leads que responderam
        /// </summary>
        public bool? HasResponded { get; set; }

        /// <summary>
        /// Incluir leads inativos (excluídos logicamente)
        /// </summary>
        public bool IncludeInactive { get; set; } = false;

        /// <summary>
        /// Campo para ordenação (name, email, createdAt, priority, score, value)
        /// </summary>
        public string SortBy { get; set; } = "createdAt";

        /// <summary>
        /// Direção da ordenação (asc ou desc)
        /// </summary>
        public string SortDirection { get; set; } = "desc";

        /// <summary>
        /// Número da página (começa em 1)
        /// </summary>
        public int PageNumber { get; set; } = 1;

        /// <summary>
        /// Tamanho da página (padrão: 20, máximo: 100)
        /// </summary>
        public int PageSize { get; set; } = 20;
    }
}
