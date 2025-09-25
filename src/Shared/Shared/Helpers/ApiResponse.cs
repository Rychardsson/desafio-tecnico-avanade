using System.Net;

namespace Shared.Helpers
{
    public class ApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string> Errors { get; set; } = new List<string>();
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        public static ApiResponse<T> Success(T data, string message = "Operação realizada com sucesso")
        {
            return new ApiResponse<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }
        
        public static ApiResponse<T> Error(string message, List<string>? errors = null)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = errors ?? new List<string>()
            };
        }
        
        public static ApiResponse<T> Error(string message, string error)
        {
            return new ApiResponse<T>
            {
                IsSuccess = false,
                Message = message,
                Errors = new List<string> { error }
            };
        }
        
        // Métodos de compatibilidade com os projetos antigos
        public static ApiResponse<T> SuccessResult(T data, string message = "Operação realizada com sucesso")
        {
            return Success(data, message);
        }
        
        public static ApiResponse<T> FailureResult(string message, List<string>? errors = null)
        {
            return Error(message, errors);
        }
        
        public static ApiResponse<T> FailureResult(string message, string error)
        {
            return Error(message, error);
        }
    }
    
    public class PaginatedResponse<T>
    {
        public List<T> Data { get; set; } = new List<T>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNextPage => CurrentPage < TotalPages;
        public bool HasPreviousPage => CurrentPage > 1;
        
        public static PaginatedResponse<T> Create(List<T> data, int currentPage, int pageSize, int totalCount)
        {
            return new PaginatedResponse<T>
            {
                Data = data,
                CurrentPage = currentPage,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}
