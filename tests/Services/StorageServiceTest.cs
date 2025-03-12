using Moq;
using BtcWalletLibrary.Models;
using BtcWalletLibrary.Services;
using BtcWalletLibrary.Services.Adapters;

namespace BtcWalletLibrary.Tests.Services
{
    public class StorageServiceTests
    {
        private readonly Mock<ISecureStorageAdapter> _mockSecureStorage;
        private readonly StorageService _storageService;

        public StorageServiceTests()
        {
            _mockSecureStorage = new Mock<ISecureStorageAdapter>();
            _mockSecureStorage.Setup(m => m.ObjectStorage).Returns(new Mock<IObjectStorage>().Object);
            _mockSecureStorage.Setup(m => m.Values).Returns(new Mock<IValueStorage>().Object);

            _storageService = new StorageService(_mockSecureStorage.Object);
        }

        [Fact]
        public void GetTransactionsFromStorage_WhenTransactionsExist_ReturnsTransactions()
        {
            // Arrange
            var expectedTransactions = new List<Transaction>
            {
                new(),
                new()
            };

            var mockObjectStorage = new Mock<IObjectStorage>();
            mockObjectStorage.Setup(os => os.LoadObject(typeof(List<Transaction>), "BitcoinTransactions"))
                .Returns(expectedTransactions);
            _mockSecureStorage.SetupGet(m => m.ObjectStorage).Returns(mockObjectStorage.Object);

            // Act
            var result = _storageService.GetTransactionsFromStorage();

            // Assert
            Assert.Equal(expectedTransactions, result);
        }

        [Fact]
        public void GetTransactionsFromStorage_WhenNoTransactions_ReturnsEmptyList()
        {
            // Arrange
            var mockObjectStorage = new Mock<IObjectStorage>();
            mockObjectStorage.Setup(os => os.LoadObject(typeof(List<Transaction>), "BitcoinTransactions"))
                .Returns(null!);
            _mockSecureStorage.SetupGet(m => m.ObjectStorage).Returns(mockObjectStorage.Object);

            // Act
            var result = _storageService.GetTransactionsFromStorage();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetLastMainAddrIdxFromStorage_WhenKeyExists_ReturnsIndex()
        {
            // Arrange
            const int expectedIndex = 5;
            var mockValuesStorage = new Mock<IValueStorage>();
            mockValuesStorage.Setup(v => v.Get("lastNonEmptyMainAddrIdx", -1))
                .Returns(expectedIndex);
            _mockSecureStorage.SetupGet(m => m.Values).Returns(mockValuesStorage.Object);

            // Act
            var result = _storageService.GetLastMainAddrIdxFromStorage();

            // Assert
            Assert.Equal(expectedIndex, result);
        }

        [Fact]
        public void GetLastMainAddrIdxFromStorage_WhenKeyDoesNotExist_ReturnsDefault()
        {
            // Arrange
            var mockValuesStorage = new Mock<IValueStorage>();
            mockValuesStorage.Setup(v => v.Get("lastNonEmptyMainAddrIdx", -1))
                .Returns(-1);
            _mockSecureStorage.SetupGet(m => m.Values).Returns(mockValuesStorage.Object);

            // Act
            var result = _storageService.GetLastMainAddrIdxFromStorage();

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void GetLastChangeAddrIdxFromStorage_WhenKeyExists_ReturnsIndex()
        {
            // Arrange
            const int expectedIndex = 3;
            var mockValuesStorage = new Mock<IValueStorage>();
            mockValuesStorage.Setup(v => v.Get("lastNonEmptyChangeAddrIdx", -1))
                .Returns(expectedIndex);
            _mockSecureStorage.SetupGet(m => m.Values).Returns(mockValuesStorage.Object);

            // Act
            var result = _storageService.GetLastChangeAddrIdxFromStorage();

            // Assert
            Assert.Equal(expectedIndex, result);
        }

        [Fact]
        public void GetLastChangeAddrIdxFromStorage_WhenKeyDoesNotExist_ReturnsDefault()
        {
            // Arrange
            var mockValuesStorage = new Mock<IValueStorage>();
            mockValuesStorage.Setup(v => v.Get("lastNonEmptyChangeAddrIdx", -1))
                .Returns(-1);
            _mockSecureStorage.SetupGet(m => m.Values).Returns(mockValuesStorage.Object);

            // Act
            var result = _storageService.GetLastChangeAddrIdxFromStorage();

            // Assert
            Assert.Equal(-1, result);
        }

        [Fact]
        public void ClearStorage_DeletesObjectsAndResetsValues()
        {
            // Arrange
            var mockObjectStorage = new Mock<IObjectStorage>();
            var mockValuesStorage = new Mock<IValueStorage>();

            _mockSecureStorage.SetupGet(m => m.ObjectStorage).Returns(mockObjectStorage.Object);
            _mockSecureStorage.SetupGet(m => m.Values).Returns(mockValuesStorage.Object);

            // Act
            _storageService.ClearStorage();

            // Assert
            mockObjectStorage.Verify(os => os.DeleteObject(typeof(List<Transaction>), "BitcoinTransactions"), Times.Once);
            mockValuesStorage.Verify(v => v.Set("MyPassPhrase", It.IsAny<string>()), Times.Once);
            mockValuesStorage.Verify(v => v.Set("lastNonEmptyMainAddrIdx", It.IsAny<int>()), Times.Once);
            mockValuesStorage.Verify(v => v.Set("lastNonEmptyChangeAddrIdx", It.IsAny<int>()), Times.Once);

            // Verify default values are set
            mockValuesStorage.Verify(v => v.Set("MyPassPhrase", ""), Times.Once);
            mockValuesStorage.Verify(v => v.Set("lastNonEmptyMainAddrIdx", -1), Times.Once);
            mockValuesStorage.Verify(v => v.Set("lastNonEmptyChangeAddrIdx", -1), Times.Once);
        }

        [Fact]
        public void StoreLastMainAddrIdx_CallsSetWithCorrectValue()
        {
            // Arrange
            const uint expectedIndex = 10;
            var mockValuesStorage = new Mock<IValueStorage>();
            _mockSecureStorage.SetupGet(m => m.Values).Returns(mockValuesStorage.Object);

            // Act
            _storageService.StoreLastMainAddrIdx(expectedIndex);

            // Assert
            mockValuesStorage.Verify(v => v.Set("lastNonEmptyMainAddrIdx", expectedIndex), Times.Once);
        }

        [Fact]
        public void StoreLastChangeAddrIdx_CallsSetWithCorrectValue()
        {
            // Arrange
            const uint expectedIndex = 8;
            var mockValuesStorage = new Mock<IValueStorage>();
            _mockSecureStorage.SetupGet(m => m.Values).Returns(mockValuesStorage.Object);

            // Act
            _storageService.StoreLastChangeAddrIdx(expectedIndex);

            // Assert
            mockValuesStorage.Verify(v => v.Set("lastNonEmptyChangeAddrIdx", expectedIndex), Times.Once);
        }

        [Fact]
        public void StoreTransactions_SavesTransactionsCorrectly()
        {
            // Arrange
            var transactions = new List<Transaction>
            {
                new(),
                new()
            };
            var mockObjectStorage = new Mock<IObjectStorage>();
            _mockSecureStorage.SetupGet(m => m.ObjectStorage).Returns(mockObjectStorage.Object);

            // Act
            _storageService.StoreTransactions(transactions);

            // Assert
            mockObjectStorage.Verify(os => os.SaveObject(transactions, "BitcoinTransactions"), Times.Once);
        }
    }
}