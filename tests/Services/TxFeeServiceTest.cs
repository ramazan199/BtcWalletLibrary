using Moq;
using Microsoft.Extensions.Options;
using NBitcoin;
using System.Net;
using BtcWalletLibrary.Configuration;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Services;
using Moq.Protected;

namespace BtcWalletLibrary.Tests.Services
{
    public class TxFeeServiceTests
    {
        private readonly Mock<IOptionsMonitor<WalletConfig>> _optionsMock;
        private readonly Mock<IAddressService> _addressServiceMock;
        private readonly Mock<ICommonService> _commonServiceMock;
        private readonly Mock<ILoggingService> _loggingServiceMock;
        private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;

        public TxFeeServiceTests()
        {
            _optionsMock = new Mock<IOptionsMonitor<WalletConfig>>();
            _addressServiceMock = new Mock<IAddressService>();
            _commonServiceMock = new Mock<ICommonService>();
            _loggingServiceMock = new Mock<ILoggingService>();
            _httpClientFactoryMock = new Mock<IHttpClientFactory>();
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            var config = new WalletConfig
            {
                BlockchainTxFeeApi = new BlockchainTxFeeApiConfigSection
                {
                    FeePath = "https://api.example.com/fees"
                }
            };

            _optionsMock.Setup(x => x.CurrentValue).Returns(config);
        }

        private TxFeeService CreateService(HttpClient httpClient)
        {
            _httpClientFactoryMock.Setup(x => x.CreateClient("TxFeeApi")).Returns(httpClient);
            return new TxFeeService(
                _optionsMock.Object,
                _addressServiceMock.Object,
                _commonServiceMock.Object,
                _loggingServiceMock.Object,
                _httpClientFactoryMock.Object
            );
        }

        [Fact]
        public async Task GetRecommendedBitFeeAsync_SuccessfulResponse_ReturnsFee()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{ ""priority"": 50 }")
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            var service = CreateService(httpClient);

            // Act
            var result = await service.GetRecommendedBitFeeAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.False(result.IsDefault);
            Assert.Equal(50, result.Fee.Satoshi);
            Assert.Null(result.OperationError);
        }

        [Fact]
        public async Task GetRecommendedBitFeeAsync_NetworkError_ReturnsDefaultFee()
        {
            // Arrange
            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            var service = CreateService(httpClient);

            // Act
            var result = await service.GetRecommendedBitFeeAsync();

            // Assert
            Assert.True(result.IsDefault);
            Assert.Contains("Network error", result.OperationError.Message);
            Assert.Equal(100, result.Fee.Satoshi);
            _loggingServiceMock.Verify(x =>
                    x.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("Network failure"))),
                Times.Once);
        }

        [Fact]
        public async Task GetRecommendedBitFeeAsync_InvalidJson_ReturnsDefaultFee()
        {
            // Arrange
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("invalid json")
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            var service = CreateService(httpClient);

            // Act
            var result = await service.GetRecommendedBitFeeAsync();

            // Assert
            Assert.True(result.IsDefault);
            Assert.Contains("Unexpected character", result.OperationError.Message);
            Assert.Equal(100, result.Fee.Satoshi);
            _loggingServiceMock.Verify(x =>
                    x.LogError(It.IsAny<Exception>(), It.Is<string>(s => s.Contains("API response parsing failed"))),
                Times.Once);
        }

        [Fact]
        public async Task CalculateTransactionFee_WithSingleCoin_ReturnsCorrectFee()
        {
            // Arrange
            var network = Network.TestNet;
            _commonServiceMock.Setup(c => c.BitcoinNetwork).Returns(network);

            var mockChangeAddr = new Key().PubKey.GetAddress(ScriptPubKeyType.Legacy, network);
            _addressServiceMock.Setup(a => a.DeriveNewChangeAddr()).Returns(mockChangeAddr);

            var httpClient = new HttpClient(_httpMessageHandlerMock.Object);
            var service = CreateService(httpClient);

            // Set the recommended fee through a test helper method
            var response = new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(@"{ ""priority"": 50 }")
            };

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(response);

            // Set the recommended fee first
            await service.GetRecommendedBitFeeAsync();

            var key = new Key();
            var destinationAddress = new Key().PubKey.GetAddress(ScriptPubKeyType.Legacy, network);
            var scriptPubKey = destinationAddress.ScriptPubKey;
            var coin = new Coin(fromTxHash: new uint256(1), fromOutputIndex: 0, amount: Money.Coins(1m),
                scriptPubKey: scriptPubKey);
            var selectedUnspentCoins = new List<Coin> { coin };
            var signingKeys = new List<Key> { key };

            // Build transaction to calculate expected fee
            var builder = network.CreateTransactionBuilder();
            var tx = builder
                .AddCoins(selectedUnspentCoins)
                .AddKeys(signingKeys.ToArray())
                .Send(destinationAddress, Money.Coins(0.5m))
                .SetChange(mockChangeAddr)
                .SendFees(0L)
                .BuildTransaction(true);

            var virtualSize = tx.GetVirtualSize();

            // Act
            var fee = service.CalculateTransactionFee(selectedUnspentCoins, signingKeys, destinationAddress,
                Money.Coins(0.5m));


            var expectedFee = new Money(service.BitFeeRecommendedFastest * virtualSize, MoneyUnit.Satoshi);

            // Assert
            Assert.Equal(expectedFee, fee);
        }
    }
}