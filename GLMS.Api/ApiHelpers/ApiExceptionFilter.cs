using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

//ST10445500 - PROG7311 - GLMS POE
//ApiExceptionFilter

//.....................................o0oSTART OF FILEo0o........................................//

// This helper turns API errors into clear HTTP responses for the frontend.

namespace GLMS.Api.ApiHelpers
{
	public class ApiExceptionFilter : IExceptionFilter
	{
		private readonly ILogger<ApiExceptionFilter> _logger;

		public ApiExceptionFilter(ILogger<ApiExceptionFilter> logger)
		{
			_logger = logger;
		}

		public void OnException(ExceptionContext context)
		{
			var (statusCode, errors) = context.Exception switch
			{
				KeyNotFoundException ex => (StatusCodes.Status404NotFound, new[] { ex.Message }),
				ArgumentException ex => (StatusCodes.Status400BadRequest, new[] { ex.Message }),
				InvalidOperationException ex => (StatusCodes.Status400BadRequest, new[] { ex.Message }),
				HttpRequestException ex => (StatusCodes.Status400BadRequest, new[] { ex.Message }),
				TaskCanceledException ex => (StatusCodes.Status400BadRequest, new[] { ex.Message }),
				UnauthorizedAccessException ex => (StatusCodes.Status403Forbidden, new[] { ex.Message }),
				_ => (StatusCodes.Status500InternalServerError, new[] { "An unexpected error occurred." })
			};

			if (statusCode == StatusCodes.Status500InternalServerError)
			{
				_logger.LogError(context.Exception, "Unhandled API exception.");
			}

			context.Result = new ObjectResult(new ApiErrorResponse(errors))
			{
				StatusCode = statusCode
			};
			context.ExceptionHandled = true;
		}
	}
}

//.....................................o0oEND OF FILEo0o..........................................//
