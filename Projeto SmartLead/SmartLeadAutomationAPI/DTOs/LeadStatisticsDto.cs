namespace SmartLeadAutomation.DTOs
{
    /// <summary>
    /// DTO com estatísticas gerais de leads.
    /// Fornece visão consolidada do funil de vendas.
    /// </summary>
    public class LeadStatisticsDto
    {
        /// <summary>
        /// Total de leads no sistema
        /// </summary>
        public int TotalLeads { get; set; }

        /// <summary>
        /// Total de leads ativos (não excluídos)
        /// </summary>
        public int ActiveLeads { get; set; }

        /// <summary>
        /// Total de leads inativos (excluídos logicamente)
        /// </summary>
        public int InactiveLeads { get; set; }

        /// <summary>
        /// Total de leads novos (últimos 7 dias)
        /// </summary>
        public int NewLeadsThisWeek { get; set; }

        /// <summary>
        /// Total de leads novos (últimos 30 dias)
        /// </summary>
        public int NewLeadsThisMonth { get; set; }

        /// <summary>
        /// Distribuição de leads por prioridade
        /// </summary>
        public PriorityDistributionDto PriorityDistribution { get; set; } = new PriorityDistributionDto();

        /// <summary>
        /// Distribuição de leads por status
        /// </summary>
        public List<StatusCountDto> StatusDistribution { get; set; } = new List<StatusCountDto>();

        /// <summary>
        /// Distribuição de leads por origem
        /// </summary>
        public List<SourceCountDto> SourceDistribution { get; set; } = new List<SourceCountDto>();

        /// <summary>
        /// Valor total estimado em oportunidades
        /// </summary>
        public decimal TotalEstimatedValue { get; set; }

        /// <summary>
        /// Valor médio estimado por lead
        /// </summary>
        public decimal AverageEstimatedValue { get; set; }

        /// <summary>
        /// Taxa de conversão (%)
        /// </summary>
        public decimal ConversionRate { get; set; }

        /// <summary>
        /// Total de leads convertidos
        /// </summary>
        public int ConvertedLeads { get; set; }

        /// <summary>
        /// Pontuação média dos leads
        /// </summary>
        public double AverageScore { get; set; }

        /// <summary>
        /// Taxa de resposta dos leads (%)
        /// </summary>
        public decimal ResponseRate { get; set; }

        /// <summary>
        /// Data e hora da geração das estatísticas
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Distribuição de leads por nível de prioridade
    /// </summary>
    public class PriorityDistributionDto
    {
        public int HighPriority { get; set; }
        public int MediumPriority { get; set; }
        public int LowPriority { get; set; }
        public decimal HighPriorityPercentage { get; set; }
        public decimal MediumPriorityPercentage { get; set; }
        public decimal LowPriorityPercentage { get; set; }
    }

    /// <summary>
    /// Contagem de leads por status
    /// </summary>
    public class StatusCountDto
    {
        public string Status { get; set; } = string.Empty;
        public string StatusLabel { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
    }

    /// <summary>
    /// Contagem de leads por origem
    /// </summary>
    public class SourceCountDto
    {
        public string Source { get; set; } = string.Empty;
        public string SourceLabel { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Percentage { get; set; }
        public decimal? TotalValue { get; set; }
    }
}
