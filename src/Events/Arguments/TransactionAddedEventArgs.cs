using BtcWalletLibrary.Models;

namespace BtcWalletLibrary.Events.Arguments
{
    /// <summary>
    /// Event arguments for when a new Bitcoin transaction is added to the wallet's transaction history.
    /// This event is published via the <see cref="Interfaces.IEventDispatcher"/> after a new transaction is discovered or synchronized with the blockchain and added to the local storage.
    /// Subscribers can listen for this event to be notified of newly added transactions, enabling real-time updates to the user interface or application logic.
    /// </summary>
    public class TransactionAddedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the newly added Bitcoin transaction information.
        /// </summary>
        public Transaction BitcoinTransaction { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionAddedEventArgs"/> class.
        /// </summary>
        /// <param name="bitcoinTransaction">The <see cref="Transaction"/> object representing the newly added Bitcoin transaction.</param>
        public TransactionAddedEventArgs(Transaction bitcoinTransaction)
        {
            BitcoinTransaction = bitcoinTransaction ?? throw new System.ArgumentNullException(nameof(bitcoinTransaction));
        }
    }
}