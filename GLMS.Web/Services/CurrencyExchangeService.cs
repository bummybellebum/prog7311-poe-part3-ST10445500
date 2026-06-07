using System.Text.Json.Serialization;

//ST10445500 - PROG7311 - GLMS POE
//CurrencyExchangeService

//.....................................o0oSTART OF FILEo0o........................................//

namespace GLMS.Web.Services
{
    public interface ICurrencyExchangeService
    {
        Task<IReadOnlyDictionary<string, string>> GetSupportedCurrenciesAsync(CancellationToken cancellationToken = default);
        Task<decimal> GetRateToZarAsync(string baseCurrencyCode, CancellationToken cancellationToken = default);
        Task<decimal> ConvertToZarAsync(decimal amount, string baseCurrencyCode, CancellationToken cancellationToken = default);
    }

    //..............................................................................//

    public class CurrencyExchangeService : ApiClientService, ICurrencyExchangeService
    {
        public CurrencyExchangeService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
            : base(httpClient, httpContextAccessor)
        {
        }

        //..............................................................................//

        public async Task<IReadOnlyDictionary<string, string>> GetSupportedCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            return await GetAsync<Dictionary<string, string>>("api/servicerequests/currencies") ?? new Dictionary<string, string>();
        }

        //..............................................................................//

        public async Task<decimal> GetRateToZarAsync(string baseCurrencyCode, CancellationToken cancellationToken = default)
        {
            var result = await GetAsync<ExchangeRateResponse>($"api/servicerequests/exchange-rate?currencyCode={Uri.EscapeDataString(baseCurrencyCode)}");
            return result?.Rate ?? 0m;
        }

        //..............................................................................//

        public async Task<decimal> ConvertToZarAsync(decimal amount, string baseCurrencyCode, CancellationToken cancellationToken = default)
        {
            var rate = await GetRateToZarAsync(baseCurrencyCode, cancellationToken);
            return decimal.Round(amount * rate, 2, MidpointRounding.AwayFromZero);
        }

        //..............................................................................//

        private class ExchangeRateResponse
        {
            [JsonPropertyName("rate")]
            public decimal Rate { get; set; }
        }
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
