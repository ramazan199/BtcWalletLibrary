using System;
using System.Threading.Tasks;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Models;
using Coin = BtcWalletLibrary.Models.Coin;

namespace BtcWalletLibrary.Services.Mappers
{
    internal class TxMapper : ITxMapper
    {
        private readonly ITxInputDetailsService _txInputDetailsService;
        private readonly ICommonService _commonService;
        public TxMapper(ITxInputDetailsService btcTxInputDetailsService, ICommonService commonService)
        {
            _txInputDetailsService = btcTxInputDetailsService;
            _commonService = commonService;
        }

        public async Task<Transaction> NBitcoinTxToBtcTxForStorage(NBitcoin.Transaction tx)
        {
            var trInputs = await _txInputDetailsService.GetTransactionInputDetails(tx);
            var trOutputs = _txInputDetailsService.GetTransactionOutputDetails(tx);
            var trDate = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

            return new Transaction()
            {
                TransactionHex = tx.ToHex(),
                TransactionId = tx.GetHash().ToString(),
                Date = trDate,
                Inputs = trInputs,
                Outputs = trOutputs,
                Confirmed = false
            };
        }

        public Coin UtxoToCoin(UtxoDetailsElectrumx utxo)
        {
            var transaction = NBitcoin.Transaction.Parse(utxo.TransactionHex, _commonService.BitcoinNetwork);
            return new Coin(transaction, (uint)utxo.TransactionPos, utxo.Confirmed);
        }
    }
}
