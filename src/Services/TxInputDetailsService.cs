using ElectrumXClient.Response;
using NBitcoin;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BtcWalletLibrary.Interfaces;
using BtcWalletLibrary.Models;
using Transaction = NBitcoin.Transaction;
using BtcWalletLibrary.Services.Factories;


namespace BtcWalletLibrary.Services
{
    internal class TxInputDetailsService : ITxInputDetailsService
    {
        private readonly ICommonService _commonService;
        private readonly IAddressService _addressService;
        private readonly IElectrumxClientFactory _electrumxClientFactory;

        public TxInputDetailsService(ICommonService btcCommonService, IElectrumxClientFactory electrumxClientFactory, IAddressService addressService)
        {
            _commonService = btcCommonService;
            _addressService = addressService;
            _electrumxClientFactory = electrumxClientFactory;
        }

        /// <summary>
        /// Gets transaction inputs from given transaction. Input details are fetch from ElectrumX server.
        /// we need to fetch input details from ElectrumX server because NBitcoin library doesn not include transactions which are inputs  inside transaction object.
        ///if it did then input transactions alsoo needed to containt input transactions and so on. This would be a recursive problem and big memory usage
        /// </summary>
        /// <param name="transaction"></param>

        public async Task<List<TransactionInput>> GetTransactionInputDetails(Transaction transaction)
        {
            var trInputs = new List<TransactionInput>();
            foreach (var vin in transaction.Inputs)
            {
                BlockchainTransactionGetResponse transactionResponse;
                try
                {
                    using var client = _electrumxClientFactory.CreateClient();
                    transactionResponse = await client.GetBlockchainTransactionGet(vin.PrevOut.Hash.ToString());
                }
                catch { break; }
                var trDetails = transactionResponse.Result;
                var prevVout = trDetails.VoutValue.Find(v => v.N == vin.PrevOut.N);
                
                var prevVoutAddress = BitcoinAddress.Create(prevVout.ScriptPubKey.Addresses?.FirstOrDefault() ?? prevVout.ScriptPubKey.Address, _commonService.BitcoinNetwork);

                trInputs.Add(new TransactionInput()
                {
                    Address = prevVoutAddress.ToString(),
                    Amount = prevVout.Value,
                    OutputIdx = (int)vin.PrevOut.N,
                    TrId = vin.PrevOut.Hash.ToString(),
                    IsUsersAddress = _addressService.IsInQueiredAddresses(prevVoutAddress)
                });
            }
            return trInputs;
        }

        /// <summary>
        /// Gets transaction outputs from given transaction.
        /// </summary>
        public List<TransactionOutput> GetTransactionOutputDetails(Transaction transaction)
        {
            var trOutputs = new List<TransactionOutput>();
            foreach (var output in transaction.Outputs)
            {
                var outputAddress = output.ScriptPubKey.GetDestinationAddress(_commonService.BitcoinNetwork);
                trOutputs.Add(new TransactionOutput()
                {
                    Address = output.ScriptPubKey.GetDestinationAddress(_commonService.BitcoinNetwork)?.ToString(),
                    Amount = (double)output.Value.ToDecimal(MoneyUnit.BTC),
                    IsUsersAddress = _addressService.IsInQueiredAddresses(outputAddress)
                });
            }
                
            return trOutputs;
        }
    }
}