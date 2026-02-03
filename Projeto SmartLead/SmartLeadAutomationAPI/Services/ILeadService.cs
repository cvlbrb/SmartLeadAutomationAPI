using SmartLeadAutomation.DTOs;
using SmartLeadAutomation.Models.Enums;

namespace SmartLeadAutomation.Services
{
    /// <summary>
    /// Interface do serviço de gestão de leads.
    /// Define as operações disponíveis para CRUD e classificação de leads.
    /// </summary>
    public interface ILeadService
    {
        /// <summary>
        /// Obtém todos os leads com suporte a filtros e paginação
        /// </summary>
        Task<PagedResponseDto<LeadResponseDto>> GetAllLeadsAsync(LeadFilterDto filter);

        /// <summary>
        /// Obtém um lead pelo ID
        /// </summary>
        Task<LeadResponseDto?> GetLeadByIdAsync(int id);

        /// <summary>
        /// Obtém um lead pelo email
        /// </summary>
        Task<LeadResponseDto?> GetLeadByEmailAsync(string email);

        /// <summary>
        /// Cria um novo lead com classificação automática
        /// </summary>
        Task<(LeadResponseDto? Lead, List<string> Errors)> CreateLeadAsync(CreateLeadDto dto);

        /// <summary>
        /// Atualiza um lead existente
        /// </summary>
        Task<(LeadResponseDto? Lead, List<string> Errors)> UpdateLeadAsync(int id, UpdateLeadDto dto);

        /// <summary>
        /// Remove um lead (exclusão lógica)
        /// </summary>
        Task<bool> DeleteLeadAsync(int id);

        /// <summary>
        /// Restaura um lead excluído logicamente
        /// </summary>
        Task<bool> RestoreLeadAsync(int id);

        /// <summary>
        /// Verifica se existe um lead com o email informado
        /// </summary>
        Task<bool> EmailExistsAsync(string email, int? excludeId = null);

        /// <summary>
        /// Atualiza o status de um lead
        /// </summary>
        Task<LeadResponseDto?> UpdateLeadStatusAsync(int id, LeadStatus newStatus);

        /// <summary>
        /// Marca um lead como respondido
        /// </summary>
        Task<LeadResponseDto?> MarkLeadAsRespondedAsync(int id);

        /// <summary>
        /// Recalcula a prioridade de um lead
        /// </summary>
        Task<LeadResponseDto?> RecalculatePriorityAsync(int id);

        /// <summary>
        /// Obtém estatísticas gerais de leads
        /// </summary>
        Task<LeadStatisticsDto> GetStatisticsAsync();

        /// <summary>
        /// Obtém distribuição de leads por origem
        /// </summary>
        Task<List<SourceCountDto>> GetSourceDistributionAsync();

        /// <summary>
        /// Obtém distribuição de leads por prioridade
        /// </summary>
        Task<PriorityDistributionDto> GetPriorityDistributionAsync();
    }
}
