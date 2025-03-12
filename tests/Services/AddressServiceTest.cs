using Moq;
using NBitcoin;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Services;
using BtcWalletLibrary.Models;
using System.Reflection;
using BtcWalletLibrary.Events.Arguments;
using BtcWalletLibrary.Events;

namespace BtcWalletLibrary.Tests.Services
{
    public class AddressServiceTests
    {
        #region Fields
        private const string MockMnemonic = "asthma attend bus original science leaf deputy nuclear ticket valley vacuum tornado";
        private readonly Mock<ICommonService> _mockCommonService;
        private readonly Mock<IStorageService> _mockStorageService;
        private readonly Mock<IEventDispatcher> _mockEventDispatcher;

        #endregion


        #region Constructor
        public AddressServiceTests()
        {
            _mockCommonService = new Mock<ICommonService>();
            _mockStorageService = new Mock<IStorageService>();
            _mockEventDispatcher = new Mock<IEventDispatcher>();

            // Generate keys and assign readonly fields
            var (mainExtPub, changeExtPub) = GenerateMockExtKeys();

            // Setup mock dependencies with generated keys
            _mockCommonService.SetupGet(c => c.MainAddressesParentExtPubKey).Returns(mainExtPub);
            _mockCommonService.SetupGet(c => c.ChangeAddressesParentExtPubKey).Returns(changeExtPub);

            // Setup mock Bitcoin network
            _mockCommonService.SetupGet(c => c.BitcoinNetwork).Returns(Network.TestNet);
        }

        private static (ExtPubKey mainExtPub, ExtPubKey changeExtPub) GenerateMockExtKeys()
        {
            var mnemo = new Mnemonic(MockMnemonic, Wordlist.English);
            var masterKey = mnemo.DeriveExtKey();

            const string pathMainAddr = "44'/1'/0'/0";
            var hardenedPathMainAddresses = new KeyPath(pathMainAddr);
            const string pathChangeAddr = "44'/1'/0'/1";
            var hardenedPathChangeAddresses = new KeyPath(pathChangeAddr);

            var mainParentKey = masterKey.Derive(hardenedPathMainAddresses);
            var changeParentKey = masterKey.Derive(hardenedPathChangeAddresses);
            var mainExtPub = mainParentKey.Neuter();
            var changeExtPub = changeParentKey.Neuter();

            return (mainExtPub, changeExtPub);
        }
        #endregion


        #region Unit Tests
        [Theory]
        [InlineData(0, 3, 3)]
        [InlineData(5, 7, 2)] 
        public async Task OnNewMainAddressesFound_WithValidIndices_AddsCorrectCountAsync(int initialIndex, uint newIndex, int expectedAdded)
        {
            // Arrange
            _mockStorageService.Setup(s => s.GetLastMainAddrIdxFromStorage()).Returns(initialIndex);
            var eventDispatcher = new EventDispatcher();
            var service = new AddressService(_mockCommonService.Object, _mockStorageService.Object, eventDispatcher);
            var initialCount = service.MainAddresses.Count;

            // Act
            eventDispatcher.Publish(this, new NewMainAddressesFoundEventArgs(newIndex));
            //wait for event to be processed
            await Task.Delay(1000);

            // Assert
            Assert.Equal(initialCount + expectedAdded, service.MainAddresses.Count);
        }


        [Fact]
        public void AddNewMainAddresses_WithLowerIndex_ThrowsArgumentException()
        {
            // Arrange
            // Set up storage to return existing index 5
            _mockStorageService.Setup(s => s.GetLastMainAddrIdxFromStorage()).Returns(5);

            var service = new AddressService(
                _mockCommonService.Object,
                _mockStorageService.Object,
                _mockEventDispatcher.Object);


            // Act & Assert
            var method = typeof(AddressService).GetMethod(
                "AddNewMainAddresses",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var exception = Assert.Throws<TargetInvocationException>(() =>
                method!.Invoke(service, [(uint)3]));

            // Verify correct exception type and message
            Assert.IsType<ArgumentException>(exception.InnerException);
            Assert.Equal(
                "The provided address index must be greater than the last main address index.",
                exception.InnerException.Message
            );
        }


        [Fact]
        public async Task OnTransactionBroadcasted_IncrementsChangeAddressIndexAsync()
        {
            // Arrange
            _mockStorageService.Setup(s => s.GetLastMainAddrIdxFromStorage()).Returns(0);
            _mockStorageService.Setup(s => s.GetLastChangeAddrIdxFromStorage()).Returns(0);

            // Use a real event dispatcher instead of mock
            var eventDispatcher = new EventDispatcher();

            var service = new AddressService(
                _mockCommonService.Object,
                _mockStorageService.Object,
                eventDispatcher
            );

            // Act - Publish through the real dispatcher
            eventDispatcher.Publish(this, new TransactionBroadcastedEventArgs(new Models.Transaction()));
            //wait for event to be processed
            await Task.Delay(1000);

            // Assert
            Assert.Equal(1, service.LastChangeAddrIdx);
            _mockStorageService.Verify(s => s.StoreLastChangeAddrIdx(1), Times.Once);
        }


        [Fact]
        public void Constructor_WhenNoStoredIndices_DoesNotGenerateAddresses()
        {
            // Arrange
            _mockStorageService.Setup(s => s.GetLastMainAddrIdxFromStorage()).Returns(-1);
            _mockStorageService.Setup(s => s.GetLastChangeAddrIdxFromStorage()).Returns(-1);

            // Act
            var service = new AddressService(_mockCommonService.Object, _mockStorageService.Object, _mockEventDispatcher.Object);

            // Assert
            Assert.Empty(service.MainAddresses);
            Assert.Empty(service.ChangeAddresses);
        }


        [Fact]
        public void RestoreAddressesFromStorage_WithValidIndices_GeneratesAddresses()
        {
            // Arrange
            _mockStorageService.Setup(s => s.GetLastMainAddrIdxFromStorage()).Returns(1);
            _mockStorageService.Setup(s => s.GetLastChangeAddrIdxFromStorage()).Returns(1);

            // Act
            var service = new AddressService(_mockCommonService.Object, _mockStorageService.Object, _mockEventDispatcher.Object);

            // Assert
            Assert.Equal(2, service.MainAddresses.Count); // Indices 0 and 1
            Assert.Equal(2, service.ChangeAddresses.Count);
            Assert.Equal(1, service.LastMainAddrIdx);
            Assert.Equal(1, service.LastChangeAddrIdx);
        }
        #endregion
    }
}