namespace SmartLeadAutomation.Models.Enums
{
    /// <summary>
    /// Define os níveis de prioridade para classificação de leads.
    /// A prioridade é calculada automaticamente com base em regras de negócio.
    /// </summary>
    public enum PriorityLevel
    {
        /// <summary>
        /// Prioridade baixa - leads com pouco potencial ou engajamento
        /// </summary>
        Baixa = 1,

        /// <summary>
        /// Prioridade média - leads com potencial moderado
        /// </summary>
        Media = 2,

        /// <summary>
        /// Prioridade alta - leads com alto potencial de conversão
        /// </summary>
        Alta = 3
    }
}
