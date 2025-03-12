using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Models;
using BtcWalletLibrary.Services;
using BtcWalletLibrary.Utilities;
using ElectrumXClient;
using ElectrumXClient.Response;
using Moq;
using NBitcoin;
using System.Linq.Expressions;
using BtcWalletLibrary.Events.Arguments;
using static ElectrumXClient.Response.BlockchainScripthashGetHistoryResponse;
using BtcWalletLibrary.Services.Factories;
using BtcWalletLibrary.Services.Strategies;
using Transaction = NBitcoin.Transaction;

namespace BtcWalletLibrary.Tests.Services
{
    public class TxHistoryServiceTest
    {
        #region Class Fields
        private readonly Mock<ICommonService> _mockBtcCommonService;
        private readonly Mock<IElectrumxClientFactory> _mockElectrumxClientFactory;
        private readonly Mock<IStorageService> _mockBtcStorageService;
        private readonly Mock<IEventDispatcher> _mockEventDispatcher;
        private readonly Mock<IAddressService> _mockBtcAddressService;
        private readonly Mock<ITxInputDetailsService> _mockBtcTxInputDetailsService;
        private readonly Mock<ILoggingService> _mockLoggingService;
        private TxHistoryService? _txHistoryService;

        private readonly BitcoinAddress _mockTestnetAddressWithTxs;
        private readonly BitcoinAddress _mockTestnetAddressEmpty;
        private readonly Mock<IClient> _mockElectrumxClient = new();
        #endregion


        public TxHistoryServiceTest()
        {
            _mockBtcCommonService = new Mock<ICommonService>();
            _mockElectrumxClientFactory = new Mock<IElectrumxClientFactory>();
            _mockBtcStorageService = new Mock<IStorageService>();
            _mockEventDispatcher = new Mock<IEventDispatcher>();
            _mockBtcAddressService = new Mock<IAddressService>();
            _mockBtcTxInputDetailsService = new Mock<ITxInputDetailsService>();
            _mockLoggingService = new Mock<ILoggingService>();

            // since LastMainAddrIdx/ChangeAddrIdx are readonly we cant initiate them with values -1. that's why we return local variable which is then updated on Events.
            int lastChangeAddrIdx = -1, lastMainAddrIdx = -1;
            _mockBtcAddressService.SetupGet(a => a.LastMainAddrIdx)
            .Returns(() => lastMainAddrIdx);
            _mockBtcAddressService.SetupGet(a => a.LastChangeAddrIdx)
           .Returns(() => lastChangeAddrIdx);
            _mockEventDispatcher.Setup(e => e.Publish(It.IsAny<object>(), It.IsAny<NewMainAddressesFoundEventArgs>()))
            .Callback<object, NewMainAddressesFoundEventArgs>((_, args) =>
            {
                lastMainAddrIdx = (int)args.NewLastAddrIdx; // Update the index
            });
            _mockEventDispatcher.Setup(e => e.Publish(It.IsAny<object>(), It.IsAny<NewChangeAddressesFoundEventArgs>()))
            .Callback<object, NewChangeAddressesFoundEventArgs>((_, args) =>
            {
                lastChangeAddrIdx = (int)args.NewLastAddrIdx; // Update the index
            });


            _mockTestnetAddressWithTxs = BitcoinAddress.Create("tb1qqawdmracdp5jhvzm4e5s2z4pp4ty4ndq33d8pe", Network.TestNet);
            _mockTestnetAddressEmpty = BitcoinAddress.Create("tb1qm0f4nu37q8u82txpj0l0cp924836gs2q4m9rdf", Network.TestNet);
        }


        #region Unit Tests
        [Fact]
        public async Task GetTransactionsElectrumxAsync_ShouldAddTransaction_WhenNewTxFetched()
        {
            // create service with empty txs list
            _txHistoryService = CreateTxHistoryServiceWithTxsFromStorage([]);

            // Mock Methods
            GetTransactionsElectrumxAsync_CommonServiceSetup();

            MockFetchingNewTxIds();

            // Act
            await _txHistoryService.SyncTransactionsAsync();

            // Assert
            Assert.Single(_txHistoryService.Transactions);
        }


        [Fact]
        public async Task GetTransactionsElectrumxAsync_ShouldUpdateTxConfirmStatus_WhenConfirmationReceived()
        {

            // create service with unconfirmed txs
            _txHistoryService = CreateTxHistoryServiceWithTxsFromStorage(MockTransactionData.TransactionsWithoutConfirmation);

            // Mock Methods
            GetTransactionsElectrumxAsync_CommonServiceSetup();

            MockFetchingConfirmedTxIds();


            // Act
            await _txHistoryService.SyncTransactionsAsync();

            // Assert
            Assert.True(_txHistoryService.Transactions.First().Confirmed);

            // Verify
            _mockEventDispatcher.Verify(m => m.Publish(It.IsAny<object>(), It.IsAny<TransactionConfirmedEventArgs>()), Times.Once);
        }


        [Fact]
        public async Task GetTransactionsElectrumxAsync_ShouldUpdateTxDate_WhenUpdatedTimeReceived()
        {
            // create service with old dates txs
            _txHistoryService = CreateTxHistoryServiceWithTxsFromStorage(MockTransactionData.TransactionsWithOldDates);

            GetTransactionsElectrumxAsync_CommonServiceSetup();

            MockFetchingTimeUpdatedTxIds();

            // Act
            await _txHistoryService.SyncTransactionsAsync();

            // Assert
            Assert.True(_txHistoryService.Transactions.First().Date > MockTransactionData.TransactionsWithOldDates.First().Date);

            // Verify
            _mockEventDispatcher.Verify(m => m.Publish(It.IsAny<object>(), It.IsAny<TransactionDateUpdatedEventArgs>()), Times.Once);
        }


        [Fact]
        public async Task GetTransactionsElectrumxAsync_ShouldChangeLastAddrIdxs_WhenNewAddrWithTxsFetched()
        {
            // create service with txs
            _txHistoryService = CreateTxHistoryServiceWithTxsFromStorage(MockTransactionData.TransactionsWithConfirmationAndUpTodate);

            GetTransactionsElectrumxAsync_CommonServiceSetup();

            MockFetchingTxIdsAlreadyConfirmedAndUptodate();

            // Act
            await _txHistoryService.SyncTransactionsAsync();

            // Assert
            Assert.Equal(0, _mockBtcAddressService.Object.LastMainAddrIdx);
            Assert.Equal(0, _mockBtcAddressService.Object.LastChangeAddrIdx);

            // Verify
            _mockEventDispatcher.Verify(m => m.Publish(It.IsAny<object>(), It.IsAny<NewMainAddressesFoundEventArgs>()), Times.Once);
            _mockEventDispatcher.Verify(m => m.Publish(It.IsAny<object>(), It.IsAny<NewChangeAddressesFoundEventArgs>()), Times.Once);

            // max empty range is 6 and since we have 1 full address with txs and 1 empty address we should have 6+1=7.
            // since we have 2 type of addresses - main and change ones  we need to double expected call number: 7*2=14
            const int expectedCalls = 14;
            _mockElectrumxClientFactory.Verify(m => m.CreateClient(), Times.Exactly(expectedCalls));
        }
        #endregion


        #region Helper Methods
        private TxHistoryService CreateTxHistoryServiceWithTxsFromStorage(List<Models.Transaction> txs)
        {
            _mockBtcStorageService
                .Setup(x => x.GetTransactionsFromStorage())
                .Returns(txs);

            // Mock Address Derivation Strategies
            var mainStrategy = new MainAddressDerivationStrategy(_mockBtcAddressService.Object, _mockEventDispatcher.Object);
            var changeStrategy = new ChangeAddressDerivationStrategy(_mockBtcAddressService.Object, _mockEventDispatcher.Object);
            var strategies = new List<IAddressDerivationStrategy> { mainStrategy, changeStrategy };

            return new TxHistoryService(_mockBtcCommonService.Object,
                _mockElectrumxClientFactory.Object,
                _mockBtcStorageService.Object,
                _mockBtcTxInputDetailsService.Object,
                _mockEventDispatcher.Object,
                _mockLoggingService.Object,
                strategies);
        }

        private void GetTransactionsElectrumxAsync_CommonServiceSetup()
        {
            MockDeriveAddress();
            MockGetTransactionInputDetails();
            MockGetBlockchainTransactionGet();
            MockGetBitcoinNetWork();
            MockGetMaxEmptyAddrRange();
        }

        #endregion



        #region Helper Mock Methods
        private void MockGetBitcoinNetWork()
        {
            _mockBtcCommonService.Setup(service => service.BitcoinNetwork)
                .Returns(Network.TestNet);
        }

        private void MockGetMaxEmptyAddrRange()
        {
            _mockBtcCommonService.Setup(c => c.MaxEmptyAddrRange).Returns(6);
        }

        private void MockGetBlockchainTransactionGet()
        {
            var results = new[]
            {
                MockTransactionData.BlockchainTransactionGetResultWithConfrim,
                MockTransactionData.BlockchainTransactionGetResultWithNewDate,
                MockTransactionData.BlockchainTransactionGetResultConfirmedAndUpToDate,
                MockTransactionData.BlockchainTransactionGetResultNewTx
             };

            
            foreach (var result in results)
            {
                 _mockElectrumxClient.Setup(mockElectrumxClient => mockElectrumxClient.GetBlockchainTransactionGet(
                        It.Is<string>(txId => txId == result.Txid)))
                    .ReturnsAsync(new BlockchainTransactionGetResponse { Result = result });
            }

            _mockElectrumxClientFactory.Setup(factory => factory.CreateClient())
                    .Returns(_mockElectrumxClient.Object);
        }

        private void MockFetchingTxIds(List<BlockchainScripthashGetHistoryResult> blockchainScripthashGetHistoryResults)
        {
            var mockAddressWithTxsReversedScriptHash = BitcoinHelper.RevertScriptHash(_mockTestnetAddressWithTxs.ScriptPubKey.WitHash.ToString());


            // Set up behavior for the specific reversed script hash
            _mockElectrumxClient.Setup(client => client.GetBlockchainScripthashGetHistory(
                    It.Is<string>(scriptHash => scriptHash == mockAddressWithTxsReversedScriptHash)))
                .ReturnsAsync(new BlockchainScripthashGetHistoryResponse
                {
                    Result = blockchainScripthashGetHistoryResults
                });

            // Set up behavior for any other script hash
            _mockElectrumxClient.Setup(client => client.GetBlockchainScripthashGetHistory(
                    It.Is<string>(scriptHash => scriptHash != mockAddressWithTxsReversedScriptHash)))
                .ReturnsAsync(new BlockchainScripthashGetHistoryResponse
                {
                    Result = []
                });

            // Configure the factory to return the mocked client
            _mockElectrumxClientFactory.Setup(factory => factory.CreateClient())
                .Returns(_mockElectrumxClient.Object);
        }

        private void MockFetchingTxIdsAlreadyConfirmedAndUptodate()
        {
            MockFetchingTxIds(MockTransactionData.BlockchainScripthashGetHistoryResultForConfirmedAndUpTodateTxs);
        }
        private void MockFetchingNewTxIds()
        {
            MockFetchingTxIds(MockTransactionData.BlockchainScripthashGetHistoryResultForNewTx);
        }
        private void MockFetchingConfirmedTxIds()
        {
            MockFetchingTxIds(MockTransactionData.BlockchainScripthashGetHistoryResultForTxsWithoutConfirms);
        }

        private void MockFetchingTimeUpdatedTxIds()
        {
            MockFetchingTxIds(MockTransactionData.BlockchainScripthashGetHistoryResultForTxsWithOldDates);
        }

        private void MockGetTransactionInputDetails()
        {
            _mockBtcTxInputDetailsService.Setup(service => service.GetTransactionInputDetails(It.IsAny<Transaction>()))
                            .ReturnsAsync(MockTransactionData.TransactionInputs);

            _mockBtcTxInputDetailsService.Setup(service => service.GetTransactionOutputDetails(It.IsAny<NBitcoin.Transaction>()))
                .Returns(MockTransactionData.TransactionOutputs);
        }

        private void MockDeriveAddress()
        {
            void SetupAddressMethod(Expression<Func<IAddressService, BitcoinAddress>> method, BitcoinAddress bitcoinAddress)
            {
                _mockBtcAddressService.Setup(method)
                    .Returns(bitcoinAddress);
            }

            SetupAddressMethod(s => s.DeriveMainAddr(It.Is<uint>(idx => idx == 0)), _mockTestnetAddressWithTxs);
            SetupAddressMethod(s => s.DeriveMainAddr(It.Is<uint>(idx => idx != 0)), _mockTestnetAddressEmpty);

            SetupAddressMethod(s => s.DerivChangeAddr(It.Is<uint>(idx => idx == 0)), _mockTestnetAddressWithTxs);
            SetupAddressMethod(s => s.DerivChangeAddr(It.Is<uint>(idx => idx != 0)), _mockTestnetAddressEmpty);
        }
        #endregion
    }
}