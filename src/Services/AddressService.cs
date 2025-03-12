using NBitcoin;
using System;
using System.Collections.Generic;
using BtcWalletLibrary.Events.Arguments;
using BtcWalletLibrary.Interfaces;

namespace BtcWalletLibrary.Services
{
    /// <summary>
    /// Service responsible for managing Bitcoin addresses, including generation, storage, and retrieval.
    /// It handles both main (receiving) and change addresses, ensuring proper derivation and persistence.
    /// </summary>
    internal class AddressService : IAddressService
    {
        private readonly ICommonService _btcCommonService; // Service for common Bitcoin functionalities.
        private readonly IStorageService _btcStorageService; // Service for persistent storage operations.
        private readonly List<BitcoinAddress> _changeAddresses;
        private readonly List<BitcoinAddress> _queriedMainAddresses;
        private readonly List<BitcoinAddress> _queriedChangeAddresses;
        private readonly List<BitcoinAddress> _mainAddresses;

        /// <summary>
                /// Gets the index of the last generated change address.
                /// </summary>
        public int LastChangeAddrIdx { get; private set; } = -1;
        /// <summary>
        /// Gets the index of the last generated main address.
        /// </summary>
        public int LastMainAddrIdx { get; private set; } = -1;
        /// <summary>
        /// Gets the most recently generated main address.
        /// </summary>
        public BitcoinAddress MainAddress { get; private set; }
        /// <summary>
        /// Gets the most recently generated change address.
        /// </summary>
        public BitcoinAddress ChangeAddress { get; private set; }
        /// <summary>
        /// Gets the list of all generated main addresses.
        /// </summary>
        public IReadOnlyList<BitcoinAddress> MainAddresses => _mainAddresses;
        /// <summary>
        /// Gets the list of all generated change addresses.
        /// </summary>
        public IReadOnlyList<BitcoinAddress> ChangeAddresses => _changeAddresses;
        /// <summary>
        /// Gets the list of main addresses that have been queried for balance or transactions.
        /// </summary>
        public IReadOnlyList<BitcoinAddress> QueriedMainAddresses => _queriedMainAddresses;
        /// <summary>
        /// Gets the list of change addresses that have been queried for balance or transactions.
        /// </summary>
        public IReadOnlyList<BitcoinAddress> QueriedChangeAddresses => _queriedChangeAddresses;

        /// <summary>
        /// Initializes a new instance of the <see cref="AddressService"/> class.
        /// </summary>
        /// <param name="btcCommonService">The common Bitcoin service.</param>
        /// <param name="btcStorageService">The storage service for persisting address data.</param>
        /// <param name="eventDispatcher">The event dispatcher for subscribing to address related events.</param>
        public AddressService(ICommonService btcCommonService, IStorageService btcStorageService, IEventDispatcher eventDispatcher)
        {
            _btcCommonService = btcCommonService;
            _btcStorageService = btcStorageService;

            _mainAddresses = new List<BitcoinAddress>();
            _changeAddresses = new List<BitcoinAddress>();
            _queriedMainAddresses = new List<BitcoinAddress>();
            _queriedChangeAddresses = new List<BitcoinAddress>();

            eventDispatcher.Subscribe<NewMainAddressesFoundEventArgs>(OnNewMainAddressesFound);
            eventDispatcher.Subscribe<NewChangeAddressesFoundEventArgs>(OnNewChangeAddressesFound);
            eventDispatcher.Subscribe<TransactionBroadcastedEventArgs>(OnTransactionBroadcasted);

            RestoreAddressesFromStorage();
        }

        private void RestoreAddressesFromStorage()
        {
            LastMainAddrIdx = _btcStorageService.GetLastMainAddrIdxFromStorage();
            LastChangeAddrIdx = _btcStorageService.GetLastChangeAddrIdxFromStorage();
            GenerateAddresses();
        }

        private void OnNewMainAddressesFound(object sender, NewMainAddressesFoundEventArgs e)
        {
            AddNewMainAddresses(e.NewLastAddrIdx);
            _btcStorageService.StoreLastMainAddrIdx((uint)LastMainAddrIdx);
        }

        private void OnNewChangeAddressesFound(object sender, NewChangeAddressesFoundEventArgs e)
        {
            AddNewChangeAddresses(e.NewLastAddrIdx);
            _btcStorageService.StoreLastChangeAddrIdx((uint)LastChangeAddrIdx);
        }

        private void OnTransactionBroadcasted(object sender, TransactionBroadcastedEventArgs e)
        {
            ChangeAddress = DeriveNewChangeAddr();
            _changeAddresses.Add(ChangeAddress);
            LastChangeAddrIdx++;
            _btcStorageService.StoreLastChangeAddrIdx((uint)LastChangeAddrIdx);
        }

        private void AddNewMainAddresses(uint newLastAddrIdx)
        {
            if (newLastAddrIdx < LastMainAddrIdx)
                throw new ArgumentException("The provided address index must be greater than the last main address index.");
            // No new addresses needed.
            if (newLastAddrIdx == LastMainAddrIdx) return;

            BitcoinAddress generatedMainAddr = null;
            for (var i = LastMainAddrIdx + 1; i <= newLastAddrIdx; i++)
            {
                generatedMainAddr = DeriveMainAddr((uint)i);
                _mainAddresses.Add(generatedMainAddr);
            }
            MainAddress = generatedMainAddr;
            LastMainAddrIdx = (int)newLastAddrIdx;
        }

        private void AddNewChangeAddresses(uint newLastAddrIdx)
        {
            if (newLastAddrIdx < LastChangeAddrIdx)
                throw new ArgumentException("The provided address index must be greater than the last main address index.");
            // No new addresses needed.
            if (newLastAddrIdx == LastChangeAddrIdx) return;

            BitcoinAddress generatedChangeAddr = null;
            for (var i = LastChangeAddrIdx + 1; i <= newLastAddrIdx; i++)
            {
                generatedChangeAddr = DerivChangeAddr((uint)i);
                _changeAddresses.Add(generatedChangeAddr);
            }
            ChangeAddress = generatedChangeAddr;
            LastChangeAddrIdx = (int)newLastAddrIdx;
        }



        /// <summary>
                /// Derives a main address for a given address index.
                /// </summary>
                /// <param name="addrIdx">The address index to derive.</param>
                /// <returns>The derived Bitcoin main address.</returns>
        public BitcoinAddress DeriveMainAddr(uint addrIdx)
        {
            return _btcCommonService.MainAddressesParentExtPubKey.Derive(addrIdx).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, _btcCommonService.BitcoinNetwork);
        }

        /// <summary>
        /// Derives a change address for a given address index.
        /// </summary>
        /// <param name="addrIdx">The address index to derive.</param>
        /// <returns>The derived Bitcoin change address.</returns>
        public BitcoinAddress DerivChangeAddr(uint addrIdx)
        {
            return _btcCommonService.ChangeAddressesParentExtPubKey.Derive(addrIdx).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, _btcCommonService.BitcoinNetwork);
        }

        /// <summary>
        /// Derives a new main address (increments the last main address index and derives the address).
        /// </summary>
        /// <returns>The newly derived Bitcoin main address.</returns>
        public BitcoinAddress DeriveNewMainAddr()
        {
            return _btcCommonService.MainAddressesParentExtPubKey.Derive((uint)(LastMainAddrIdx + 1)).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, _btcCommonService.BitcoinNetwork);
        }

        /// <summary>
        /// Derives a new change address (increments the last change address index and derives the address).
        /// </summary>
        /// <returns>The newly derived Bitcoin change address.</returns>
        public BitcoinAddress DeriveNewChangeAddr()
        {
            return _btcCommonService.ChangeAddressesParentExtPubKey.Derive((uint)(LastChangeAddrIdx + 1)).GetPublicKey().GetAddress(ScriptPubKeyType.Legacy, _btcCommonService.BitcoinNetwork);
        }


        /// <summary>
        /// Generates initial main and change addresses on service startup, based on stored indices.
        /// </summary>
        private void GenerateAddresses()
        {
            GenerateAddressType(_mainAddresses, _queriedMainAddresses, DeriveMainAddr, LastMainAddrIdx);
            GenerateAddressType(_changeAddresses, _queriedChangeAddresses, DerivChangeAddr, LastChangeAddrIdx);
        }


        /// <summary>
        /// Helper method to generate addresses of a specific type (main or change).
        /// </summary>
        /// <param name="addresses">The list to store the generated addresses.</param>
        /// <param name="queriedAddresses">The list to store addresses that have been queried (for balance, etc.).</param>
        /// <param name="deriveAddressFunc">The function to use for deriving addresses (DeriveMainAddr or DerivChangeAddr).</param>
        /// <param name="lastIndex">The last address index to generate up to.</param>
        private void GenerateAddressType(
      List<BitcoinAddress> addresses,
      List<BitcoinAddress> queriedAddresses,
      Func<uint, BitcoinAddress> deriveAddressFunc,
      int lastIndex)
        {
            if (lastIndex < 0) return;
            // Generate addresses in the specified range
            for (uint i = 0; i <= lastIndex; i++)
            {
                var generatedAddress = deriveAddressFunc(i);
                addresses.Add(generatedAddress);
                queriedAddresses.Add(generatedAddress);
            }
        }

        /// <summary>
        /// Adds a Bitcoin address to the list of queried main addresses.
        /// This indicates that this address has been used in a query (e.g., balance check).
        /// </summary>
        /// <param name="address">The Bitcoin address to add.</param>
        public void AddAddressToQueriedMainAddrLst(BitcoinAddress address)
        {
            _queriedMainAddresses.Add(address);
        }

        /// <summary>
        /// Adds a Bitcoin address to the list of queried change addresses.
        /// This indicates that this address has been used in a query (e.g., balance check).
        /// </summary>
        /// <param name="address">The Bitcoin address to add.</param>
        public void AddAddressToQueriedChangeAddrLst(BitcoinAddress address)
        {
            _queriedChangeAddresses.Add(address);
        }

        /// <summary>
        /// Checks if a given Bitcoin address is present in the list of queried addresses (either main or change).
        /// </summary>
        /// <param name="bitcoinAddress">The Bitcoin address to check.</param>
        /// <returns>True if the address is in the queried addresses list; otherwise, false.</returns>
        public bool IsInQueiredAddresses(BitcoinAddress bitcoinAddress)
        {
            return _queriedMainAddresses.Contains(bitcoinAddress) || _queriedChangeAddresses.Contains(bitcoinAddress);
        }
    }
}