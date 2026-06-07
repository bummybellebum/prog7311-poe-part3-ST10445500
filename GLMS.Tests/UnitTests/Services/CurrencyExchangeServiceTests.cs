using System.Net;
using GLMS.Api.Services;
using Microsoft.Extensions.Configuration;

namespace GLMS.Tests.UnitTests.Services
{
    public class CurrencyExchangeServiceTests
    {
        [Fact]
        public async Task ConvertToZarAsync_WithUsdAmount_ReturnsConvertedAmount()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.ConvertToZarAsync(100m, "USD");

            // Assert
            Assert.Equal(1850m, result);
        }

        [Fact]
        public async Task GetRateToZarAsync_WithZar_ReturnsOne()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.GetRateToZarAsync("zar");

            // Assert
            Assert.Equal(1m, result);
        }

        [Fact]
        public async Task ConvertToZarAsync_WithZeroAmount_ThrowsValidationError()
        {
            // Arrange
            var service = CreateService();

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.ConvertToZarAsync(0m, "USD"));

            // Assert
            Assert.StartsWith("Amount must be greater than zero.", exception.Message);
        }

        [Fact]
        public async Task GetRateToZarAsync_WithNullCurrency_ThrowsValidationError()
        {
            // Arrange
            var service = CreateService();

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GetRateToZarAsync(null!));

            // Assert
            Assert.StartsWith("Currency code is required.", exception.Message);
        }

        [Fact]
        public async Task GetRateToZarAsync_WithInvalidCurrency_ThrowsValidationError()
        {
            // Arrange
            var service = CreateService();

            // Act
            var exception = await Assert.ThrowsAsync<ArgumentException>(() => service.GetRateToZarAsync("US"));

            // Assert
            Assert.StartsWith("Currency code must be a 3-letter ISO code.", exception.Message);
        }

        private static CurrencyExchangeService CreateService()
        {
            var client = new HttpClient(new FakeExchangeHandler())
            {
                BaseAddress = new Uri("https://exchange.test")
            };

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["CurrencyApi:TargetCurrency"] = "ZAR"
                })
                .Build();

            return new CurrencyExchangeService(client, configuration);
        }

        private sealed class FakeExchangeHandler : HttpMessageHandler
        {
            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                var path = request.RequestUri?.AbsolutePath;
                var json = path switch
                {
                    "/v2/currencies" => """[{"iso_code":"USD","name":"US Dollar"},{"iso_code":"ZAR","name":"South African Rand"}]""",
                    "/v2/rate/USD/ZAR" => """{"rate":18.5}""",
                    _ => throw new InvalidOperationException($"Unexpected exchange URL: {path}")
                };

                var response = new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(json)
                };

                return Task.FromResult(response);
            }
        }
    }
}
