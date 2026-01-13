namespace DMS.Api.Shared
{
    /// <summary>
    /// Standard API response structure
    /// </summary>
    public class ApiResponse<T>
    {
        public ResponseStatus Status { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }
        public DateTime Timestamp { get; set; }

        public ApiResponse()
        {
            Timestamp = DateTime.Now;
        }

        public ApiResponse(ResponseStatus status, string message, T? data = default)
        {
            Status = status;
            Message = message;
            Data = data;
            Timestamp = DateTime.Now;
        }

        // Success response helpers
        public static ApiResponse<T> SuccessResponse(T data, string message = "Operation successful")
        {
            return new ApiResponse<T>(ResponseStatus.Success, message, data);
        }

        public static ApiResponse<T> SuccessResponse(ResponseStatus status, string message, T data)
        {
            return new ApiResponse<T>(status, message, data);
        }

        // Error response helpers
        public static ApiResponse<T> ErrorResponse(ResponseStatus status, string message)
        {
            return new ApiResponse<T>(status, message, default);
        }

        public static ApiResponse<T> ErrorResponse(string message)
        {
            return new ApiResponse<T>(ResponseStatus.InternalServerError, message, default);
        }
    }

    /// <summary>
    /// API response without data payload
    /// </summary>
    public class ApiResponse : ApiResponse<object>
    {
        public ApiResponse() : base() { }

        public ApiResponse(ResponseStatus status, string message) : base(status, message, null) { }

        public new static ApiResponse SuccessResponse(string message = "Operation successful")
        {
            return new ApiResponse(ResponseStatus.Success, message);
        }

        public new static ApiResponse ErrorResponse(ResponseStatus status, string message)
        {
            return new ApiResponse(status, message);
        }
    }
}
