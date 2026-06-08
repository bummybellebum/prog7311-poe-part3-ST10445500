//ST10445500 - PROG7311 - GLMS POE
//ApiClientResult

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.ApiClients
{
    public class ApiClientResult
    {
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public int? StatusCode { get; set; }

        public static ApiClientResult Success()
        {
            return new ApiClientResult { IsSuccess = true };
        }

        public static ApiClientResult Failure(string message, int? statusCode = null)
        {
            return new ApiClientResult
            {
                IsSuccess = false,
                ErrorMessage = message,
                StatusCode = statusCode
            };
        }
    }

    public class ApiClientResult<T> : ApiClientResult
    {
        public T? Data { get; set; }

        public static ApiClientResult<T> Success(T data)
        {
            return new ApiClientResult<T>
            {
                IsSuccess = true,
                Data = data
            };
        }

        public static new ApiClientResult<T> Failure(string message, int? statusCode = null)
        {
            return new ApiClientResult<T>
            {
                IsSuccess = false,
                ErrorMessage = message,
                StatusCode = statusCode
            };
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
