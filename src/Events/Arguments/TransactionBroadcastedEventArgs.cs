using System;
using BtcWalletLibrary.Models;

namespace BtcWalletLibrary.Events.Arguments
{
    /// <summary>
    /// Event arguments for when a transaction is successfully broadcasted to the Bitcoin network.
    /// This event is published via the <see cref="Interfaces.IEventDispatcher"/> after the wallet has successfully submitted a transaction to the Bitcoin network for broadcasting.
    /// Subscribers can listen for this event (to update UI e.g.) to be notified when a transaction initiated by the wallet is successfully sent.
    /// </summary>
    public class TransactionBroadcastedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the <see cref="TransactionForStorage"/> object representing the transaction that was broadcasted.
        /// This object contains details of the broadcasted transaction, including its transaction ID and other relevant information.
        /// </summary>
        public Transaction TransactionForStorage { get; }

        internal TransactionBroadcastedEventArgs(Transaction transactionForStorage)
        {
            TransactionForStorage = transactionForStorage ??
                        throw new ArgumentNullException(nameof(transactionForStorage));
        }
    }
}