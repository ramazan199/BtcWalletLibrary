using ElectrumXClient.Response;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcWalletLibrary.DTOs.Responses;
using BtcWalletLibrary.Events.Arguments;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Utilities;
using static ElectrumXClient.Response.BlockchainScripthashGetHistoryResponse;
using BtcWalletLibrary.Services.Factories;

namespace BtcWalletLibrary.Services
{
    internal class TxHistoryService : ITxHistoryService
    {
        private readonly ICommonService _commonService;
        private readonly IElectrumxClientFactory _electrumxClientFactory;
        private readonly IStorageService _storageService;
        private readonly ITxInputDetailsService _txInputDetailsService;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ILoggingService _logService;
        private readonly IEnumerable<IAddressDerivationStrategy> _addressDerivationStrategies;
        private readonly List<Models.Transaction> _transactions;
        public IReadOnlyList<Models.Transaction> Transactions => _transactions.AsReadOnly();
        public TxHistoryService(
            ICommonService btcCommonService,
            IElectrumxClientFactory electrumxClientFactory,
            IStorageService btcStorageService,
            ITxInputDetailsService btcTxInputDetailsService,
            IEventDispatcher eventDispatcher,
            ILoggingService logService,
            IEnumerable<IAddressDerivationStrategy> addrDerivationStrategies)
        {
            _commonService = btcCommonService ?? throw new ArgumentNullException(nameof(btcCommonService));
            _electrumxClientFactory = electrumxClientFactory;
            _storageService = btcStorageService ?? throw new ArgumentNullException(nameof(btcStorageService));
            _txInputDetailsService = btcTxInputDetailsService ??
                                     throw new ArgumentNullException(nameof(btcTxInputDetailsService));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _eventDispatcher.Subscribe<TransactionBroadcastedEventArgs>(OnTransactionBroadcasted);
            _logService = logService ?? throw new ArgumentNullException(nameof(logService));

            //_storageService.ClearStorage();
            _transactions = _storageService.GetTransactionsFromStorage();
            _addressDerivationStrategies = addrDerivationStrategies;
        }


        public async Task<TransactionFetchResult> SyncTransactionsAsync()
        {
            _eventDispatcher.Publish(this, new FetchingStartedEventArgs());

            var hasNetworkErrors = false;
            foreach (var addrDerivationStrategy in _addressDerivationStrategies)
            {
                var result = await SyncTransactionsElectrumxAsync(addrDerivationStrategy);
                hasNetworkErrors |= result.HasNetworkErrors;
            }

            _eventDispatcher.Publish(this, new FetchingCompletedEventArgs());

            _storageService.StoreTransactions(_transactions);

            return new TransactionFetchResult(hasNetworkErrors);
        }

        private async Task<TransactionFetchResult> SyncTransactionsElectrumxAsync(
            IAddressDerivationStrategy addrDerivationStrategy)
        {
            var emptyAddressCount = 0;
            var lastNonEmptyAddressIndex = -1;
            uint currentAddressIndex = 0;
            var hasNetworkErrors = false;

            while (emptyAddressCount < _commonService.MaxEmptyAddrRange)
            {
                var currentAddress = addrDerivationStrategy.DeriveAddr(currentAddressIndex);
                addrDerivationStrategy.AddAddressToQueriedAddrLst(currentAddressIndex);

                List<BlockchainScripthashGetHistoryResult> addressTxIds;
                var fetchAddressTxIdsResult = await GetAddressTransactions(currentAddress);

                if (fetchAddressTxIdsResult.HasNetworkError || fetchAddressTxIdsResult.AddressTxIds == null)
                {
                    hasNetworkErrors = true;
                    _logService.LogWarning($"TxIds could not be fetched for address {currentAddress}");
                    var existingAddrTxIds = Transactions
                        .Where(tr => tr.Inputs.Any(input => input.Address == currentAddress.ToString())
                                     || tr.Outputs.Any(output => output.Address == currentAddress.ToString()))
                        .Select(x => new BlockchainScripthashGetHistoryResult { TxHash = x.TransactionId })
                        .ToList();
                    addressTxIds = existingAddrTxIds;
                }
                else
                {
                    addressTxIds = fetchAddressTxIdsResult.AddressTxIds;
                }

                if (!addressTxIds.Any())
                {
                    emptyAddressCount++;
                }
                else
                {
                    emptyAddressCount = 0;
                    lastNonEmptyAddressIndex = (int)currentAddressIndex;
                    var fetchAddressTxDetialsResult = await GetTransactionDetailsOfAddr(addressTxIds, currentAddress);

                    if (fetchAddressTxDetialsResult.HasNetworkErrors)
                    {
                        hasNetworkErrors = true;
                        _logService.LogWarning($"Some transactions could not be fetched for address {currentAddress}");
                    }
                }

                currentAddressIndex++;
            }

            HandleAddressSearchCompletion(addrDerivationStrategy, lastNonEmptyAddressIndex);

            return new TransactionFetchResult(hasNetworkErrors);
        }

        private async Task<AddressTxIdsFetchResult> GetAddressTransactions(BitcoinAddress address)
        {
            var scriptHash = address.ScriptPubKey.WitHash.ToString();
            var reversedScriptHash = BitcoinHelper.RevertScriptHash(scriptHash);

            var result = new AddressTxIdsFetchResult();

            try
            {
                using var client = _electrumxClientFactory.CreateClient();
                var response = await client.GetBlockchainScripthashGetHistory(reversedScriptHash);

                result.AddressTxIds = response.Result;
                result.HasNetworkError = false;
            }
            catch (Exception ex)
            {
                _logService.LogError(ex, "Failed to fetch address transactions, will continue with ones from storage");

                result.AddressTxIds = null;
                result.HasNetworkError = true;
            }

            return result;
        }


        private async Task<TransactionFetchResult> GetTransactionDetailsOfAddr(
            List<BlockchainScripthashGetHistoryResult> txIds,
            BitcoinAddress bitcoinAddress)
        {
            var address = bitcoinAddress.ToString();
            var transactionsOfAddr = new List<Models.Transaction>();
            var hasNetworkErrors = false;

            var tasks = txIds.Select(async txId =>
            {
                try
                {
                    var result = await ProcessTransaction(txId.TxHash, address);
                    if (result.Transaction != null)
                    {
                        transactionsOfAddr.Add(result.Transaction);
                    }

                    if (result.HasError)
                    {
                        hasNetworkErrors = true;
                    }
                }
                catch (Exception ex)
                {
                    _logService.LogError(ex, $"Error processing transaction {txId.TxHash}");
                    hasNetworkErrors = true;
                }
            });

            await Task.WhenAll(tasks);

            _eventDispatcher.Publish(this, new AddressTxsFetchedEventArgs(address, transactionsOfAddr));

            return new TransactionFetchResult(
                hasNetworkErrors);
        }

        private class TransactionProcessResult
        {
            public Models.Transaction Transaction { get; set; }
            public bool HasError { get; set; }
        }

        private async Task<TransactionProcessResult> ProcessTransaction(string txId, string address)
        {
            var existingTransaction = _transactions.Find(tr => tr.TransactionId == txId);
            if (existingTransaction != null)
            {
                return await UpdateExistingTransaction(existingTransaction, address);
            }

            return await CreateNewTransaction(txId);
        }

        private async Task<TransactionProcessResult> UpdateExistingTransaction(
            Models.Transaction transaction,
            string address)
        {
            if (!transaction.Confirmed)
            {
                try
                {
                    using var client = _electrumxClientFactory.CreateClient();
                    var response = await client.GetBlockchainTransactionGet(transaction.TransactionId);

                    UpdateTransactionStatus(transaction, response.Result);
                    UpdateTransactionDate(transaction, response.Result);
                }
                catch (Exception ex)
                {
                    _logService.LogError(ex, "Failed to update transaction status");
                    return new TransactionProcessResult { Transaction = transaction, HasError = true };
                }
            }

            UpdateTrxInputOutputAddresses(transaction, address);

            return new TransactionProcessResult { Transaction = transaction, HasError = false };
        }

        private async Task<TransactionProcessResult> CreateNewTransaction(string txId)
        {
            try
            {
                using var client = _electrumxClientFactory.CreateClient();
                var response = await client.GetBlockchainTransactionGet(txId);

                var transaction = await BuildNewTransaction(response.Result);
                _transactions.Add(transaction);

                _eventDispatcher.Publish(this, new TransactionAddedEventArgs(transaction));

                return new TransactionProcessResult { Transaction = transaction, HasError = false };
            }
            catch (Exception ex)
            {
                _logService.LogError(ex, $"Failed to create new transaction {txId}");
                return new TransactionProcessResult { HasError = true };
            }
        }

        private async Task<Models.Transaction> BuildNewTransaction(
            BlockchainTransactionGetResponse.BlockchainTransactionGetResult transactionDetail)
        {
            var transaction = NBitcoin.Transaction.Parse(transactionDetail.Hex, _commonService.BitcoinNetwork);
            var inputs = await _txInputDetailsService.GetTransactionInputDetails(transaction);
            var outputs = _txInputDetailsService.GetTransactionOutputDetails(transaction);
            var date = DateTimeOffset.FromUnixTimeSeconds(transactionDetail.Time).DateTime;

            return new Models.Transaction
            {
                TransactionHex = transactionDetail.Hex,
                TransactionId = transactionDetail.Txid,
                Date = date,
                Inputs = inputs,
                Outputs = outputs,
                Confirmed = transactionDetail.Confirmations > 6
            };
        }

        private void UpdateTransactionStatus(
            Models.Transaction transaction,
            BlockchainTransactionGetResponse.BlockchainTransactionGetResult detail)
        {
            var confirmed = detail.Confirmations >= 6;
            if (!confirmed || transaction.Confirmed) return;
            transaction.Confirmed = true;
            _eventDispatcher.Publish(this, new TransactionConfirmedEventArgs(transaction.TransactionId));
        }

        private void UpdateTransactionDate(Models.Transaction transaction,
            BlockchainTransactionGetResponse.BlockchainTransactionGetResult detail)
        {
            if (detail.Time == 0 || transaction.Date.Year != 1970) return;
            var updateDate = DateTimeOffset.FromUnixTimeSeconds(detail.Time).DateTime;
            transaction.Date = updateDate;
            _eventDispatcher.Publish(this, new TransactionDateUpdatedEventArgs(transaction.TransactionId, updateDate));
        }

        private void UpdateTrxInputOutputAddresses(Models.Transaction transaction, string address)
        {
            var output = transaction.Outputs.FirstOrDefault(x => x.Address == address);
            if (output is { IsUsersAddress: false })
            {
                output.IsUsersAddress = true;
                _eventDispatcher.Publish(this,
                    new TxOutputAddrMarkedAsUserAddrEventArgs(transaction.TransactionId, output));
            }

            var input = transaction.Inputs.FirstOrDefault(x => x.Address == address);
            if (input == null || input.IsUsersAddress) return;
            input.IsUsersAddress = true;
            _eventDispatcher.Publish(this, new TxInputAddrMarkedAsUserAddrEventArgs(transaction.TransactionId, input));
        }

        private void HandleAddressSearchCompletion(IAddressDerivationStrategy addrDerivationStrategy,
            int lastNonEmptyAddressIndex)
        {
            if (lastNonEmptyAddressIndex == -1) return;

            if (lastNonEmptyAddressIndex > addrDerivationStrategy.LastKnownIdx)
            {
                addrDerivationStrategy.PublishNewFoundAddr(this, (uint)lastNonEmptyAddressIndex);
            }
        }

        private void OnTransactionBroadcasted(object sender, TransactionBroadcastedEventArgs e)
        {
            _transactions.Add(e.TransactionForStorage);
        }
    }
}