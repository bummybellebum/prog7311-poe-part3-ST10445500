using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

//ST10445500 - PROG7311 - GLMS POE
//ApiClientBase

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.ApiClients
{
	public abstract class ApiClientBase(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
	{
		private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
		protected readonly HttpClient HttpClient = httpClient;

		protected static readonly JsonSerializerOptions JsonOptions = new()
		{
			PropertyNameCaseInsensitive = true,
			ReferenceHandler = ReferenceHandler.IgnoreCycles
		};

		//..............................................................................//

		protected Task<ApiClientResult<T>> GetAsync<T>(string endpoint)
		{
			return SendForJsonAsync<T>(HttpMethod.Get, endpoint);
		}

		//..............................................................................//

		protected Task<ApiClientResult<TResponse>> PostAsync<TRequest, TResponse>(string endpoint, TRequest request)
		{
			return SendForJsonAsync<TResponse>(HttpMethod.Post, endpoint, request);
		}

		//..............................................................................//

		protected Task<ApiClientResult> PostAsync<TRequest>(string endpoint, TRequest request)
		{
			return SendNoContentAsync(HttpMethod.Post, endpoint, request);
		}

		//..............................................................................//

		protected Task<ApiClientResult<TResponse>> PutAsync<TRequest, TResponse>(string endpoint, TRequest request)
		{
			return SendForJsonAsync<TResponse>(HttpMethod.Put, endpoint, request);
		}

		//..............................................................................//

		protected Task<ApiClientResult> PutAsync<TRequest>(string endpoint, TRequest request)
		{
			return SendNoContentAsync(HttpMethod.Put, endpoint, request);
		}

		//..............................................................................//

		protected Task<ApiClientResult<TResponse>> PatchAsync<TRequest, TResponse>(string endpoint, TRequest request)
		{
			return SendForJsonAsync<TResponse>(HttpMethod.Patch, endpoint, request);
		}

		//..............................................................................//

		protected Task<ApiClientResult> PatchAsync<TRequest>(string endpoint, TRequest request)
		{
			return SendNoContentAsync(HttpMethod.Patch, endpoint, request);
		}

		//..............................................................................//

		protected Task<ApiClientResult> DeleteAsync(string endpoint)
		{
			return SendNoContentAsync(HttpMethod.Delete, endpoint);
		}

		//..............................................................................//

		protected async Task<ApiClientResult<TResponse>> PostMultipartFileAsync<TResponse>(
			string endpoint,
			string fieldName,
			IFormFile file)
		{
			try
			{
				using var content = new MultipartFormDataContent();
				await using var stream = file.OpenReadStream();
				using var fileContent = new StreamContent(stream);
				fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
				content.Add(fileContent, fieldName, file.FileName);

				using var request = CreateRequest(HttpMethod.Post, endpoint);
				request.Content = content;
				using var response = await SendAsync(request);

				if (!response.IsSuccessStatusCode)
				{
					return await FailureAsync<TResponse>(response);
				}

				var data = await ReadAsync<TResponse>(response);
				return data == null
					? ApiClientResult<TResponse>.Failure("The API response was empty.", (int)response.StatusCode)
					: ApiClientResult<TResponse>.Success(data);
			}
			catch (InvalidOperationException ex)
			{
				return ApiClientResult<TResponse>.Failure(ex.Message);
			}
		}

		//..............................................................................//

		protected async Task<ApiClientResult<DownloadedFile>> DownloadFileAsync(string endpoint, string fallbackFileName, string fallbackContentType)
		{
			try
			{
				using var request = CreateRequest(HttpMethod.Get, endpoint);
				using var response = await SendAsync(request);

				if (!response.IsSuccessStatusCode)
				{
					return await FailureAsync<DownloadedFile>(response);
				}

				var bytes = await response.Content.ReadAsByteArrayAsync();
				var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
					?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
					?? fallbackFileName;
				var contentType = response.Content.Headers.ContentType?.MediaType ?? fallbackContentType;

				return ApiClientResult<DownloadedFile>.Success(new DownloadedFile(bytes, contentType, fileName));
			}
			catch (InvalidOperationException ex)
			{
				return ApiClientResult<DownloadedFile>.Failure(ex.Message);
			}
		}

		//..............................................................................//

		private async Task<ApiClientResult<T>> SendForJsonAsync<T>(HttpMethod method, string endpoint, object? value = null)
		{
			try
			{
				using var request = CreateRequest(method, endpoint);
				if (value != null)
				{
					request.Content = CreateJsonContent(value);
				}

				using var response = await SendAsync(request);
				if (!response.IsSuccessStatusCode)
				{
					return await FailureAsync<T>(response);
				}

				var data = await ReadAsync<T>(response);
				return data == null
					? ApiClientResult<T>.Failure("The API response was empty.", (int)response.StatusCode)
					: ApiClientResult<T>.Success(data);
			}
			catch (InvalidOperationException ex)
			{
				return ApiClientResult<T>.Failure(ex.Message);
			}
		}

		//..............................................................................//

		private async Task<ApiClientResult> SendNoContentAsync(HttpMethod method, string endpoint, object? value = null)
		{
			try
			{
				using var request = CreateRequest(method, endpoint);
				if (value != null)
				{
					request.Content = CreateJsonContent(value);
				}

				using var response = await SendAsync(request);
				return response.IsSuccessStatusCode
					? ApiClientResult.Success()
					: await FailureAsync(response);
			}
			catch (InvalidOperationException ex)
			{
				return ApiClientResult.Failure(ex.Message);
			}
		}

		//..............................................................................//

		private HttpRequestMessage CreateRequest(HttpMethod method, string endpoint)
		{
			var request = new HttpRequestMessage(method, endpoint);
			AddBearerToken(request);
			return request;
		}

		//..............................................................................//

		private StringContent CreateJsonContent(object value)
		{
			var json = JsonSerializer.Serialize(value, JsonOptions);
			return new StringContent(json, Encoding.UTF8, "application/json");
		}

		//..............................................................................//

		private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
		{
			try
			{
				return await HttpClient.SendAsync(request);
			}
			catch (HttpRequestException ex) when (ex.InnerException is SocketException)
			{
				throw new InvalidOperationException("The GLMS API is unavailable. Please make sure the API project or Docker API service is running.", ex);
			}
			catch (TaskCanceledException ex)
			{
				throw new InvalidOperationException("The GLMS API did not respond in time. Please try again.", ex);
			}
			catch (HttpRequestException ex)
			{
				throw new InvalidOperationException("The GLMS API could not be reached. Please check the API base URL configuration.", ex);
			}
		}

		//..............................................................................//

		private void AddBearerToken(HttpRequestMessage request)
		{
			var token = _httpContextAccessor.HttpContext?.User.FindFirst("ApiToken")?.Value;
			if (!string.IsNullOrWhiteSpace(token))
			{
				request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
			}
		}

		//..............................................................................//

		private static async Task<T?> ReadAsync<T>(HttpResponseMessage response)
		{
			if (response.StatusCode == HttpStatusCode.NoContent)
			{
				return default;
			}

			if (response.Content.Headers.ContentLength == 0)
			{
				return default;
			}

			await using var stream = await response.Content.ReadAsStreamAsync();
			return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
		}

		//..............................................................................//

		private static async Task<ApiClientResult> FailureAsync(HttpResponseMessage response)
		{
			var message = await ReadErrorMessageAsync(response);
			return ApiClientResult.Failure(message, (int)response.StatusCode);
		}

		//..............................................................................//

		private static async Task<ApiClientResult<T>> FailureAsync<T>(HttpResponseMessage response)
		{
			var message = await ReadErrorMessageAsync(response);
			return ApiClientResult<T>.Failure(message, (int)response.StatusCode);
		}

		//..............................................................................//

		private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
		{
			var fallback = response.StatusCode switch
			{
				HttpStatusCode.Unauthorized => "Your session has expired. Please log in again.",
				HttpStatusCode.Forbidden => "Your account does not have permission to perform this action.",
				HttpStatusCode.NotFound => "The requested record could not be found.",
				_ => $"API request failed with status code {(int)response.StatusCode}."
			};

			var content = await response.Content.ReadAsStringAsync();
			if (string.IsNullOrWhiteSpace(content))
			{
				return fallback;
			}

			try
			{
				var apiError = JsonSerializer.Deserialize<ApiErrorResponse>(content, JsonOptions);
				if (apiError?.Errors?.Count > 0)
				{
					return string.Join(" ", apiError.Errors);
				}

				var validation = JsonSerializer.Deserialize<ValidationProblemResponse>(content, JsonOptions);
				if (validation?.Errors?.Count > 0)
				{
					return string.Join(" ", validation.Errors.SelectMany(error => error.Value));
				}

				var problem = JsonSerializer.Deserialize<ProblemResponse>(content, JsonOptions);
				if (!string.IsNullOrWhiteSpace(problem?.Detail))
				{
					return problem.Detail;
				}

				if (!string.IsNullOrWhiteSpace(problem?.Title))
				{
					return problem.Title;
				}
			}
			catch
			{
				return content;
			}

			return fallback;
		}

		//..............................................................................//

		private class ApiErrorResponse
		{
			public List<string> Errors { get; set; } = new();
		}

		private class ValidationProblemResponse
		{
			public Dictionary<string, List<string>> Errors { get; set; } = new();
		}

		private class ProblemResponse
		{
			public string? Title { get; set; }
			public string? Detail { get; set; }
		}
	}
}

//.....................................o0oEND OF FILEo0o..........................................//
