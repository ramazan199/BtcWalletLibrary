using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Coin = NBitcoin.Coin;
using Transaction = NBitcoin.Transaction;

namespace BtcWalletLibrary.Services.Mappers
{
    // Maps different coin representations to NBitcoin.Coin
    internal class CoinMapper : ICoinMapper
    {
        private readonly IBalanceService _btcBalanceService;
        private readonly ICommonService _commonService;

        public CoinMapper(IBalanceService btcBalanceService, ICommonService commonService)
        {
            _btcBalanceService = btcBalanceService;
            _commonService = commonService;
        }

        public List<Coin> UnspentCoinsToNbitcoinCoin(List<UnspentCoin> selectedUnspentCoins)
        {
            // Precompute a lookup table for UTXOs
            var utxoLookup = _btcBalanceService.Utxos
                .ToLookup(utxo => (utxo.TransactionId.ToString(), utxo.Address));

            var nBitcoinSelectedUnspentCoins = new List<Coin>();
            foreach (var selectedUnspentCoin in selectedUnspentCoins)
            {
                // Retrieve the matching UTXO from the lookup table
                var key = (selectedUnspentCoin.TransactionId, selectedUnspentCoin.Address);
                var selectedCoin = utxoLookup[key].SingleOrDefault()
                    ?? throw new InvalidOperationException($"No matching UTXO found for TransactionId: {key.TransactionId}, Address: {key.Address}");

                // Parse the transaction hex and create the NBitcoin Coin
                var transaction = Transaction.Parse(selectedCoin.TransactionHex, _commonService.BitcoinNetwork);
                var coin = new Coin(transaction, (uint)selectedCoin.TransactionPos);
                nBitcoinSelectedUnspentCoins.Add(coin);
            }

            return nBitcoinSelectedUnspentCoins;
        }

        public List<Coin> UtxoDetailsToNBitcoinCoin(List<UtxoDetailsElectrumx> utxoDetails)
        {
            return utxoDetails.Select(utxo => new Coin(Transaction.Parse(utxo.TransactionHex, _commonService.BitcoinNetwork), (uint)utxo.TransactionPos)).ToList();
        }
    }
}