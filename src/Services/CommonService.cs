using Microsoft.Extensions.Options;
using NBitcoin;
using System;
using BtcWalletLibrary.Configuration;
using BtcWalletLibrary.Interfaces;

namespace BtcWalletLibrary.Services
{
    /// <summary>
    /// Service providing common functionalities and configurations for the wallet.
    /// </summary>
    internal class CommonService : ICommonService
    {
        /// <summary>
        /// Extended private key for main addresses.
        /// </summary>
        public ExtKey MainAddressesParentExtKey { get; }

        /// <summary>
        /// Extended private key for change addresses.
        /// </summary>
        public ExtKey ChangeAddressesParentExtKey { get; }

        /// <summary>
        /// Extended public key for main addresses.
        /// </summary>
        public ExtPubKey MainAddressesParentExtPubKey { get; }

        /// <summary>
        /// Extended public key for change addresses.
        /// </summary>
        public ExtPubKey ChangeAddressesParentExtPubKey { get; }

        /// <summary>
        /// Bitcoin network the wallet is operating on.
        /// </summary>
        public Network BitcoinNetwork { get; }

        /// <summary>
        /// Maximum range of empty addresses to check during address discovery.
        /// </summary>
        public int MaxEmptyAddrRange { get; }


        /// <summary>
        /// Constructor for CommonService.
        /// </summary>
        /// <param name="mnemonicWords">Mnemonic words for wallet seed.</param>
        /// <param name="options">Wallet configuration options.</param>
        public CommonService(string mnemonicWords, IOptionsMonitor<WalletConfig> options)
        {
            var nodeConfig = options.CurrentValue.NodeConfiguration;
            MaxEmptyAddrRange = nodeConfig.MaxRangeEmptyAddr;

            BitcoinNetwork = nodeConfig.Network switch
            {
                Configuration.NetworkType.Main => Network.Main,
                Configuration.NetworkType.TestNet => Network.TestNet,
                Configuration.NetworkType.RegTest => Network.RegTest,
                _ => throw new ArgumentException("Invalid network type.")
            };

            var mnemo = new Mnemonic(mnemonicWords, Wordlist.English);
            var masterKey = mnemo.DeriveExtKey();

            var hardenedPathMainAddresses = new KeyPath("44'/1'/0'/0");
            var hardenedPathChangeAddresses = new KeyPath("44'/1'/0'/1");

            MainAddressesParentExtKey = masterKey.Derive(hardenedPathMainAddresses);
            ChangeAddressesParentExtKey = masterKey.Derive(hardenedPathChangeAddresses);
            MainAddressesParentExtPubKey = masterKey.Derive(hardenedPathMainAddresses).Neuter();
            ChangeAddressesParentExtPubKey = masterKey.Derive(hardenedPathChangeAddresses).Neuter();
        }
    }
}