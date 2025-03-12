using System.Collections.Generic;
using System.Linq;
using BtcWalletLibrary.Events.Arguments;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Models;

namespace BtcWalletLibrary.Services
{
    /// <summary>
    /// Service to get account balance and unspent transactions based on fetched transactions.
    /// </summary>
    internal class BalanceService : IBalanceService
    {
        private readonly ITxHistoryService _btcTxHistoryService;
        private readonly IEventDispatcher _eventDispatcher;
        private readonly Dictionary<string, List<UtxoDetailsElectrumx>> _utxosPerAddress = new();

        /// <summary>
        /// Dictionary to store UTXOs per address. Key is address and Value is list of UtxoDetailsElectrumx
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyList<UtxoDetailsElectrumx>> UtxosPerAddress =>
            _utxosPerAddress.ToDictionary(kv => kv.Key, kv => (IReadOnlyList<UtxoDetailsElectrumx>)kv.Value.AsReadOnly());


        /// <summary>
        /// List of all UTXOs across all addresses
        /// </summary>
        public IEnumerable<UtxoDetailsElectrumx> Utxos => _utxosPerAddress.Values.SelectMany(utxoList => utxoList);
        /// <summary>
        /// Total confirmed balance of all addresses
        /// </summary>
        public double TotalConfirmedBalance { get; private set; }

        /// <summary>
        /// Total unconfirmed balance of all addresses
        /// </summary>
        public double TotalUncnfirmedBalance { get; private set; }


        /// <summary>
        /// Constructor for BalanceService. Restores UTXOs and balance from storage and subscribes to events.
        /// </summary>
        /// <param name="btcTxHistoryService">Service to get transaction history</param>
        /// <param name="eventDispatcher">Event dispatcher to publish and subscribe to events</param>
        public BalanceService(ITxHistoryService btcTxHistoryService, IEventDispatcher eventDispatcher)
        {
            _btcTxHistoryService = btcTxHistoryService;
            _eventDispatcher = eventDispatcher;

            RestoreUTXOsAndBalanceFromStorage();

            _eventDispatcher.Subscribe<AddressTxsFetchedEventArgs>(OnAddressTransactionsFetched);
            _eventDispatcher.Subscribe<TransactionBroadcastedEventArgs>(OnTransactionBroadcasted);
        }

        // Based on fetched address transactions updates/calculates(if storage was empty) unspent tranacion outputs, confirmed (transactions with comfied status) and total
        // (transactions with confirmed + unconfirmed status) balance for user's each adress. In addtion total  and confirmed balance of all addresses are calculated.
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments containing address and transactions</param>
        private void OnAddressTransactionsFetched(object sender, AddressTxsFetchedEventArgs e)
        {
            var transactions = e.Transactions;
            var address = e.Address;

            //First collected all transactions of given address and only then start checking if this tr is spent or not.
            //Otherwise, later u can receive tr which spends tr u already indicated as unspent. (or u need update utxo list each time u receive tr which spends your utxo)
            var confirmedBalance = .0;
            var unConfirmedBalance = .0;
            var receivedTransactions = transactions.Where(tr => tr.Outputs.Any(output => output.Address == address))
                .ToList();
            foreach (var transaction in receivedTransactions)
            {
                var outputIdx = transaction.Outputs
                    .Select((output, index) => new { output, index })
                    .FirstOrDefault(x => x.output.Address == address)?.index ?? -1;

                if (_btcTxHistoryService.Transactions.Any(tr => tr.Inputs.Any(input =>
                    input.TrId == transaction.TransactionId && input.OutputIdx == outputIdx)))
                {
                    if (_utxosPerAddress.TryGetValue(address, out var utxos))
                    {
                        if (utxos.RemoveAll(utxo => utxo.TransactionId == transaction.TransactionId) > 0)
                        {
                            if (transaction.Confirmed) confirmedBalance -= transaction.Outputs[outputIdx].Amount;
                            else unConfirmedBalance -= transaction.Outputs[outputIdx].Amount;
                        }
                    }

                    continue;
                }

                if (_utxosPerAddress.TryGetValue(address, out var addrUtxos) &&
                    addrUtxos.Exists(utxo => utxo.TransactionId == transaction.TransactionId)) continue;

                var utxoDetail = new UtxoDetailsElectrumx()
                {
                    Address = address,
                    Confirmed = transaction.Confirmed,
                    TransactionHex = transaction.TransactionHex,
                    TransactionId = transaction.TransactionId,
                    TransactionPos = outputIdx
                };
                if (_utxosPerAddress.TryGetValue(address, out var val)) val.Add(utxoDetail);
                else _utxosPerAddress[address] = new List<UtxoDetailsElectrumx>() { utxoDetail };

                if (transaction.Confirmed) confirmedBalance += transaction.Outputs[outputIdx].Amount;
                else unConfirmedBalance += transaction.Outputs[outputIdx].Amount;
            }


            TotalConfirmedBalance += confirmedBalance;
            TotalUncnfirmedBalance += unConfirmedBalance;

            _eventDispatcher.Publish(this, new BalanceUpdatedEventArgs(TotalConfirmedBalance, TotalUncnfirmedBalance));
        }

        // Restores unspent transaction outputs and balance using transaction list which was restored from secure storage. As a result  confined and total(confirmed + unconfined) balance of user's each address are obtained.
        // In addition, total and confirmed balance of all addresses are calculated.
        private void RestoreUTXOsAndBalanceFromStorage()
        {
            var confirmedBalance = .0;
            var unConfirmedBalance = .0;
            foreach (var transaction in _btcTxHistoryService.Transactions)
            {
                var outputIdx = -1;
                foreach (var output in transaction.Outputs)
                {
                    outputIdx++;
                    string address;
                    if (output.IsUsersAddress) address = output.Address;
                    else continue;
                    if (_btcTxHistoryService.Transactions.Any(tr =>
                            tr.Inputs.Any(input =>
                                input.TrId == transaction.TransactionId && input.OutputIdx == outputIdx)))
                    {
                        if (_utxosPerAddress.TryGetValue(address, out var utxos))
                        {
                            if (utxos.RemoveAll(utxo => utxo.TransactionId == transaction.TransactionId) > 0)
                            {
                                if (transaction.Confirmed) confirmedBalance -= transaction.Outputs[outputIdx].Amount;
                                else unConfirmedBalance -= transaction.Outputs[outputIdx].Amount;
                            }
                        }

                        continue;
                    }

                    if (_utxosPerAddress.TryGetValue(address, out var value) &&
                        value.Any(utxo => utxo.TransactionId == transaction.TransactionId)) continue;

                    var utxoDetail = new UtxoDetailsElectrumx()
                    {
                        Address = address,
                        Confirmed = transaction.Confirmed,
                        TransactionHex = transaction.TransactionHex,
                        TransactionId = transaction.TransactionId,
                        TransactionPos = outputIdx
                    };
                    if (_utxosPerAddress.TryGetValue(address, out var utxo)) utxo.Add(utxoDetail);
                    else _utxosPerAddress[address] = new List<UtxoDetailsElectrumx>() { utxoDetail };

                    if (transaction.Confirmed) confirmedBalance += transaction.Outputs[outputIdx].Amount;
                    else unConfirmedBalance += transaction.Outputs[outputIdx].Amount;
                }
            }

            TotalConfirmedBalance += confirmedBalance;
            TotalUncnfirmedBalance += unConfirmedBalance;
        }

        // Updates unspent transaction outputs list and balance after transaction is broadcasted.
        /// <param name="sender">Event sender</param>
        /// <param name="e">Event arguments containing transaction details</param>
        private void OnTransactionBroadcasted(object sender, TransactionBroadcastedEventArgs e)
        {
            var transaction = e.TransactionForStorage;
            //adding new utxo
            var outputIdx = 0;
            foreach (var txOut in transaction.Outputs)
            {
                if (txOut.IsUsersAddress)
                {
                    var outputAddr = txOut.Address;
                    UtxoDetailsElectrumx utxoDetail = new()
                    {
                        Address = outputAddr,
                        Confirmed = false,
                        TransactionHex = transaction.TransactionHex,
                        TransactionId = transaction.TransactionId,
                        TransactionPos = outputIdx,
                    };
                    if (_utxosPerAddress.TryGetValue(outputAddr, out var value)) value.Add(utxoDetail);
                    else _utxosPerAddress[outputAddr] = new List<UtxoDetailsElectrumx>() { utxoDetail };

                    if (transaction.Confirmed) TotalConfirmedBalance += txOut.Amount;
                    else TotalUncnfirmedBalance += txOut.Amount;
                }

                outputIdx++;
            }

            //removing spent ones
            foreach (var txInput in transaction.Inputs)
            {
                _utxosPerAddress[txInput.Address].RemoveAll(utxo => utxo.TransactionId == txInput.TrId);
                if (transaction.Confirmed) TotalConfirmedBalance -= txInput.Amount;
                else TotalUncnfirmedBalance -= txInput.Amount;
            }
        }
    }
}