using BtcWalletLibrary.Events.Arguments;
using Moq;
using BtcWalletLibrary.Models;
using BtcWalletLibrary.Services;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Events;

namespace BtcWalletLibrary.Tests.Services
{
    public class BalanceServiceTests
    {
        private readonly Mock<ITxHistoryService> _mockTxHistoryService;
        private readonly IEventDispatcher _eventDispatcher;
        private BalanceService? _balanceService;

        public BalanceServiceTests()
        {
            _eventDispatcher = new EventDispatcher();
            _mockTxHistoryService = new Mock<ITxHistoryService>();
        }

        [Fact]
        public async Task OnAddressTransactionsFetched_AddsNewUtxoAndUpdatesBalanceAsync()
        {
            // Arrange
            const string address = "testAddress";
            var tx = new Transaction
            {
                TransactionId = "tx1",
                Inputs = [new() { TrId = "someTxId", OutputIdx = 0, Address = "someAddr" }],
                Outputs = [new() { Address = address, Amount = 1.0, IsUsersAddress = true }],
                Confirmed = true
            };
            _mockTxHistoryService.SetupGet(service => service.Transactions).Returns([]);
            _balanceService = new BalanceService(_mockTxHistoryService.Object, _eventDispatcher);

            // Act
            BalanceUpdatedEventArgs? eventArgs = null;
            _eventDispatcher.Subscribe<BalanceUpdatedEventArgs>((_, args) => eventArgs = args);

            var args = new AddressTxsFetchedEventArgs(address, [tx]);
            _eventDispatcher.Publish(this, args);
            //wait for event to be processed
            await Task.Delay(1000);
            // Assert
            Assert.Single(_balanceService.UtxosPerAddress[address]);
            Assert.Equal(1.0, _balanceService.TotalConfirmedBalance);
            Assert.NotNull(eventArgs);
            Assert.Equal(1.0, eventArgs.NewConfirmedBalance);
            Assert.Equal(0.0, eventArgs.NewUnconfirmedBalance);
        }

        [Fact]
        public void OnAddressTransactionsFetched_RemovesSpentUtxoAndUpdatesBalance()
        {
            // Arrange
            var address = "userAddr";

            var unspentTxId = new Transaction
            {
                TransactionId = "unspentTxId",
                Outputs = [new() { Address = address, Amount = 1.0, IsUsersAddress = true }],
                Inputs = [new() { TrId = "someTxId0", OutputIdx = 0, Address = "someAddr0" }],
                Confirmed = true
            };

            var spentTxId = new Transaction
            {
                TransactionId = "spentTxId",
                Inputs = [new() { TrId = "someTxId1", OutputIdx = 0, Address = "someAddr1" }],
                Outputs = [new() { Address = address, Amount = 1.0, IsUsersAddress = true }],
                Confirmed = true
            };

            var spenderTxId = new Transaction
            {
                TransactionId = "spenderTxId",
                Inputs = [new() { TrId = spentTxId.TransactionId, OutputIdx = 0, Address = address }],
                Outputs = [new() { Address = "someTxId2", Amount = 1.0, IsUsersAddress = false }],
                Confirmed = true
            };


            _mockTxHistoryService.SetupGet(service => service.Transactions).Returns([unspentTxId, spenderTxId]);
            _balanceService = new BalanceService(_mockTxHistoryService.Object, _eventDispatcher);

            // Act
            var args = new AddressTxsFetchedEventArgs(address, [spentTxId]);
            _eventDispatcher.Publish(this, args);

            // Assert
            Assert.Single(_balanceService.UtxosPerAddress[address]);
            Assert.Contains(_balanceService.UtxosPerAddress[address], u => u.TransactionId == "unspentTxId");
        }

        [Fact]
        public async Task OnTransactionBroadcasted_AddsOutputsAndRemovesInputsAsync()
        {
            // Arrange
            var address = "addr1";
            var initialTx = new Transaction
            {
                TransactionId = "tx1",
                Confirmed = true,
                Outputs = [new() { Address = address, Amount = 2.0, IsUsersAddress = true }],
                Inputs = [new() { TrId = "someTxId", OutputIdx = 0, Address = "someUtxoAddr", Amount = 2.0 }],
            };
            _mockTxHistoryService.SetupGet(service => service.Transactions).Returns([initialTx]);
            _balanceService = new BalanceService(_mockTxHistoryService.Object, _eventDispatcher);

            var broadcastedTx = new Transaction
            {
                TransactionId = "txBroadcast",
                Outputs = [new() { Address = "destAddr", Amount = 2.0, IsUsersAddress = false }], // Not a user's address
                Inputs = [new() { TrId = initialTx.TransactionId, OutputIdx = 0, Address = address, Amount = 2.0 }],
                Confirmed = false,
                TransactionHex = "TxHex"
            };

            // Act
            var args = new TransactionBroadcastedEventArgs(broadcastedTx);
            _eventDispatcher.Publish(this, args);
            //wait for event to be processed
            await Task.Delay(1000);

            // Assert
            Assert.Empty(_balanceService.UtxosPerAddress[address]); // Spent UTXO removed
            Assert.Equal(2.0, _balanceService.TotalConfirmedBalance); // Initial confirmed balance
            Assert.Equal(-2.0, _balanceService.TotalUncnfirmedBalance); // Unconfirmed spend of confirmed UTXO
        }
    }
}