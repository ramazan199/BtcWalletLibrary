using BtcWalletLibrary.Events.Arguments;
using Moq;
using BtcWalletLibrary.Models;
using BtcWalletLibrary.Interfaces;
using NBitcoin;
using ElectrumXClient;
using BtcWalletLibrary.Services;
using Transaction = NBitcoin.Transaction;
using BtcWalletLibrary.Exceptions;

namespace BtcWalletLibrary.Tests.Services
{
    public class TransferServiceTests
    {
        private readonly Mock<IClient> _electrumMock;
        private readonly Mock<ITxMapper> _txMapperMock;
        private readonly Mock<IEventDispatcher> _eventDispatcherMock;
        private readonly Mock<ILoggingService> _loggerMock;
        private readonly TransferService _service;

        // Common test data
        private readonly string? _defaultTxId;
        private readonly Transaction _defaultTransaction;
        private readonly Models.Transaction? _defaultStorageTransaction;

        public TransferServiceTests()
        {
            // Initialize mocks
            _electrumMock = new Mock<IClient>();
            _txMapperMock = new Mock<ITxMapper>();
            _eventDispatcherMock = new Mock<IEventDispatcher>();
            _loggerMock = new Mock<ILoggingService>();

            // Initialize service
            _service = new TransferService(
                _electrumMock.Object,
                _txMapperMock.Object,
                _eventDispatcherMock.Object,
                _loggerMock.Object);

            // Initialize common test data
            BitcoinAddress.Create("1A1zP1eP5QGefi2DMPTfTL5SLmv7DivfNa", Network.Main);
            _defaultTxId = "txid123";
            _defaultTransaction = Transaction.Create(Network.Main);
            _defaultStorageTransaction = new Models.Transaction { TransactionId = _defaultTxId };
        }

        private void SetupSuccessfulBroadcastMocks(string? txId = null, Models.Transaction? storageTransaction = null)
        {
            txId ??= _defaultTxId;
            storageTransaction ??= _defaultStorageTransaction;

            _electrumMock.Setup(e => e.BlockchainTransactionBroadcast(It.IsAny<string>()))
                .ReturnsAsync(new ElectrumXClient.Response.BlockchainTransactionBroadcastResponse { Result = txId });
            _txMapperMock.Setup(m => m.NBitcoinTxToBtcTxForStorage(It.IsAny<Transaction>()))
                .ReturnsAsync(storageTransaction);
        }

        private void VerifySuccessfulBroadcast(Transaction transaction, Times times)
        {
            _electrumMock.Verify(e => e.BlockchainTransactionBroadcast(transaction.ToHex()), times);
            _txMapperMock.Verify(m => m.NBitcoinTxToBtcTxForStorage(transaction), times);
            _eventDispatcherMock.Verify(e => e.Publish(_service, It.IsAny<TransactionBroadcastedEventArgs>()), times);
        }

        [Fact]
        public async Task BroadcastTransactionAsync_Success_ReturnsSuccessResult()
        {
            // Arrange
            SetupSuccessfulBroadcastMocks();

            // Act
            var result = await _service.BroadcastTransactionAsync(_defaultTransaction);

            // Assert
            Assert.True(result.Success);
            Assert.Equal(_defaultTxId, result.TransactionId);
            VerifySuccessfulBroadcast(_defaultTransaction, Times.Once());
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task BroadcastTransactionAsync_NullTransaction_ThrowsArgumentNullException()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() => _service.BroadcastTransactionAsync(null));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public async Task BroadcastTransactionAsync_InvalidElectrumResponse_ReturnsFailure(string response)
        {
            // Arrange
            _electrumMock.Setup(e => e.BlockchainTransactionBroadcast(It.IsAny<string>()))
                .ReturnsAsync(new ElectrumXClient.Response.BlockchainTransactionBroadcastResponse { Result = response });

            // Act
            var result = await _service.BroadcastTransactionAsync(_defaultTransaction);

            // Assert
            Assert.False(result.Success);
            Assert.Null(result.TransactionId);
            Assert.Contains("Response string is invalid", result.OperationError.Message);
            _loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task BroadcastTransactionAsync_ElectrumThrowsException_ReturnsFailureWithErrorMessage()
        {
            // Arrange
            var errorMessage = "Electrum error";
            _electrumMock.Setup(e => e.BlockchainTransactionBroadcast(It.IsAny<string>()))
                .ThrowsAsync(new Exception(errorMessage));

            // Act
            var result = await _service.BroadcastTransactionAsync(_defaultTransaction);

            // Assert
            Assert.False(result.Success);
            Assert.Contains(errorMessage, result.OperationError.Message);
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), It.IsAny<string>()), Times.Once);
        }

        

        [Fact]
        public async Task BroadcastTransactionAsync_EventDispatchFails_StillReturnsSuccess()
        {
            // Arrange
            SetupSuccessfulBroadcastMocks();
            _eventDispatcherMock.Setup(e => e.Publish(It.IsAny<object>(), It.IsAny<EventArgs>()))
                .Throws(new Exception("Event failed"));

            // Act
            var result = await _service.BroadcastTransactionAsync(_defaultTransaction);

            // Assert
            Assert.True(result.Success);
            _loggerMock.Verify(l => l.LogError(It.IsAny<Exception>(), "Failed to publish transaction event"), Times.Once);
        }

        [Fact]
        public async Task BroadcastTransactionAsync_ConcurrencyHandling()
        {
            // Arrange
            SetupSuccessfulBroadcastMocks();
            _electrumMock.Setup(e => e.BlockchainTransactionBroadcast(It.IsAny<string>()))
                .ReturnsAsync(new ElectrumXClient.Response.BlockchainTransactionBroadcastResponse { Result = _defaultTxId })
                .Callback(() => Thread.Sleep(100)); // Simulate delay

            // Act
            var tasks = new[]
            {
                _service.BroadcastTransactionAsync(_defaultTransaction),
                _service.BroadcastTransactionAsync(_defaultTransaction),
                _service.BroadcastTransactionAsync(_defaultTransaction)
            };

            var results = await Task.WhenAll(tasks);

            // Assert
            Assert.All(results, result => Assert.True(result.Success));
            _electrumMock.Verify(e => e.BlockchainTransactionBroadcast(It.IsAny<string>()), Times.Exactly(3));
        }

        [Fact]
        public async Task BroadcastTransactionAsync_SemaphoreReleasedOnException()
        {
            // Arrange
            _electrumMock.SetupSequence(e => e.BlockchainTransactionBroadcast(It.IsAny<string>()))
                .ThrowsAsync(new Exception("Error"))
                .ReturnsAsync(new ElectrumXClient.Response.BlockchainTransactionBroadcastResponse { Result = _defaultTxId });

            SetupSuccessfulBroadcastMocks();

            // Act & Assert (No deadlock occurs)
            await _service.BroadcastTransactionAsync(_defaultTransaction);
            var result = await _service.BroadcastTransactionAsync(_defaultTransaction);

            Assert.True(result.Success);
            _electrumMock.Verify(e => e.BlockchainTransactionBroadcast(It.IsAny<string>()), Times.Exactly(2));
        }
    }
}