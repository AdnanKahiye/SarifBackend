namespace Backend.Wrapper
{
    /// <summary>
    /// Generic response wrapper used across API and Application layers.
    /// Provides a unified structure for success, failure, and exception responses.
    /// </summary>
    /// <typeparam name="T">The type of the response data payload.</typeparam>
    public class ResponseWrapper<T> : IResponseWrapper<T>
    {
        /// <summary>
        /// The actual data payload of the response.
        /// Nullable because failed operations may not return data.
        /// </summary>
        public T? Data { get; set; }

        /// <inheritdoc/>
        public bool Success { get; set; }

        /// <inheritdoc/>
        public string Message { get; set; } = string.Empty;

        /// <inheritdoc/>
        public List<string> Errors { get; set; } = new();

        /// <inheritdoc/>
        public int StatusCode { get; set; } = 200;

        // ============================================================
        // Constructors
        // ============================================================

        /// <summary>
        /// Creates a successful response with data.
        /// </summary>
        public ResponseWrapper(T data, string message = "", int statusCode = 200)
        {
            Data = data;
            Success = true;
            Message = message;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Creates a failed response with multiple error messages.
        /// </summary>
        public ResponseWrapper(List<string> errors, string message = "An error occurred.", int statusCode = 400)
        {
            Errors = errors ?? new List<string>();
            Success = false;
            Message = message;
            StatusCode = statusCode;
        }

        /// <summary>
        /// Creates a failed response with a single error message.
        /// </summary>
        public ResponseWrapper(string message, int statusCode = 400)
        {
            Success = false;
            Message = message;
            Errors = new List<string> { message };
            StatusCode = statusCode;
        }

        // ============================================================
        // Factory Helper Methods (Synchronous)
        // ============================================================

        /// <summary>
        /// Returns a standardized successful response.
        /// </summary>
        public static ResponseWrapper<T> Ok(T data, string message = "", int statusCode = 200)
            => new ResponseWrapper<T>(data, message, statusCode);

        /// <summary>
        /// Returns a failed response with a list of error messages.
        /// </summary>
        public static ResponseWrapper<T> Fail(List<string> errors, string message = "An error occurred.", int statusCode = 400)
            => new ResponseWrapper<T>(errors, message, statusCode);

        /// <summary>
        /// Returns a failed response with a single error message.
        /// </summary>
        public static ResponseWrapper<T> Fail(string error, string message = "An error occurred.", int statusCode = 400)
            => new ResponseWrapper<T>(new List<string> { error }, message, statusCode);

        /// <summary>
        /// Returns a standardized internal server error response.
        /// </summary>
        public static ResponseWrapper<T> ServerError(string message = "Internal server error.")
            => new ResponseWrapper<T>(message, 500);

        // ============================================================
        // Factory Helper Methods (Asynchronous)
        // ============================================================

        /// <summary>
        /// Async version for successful responses.
        /// </summary>
        public static Task<ResponseWrapper<T>> SuccessAsync(T data, string message = "", int statusCode = 200)
            => Task.FromResult(Ok(data, message, statusCode));

        /// <summary>
        /// Async version for failed responses (multiple errors).
        /// </summary>
        public static Task<ResponseWrapper<T>> FailureAsync(List<string> errors, string message = "An error occurred.", int statusCode = 400)
            => Task.FromResult(Fail(errors, message, statusCode));

        /// <summary>
        /// Async version for failed responses (single error).
        /// </summary>
        public static Task<ResponseWrapper<T>> FailureAsync(string error, string message = "An error occurred.", int statusCode = 400)
            => Task.FromResult(Fail(error, message, statusCode));

        /// <summary>
        /// Async version for internal server errors.
        /// </summary>
        public static Task<ResponseWrapper<T>> ServerErrorAsync(string message = "Internal server error.")
            => Task.FromResult(ServerError(message));
    }

    // ============================================================
    // Interface
    // ============================================================

    /// <summary>
    /// Generic interface to standardize API and Application responses.
    /// </summary>
    /// <typeparam name="T">The type of the response data payload.</typeparam>
    public interface IResponseWrapper<T>
    {
        /// <summary>
        /// Indicates whether the operation succeeded.
        /// </summary>
        bool Success { get; set; }

        /// <summary>
        /// A descriptive message for the consumer (success or failure message).
        /// </summary>
        string Message { get; set; }

        /// <summary>
        /// The HTTP-like status code for the result (e.g., 200, 400, 500).
        /// </summary>
        int StatusCode { get; set; }

        /// <summary>
        /// A collection of error messages (for validation or system errors).
        /// </summary>
        List<string> Errors { get; set; }
    }
}
