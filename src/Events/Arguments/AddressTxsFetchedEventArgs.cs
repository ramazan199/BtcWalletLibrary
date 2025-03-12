using System.Collections.Generic;
using BtcWalletLibrary.Models;

namespace BtcWalletLibrary.Events.Arguments
{
    /// <summary>
    /// Event arguments for when transaction fetching for a specific address is completed.
    /// This event is published via the <see cref="Interfaces.IEventDispatcher"/> after the wallet has finished fetching transaction history for a single Bitcoin address.
    /// Note that addresses are queried individually for their transaction history, such as during wallet synchronization or initial address discovery.
    /// </summary>
    public class AddressTxsFetchedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the Bitcoin address for which transactions were fetched.
        /// </summary>
        public string Address { get; }
        /// <summary>
        /// Gets the list of transactions that were fetched for the specified address.
        /// </summary>
        public List<Transaction> Transactions { get; }

        internal AddressTxsFetchedEventArgs(string address, List<Transaction> transactions)
        {
            Address = address ?? throw new System.ArgumentNullException(nameof(address));
            Transactions = transactions ?? throw new System.ArgumentNullException(nameof(transactions));
        }
    }
}