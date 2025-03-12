using System;
using System.Threading.Tasks;
using BtcWalletLibrary.Interfaces;
using ElectrumXClient;
using System.Threading;
using BtcWalletLibrary.DTOs.Responses;
using BtcWalletLibrary.Events.Arguments;
using Transaction = NBitcoin.Transaction;
using BtcWalletLibrary.Exceptions;

namespace BtcWalletLibrary.Services
{
    internal class TransferService : ITransferService
    {
        private readonly IClient _electrumxClient;
        private readonly ITxMapper _transactionMapper;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly ILoggingService _logger;
        private readonly SemaphoreSlim _broadcastLock = new(1, 1);

        public TransferService(
            IClient electrumxClient,
            ITxMapper transactionMapper,
            IEventDispatcher eventDispatcher,
            ILoggingService logger)
        {
            _electrumxClient = electrumxClient ?? throw new ArgumentNullException(nameof(electrumxClient));
            _transactionMapper = transactionMapper ?? throw new ArgumentNullException(nameof(transactionMapper));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<TransferResult> BroadcastTransactionAsync(Transaction transaction)
        {
            if (transaction == null)
            {
                throw new ArgumentNullException(nameof(transaction));
            }

            try
            {
                await _broadcastLock.WaitAsync();

                var (success, txId) = await ExecuteBroadcastAsync(transaction);
                return new TransferResult(success, txId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to broadcast transaction: {ex.Message}");
                return new TransferResult(false, null, new TransferError(ex.Message));
            }
            finally
            {
                _broadcastLock.Release();
            }
        }


        private async Task<(bool success, string txId)> ExecuteBroadcastAsync(Transaction transaction)
        {
            var txHex = transaction.ToHex();
            _logger.LogInformation($"Attempting to broadcast transaction: {txHex}");

            var broadcastResponse = await _electrumxClient.BlockchainTransactionBroadcast(txHex);

            if (broadcastResponse?.Result is null or "")
            {
                _logger.LogWarning($"Transaction broadcast failed. {txHex}");
                throw new Exception("Response string is invalid");
            }

            var txForStorage = await _transactionMapper.NBitcoinTxToBtcTxForStorage(transaction);

            try
            {
                _eventDispatcher.Publish(this, new TransactionBroadcastedEventArgs(txForStorage));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to publish transaction event");
            }
            finally
            {
                _logger.LogInformation($"Transaction broadcast successful. ID: {txForStorage.TransactionId}");
            }

            return (true, txForStorage.TransactionId);
        }
    }
}