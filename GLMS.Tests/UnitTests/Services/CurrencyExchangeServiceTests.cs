using System.Net;
using GLMS.Api.Services;
using Microsoft.Extensions.Configuration;

//ST10445500 - PROG7311 - GLMS POE
//CurrencyExchangeServiceTests

//.....................................o0oSTART OF FILEo0o........................................//

// The service tests check business rules without needing a browser or MVC page.

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
        public async Task ConvertToZarAsync_WithDecimalResult_RoundsAwayFromZeroToTwoDecimals()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.ConvertToZarAsync(1m, "EUR");

            // Assert
            Assert.Equal(18.56m, result);
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
                    "/v2/currencies" => """[{"iso_code":"USD","name":"US Dollar"},{"iso_code":"EUR","name":"Euro"},{"iso_code":"ZAR","name":"South African Rand"}]""",
                    "/v2/rate/USD/ZAR" => """{"rate":18.5}""",
                    "/v2/rate/EUR/ZAR" => """{"rate":18.555}""",
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

//.....................................o0oEND OF FILEo0o..........................................//
