using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

//ST10445500 - PROG7311 - GLMS POE
//ApiClientService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    public abstract class ApiClientService
    {
        protected readonly HttpClient HttpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        protected static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = ReferenceHandler.IgnoreCycles
        };

        protected ApiClientService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            HttpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }

        //..............................................................................//

        protected async Task<T?> GetAsync<T>(string url)
        {
            using var request = CreateRequest(HttpMethod.Get, url);
            using var response = await SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return default;
            }

            await EnsureSuccessAsync(response);
            return await ReadAsync<T>(response);
        }

        //..............................................................................//

        protected async Task<T> PostAsync<T>(string url, object value)
        {
            using var request = CreateJsonRequest(HttpMethod.Post, url, value);
            using var response = await SendAsync(request);
            await EnsureSuccessAsync(response);
            return (await ReadAsync<T>(response))!;
        }

        //..............................................................................//

        protected async Task PostNoResultAsync(string url, object value)
        {
            using var request = CreateJsonRequest(HttpMethod.Post, url, value);
            using var response = await SendAsync(request);
            await EnsureSuccessAsync(response);
        }

        //..............................................................................//

        protected async Task PutAsync(string url, object value)
        {
            using var request = CreateJsonRequest(HttpMethod.Put, url, value);
            using var response = await SendAsync(request);
            await EnsureSuccessAsync(response);
        }

        //..............................................................................//

        protected async Task PatchAsync(string url, object value)
        {
            using var request = CreateJsonRequest(HttpMethod.Patch, url, value);
            using var response = await SendAsync(request);
            await EnsureSuccessAsync(response);
        }

        //..............................................................................//

        protected async Task DeleteAsync(string url)
        {
            using var request = CreateRequest(HttpMethod.Delete, url);
            using var response = await SendAsync(request);
            await EnsureSuccessAsync(response);
        }

        //..............................................................................//

        protected HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            var request = new HttpRequestMessage(method, url);
            AddBearerToken(request);
            return request;
        }

        //..............................................................................//

        protected async Task EnsureSuccessAsync(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            var message = await ReadErrorMessageAsync(response);
            throw new ApiClientException((int)response.StatusCode, message);
        }

        //..............................................................................//

        protected static async Task<T?> ReadAsync<T>(HttpResponseMessage response)
        {
            if (response.Content.Headers.ContentLength == 0)
            {
                return default;
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream, JsonOptions);
        }

        //..............................................................................//

        private HttpRequestMessage CreateJsonRequest(HttpMethod method, string url, object value)
        {
            var request = CreateRequest(method, url);
            var json = JsonSerializer.Serialize(value, JsonOptions);
            request.Content = new StringContent(json, Encoding.UTF8, "application/json");
            return request;
        }

        //..............................................................................//

        protected async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            try
            {
                return await HttpClient.SendAsync(request);
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException)
            {
                throw new ApiUnavailableException("The GLMS API is unavailable. Please make sure the API project or Docker API service is running.", ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new ApiUnavailableException("The GLMS API did not respond in time. Please try again.", ex);
            }
            catch (HttpRequestException ex)
            {
                throw new ApiUnavailableException("The GLMS API could not be reached. Please check the API base URL configuration.", ex);
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

        private static async Task<string> ReadErrorMessageAsync(HttpResponseMessage response)
        {
            var fallback = response.StatusCode switch
            {
                System.Net.HttpStatusCode.Unauthorized => "Your session has expired. Please log in again.",
                System.Net.HttpStatusCode.Forbidden => "Your account does not have permission to perform this action.",
                System.Net.HttpStatusCode.NotFound => "The requested record could not be found.",
                _ => $"API request failed with status code {(int)response.StatusCode}."
            };
            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return fallback;
            }

            try
            {
                var error = JsonSerializer.Deserialize<ApiErrorResponse>(content, JsonOptions);
                if (error?.Errors?.Count > 0)
                {
                    return string.Join(" ", error.Errors);
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
    }

    public class ApiClientException : InvalidOperationException
    {
        public ApiClientException(int statusCode, string message)
            : base(message)
        {
            StatusCode = statusCode;
        }

        public int StatusCode { get; }
        public bool IsUnauthorized => StatusCode == StatusCodes.Status401Unauthorized;
        public bool IsForbidden => StatusCode == StatusCodes.Status403Forbidden;
        public bool IsNotFound => StatusCode == StatusCodes.Status404NotFound;
    }

    public class ApiUnavailableException : InvalidOperationException
    {
        public ApiUnavailableException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
