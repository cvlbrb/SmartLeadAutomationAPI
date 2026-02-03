using Microsoft.AspNetCore.Mvc;
using SmartLeadAutomation.DTOs;
using SmartLeadAutomation.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartLeadAutomation.Controllers
{
    /// <summary>
    /// Controller para estatísticas e relatórios de leads.
    /// Fornece endpoints para análise de desempenho e métricas do funil de vendas.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Estatísticas e Relatórios - Métricas do funil de vendas")]
    public class StatsController : ControllerBase
    {
        private readonly ILeadService _leadService;
        private readonly ILogger<StatsController> _logger;

        /// <summary>
        /// Construtor com injeção de dependências
        /// </summary>
        public StatsController(ILeadService leadService, ILogger<StatsController> logger)
        {
            _leadService = leadService;
            _logger = logger;
        }

        /// <summary>
        /// Obtém estatísticas gerais de leads
        /// </summary>
        /// <remarks>
        /// Retorna um relatório completo com métricas do funil de vendas, incluindo:
        /// - Total de leads (ativos, inativos, novos)
        /// - Distribuição por prioridade (Alta, Média, Baixa)
        /// - Distribuição por status (Novo, Qualificado, Convertido, etc.)
        /// - Distribuição por origem (Website, LinkedIn, Indicação, etc.)
        /// - Valores estimados e taxas de conversão
        /// - Pontuação média e taxa de resposta
        /// 
        /// **Exemplo de resposta:**
        /// ```json
        /// {
        ///   "success": true,
        ///   "data": {
        ///     "totalLeads": 150,
        ///     "activeLeads": 145,
        ///     "newLeadsThisWeek": 12,
        ///     "priorityDistribution": {
        ///       "highPriority": 30,
        ///       "mediumPriority": 75,
        ///       "lowPriority": 45
        ///     },
        ///     "conversionRate": 15.5,
        ///     "totalEstimatedValue": 2500000.00
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <returns>Estatísticas completas de leads</returns>
        /// <response code="200">Estatísticas retornadas com sucesso</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Estatísticas gerais",
            Description = "Retorna estatísticas completas do funil de vendas",
            OperationId = "GetGeneralStatistics"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Estatísticas obtidas com sucesso", typeof(ApiResponseDto<LeadStatisticsDto>))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Erro interno", typeof(ApiResponseDto))]
        public async Task<ActionResult<ApiResponseDto<LeadStatisticsDto>>> GetStatistics()
        {
            try
            {
                _logger.LogInformation("Obtendo estatísticas gerais de leads");

                var stats = await _leadService.GetStatisticsAsync();

                return Ok(ApiResponseDto<LeadStatisticsDto>.SuccessResponse(
                    stats,
                    "Estatísticas obtidas com sucesso"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter estatísticas");
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        /// <summary>
        /// Obtém distribuição de leads por prioridade
        /// </summary>
        /// <remarks>
        /// Retorna a quantidade e percentual de leads em cada nível de prioridade.
        /// Útil para dashboards e relatórios de priorização.
        /// 
        /// **Exemplo de resposta:**
        /// ```json
        /// {
        ///   "success": true,
        ///   "data": {
        ///     "highPriority": 25,
        ///     "mediumPriority": 50,
        ///     "lowPriority": 25,
        ///     "highPriorityPercentage": 25.0,
        ///     "mediumPriorityPercentage": 50.0,
        ///     "lowPriorityPercentage": 25.0
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <returns>Distribuição por prioridade</returns>
        [HttpGet("by-priority")]
        [SwaggerOperation(
            Summary = "Distribuição por prioridade",
            Description = "Retorna a distribuição de leads por nível de prioridade",
            OperationId = "GetPriorityDistribution"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Distribuição obtida", typeof(ApiResponseDto<PriorityDistributionDto>))]
        public async Task<ActionResult<ApiResponseDto<PriorityDistributionDto>>> GetPriorityDistribution()
        {
            try
            {
                _logger.LogInformation("Obtendo distribuição por prioridade");

                var distribution = await _leadService.GetPriorityDistributionAsync();

                return Ok(ApiResponseDto<PriorityDistributionDto>.SuccessResponse(
                    distribution,
                    "Distribuição por prioridade obtida com sucesso"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter distribuição por prioridade");
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        /// <summary>
        /// Obtém distribuição de leads por origem
        /// </summary>
        /// <remarks>
        /// Retorna a quantidade, percentual e valor estimado de leads por canal de origem.
        /// Útil para analisar a efetividade de canais de marketing.
        /// 
        /// **Exemplo de resposta:**
        /// ```json
        /// {
        ///   "success": true,
        ///   "data": [
        ///     {
        ///       "source": "LinkedIn",
        ///       "sourceLabel": "LinkedIn",
        ///       "count": 45,
        ///       "percentage": 30.0,
        ///       "totalValue": 850000.00
        ///     },
        ///     {
        ///       "source": "Website",
        ///       "sourceLabel": "Website",
        ///       "count": 35,
        ///       "percentage": 23.33,
        ///       "totalValue": 420000.00
        ///     }
        ///   ]
        /// }
        /// ```
        /// </remarks>
        /// <returns>Lista de distribuição por origem</returns>
        [HttpGet("by-source")]
        [SwaggerOperation(
            Summary = "Distribuição por origem",
            Description = "Retorna a distribuição de leads por canal de origem",
            OperationId = "GetSourceDistribution"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Distribuição obtida", typeof(ApiResponseDto<List<SourceCountDto>>))]
        public async Task<ActionResult<ApiResponseDto<List<SourceCountDto>>>> GetSourceDistribution()
        {
            try
            {
                _logger.LogInformation("Obtendo distribuição por origem");

                var distribution = await _leadService.GetSourceDistributionAsync();

                return Ok(ApiResponseDto<List<SourceCountDto>>.SuccessResponse(
                    distribution,
                    "Distribuição por origem obtida com sucesso"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter distribuição por origem");
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        /// <summary>
        /// Obtém resumo rápido para dashboard
        /// </summary>
        /// <remarks>
        /// Retorna um resumo simplificado com as métricas mais importantes para dashboards.
        /// Inclui totais, leads novos e taxas de conversão.
        /// 
        /// **Exemplo de resposta:**
        /// ```json
        /// {
        ///   "success": true,
        ///   "data": {
        ///     "totalLeads": 150,
        ///     "activeLeads": 145,
        ///     "newLeadsThisWeek": 12,
        ///     "newLeadsThisMonth": 45,
        ///     "highPriorityCount": 30,
        ///     "conversionRate": 15.5,
        ///     "responseRate": 42.0,
        ///     "totalEstimatedValue": 2500000.00,
        ///     "averageScore": 65.5
        ///   }
        /// }
        /// ```
        /// </remarks>
        /// <returns>Resumo para dashboard</returns>
        [HttpGet("dashboard")]
        [SwaggerOperation(
            Summary = "Resumo do dashboard",
            Description = "Retorna métricas resumidas para exibição em dashboards",
            OperationId = "GetDashboardSummary"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Resumo obtido", typeof(ApiResponseDto<object>))]
        public async Task<ActionResult<ApiResponseDto<object>>> GetDashboardSummary()
        {
            try
            {
                _logger.LogInformation("Obtendo resumo para dashboard");

                var stats = await _leadService.GetStatisticsAsync();

                // Cria objeto simplificado para dashboard
                var summary = new
                {
                    stats.TotalLeads,
                    stats.ActiveLeads,
                    stats.NewLeadsThisWeek,
                    stats.NewLeadsThisMonth,
                    HighPriorityCount = stats.PriorityDistribution.HighPriority,
                    stats.ConversionRate,
                    stats.ResponseRate,
                    stats.TotalEstimatedValue,
                    stats.AverageScore,
                    TopSource = stats.SourceDistribution.OrderByDescending(s => s.Count).FirstOrDefault()?.SourceLabel ?? "N/A",
                    GeneratedAt = stats.GeneratedAt
                };

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    summary,
                    "Resumo do dashboard obtido com sucesso"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter resumo do dashboard");
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        /// <summary>
        /// Obtém métricas de conversão detalhadas
        /// </summary>
        /// <remarks>
        /// Retorna métricas detalhadas sobre conversão de leads.
        /// Inclui taxas por origem, prioridade e período.
        /// </remarks>
        /// <returns>Métricas de conversão</returns>
        [HttpGet("conversion-metrics")]
        [SwaggerOperation(
            Summary = "Métricas de conversão",
            Description = "Retorna métricas detalhadas de conversão de leads",
            OperationId = "GetConversionMetrics"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Métricas obtidas", typeof(ApiResponseDto<object>))]
        public async Task<ActionResult<ApiResponseDto<object>>> GetConversionMetrics()
        {
            try
            {
                _logger.LogInformation("Obtendo métricas de conversão");

                var stats = await _leadService.GetStatisticsAsync();

                // Calcula métricas adicionais
                var conversionBySource = stats.SourceDistribution
                    .Select(s => new
                    {
                        s.SourceLabel,
                        s.Count,
                        s.TotalValue,
                        ConversionRate = stats.TotalLeads > 0
                            ? Math.Round((decimal)s.Count / stats.TotalLeads * 100, 2)
                            : 0
                    })
                    .OrderByDescending(x => x.Count)
                    .ToList();

                var metrics = new
                {
                    stats.ConvertedLeads,
                    stats.ConversionRate,
                    stats.TotalEstimatedValue,
                    stats.AverageEstimatedValue,
                    conversionBySource,
                    PriorityBreakdown = stats.PriorityDistribution,
                    StatusBreakdown = stats.StatusDistribution
                };

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    metrics,
                    "Métricas de conversão obtidas com sucesso"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter métricas de conversão");
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }
    }
}
