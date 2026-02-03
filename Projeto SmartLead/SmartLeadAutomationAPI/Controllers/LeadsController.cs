using Microsoft.AspNetCore.Mvc;
using SmartLeadAutomation.DTOs;
using SmartLeadAutomation.Models.Enums;
using SmartLeadAutomation.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace SmartLeadAutomation.Controllers
{
    /// <summary>
    /// Controller para gestão de leads (CRUD completo).
    /// Fornece endpoints para criação, consulta, atualização, exclusão e classificação de leads.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    [SwaggerTag("Gestão de Leads - CRUD completo com classificação automática")]
    public class LeadsController : ControllerBase
    {
        private readonly ILeadService _leadService;
        private readonly ILogger<LeadsController> _logger;

        /// <summary>
        /// Construtor com injeção de dependências
        /// </summary>
        public LeadsController(ILeadService leadService, ILogger<LeadsController> logger)
        {
            _leadService = leadService;
            _logger = logger;
        }

        #region Operações de Consulta

        /// <summary>
        /// Lista todos os leads com suporte a filtros e paginação
        /// </summary>
        /// <remarks>
        /// Retorna uma lista paginada de leads. Suporta filtros por:
        /// - Termo de busca (nome, email, empresa)
        /// - Prioridade (Alta, Media, Baixa)
        /// - Status (Novo, EmQualificacao, Qualificado, EmNegociacao, Convertido, Descartado, Arquivado)
        /// - Origem (Website, Facebook, Instagram, LinkedIn, GoogleAds, EmailMarketing, Indicacao, Evento, Telefone, Chat)
        /// - Valor estimado (mínimo e máximo)
        /// - Data de criação
        /// - Consentimento de marketing
        /// 
        /// **Exemplo de requisição:**
        /// ```
        /// GET /api/leads?searchTerm=joão&amp;priority=Alta&amp;pageSize=10
        /// ```
        /// </remarks>
        /// <param name="filter">Filtros e parâmetros de paginação</param>
        /// <returns>Lista paginada de leads</returns>
        /// <response code="200">Retorna a lista de leads</response>
        /// <response code="400">Parâmetros de filtro inválidos</response>
        /// <response code="500">Erro interno do servidor</response>
        [HttpGet]
        [SwaggerOperation(
            Summary = "Listar leads",
            Description = "Retorna lista paginada de leads com suporte a filtros avançados",
            OperationId = "GetAllLeads"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Lista de leads retornada com sucesso", typeof(ApiResponseDto<PagedResponseDto<LeadResponseDto>>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Parâmetros inválidos", typeof(ApiResponseDto))]
        [SwaggerResponse(StatusCodes.Status500InternalServerError, "Erro interno", typeof(ApiResponseDto))]
        public async Task<ActionResult<ApiResponseDto<PagedResponseDto<LeadResponseDto>>>> GetAll([FromQuery] LeadFilterDto filter)
        {
            try
            {
                _logger.LogInformation("Buscando leads com filtros: {@Filter}", filter);

                var result = await _leadService.GetAllLeadsAsync(filter);

                return Ok(ApiResponseDto<PagedResponseDto<LeadResponseDto>>.SuccessResponse(
                    result,
                    $"{result.TotalCount} lead(s) encontrado(s)"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar leads");
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        /// <summary>
        /// Obtém um lead específico pelo ID
        /// </summary>
        /// <remarks>
        /// Retorna os detalhes completos de um lead, incluindo histórico e classificação.
        /// 
        /// **Exemplo de requisição:**
        /// ```
        /// GET /api/leads/1
        /// ```
        /// </remarks>
        /// <param name="id">ID do lead</param>
        /// <returns>Dados completos do lead</returns>
        /// <response code="200">Lead encontrado</response>
        /// <response code="404">Lead não encontrado</response>
        [HttpGet("{id:int}")]
        [SwaggerOperation(
            Summary = "Obter lead por ID",
            Description = "Retorna os detalhes completos de um lead específico",
            OperationId = "GetLeadById"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Lead encontrado", typeof(ApiResponseDto<LeadResponseDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Lead não encontrado", typeof(ApiResponseDto))]
        public async Task<ActionResult<ApiResponseDto<LeadResponseDto>>> GetById(int id)
        {
            try
            {
                _logger.LogInformation("Buscando lead por ID: {LeadId}", id);

                var lead = await _leadService.GetLeadByIdAsync(id);

                if (lead == null)
                {
                    return NotFound(ApiResponseDto<LeadResponseDto>.ErrorResponse(
                        $"Lead com ID {id} não encontrado",
                        statusCode: 404
                    ));
                }

                return Ok(ApiResponseDto<LeadResponseDto>.SuccessResponse(lead));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar lead por ID: {LeadId}", id);
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        /// <summary>
        /// Busca um lead pelo endereço de email
        /// </summary>
        /// <remarks>
        /// Útil para verificar se um email já está cadastrado no sistema.
        /// 
        /// **Exemplo de requisição:**
        /// ```
        /// GET /api/leads/by-email/joao.silva@empresa.com
        /// ```
        /// </remarks>
        /// <param name="email">Endereço de email do lead</param>
        /// <returns>Dados do lead</returns>
        /// <response code="200">Lead encontrado</response>
        /// <response code="404">Lead não encontrado</response>
        [HttpGet("by-email/{email}")]
        [SwaggerOperation(
            Summary = "Buscar lead por email",
            Description = "Busca um lead pelo endereço de email",
            OperationId = "GetLeadByEmail"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Lead encontrado", typeof(ApiResponseDto<LeadResponseDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Lead não encontrado", typeof(ApiResponseDto))]
        public async Task<ActionResult<ApiResponseDto<LeadResponseDto>>> GetByEmail(string email)
        {
            try
            {
                _logger.LogInformation("Buscando lead por email: {Email}", email);

                var lead = await _leadService.GetLeadByEmailAsync(email);

                if (lead == null)
                {
                    return NotFound(ApiResponseDto<LeadResponseDto>.ErrorResponse(
                        $"Lead com email '{email}' não encontrado",
                        statusCode: 404
                    ));
                }

                return Ok(ApiResponseDto<LeadResponseDto>.SuccessResponse(lead));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar lead por email: {Email}", email);
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        /// <summary>
        /// Verifica se um email já está cadastrado
        /// </summary>
        /// <remarks>
        /// Endpoint rápido para validação de duplicidade de email.
        /// 
        /// **Exemplo de requisição:**
        /// ```
        /// GET /api/leads/check-email/joao.silva@empresa.com
        /// ```
        /// </remarks>
        /// <param name="email">Email a ser verificado</param>
        /// <returns>Status da verificação</returns>
        [HttpGet("check-email/{email}")]
        [SwaggerOperation(
            Summary = "Verificar disponibilidade de email",
            Description = "Verifica se um email já está cadastrado no sistema",
            OperationId = "CheckEmailExists"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Resultado da verificação")]
        public async Task<ActionResult<ApiResponseDto<object>>> CheckEmailExists(string email)
        {
            try
            {
                var exists = await _leadService.EmailExistsAsync(email);

                return Ok(ApiResponseDto<object>.SuccessResponse(
                    new { exists, email },
                    exists ? "Email já cadastrado" : "Email disponível"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar email: {Email}", email);
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        #endregion

        #region Operações de Criação e Atualização

        /// <summary>
        /// Cria um novo lead
        /// </summary>
        /// <remarks>
        /// Cria um novo lead no sistema com classificação automática de prioridade.
        /// O email deve ser único no sistema.
        /// 
        /// **Exemplo de requisição:**
        /// ```json
        /// {
        ///   "name": "João Silva",
        ///   "email": "joao.silva@empresa.com",
        ///   "phone": "(11) 98765-4321",
        ///   "company": "Empresa XYZ",
        ///   "jobTitle": "Diretor de TI",
        ///   "source": "LinkedIn",
        ///   "estimatedValue": 50000,
        ///   "marketingConsent": true,
        ///   "notes": "Interessado em automação"
        /// }
        /// ```
        /// </remarks>
        /// <param name="dto">Dados do lead a ser criado</param>
        /// <returns>Lead criado com sua classificação</returns>
        /// <response code="201">Lead criado com sucesso</response>
        /// <response code="400">Dados inválidos ou email duplicado</response>
        [HttpPost]
        [SwaggerOperation(
            Summary = "Criar lead",
            Description = "Cria um novo lead com classificação automática de prioridade",
            OperationId = "CreateLead"
        )]
        [SwaggerResponse(StatusCodes.Status201Created, "Lead criado com sucesso", typeof(ApiResponseDto<LeadResponseDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inválidos", typeof(ApiResponseDto))]
        [SwaggerResponse(StatusCodes.Status409Conflict, "Email já cadastrado", typeof(ApiResponseDto))]
        public async Task<ActionResult<ApiResponseDto<LeadResponseDto>>> Create([FromBody] CreateLeadDto dto)
        {
            try
            {
                _logger.LogInformation("Criando novo lead: {Email}", dto.Email);

                if (!ModelState.IsValid)
                {
                    var validationErrors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponseDto<LeadResponseDto>.ErrorResponse(
                        "Dados de entrada inválidos",
                        validationErrors
                    ));
                }

                var (lead, errors) = await _leadService.CreateLeadAsync(dto);

                if (errors.Any())
                {
                    return BadRequest(ApiResponseDto<LeadResponseDto>.ErrorResponse(
                        "Erro de validação",
                        errors
                    ));
                }

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = lead!.Id },
                    ApiResponseDto<LeadResponseDto>.CreatedResponse(lead, "Lead criado com sucesso")
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar lead: {Email}", dto.Email);
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        /// <summary>
        /// Atualiza um lead existente
        /// </summary>
        /// <remarks>
        /// Atualiza os dados de um lead existente. A classificação de prioridade é recalculada automaticamente.
        /// 
        /// **Exemplo de requisição:**
        /// ```json
        /// {
        ///   "name": "João Silva Atualizado",
        ///   "phone": "(11) 98765-4321",
        ///   "company": "Nova Empresa",
        ///   "jobTitle": "CEO",
        ///   "status": "Qualificado",
        ///   "source": "Website",
        ///   "estimatedValue": 75000,
        ///   "conversionProbability": 60,
        ///   "marketingConsent": true,
        ///   "hasResponded": true,
        ///   "notes": "Lead qualificado após reunião"
        /// }
        /// ```
        /// </remarks>
        /// <param name="id">ID do lead</param>
        /// <param name="dto">Dados atualizados</param>
        /// <returns>Lead atualizado</returns>
        /// <response code="200">Lead atualizado com sucesso</response>
        /// <response code="400">Dados inválidos</response>
        /// <response code="404">Lead não encontrado</response>
        [HttpPut("{id:int}")]
        [SwaggerOperation(
            Summary = "Atualizar lead",
            Description = "Atualiza os dados de um lead existente",
            OperationId = "UpdateLead"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Lead atualizado", typeof(ApiResponseDto<LeadResponseDto>))]
        [SwaggerResponse(StatusCodes.Status400BadRequest, "Dados inválidos", typeof(ApiResponseDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Lead não encontrado", typeof(ApiResponseDto))]
        public async Task<ActionResult<ApiResponseDto<LeadResponseDto>>> Update(int id, [FromBody] UpdateLeadDto dto)
        {
            try
            {
                _logger.LogInformation("Atualizando lead: {LeadId}", id);

                if (!ModelState.IsValid)
                {
                    var validationErrors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(ApiResponseDto<LeadResponseDto>.ErrorResponse(
                        "Dados de entrada inválidos",
                        validationErrors
                    ));
                }

                var (lead, errors) = await _leadService.UpdateLeadAsync(id, dto);

                if (errors.Any())
                {
                    if (errors.First().Contains("não encontrado"))
                    {
                        return NotFound(ApiResponseDto<LeadResponseDto>.ErrorResponse(
                            errors.First(),
                            statusCode: 404
                        ));
                    }

                    return BadRequest(ApiResponseDto<LeadResponseDto>.ErrorResponse(
                        "Erro de validação",
                        errors
                    ));
                }

                return Ok(ApiResponseDto<LeadResponseDto>.SuccessResponse(
                    lead!,
                    "Lead atualizado com sucesso"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar lead: {LeadId}", id);
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        #endregion

        #region Operações de Exclusão

        /// <summary>
        /// Remove um lead (exclusão lógica)
        /// </summary>
        /// <remarks>
        /// Realiza exclusão lógica do lead (marca como inativo).
        /// O lead pode ser restaurado posteriormente.
        /// 
        /// **Exemplo de requisição:**
        /// ```
        /// DELETE /api/leads/1
        /// ```
        /// </remarks>
        /// <param name="id">ID do lead</param>
        /// <returns>Confirmação de exclusão</returns>
        /// <response code="200">Lead removido com sucesso</response>
        /// <response code="404">Lead não encontrado</response>
        [HttpDelete("{id:int}")]
        [SwaggerOperation(
            Summary = "Remover lead",
            Description = "Remove um lead do sistema (exclusão lógica)",
            OperationId = "DeleteLead"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Lead removido", typeof(ApiResponseDto))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Lead não encontrado", typeof(ApiResponseDto))]
        public async Task<ActionResult<ApiResponseDto>> Delete(int id)
        {
            try
            {
                _logger.LogInformation("Removendo lead: {LeadId}", id);

                var result = await _leadService.DeleteLeadAsync(id);

                if (!result)
                {
                    return NotFound(ApiResponseDto.ErrorResponse(
                        $"Lead com ID {id} não encontrado",
                        statusCode: 404
                    ));
                }

                return Ok(ApiResponseDto.DeletedResponse("Lead removido com sucesso"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao remover lead: {LeadId}", id);
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        /// <summary>
        /// Restaura um lead removido
        /// </summary>
        /// <remarks>
        /// Restaura um lead que foi previamente removido (exclusão lógica).
        /// 
        /// **Exemplo de requisição:**
        /// ```
        /// POST /api/leads/1/restore
        /// ```
        /// </remarks>
        /// <param name="id">ID do lead</param>
        /// <returns>Lead restaurado</returns>
        /// <response code="200">Lead restaurado com sucesso</response>
        /// <response code="404">Lead não encontrado ou já ativo</response>
        [HttpPost("{id:int}/restore")]
        [SwaggerOperation(
            Summary = "Restaurar lead",
            Description = "Restaura um lead previamente removido",
            OperationId = "RestoreLead"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Lead restaurado", typeof(ApiResponseDto<LeadResponseDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Lead não encontrado", typeof(ApiResponseDto))]
        public async Task<ActionResult<ApiResponseDto<LeadResponseDto>>> Restore(int id)
        {
            try
            {
                _logger.LogInformation("Restaurando lead: {LeadId}", id);

                var result = await _leadService.RestoreLeadAsync(id);

                if (!result)
                {
                    return NotFound(ApiResponseDto.ErrorResponse(
                        $"Lead com ID {id} não encontrado ou já está ativo",
                        statusCode: 404
                    ));
                }

                // Busca o lead restaurado
                var lead = await _leadService.GetLeadByIdAsync(id);

                return Ok(ApiResponseDto<LeadResponseDto>.SuccessResponse(
                    lead!,
                    "Lead restaurado com sucesso"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao restaurar lead: {LeadId}", id);
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        #endregion

        #region Operações de Status e Classificação

        /// <summary>
        /// Atualiza o status de um lead
        /// </summary>
        /// <remarks>
        /// Altera o status do lead no funil de vendas.
        /// Status disponíveis: Novo, EmQualificacao, Qualificado, EmNegociacao, Convertido, Descartado, Arquivado
        /// 
        /// **Exemplo de requisição:**
        /// ```
        /// PATCH /api/leads/1/status?newStatus=Qualificado
        /// ```
        /// </remarks>
        /// <param name="id">ID do lead</param>
        /// <param name="newStatus">Novo status</param>
        /// <returns>Lead com status atualizado</returns>
        [HttpPatch("{id:int}/status")]
        [SwaggerOperation(
            Summary = "Atualizar status",
            Description = "Atualiza o status de um lead no funil de vendas",
            OperationId = "UpdateLeadStatus"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Status atualizado", typeof(ApiResponseDto<LeadResponseDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Lead não encontrado", typeof(ApiResponseDto))]
        public async Task<ActionResult<ApiResponseDto<LeadResponseDto>>> UpdateStatus(
            int id,
            [FromQuery] LeadStatus newStatus)
        {
            try
            {
                _logger.LogInformation("Atualizando status do lead {LeadId} para {NewStatus}", id, newStatus);

                var lead = await _leadService.UpdateLeadStatusAsync(id, newStatus);

                if (lead == null)
                {
                    return NotFound(ApiResponseDto<LeadResponseDto>.ErrorResponse(
                        $"Lead com ID {id} não encontrado",
                        statusCode: 404
                    ));
                }

                return Ok(ApiResponseDto<LeadResponseDto>.SuccessResponse(
                    lead,
                    $"Status atualizado para '{newStatus}'"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar status do lead: {LeadId}", id);
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        /// <summary>
        /// Marca um lead como respondido
        /// </summary>
        /// <remarks>
        /// Registra que o lead respondeu a uma comunicação.
        /// Incrementa o contador de interações e atualiza a data do último contato.
        /// 
        /// **Exemplo de requisição:**
        /// ```
        /// POST /api/leads/1/mark-responded
        /// ```
        /// </remarks>
        /// <param name="id">ID do lead</param>
        /// <returns>Lead atualizado</returns>
        [HttpPost("{id:int}/mark-responded")]
        [SwaggerOperation(
            Summary = "Marcar como respondido",
            Description = "Marca um lead como respondido e incrementa interações",
            OperationId = "MarkLeadAsResponded"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Lead marcado como respondido", typeof(ApiResponseDto<LeadResponseDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Lead não encontrado", typeof(ApiResponseDto))]
        public async Task<ActionResult<ApiResponseDto<LeadResponseDto>>> MarkAsResponded(int id)
        {
            try
            {
                _logger.LogInformation("Marcando lead como respondido: {LeadId}", id);

                var lead = await _leadService.MarkLeadAsRespondedAsync(id);

                if (lead == null)
                {
                    return NotFound(ApiResponseDto<LeadResponseDto>.ErrorResponse(
                        $"Lead com ID {id} não encontrado",
                        statusCode: 404
                    ));
                }

                return Ok(ApiResponseDto<LeadResponseDto>.SuccessResponse(
                    lead,
                    "Lead marcado como respondido"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao marcar lead como respondido: {LeadId}", id);
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        /// <summary>
        /// Recalcula a prioridade de um lead
        /// </summary>
        /// <remarks>
        /// Força o recálculo da classificação de prioridade do lead.
        /// Útil quando as regras de negócio mudam ou após atualizações em lote.
        /// 
        /// **Exemplo de requisição:**
        /// ```
        /// POST /api/leads/1/recalculate-priority
        /// ```
        /// </remarks>
        /// <param name="id">ID do lead</param>
        /// <returns>Lead com prioridade recalculada</returns>
        [HttpPost("{id:int}/recalculate-priority")]
        [SwaggerOperation(
            Summary = "Recalcular prioridade",
            Description = "Força o recálculo da classificação de prioridade do lead",
            OperationId = "RecalculateLeadPriority"
        )]
        [SwaggerResponse(StatusCodes.Status200OK, "Prioridade recalculada", typeof(ApiResponseDto<LeadResponseDto>))]
        [SwaggerResponse(StatusCodes.Status404NotFound, "Lead não encontrado", typeof(ApiResponseDto))]
        public async Task<ActionResult<ApiResponseDto<LeadResponseDto>>> RecalculatePriority(int id)
        {
            try
            {
                _logger.LogInformation("Recalculando prioridade do lead: {LeadId}", id);

                var lead = await _leadService.RecalculatePriorityAsync(id);

                if (lead == null)
                {
                    return NotFound(ApiResponseDto<LeadResponseDto>.ErrorResponse(
                        $"Lead com ID {id} não encontrado",
                        statusCode: 404
                    ));
                }

                return Ok(ApiResponseDto<LeadResponseDto>.SuccessResponse(
                    lead,
                    $"Prioridade recalculada: {lead.Priority} (Score: {lead.Score})"
                ));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao recalcular prioridade do lead: {LeadId}", id);
                return StatusCode(500, ApiResponseDto.ErrorResponse(
                    "Erro interno ao processar a solicitação",
                    statusCode: 500
                ));
            }
        }

        #endregion
    }
}
