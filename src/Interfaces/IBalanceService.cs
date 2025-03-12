using System.Collections.Generic;
using BtcWalletLibrary.Models;

namespace BtcWalletLibrary.Interfaces
{
    /// <summary>
    /// Interface for services that manages and provides wallet balance information.
    /// </summary>
    public interface IBalanceService
    {
        /// <summary>
        /// Gets the total confirmed balance of the wallet across all addresses.
        /// </summary>
        double TotalConfirmedBalance { get; }
        /// <summary>
        /// Gets the total unconfirmed balance of the wallet across all addresses.
        /// </summary>
        double TotalUncnfirmedBalance { get; }
        /// <summary>
        /// Gets a dictionary of Unspent Transaction Outputs (UTXOs) grouped by address.
        /// The key is the Bitcoin address, and the value is a list of <see cref="UtxoDetailsElectrumx"/> for that address.
        /// </summary>
        IReadOnlyDictionary<string, IReadOnlyList<UtxoDetailsElectrumx>> UtxosPerAddress { get; }
        /// <summary>
        /// Gets a list of all Unspent Transaction Outputs (UTXOs) for the wallet, aggregated from all addresses.
        /// </summary>
        IEnumerable<UtxoDetailsElectrumx> Utxos { get; }
    }
}