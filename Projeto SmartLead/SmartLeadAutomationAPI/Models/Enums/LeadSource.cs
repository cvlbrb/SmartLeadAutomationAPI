namespace SmartLeadAutomation.Models.Enums
{
    /// <summary>
    /// Define as possíveis origens/canais de captação de leads.
    /// Utilizado para análise de efetividade de canais de marketing.
    /// </summary>
    public enum LeadSource
    {
        /// <summary>
        /// Origem desconhecida ou não especificada
        /// </summary>
        Outro = 0,

        /// <summary>
        /// Lead captado através do site institucional
        /// </summary>
        Website = 1,

        /// <summary>
        /// Lead captado através de campanhas no Facebook
        /// </summary>
        Facebook = 2,

        /// <summary>
        /// Lead captado através do Instagram
        /// </summary>
        Instagram = 3,

        /// <summary>
        /// Lead captado através do LinkedIn
        /// </summary>
        LinkedIn = 4,

        /// <summary>
        /// Lead captado através de campanhas no Google Ads
        /// </summary>
        GoogleAds = 5,

        /// <summary>
        /// Lead captado através de email marketing
        /// </summary>
        EmailMarketing = 6,

        /// <summary>
        /// Lead indicado por outro cliente ou parceiro
        /// </summary>
        Indicacao = 7,

        /// <summary>
        /// Lead captado através de eventos ou feiras
        /// </summary>
        Evento = 8,

        /// <summary>
        /// Lead captado através de contato telefônico
        /// </summary>
        Telefone = 9,

        /// <summary>
        /// Lead captado através de chat ou WhatsApp
        /// </summary>
        Chat = 10
    }
}
