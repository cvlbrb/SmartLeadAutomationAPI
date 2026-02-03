namespace SmartLeadAutomation.DTOs
{
    /// <summary>
    /// DTO para respostas paginadas da API.
    /// Fornece metadados de paginação junto com os dados.
    /// </summary>
    /// <typeparam name="T">Tipo dos itens na lista</typeparam>
    public class PagedResponseDto<T>
    {
        /// <summary>
        /// Lista de itens da página atual
        /// </summary>
        public List<T> Items { get; set; } = new List<T>();

        /// <summary>
        /// Número da página atual
        /// </summary>
        public int PageNumber { get; set; }

        /// <summary>
        /// Tamanho da página
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Total de registros encontrados
        /// </summary>
        public int TotalCount { get; set; }

        /// <summary>
        /// Total de páginas disponíveis
        /// </summary>
        public int TotalPages { get; set; }

        /// <summary>
        /// Indica se existe página anterior
        /// </summary>
        public bool HasPreviousPage => PageNumber > 1;

        /// <summary>
        /// Indica se existe próxima página
        /// </summary>
        public bool HasNextPage => PageNumber < TotalPages;

        /// <summary>
        /// Cria uma resposta paginada vazia
        /// </summary>
        public static PagedResponseDto<T> Empty(int pageNumber = 1, int pageSize = 20)
        {
            return new PagedResponseDto<T>
            {
                Items = new List<T>(),
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = 0,
                TotalPages = 0
            };
        }

        /// <summary>
        /// Cria uma resposta paginada com dados
        /// </summary>
        public static PagedResponseDto<T> Create(List<T> items, int pageNumber, int pageSize, int totalCount)
        {
            return new PagedResponseDto<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
    }
}
