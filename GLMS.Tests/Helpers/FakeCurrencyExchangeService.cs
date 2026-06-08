using GLMS.Api.Services;

namespace GLMS.Tests.Helpers
{
    public class FakeCurrencyExchangeService : ICurrencyExchangeService
    {
        public Task<IReadOnlyDictionary<string, string>> GetSupportedCurrenciesAsync(CancellationToken cancellationToken = default)
        {
            IReadOnlyDictionary<string, string> currencies = new Dictionary<string, string>
            {
                ["USD"] = "US Dollar",
                ["ZAR"] = "South African Rand"
            };

            return Task.FromResult(currencies);
        }

        public Task<decimal> GetRateToZarAsync(string baseCurrencyCode, CancellationToken cancellationToken = default)
        {
            var rate = string.Equals(baseCurrencyCode, "ZAR", StringComparison.OrdinalIgnoreCase)
                ? 1m
                : 18.5m;

            return Task.FromResult(rate);
        }

        public async Task<decimal> ConvertToZarAsync(decimal amount, string baseCurrencyCode, CancellationToken cancellationToken = default)
        {
            var rate = await GetRateToZarAsync(baseCurrencyCode, cancellationToken);
            return decimal.Round(amount * rate, 2, MidpointRounding.AwayFromZero);
        }
    }
}
