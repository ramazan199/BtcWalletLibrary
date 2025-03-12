using NBitcoin;
using System.Collections.Generic;

namespace BtcWalletLibrary.Interfaces
{
    /// <summary>
    /// Interface for services that manage Bitcoin addresses within the wallet.
    /// </summary>
    public interface IAddressService
    {
        /// <summary>
        /// Gets the current change address.
        /// </summary>
        BitcoinAddress ChangeAddress { get; }
        /// <summary>
        /// Gets the list of all derived change addresses.
        /// </summary>
        IReadOnlyList<BitcoinAddress> ChangeAddresses { get; }
        /// <summary>
        /// Gets the index of the last derived change address.
        /// </summary>
        int LastChangeAddrIdx { get; }
        /// <summary>
        /// Gets the current main (receiving) address.
        /// </summary>
        BitcoinAddress MainAddress { get; }
        /// <summary>
        /// Gets the list of all derived main (receiving) addresses.
        /// </summary>
        IReadOnlyList<BitcoinAddress> MainAddresses { get; }
        /// <summary>
        /// Gets the index of the last derived main address.
        /// </summary>
        int LastMainAddrIdx { get; }
        /// <summary>
        /// Gets the list of main addresses that have been queried from the blockchain.
        /// </summary>
        IReadOnlyList<BitcoinAddress> QueriedMainAddresses { get; }
        /// <summary>
        /// Gets the list of change addresses that have been queried from the blockchain.
        /// </summary>
        IReadOnlyList<BitcoinAddress> QueriedChangeAddresses { get; }

        /// <summary>
        /// Derives a change address at a specific index.
        /// </summary>
        /// <param name="addrIdx">The address index to derive.</param>
        /// <returns>The derived Bitcoin change address.</returns>
        BitcoinAddress DerivChangeAddr(uint addrIdx);
        /// <summary>
        /// Derives a main (receiving) address at a specific index.
        /// </summary>
        /// <param name="addrIdx">The address index to derive.</param>
        /// <returns>The derived Bitcoin main address.</returns>
        BitcoinAddress DeriveMainAddr(uint addrIdx);
        /// <summary>
        /// Derives a new change address.
        /// </summary>
        /// <returns>The newly derived Bitcoin change address.</returns>
        BitcoinAddress DeriveNewChangeAddr();
        /// <summary>
        /// Derives a new main (receiving) address.
        /// </summary>
        /// <returns>The newly derived Bitcoin main address.</returns>
        BitcoinAddress DeriveNewMainAddr();

        /// <summary>
        /// Adds a main address to the list of queried main addresses.
        /// </summary>
        /// <param name="address">The Bitcoin address to add to the queried main addresses list.</param>
        void AddAddressToQueriedMainAddrLst(BitcoinAddress address);
        /// <summary>
        /// Adds a change address to the list of queried change addresses.
        /// </summary>
        /// <param name="address">The Bitcoin address to add to the queried change addresses list.</param>
        void AddAddressToQueriedChangeAddrLst(BitcoinAddress address);
        /// <summary>
        /// Checks if a given Bitcoin address is in the list of queried addresses (main or change).
        /// </summary>
        /// <param name="bitcoinAddress">The Bitcoin address to check.</param>
        /// <returns>True if the address is in the queried addresses list, otherwise false.</returns>
        bool IsInQueiredAddresses(BitcoinAddress bitcoinAddress);
    }
}