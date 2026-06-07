using System.Collections.Concurrent;
using System.Text.Json;
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

    public class CurrencyExchangeService : ICurrencyExchangeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        private IReadOnlyDictionary<string, string>? _cachedCurrencies;
        private DateTime _currenciesCachedAt = DateTime.MinValue;
        private readonly ConcurrentDictionary<string, (decimal Rate, DateTime CachedAt)> _rateCache = new();

        public CurrencyExchangeService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;

            _httpClient.Timeout = TimeSpan.FromSeconds(15);
        }

        //..............................................................................//

        public async Task<IReadOnlyDictionary<string, string>> GetSupportedCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            if (_cachedCurrencies != null && DateTime.UtcNow - _currenciesCachedAt < TimeSpan.FromHours(6))
            {
                return _cachedCurrencies;
            }

            using var response = await _httpClient.GetAsync("/v2/currencies", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Unable to load supported currencies from the exchange API.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var items = await JsonSerializer.DeserializeAsync<List<FrankfurterCurrencyItem>>(stream, cancellationToken: cancellationToken);

            if (items == null || items.Count == 0)
            {
                throw new InvalidOperationException("Exchange API returned an empty currency list.");
            }

            var target = (_configuration["CurrencyApi:TargetCurrency"] ?? "ZAR").Trim().ToUpperInvariant();

            var currencies = items
                .Where(c => !string.IsNullOrWhiteSpace(c.IsoCode) && !string.IsNullOrWhiteSpace(c.Name))
                .ToDictionary(c => c.IsoCode!.ToUpperInvariant(), c => c.Name!, StringComparer.OrdinalIgnoreCase);

            if (!currencies.ContainsKey(target))
            {
                throw new InvalidOperationException($"Target currency '{target}' is not supported by the exchange API.");
            }

            _cachedCurrencies = currencies;
            _currenciesCachedAt = DateTime.UtcNow;

            return currencies;
        }

        //..............................................................................//

        public async Task<decimal> GetRateToZarAsync(string baseCurrencyCode, CancellationToken cancellationToken = default)
        {
            var normalizedBase = NormalizeCurrencyCode(baseCurrencyCode);
            var target = (_configuration["CurrencyApi:TargetCurrency"] ?? "ZAR").Trim().ToUpperInvariant();

            if (normalizedBase == target)
            {
                return 1m;
            }

            if (_rateCache.TryGetValue(normalizedBase, out var cached) && DateTime.UtcNow - cached.CachedAt < TimeSpan.FromMinutes(10))
            {
                return cached.Rate;
            }

            var supported = await GetSupportedCurrenciesAsync(cancellationToken);
            if (!supported.ContainsKey(normalizedBase))
            {
                throw new InvalidOperationException($"Currency '{normalizedBase}' is not supported by the exchange API.");
            }

            using var response = await _httpClient.GetAsync($"/v2/rate/{normalizedBase}/{target}", cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException($"Unable to retrieve exchange rate from {normalizedBase} to {target}.");
            }

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var payload = await JsonSerializer.DeserializeAsync<FrankfurterRateResponse>(stream, cancellationToken: cancellationToken);

            if (payload == null || payload.Rate <= 0)
            {
                throw new InvalidOperationException("Exchange API returned an invalid exchange rate.");
            }

            _rateCache[normalizedBase] = (payload.Rate, DateTime.UtcNow);
            return payload.Rate;
        }

        //..............................................................................//

        public async Task<decimal> ConvertToZarAsync(decimal amount, string baseCurrencyCode, CancellationToken cancellationToken = default)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Amount must be greater than zero.", nameof(amount));
            }

            var rate = await GetRateToZarAsync(baseCurrencyCode, cancellationToken);
            return decimal.Round(amount * rate, 2, MidpointRounding.AwayFromZero);
        }

        //..............................................................................//

        private static string NormalizeCurrencyCode(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Currency code is required.", nameof(code));
            }

            var normalized = code.Trim().ToUpperInvariant();
            if (normalized.Length != 3)
            {
                throw new ArgumentException("Currency code must be a 3-letter ISO code.", nameof(code));
            }

            return normalized;
        }

        //..............................................................................//

        private sealed class FrankfurterCurrencyItem
        {
            [JsonPropertyName("iso_code")]
            public string? IsoCode { get; set; }

            [JsonPropertyName("name")]
            public string? Name { get; set; }
        }

        //..............................................................................//

        private sealed class FrankfurterRateResponse
        {
            [JsonPropertyName("rate")]
            public decimal Rate { get; set; }
        }

        //..............................................................................//
    }
}

//.....................................o0oEND OF FILEo0o..........................................//
