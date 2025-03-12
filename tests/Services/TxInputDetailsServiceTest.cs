using Moq;
using NBitcoin;
using ElectrumXClient.Response;
using BtcWalletLibrary.Services;
using BtcWalletLibrary.Models;
using BtcWalletLibrary.Interfaces;
using Transaction = NBitcoin.Transaction;
using ElectrumXClient;
using BtcWalletLibrary.Services.Factories;

namespace BtcWalletLibrary.Tests.Services
{
    public class TxInputDetailsServiceTests
    {
        private const string TestAddress = "mkHS9ne12qx9pS9VojpwU5xtRd4T7X7ZUt";
        private const double TestAmount = 0.1;
        private static readonly uint256 MockTxHash = uint256.Parse("1234567890abcdef1234567890abcdef1234567890abcdef1234567890abcdef");

        private readonly Mock<IElectrumxClientFactory> _mockElectrumClientFactory;
        private readonly TxInputDetailsService _txInputDetailsService;


        public TxInputDetailsServiceTests()
        {
            Mock<ICommonService> mockCommonService = new();
            Mock<IAddressService> mockAddressService = new();
            //var mockIoptions = new Mock<IOptionsMonitor<WalletConfig>>();
            _mockElectrumClientFactory = new Mock<IElectrumxClientFactory>();

            mockCommonService.Setup(x => x.BitcoinNetwork)
                .Returns(Network.TestNet);

            _txInputDetailsService = new TxInputDetailsService(mockCommonService.Object, _mockElectrumClientFactory.Object, mockAddressService.Object);
        }

        [Fact]
        public async Task GetTransactionInputDetails_ShouldReturnInputDetails()
        {
            // Arrange
            var (transaction, prevOutHash) = CreateTransactionWithInput;
            var mockResponse = CreateMockTransactionResponse();

            SetupTransactionGetMock(prevOutHash, mockResponse);

            // Act
            var result = await _txInputDetailsService.GetTransactionInputDetails(transaction);

            // Assert
            AssertExpectedInputDetails(result);
        }

        [Fact]
        public void GetTransactionOutputDetails_ShouldReturnOutputDetails()
        {
            // Arrange
            var transaction = CreateTransactionWithOutput();

            // Act
            var result = _txInputDetailsService.GetTransactionOutputDetails(transaction);

            // Assert
            AssertExpectedOutputDetails(result);
        }

        private (Transaction transaction, string prevOutHash) CreateTransactionWithInput
        {
            get
            {
                var transaction = Transaction.Create(Network.TestNet);
                var prevOut = new OutPoint(MockTxHash, 0);
                transaction.Inputs.Add(new TxIn(prevOut));
                return (transaction, prevOut.Hash.ToString());
            }
        }

        private BlockchainTransactionGetResponse CreateMockTransactionResponse()
        {
            return new BlockchainTransactionGetResponse
            {
                Result = new BlockchainTransactionGetResponse.BlockchainTransactionGetResult
                {
                    VoutValue =
                    [
                        new BlockchainTransactionGetResponse.BlockchainTransactionGetResult.Vout
                        {
                            N = 0,
                            Value = TestAmount,
                            ScriptPubKey = new BlockchainTransactionGetResponse.BlockchainTransactionGetResult.ScriptPubKey
                            {
                                Addresses = [TestAddress]
                            }
                        }
                    ]
                }
            };
        }

        private void SetupTransactionGetMock(string expectedHash, BlockchainTransactionGetResponse response)
        {
            var mocElectrumxClient = new Mock<IClient>();
            _mockElectrumClientFactory.Setup(x => x.CreateClient())
                .Returns(mocElectrumxClient.Object);

            mocElectrumxClient.Setup(client => client.GetBlockchainTransactionGet(expectedHash))
                .ReturnsAsync(response)
                .Verifiable();
        }

        private static void AssertExpectedInputDetails(List<TransactionInput> result)
        {
            Assert.NotNull(result);
            var input = Assert.Single(result);
            Assert.Equal(TestAddress, input.Address);
            Assert.Equal(TestAmount, input.Amount);
            Assert.Equal(0, input.OutputIdx);
            Assert.Equal(MockTxHash.ToString(), input.TrId);
        }

        private static Transaction CreateTransactionWithOutput()
        {
            var transaction = Transaction.Create(Network.TestNet);
            var scriptPubKey = BitcoinAddress.Create(TestAddress, Network.TestNet).ScriptPubKey;
            transaction.Outputs.Add(new TxOut(Money.Coins((decimal)TestAmount), scriptPubKey));
            return transaction;
        }

        private static void AssertExpectedOutputDetails(List<TransactionOutput> result)
        {
            Assert.NotNull(result);
            var output = Assert.Single(result);
            Assert.Equal(TestAddress, output.Address);
            Assert.Equal(TestAmount, output.Amount);
        }
    }
}