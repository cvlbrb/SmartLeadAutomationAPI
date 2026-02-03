using Microsoft.EntityFrameworkCore;
using SmartLeadAutomation.Data;
using SmartLeadAutomation.DTOs;
using SmartLeadAutomation.Models.Entities;
using SmartLeadAutomation.Models.Enums;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace SmartLeadAutomation.Services
{
    /// <summary>
    /// Serviço de gestão de leads com regras de negócio para classificação automática.
    /// Implementa lógica de pontuação e priorização baseada em múltiplos critérios.
    /// </summary>
    public class LeadService : ILeadService
    {
        private readonly SmartLeadDbContext _context;
        private readonly ILogger<LeadService> _logger;
        private readonly IConfiguration _configuration;

        // Configurações de classificação (carregadas do appsettings.json)
        private int HighPriorityScoreThreshold => _configuration.GetValue<int>("LeadClassificationRules:HighPriorityScoreThreshold", 80);
        private int MediumPriorityScoreThreshold => _configuration.GetValue<int>("LeadClassificationRules:MediumPriorityScoreThreshold", 50);
        private decimal HighValueThreshold => _configuration.GetValue<decimal>("LeadClassificationRules:HighValueThreshold", 10000);
        private decimal MediumValueThreshold => _configuration.GetValue<decimal>("LeadClassificationRules:MediumValueThreshold", 5000);
        private int DaysToConsiderRecent => _configuration.GetValue<int>("LeadClassificationRules:DaysToConsiderRecent", 7);

        /// <summary>
        /// Construtor com injeção de dependências
        /// </summary>
        public LeadService(
            SmartLeadDbContext context,
            ILogger<LeadService> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Obtém todos os leads com filtros e paginação
        /// </summary>
        public async Task<PagedResponseDto<LeadResponseDto>> GetAllLeadsAsync(LeadFilterDto filter)
        {
            try
            {
                // Inicia a query base
                var query = _context.Leads.AsQueryable();

                // Aplica filtro de ativos/inativos
                if (filter.IncludeInactive)
                {
                    // Ignora o filtro global para incluir inativos
                    query = query.IgnoreQueryFilters();
                }

                // Aplica filtros de busca
                query = ApplyFilters(query, filter);

                // Aplica ordenação
                query = ApplySorting(query, filter.SortBy, filter.SortDirection);

                // Conta total de registros
                var totalCount = await query.CountAsync();

                // Aplica paginação
                var pageSize = Math.Min(filter.PageSize, 100); // Máximo 100 por página
                var pageNumber = Math.Max(filter.PageNumber, 1); // Mínimo página 1

                var leads = await query
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                // Mapeia para DTO de resposta
                var leadDtos = leads.Select(MapToResponseDto).ToList();

                return PagedResponseDto<LeadResponseDto>.Create(
                    leadDtos,
                    pageNumber,
                    pageSize,
                    totalCount
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar leads com filtros");
                throw;
            }
        }

        /// <summary>
        /// Obtém um lead pelo ID
        /// </summary>
        public async Task<LeadResponseDto?> GetLeadByIdAsync(int id)
        {
            try
            {
                var lead = await _context.Leads
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(l => l.Id == id);

                return lead != null ? MapToResponseDto(lead) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar lead por ID: {LeadId}", id);
                throw;
            }
        }

        /// <summary>
        /// Obtém um lead pelo email
        /// </summary>
        public async Task<LeadResponseDto?> GetLeadByEmailAsync(string email)
        {
            try
            {
                var lead = await _context.Leads
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(l => l.Email.ToLower() == email.ToLower());

                return lead != null ? MapToResponseDto(lead) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar lead por email: {Email}", email);
                throw;
            }
        }

        /// <summary>
        /// Cria um novo lead com validação e classificação automática
        /// </summary>
        public async Task<(LeadResponseDto? Lead, List<string> Errors)> CreateLeadAsync(CreateLeadDto dto)
        {
            var errors = new List<string>();

            try
            {
                // Validação de email
                if (!IsValidEmail(dto.Email))
                {
                    errors.Add("O email fornecido não é válido.");
                    return (null, errors);
                }

                // Verifica duplicidade de email
                if (await EmailExistsAsync(dto.Email))
                {
                    errors.Add($"Já existe um lead cadastrado com o email '{dto.Email}'.");
                    return (null, errors);
                }

                // Cria a entidade Lead
                var lead = new Lead
                {
                    Name = dto.Name.Trim(),
                    Email = dto.Email.ToLower().Trim(),
                    Phone = dto.Phone?.Trim(),
                    Company = dto.Company?.Trim(),
                    JobTitle = dto.JobTitle?.Trim(),
                    Source = dto.Source,
                    EstimatedValue = dto.EstimatedValue,
                    MarketingConsent = dto.MarketingConsent,
                    Notes = dto.Notes?.Trim(),
                    Tags = dto.Tags?.Trim(),
                    ExternalId = dto.ExternalId?.Trim(),
                    Status = LeadStatus.Novo,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                // Aplica classificação automática
                ClassifyLead(lead);

                // Salva no banco de dados
                _context.Leads.Add(lead);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Lead criado com sucesso. ID: {LeadId}, Email: {Email}, Prioridade: {Priority}, Score: {Score}",
                    lead.Id, lead.Email, lead.Priority, lead.Score);

                return (MapToResponseDto(lead), errors);
            }
            catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("UNIQUE") == true)
            {
                errors.Add("Já existe um lead com este email ou ID externo.");
                _logger.LogWarning(ex, "Tentativa de criar lead duplicado: {Email}", dto.Email);
                return (null, errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar lead: {Email}", dto.Email);
                errors.Add("Erro interno ao processar a solicitação.");
                return (null, errors);
            }
        }

        /// <summary>
        /// Atualiza um lead existente
        /// </summary>
        public async Task<(LeadResponseDto? Lead, List<string> Errors)> UpdateLeadAsync(int id, UpdateLeadDto dto)
        {
            var errors = new List<string>();

            try
            {
                var lead = await _context.Leads.FindAsync(id);

                if (lead == null)
                {
                    errors.Add("Lead não encontrado.");
                    return (null, errors);
                }

                // Atualiza propriedades
                lead.Name = dto.Name.Trim();
                lead.Phone = dto.Phone?.Trim();
                lead.Company = dto.Company?.Trim();
                lead.JobTitle = dto.JobTitle?.Trim();
                lead.Status = dto.Status;
                lead.Source = dto.Source;
                lead.EstimatedValue = dto.EstimatedValue;
                lead.ConversionProbability = dto.ConversionProbability;
                lead.MarketingConsent = dto.MarketingConsent;
                lead.HasResponded = dto.HasResponded;
                lead.Notes = dto.Notes?.Trim();
                lead.Tags = dto.Tags?.Trim();
                lead.UpdatedAt = DateTime.UtcNow;

                // Recalcula classificação se valores relevantes mudaram
                ClassifyLead(lead);

                await _context.SaveChangesAsync();

                _logger.LogInformation("Lead atualizado com sucesso. ID: {LeadId}", id);

                return (MapToResponseDto(lead), errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar lead: {LeadId}", id);
                errors.Add("Erro interno ao processar a solicitação.");
                return (null, errors);
            }
        }

        /// <summary>
        /// Remove um lead (exclusão lógica)
        /// </summary>
        public async Task<bool> DeleteLeadAsync(int id)
        {
            try
            {
                var lead = await _context.Leads.FindAsync(id);

                if (lead == null)
                {
                    return false;
                }

                lead.SoftDelete();
                await _context.SaveChangesAsync();

                _logger.LogInformation("Lead excluído logicamente. ID: {LeadId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir lead: {LeadId}", id);
                throw;
            }
        }

        /// <summary>
        /// Restaura um lead excluído logicamente
        /// </summary>
        public async Task<bool> RestoreLeadAsync(int id)
        {
            try
            {
                var lead = await _context.Leads
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(l => l.Id == id);

                if (lead == null || lead.IsActive)
                {
                    return false;
                }

                lead.Restore();
                await _context.SaveChangesAsync();

                _logger.LogInformation("Lead restaurado. ID: {LeadId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao restaurar lead: {LeadId}", id);
                throw;
            }
        }

        /// <summary>
        /// Verifica se existe um lead com o email informado
        /// </summary>
        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            var normalizedEmail = email.ToLower().Trim();
            var query = _context.Leads
                .IgnoreQueryFilters()
                .Where(l => l.Email == normalizedEmail);

            if (excludeId.HasValue)
            {
                query = query.Where(l => l.Id != excludeId.Value);
            }

            return await query.AnyAsync();
        }

        /// <summary>
        /// Atualiza o status de um lead
        /// </summary>
        public async Task<LeadResponseDto?> UpdateLeadStatusAsync(int id, LeadStatus newStatus)
        {
            try
            {
                var lead = await _context.Leads.FindAsync(id);

                if (lead == null)
                {
                    return null;
                }

                var oldStatus = lead.Status;
                lead.Status = newStatus;
                lead.UpdatedAt = DateTime.UtcNow;

                // Se converteu, atualiza probabilidade
                if (newStatus == LeadStatus.Convertido && lead.ConversionProbability < 100)
                {
                    lead.ConversionProbability = 100;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Status do lead alterado. ID: {LeadId}, De: {OldStatus}, Para: {NewStatus}",
                    id, oldStatus, newStatus);

                return MapToResponseDto(lead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do lead: {LeadId}", id);
                throw;
            }
        }

        /// <summary>
        /// Marca um lead como respondido
        /// </summary>
        public async Task<LeadResponseDto?> MarkLeadAsRespondedAsync(int id)
        {
            try
            {
                var lead = await _context.Leads.FindAsync(id);

                if (lead == null)
                {
                    return null;
                }

                lead.MarkAsResponded();
                await _context.SaveChangesAsync();

                _logger.LogInformation("Lead marcado como respondido. ID: {LeadId}", id);

                return MapToResponseDto(lead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar lead como respondido: {LeadId}", id);
                throw;
            }
        }

        /// <summary>
        /// Recalcula a prioridade de um lead
        /// </summary>
        public async Task<LeadResponseDto?> RecalculatePriorityAsync(int id)
        {
            try
            {
                var lead = await _context.Leads.FindAsync(id);

                if (lead == null)
                {
                    return null;
                }

                var oldPriority = lead.Priority;
                ClassifyLead(lead);
                lead.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                if (oldPriority != lead.Priority)
                {
                    _logger.LogInformation(
                        "Prioridade do lead recalculada. ID: {LeadId}, De: {OldPriority}, Para: {NewPriority}, Score: {Score}",
                        id, oldPriority, lead.Priority, lead.Score);
                }

                return MapToResponseDto(lead);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recalcular prioridade do lead: {LeadId}", id);
                throw;
            }
        }

        /// <summary>
        /// Obtém estatísticas gerais de leads
        /// </summary>
        public async Task<LeadStatisticsDto> GetStatisticsAsync()
        {
            try
            {
                // Query base para leads ativos
                var activeLeadsQuery = _context.Leads.Where(l => l.IsActive);
                var allLeadsQuery = _context.Leads.IgnoreQueryFilters();

                // Contagens básicas
                var totalLeads = await allLeadsQuery.CountAsync();
                var activeLeads = await activeLeadsQuery.CountAsync();
                var inactiveLeads = totalLeads - activeLeads;

                // Leads recentes
                var lastWeek = DateTime.UtcNow.AddDays(-7);
                var lastMonth = DateTime.UtcNow.AddDays(-30);
                var newLeadsThisWeek = await activeLeadsQuery.CountAsync(l => l.CreatedAt >= lastWeek);
                var newLeadsThisMonth = await activeLeadsQuery.CountAsync(l => l.CreatedAt >= lastMonth);

                // Distribuição por prioridade
                var highPriority = await activeLeadsQuery.CountAsync(l => l.Priority == PriorityLevel.Alta);
                var mediumPriority = await activeLeadsQuery.CountAsync(l => l.Priority == PriorityLevel.Media);
                var lowPriority = await activeLeadsQuery.CountAsync(l => l.Priority == PriorityLevel.Baixa);
                var totalActive = activeLeads > 0 ? activeLeads : 1; // Evita divisão por zero

                // Distribuição por status
                var statusDistribution = await activeLeadsQuery
                    .GroupBy(l => l.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Distribuição por origem
                var sourceDistribution = await activeLeadsQuery
                    .GroupBy(l => l.Source)
                    .Select(g => new
                    {
                        Source = g.Key,
                        Count = g.Count(),
                        TotalValue = g.Sum(l => (decimal?)l.EstimatedValue) ?? 0
                    })
                    .ToListAsync();

                // Valores
                var totalEstimatedValue = await activeLeadsQuery
                    .SumAsync(l => (decimal?)l.EstimatedValue) ?? 0;
                var averageEstimatedValue = activeLeads > 0
                    ? totalEstimatedValue / activeLeads
                    : 0;

                // Conversão
                var convertedLeads = await activeLeadsQuery
                    .CountAsync(l => l.Status == LeadStatus.Convertido);
                var conversionRate = activeLeads > 0
                    ? (decimal)convertedLeads / activeLeads * 100
                    : 0;

                // Pontuação média
                var averageScore = await activeLeadsQuery.AverageAsync(l => (double?)l.Score) ?? 0;

                // Taxa de resposta
                var respondedLeads = await activeLeadsQuery.CountAsync(l => l.HasResponded);
                var responseRate = activeLeads > 0
                    ? (decimal)respondedLeads / activeLeads * 100
                    : 0;

                return new LeadStatisticsDto
                {
                    TotalLeads = totalLeads,
                    ActiveLeads = activeLeads,
                    InactiveLeads = inactiveLeads,
                    NewLeadsThisWeek = newLeadsThisWeek,
                    NewLeadsThisMonth = newLeadsThisMonth,
                    PriorityDistribution = new PriorityDistributionDto
                    {
                        HighPriority = highPriority,
                        MediumPriority = mediumPriority,
                        LowPriority = lowPriority,
                        HighPriorityPercentage = Math.Round((decimal)highPriority / totalActive * 100, 2),
                        MediumPriorityPercentage = Math.Round((decimal)mediumPriority / totalActive * 100, 2),
                        LowPriorityPercentage = Math.Round((decimal)lowPriority / totalActive * 100, 2)
                    },
                    StatusDistribution = statusDistribution.Select(s => new StatusCountDto
                    {
                        Status = s.Status.ToString(),
                        StatusLabel = GetStatusLabel(s.Status),
                        Count = s.Count,
                        Percentage = Math.Round((decimal)s.Count / activeLeads * 100, 2)
                    }).ToList(),
                    SourceDistribution = sourceDistribution.Select(s => new SourceCountDto
                    {
                        Source = s.Source.ToString(),
                        SourceLabel = GetSourceLabel(s.Source),
                        Count = s.Count,
                        Percentage = Math.Round((decimal)s.Count / activeLeads * 100, 2),
                        TotalValue = s.TotalValue
                    }).ToList(),
                    TotalEstimatedValue = totalEstimatedValue,
                    AverageEstimatedValue = averageEstimatedValue,
                    ConvertedLeads = convertedLeads,
                    ConversionRate = Math.Round(conversionRate, 2),
                    AverageScore = Math.Round(averageScore, 2),
                    ResponseRate = Math.Round(responseRate, 2),
                    GeneratedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao calcular estatísticas");
                throw;
            }
        }

        /// <summary>
        /// Obtém distribuição de leads por origem
        /// </summary>
        public async Task<List<SourceCountDto>> GetSourceDistributionAsync()
        {
            var stats = await GetStatisticsAsync();
            return stats.SourceDistribution;
        }

        /// <summary>
        /// Obtém distribuição de leads por prioridade
        /// </summary>
        public async Task<PriorityDistributionDto> GetPriorityDistributionAsync()
        {
            var stats = await GetStatisticsAsync();
            return stats.PriorityDistribution;
        }

        #region Métodos Privados de Classificação

        /// <summary>
        /// Aplica regras de negócio para classificar o lead automaticamente.
        /// Calcula pontuação baseada em múltiplos critérios e define a prioridade.
        /// </summary>
        private void ClassifyLead(Lead lead)
        {
            int score = 0;

            // 1. Pontuação por valor estimado (0-30 pontos)
            if (lead.EstimatedValue.HasValue)
            {
                if (lead.EstimatedValue >= HighValueThreshold)
                    score += 30;
                else if (lead.EstimatedValue >= MediumValueThreshold)
                    score += 20;
                else if (lead.EstimatedValue > 0)
                    score += 10;
            }

            // 2. Pontuação por origem (0-20 pontos)
            score += GetSourceScore(lead.Source);

            // 3. Pontuação por dados completos (0-20 pontos)
            if (!string.IsNullOrEmpty(lead.Company)) score += 5;
            if (!string.IsNullOrEmpty(lead.JobTitle)) score += 5;
            if (!string.IsNullOrEmpty(lead.Phone)) score += 5;
            if (!string.IsNullOrEmpty(lead.Notes)) score += 5;

            // 4. Pontuação por engajamento (0-20 pontos)
            if (lead.HasResponded) score += 15;
            if (lead.InteractionCount > 0) score += Math.Min(lead.InteractionCount * 3, 5);
            if (lead.MarketingConsent) score += 5;

            // 5. Pontuação por recência (0-10 pontos)
            var daysSinceCreation = (DateTime.UtcNow - lead.CreatedAt).TotalDays;
            if (daysSinceCreation <= DaysToConsiderRecent)
                score += 10;
            else if (daysSinceCreation <= 30)
                score += 5;

            // 6. Pontuação por cargo/posição (0-10 pontos)
            score += GetJobTitleScore(lead.JobTitle);

            // 7. Bônus para leads indicados
            if (lead.Source == LeadSource.Indicacao)
                score += 10;

            // Limita a pontuação máxima a 100
            lead.Score = Math.Min(score, 100);

            // Define a prioridade baseada na pontuação
            lead.Priority = CalculatePriorityFromScore(lead.Score);

            _logger.LogDebug(
                "Lead classificado. ID: {LeadId}, Score: {Score}, Prioridade: {Priority}",
                lead.Id, lead.Score, lead.Priority);
        }

        /// <summary>
        /// Calcula a prioridade baseada na pontuação
        /// </summary>
        private PriorityLevel CalculatePriorityFromScore(int score)
        {
            if (score >= HighPriorityScoreThreshold)
                return PriorityLevel.Alta;
            else if (score >= MediumPriorityScoreThreshold)
                return PriorityLevel.Media;
            else
                return PriorityLevel.Baixa;
        }

        /// <summary>
        /// Retorna pontuação baseada na origem do lead
        /// </summary>
        private int GetSourceScore(LeadSource source)
        {
            return source switch
            {
                LeadSource.Indicacao => 20,      // Melhor origem
                LeadSource.LinkedIn => 18,       // Profissional qualificado
                LeadSource.Website => 15,        // Interesse direto
                LeadSource.GoogleAds => 12,      // Intenção de compra
                LeadSource.EmailMarketing => 10, // Já conhece a marca
                LeadSource.Evento => 10,         // Engajamento presencial
                LeadSource.Instagram => 8,
                LeadSource.Facebook => 6,
                LeadSource.Chat => 8,
                LeadSource.Telefone => 12,
                _ => 5                           // Outros
            };
        }

        /// <summary>
        /// Retorna pontuação baseada no cargo do lead
        /// </summary>
        private int GetJobTitleScore(string? jobTitle)
        {
            if (string.IsNullOrEmpty(jobTitle))
                return 0;

            var title = jobTitle.ToLower();

            // Cargos de decisão
            if (title.Contains("diretor") || title.Contains("ceo") ||
                title.Contains("presidente") || title.Contains("sócio") ||
                title.Contains("founder") || title.Contains("c-level") ||
                title.Contains("cfo") || title.Contains("cto") || title.Contains("cmo"))
                return 10;

            // Cargos gerenciais
            if (title.Contains("gerente") || title.Contains("manager") ||
                title.Contains("coordenador") || title.Contains("supervisor") ||
                title.Contains("head") || title.Contains("líder"))
                return 7;

            // Cargos analistas/especialistas
            if (title.Contains("analista") || title.Contains("especialista") ||
                title.Contains("consultor") || title.Contains("engineer") ||
                title.Contains("desenvolvedor"))
                return 4;

            return 2; // Outros cargos
        }

        /// <summary>
        /// Aplica filtros à query de leads
        /// </summary>
        private IQueryable<Lead> ApplyFilters(IQueryable<Lead> query, LeadFilterDto filter)
        {
            // Busca por termo (nome, email ou empresa)
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var term = filter.SearchTerm.ToLower().Trim();
                query = query.Where(l =>
                    l.Name.ToLower().Contains(term) ||
                    l.Email.ToLower().Contains(term) ||
                    (l.Company != null && l.Company.ToLower().Contains(term)));
            }

            // Filtro por prioridade
            if (filter.Priority.HasValue)
            {
                query = query.Where(l => l.Priority == filter.Priority.Value);
            }

            // Filtro por status
            if (filter.Status.HasValue)
            {
                query = query.Where(l => l.Status == filter.Status.Value);
            }

            // Filtro por origem
            if (filter.Source.HasValue)
            {
                query = query.Where(l => l.Source == filter.Source.Value);
            }

            // Filtro por valor mínimo
            if (filter.MinValue.HasValue)
            {
                query = query.Where(l => l.EstimatedValue >= filter.MinValue.Value);
            }

            // Filtro por valor máximo
            if (filter.MaxValue.HasValue)
            {
                query = query.Where(l => l.EstimatedValue <= filter.MaxValue.Value);
            }

            // Filtro por data de criação (de)
            if (filter.CreatedFrom.HasValue)
            {
                query = query.Where(l => l.CreatedAt >= filter.CreatedFrom.Value.ToUniversalTime());
            }

            // Filtro por data de criação (até)
            if (filter.CreatedTo.HasValue)
            {
                query = query.Where(l => l.CreatedAt <= filter.CreatedTo.Value.ToUniversalTime());
            }

            // Filtro por consentimento de marketing
            if (filter.HasMarketingConsent.HasValue)
            {
                query = query.Where(l => l.MarketingConsent == filter.HasMarketingConsent.Value);
            }

            // Filtro por resposta
            if (filter.HasResponded.HasValue)
            {
                query = query.Where(l => l.HasResponded == filter.HasResponded.Value);
            }

            return query;
        }

        /// <summary>
        /// Aplica ordenação à query de leads
        /// </summary>
        private IQueryable<Lead> ApplySorting(IQueryable<Lead> query, string sortBy, string sortDirection)
        {
            // Mapeia nomes de propriedades para expressões
            var sortExpressions = new Dictionary<string, Expression<Func<Lead, object>>>(StringComparer.OrdinalIgnoreCase)
            {
                ["name"] = l => l.Name,
                ["email"] = l => l.Email,
                ["company"] = l => l.Company ?? string.Empty,
                ["createdAt"] = l => l.CreatedAt,
                ["updatedAt"] = l => l.UpdatedAt ?? DateTime.MinValue,
                ["priority"] = l => l.Priority,
                ["score"] = l => l.Score,
                ["value"] = l => l.EstimatedValue ?? 0,
                ["status"] = l => l.Status,
                ["source"] = l => l.Source
            };

            // Obtém a expressão de ordenação ou usa createdAt como padrão
            var orderByExpression = sortExpressions.TryGetValue(sortBy, out var expression)
                ? expression
                : sortExpressions["createdAt"];

            // Aplica ordenação ascendente ou descendente
            var isDescending = sortDirection.Equals("desc", StringComparison.OrdinalIgnoreCase);

            return isDescending
                ? query.OrderByDescending(orderByExpression)
                : query.OrderBy(orderByExpression);
        }

        /// <summary>
        /// Mapeia uma entidade Lead para o DTO de resposta
        /// </summary>
        private LeadResponseDto MapToResponseDto(Lead lead)
        {
            return new LeadResponseDto
            {
                Id = lead.Id,
                ExternalId = lead.ExternalId,
                Name = lead.Name,
                Email = lead.Email,
                Phone = lead.Phone,
                Company = lead.Company,
                JobTitle = lead.JobTitle,
                Priority = lead.Priority.ToString(),
                Score = lead.Score,
                Status = lead.Status.ToString(),
                Source = lead.Source.ToString(),
                EstimatedValue = lead.EstimatedValue?.ToString("C", new System.Globalization.CultureInfo("pt-BR")),
                EstimatedValueNumeric = lead.EstimatedValue,
                ConversionProbability = lead.ConversionProbability,
                MarketingConsent = lead.MarketingConsent,
                HasResponded = lead.HasResponded,
                LastContactDate = lead.LastContactDate,
                InteractionCount = lead.InteractionCount,
                CreatedAt = lead.CreatedAt,
                UpdatedAt = lead.UpdatedAt,
                Notes = lead.Notes,
                Tags = lead.Tags,
                IsActive = lead.IsActive,
                PriorityColor = GetPriorityColor(lead.Priority)
            };
        }

        /// <summary>
        /// Retorna a cor associada à prioridade (para UI)
        /// </summary>
        private string GetPriorityColor(PriorityLevel priority)
        {
            return priority switch
            {
                PriorityLevel.Alta => "#dc3545",   // Vermelho
                PriorityLevel.Media => "#ffc107",  // Amarelo
                PriorityLevel.Baixa => "#28a745",  // Verde
                _ => "#6c757d"                     // Cinza
            };
        }

        /// <summary>
        /// Retorna o label amigável do status
        /// </summary>
        private string GetStatusLabel(LeadStatus status)
        {
            return status switch
            {
                LeadStatus.Novo => "Novo",
                LeadStatus.EmQualificacao => "Em Qualificação",
                LeadStatus.Qualificado => "Qualificado",
                LeadStatus.EmNegociacao => "Em Negociação",
                LeadStatus.Convertido => "Convertido",
                LeadStatus.Descartado => "Descartado",
                LeadStatus.Arquivado => "Arquivado",
                _ => status.ToString()
            };
        }

        /// <summary>
        /// Retorna o label amigável da origem
        /// </summary>
        private string GetSourceLabel(LeadSource source)
        {
            return source switch
            {
                LeadSource.Website => "Website",
                LeadSource.Facebook => "Facebook",
                LeadSource.Instagram => "Instagram",
                LeadSource.LinkedIn => "LinkedIn",
                LeadSource.GoogleAds => "Google Ads",
                LeadSource.EmailMarketing => "Email Marketing",
                LeadSource.Indicacao => "Indicação",
                LeadSource.Evento => "Evento",
                LeadSource.Telefone => "Telefone",
                LeadSource.Chat => "Chat/WhatsApp",
                _ => "Outro"
            };
        }

        /// <summary>
        /// Valida formato de email usando expressão regular
        /// </summary>
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            // Regex simples mas efetiva para validação de email
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return regex.IsMatch(email.Trim());
        }

        #endregion
    }
}
