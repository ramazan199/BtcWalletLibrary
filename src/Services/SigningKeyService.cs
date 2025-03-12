using BtcWalletLibrary.Interfaces;
using NBitcoin;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BtcWalletLibrary.Services
{
    /// <summary>
    /// Service responsible for creating signing keys for a transaction.
    /// </summary>
    internal class SigningKeyService : ISigningKeyService
    {
        private readonly IAddressService _btcAddressService;
        private readonly ICommonService _btcCommonService;

        /// <summary>
        /// Constructor for SigningKeyService.
        /// </summary>
        /// <param name="btcAddressService">Service to get wallet addresses</param>
        /// <param name="btcCommonService">Service providing common wallet functionalities</param>
        public SigningKeyService(IAddressService btcAddressService, ICommonService btcCommonService)
        {
            _btcAddressService = btcAddressService;
            _btcCommonService = btcCommonService;
        }

        /// <summary>
        /// Prepares a list of signing keys for the provided unspent coins.
        /// </summary>
        /// <param name="selectedUnspentCoins">List of coins to be spent</param>
        /// <returns>List of NBitcoin.Key objects for signing</returns>
        public List<Key> PrepareSigningKeys(IEnumerable<Coin> selectedUnspentCoins)
        {
            var signingKeys = new List<Key>();
            foreach (var coin in selectedUnspentCoins)
            {
                var address = coin.TxOut.ScriptPubKey.GetDestinationAddress(_btcCommonService.BitcoinNetwork);

                var addrIdxInMainAddrList = _btcAddressService.MainAddresses
                     .Select((addr, index) => new { addr, index })
                     .FirstOrDefault(x => x.addr.ScriptPubKey.GetDestinationAddress(_btcCommonService.BitcoinNetwork) == address)?.index ?? -1;

                var addrIdxInChangeAddrList = _btcAddressService.ChangeAddresses
                    .Select((addr, index) => new { addr, index })
                    .FirstOrDefault(x => x.addr.ScriptPubKey.GetDestinationAddress(_btcCommonService.BitcoinNetwork) == address)?.index ?? -1;

                var addrIdx = addrIdxInMainAddrList != -1 ? addrIdxInMainAddrList : addrIdxInChangeAddrList;
                if (addrIdx == -1) throw new ArgumentException("Address not found in Device Storage");

                var signingKey = addrIdxInMainAddrList != -1
                  ? _btcCommonService.MainAddressesParentExtKey.Derive((uint)addrIdx).PrivateKey
                  : _btcCommonService.ChangeAddressesParentExtKey.Derive((uint)addrIdx).PrivateKey;
                signingKeys.Add(signingKey);
            }

            return signingKeys;
        }
    }
}