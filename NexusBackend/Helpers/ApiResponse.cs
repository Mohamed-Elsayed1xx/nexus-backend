namespace NexusBackend.Helpers;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
    public int StatusCode { get; set; }

    public static ApiResponse<T> Ok(T data, string? message = null) => new()
    {
        Success = true,
        Data = data,
        Message = message,
        StatusCode = 200
    };

    public static ApiResponse<T> Created(T data, string? message = "Created successfully") => new()
    {
        Success = true,
        Data = data,
        Message = message,
        StatusCode = 201
    };

    public static ApiResponse<T> Fail(string message, int statusCode = 400) => new()
    {
        Success = false,
        Message = message,
        StatusCode = statusCode
    };
}

public class PaginatedResponse<T>
{
    public bool Success { get; set; } = true;
    public IEnumerable<T> Data { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrevious => Page > 1;
}