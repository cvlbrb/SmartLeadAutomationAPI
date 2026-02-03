namespace SmartLeadAutomation.DTOs
{
    /// <summary>
    /// DTO padronizado para respostas da API.
    /// Fornece estrutura consistente para sucesso e erro.
    /// </summary>
    /// <typeparam name="T">Tipo do dado retornado</typeparam>
    public class ApiResponseDto<T>
    {
        /// <summary>
        /// Indica se a operação foi bem-sucedida
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensagem descritiva do resultado
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Dados retornados (quando aplicável)
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Lista de erros de validação (quando aplicável)
        /// </summary>
        public List<string> Errors { get; set; } = new List<string>();

        /// <summary>
        /// Código de status HTTP
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        /// Timestamp da resposta
        /// </summary>
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// ID da requisição para rastreamento
        /// </summary>
        public string RequestId { get; set; } = Guid.NewGuid().ToString("N")[..8];

        /// <summary>
        /// Cria uma resposta de sucesso
        /// </summary>
        public static ApiResponseDto<T> SuccessResponse(T data, string message = "Operação realizada com sucesso", int statusCode = 200)
        {
            return new ApiResponseDto<T>
            {
                Success = true,
                Message = message,
                Data = data,
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Cria uma resposta de erro
        /// </summary>
        public static ApiResponseDto<T> ErrorResponse(string message, List<string>? errors = null, int statusCode = 400)
        {
            return new ApiResponseDto<T>
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>(),
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Cria uma resposta de erro simples
        /// </summary>
        public static ApiResponseDto<T> ErrorResponse(string message, int statusCode = 400)
        {
            return ErrorResponse(message, null, statusCode);
        }

        /// <summary>
        /// Cria uma resposta de criação bem-sucedida
        /// </summary>
        public static ApiResponseDto<T> CreatedResponse(T data, string message = "Recurso criado com sucesso")
        {
            return SuccessResponse(data, message, 201);
        }

        /// <summary>
        /// Cria uma resposta de exclusão bem-sucedida
        /// </summary>
        public static ApiResponseDto<T> DeletedResponse(string message = "Recurso removido com sucesso")
        {
            return SuccessResponse(default!, message, 200);
        }
    }

    /// <summary>
    /// DTO para respostas simples sem dados tipados
    /// </summary>
    public class ApiResponseDto : ApiResponseDto<object>
    {
        /// <summary>
        /// Cria uma resposta de sucesso simples
        /// </summary>
        public static ApiResponseDto SuccessResponse(string message = "Operação realizada com sucesso", int statusCode = 200)
        {
            return new ApiResponseDto
            {
                Success = true,
                Message = message,
                StatusCode = statusCode
            };
        }

        /// <summary>
        /// Cria uma resposta de erro simples
        /// </summary>
        public static new ApiResponseDto ErrorResponse(string message, List<string>? errors = null, int statusCode = 400)
        {
            return new ApiResponseDto
            {
                Success = false,
                Message = message,
                Errors = errors ?? new List<string>(),
                StatusCode = statusCode
            };
        }
    }
}
