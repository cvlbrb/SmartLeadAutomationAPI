namespace SmartLeadAutomation.Models.Enums
{
    /// <summary>
    /// Define os status possíveis no ciclo de vida de um lead.
    /// Permite acompanhamento do funil de vendas.
    /// </summary>
    public enum LeadStatus
    {
        /// <summary>
        /// Lead recém-captado, ainda não processado
        /// </summary>
        Novo = 1,

        /// <summary>
        /// Lead em processo de qualificação
        /// </summary>
        EmQualificacao = 2,

        /// <summary>
        /// Lead qualificado e pronto para contato comercial
        /// </summary>
        Qualificado = 3,

        /// <summary>
        /// Lead em negociação ativa
        /// </summary>
        EmNegociacao = 4,

        /// <summary>
        /// Lead convertido em cliente
        /// </summary>
        Convertido = 5,

        /// <summary>
        /// Lead não qualificado ou perdido
        /// </summary>
        Descartado = 6,

        /// <summary>
        /// Lead arquivado para follow-up futuro
        /// </summary>
        Arquivado = 7
    }
}
