using System;

namespace BtcWalletLibrary.Events.Arguments
{
    /// <summary>
    /// Event arguments for when a transaction is considered confirmed on the Bitcoin network.
    /// This event is published via the <see cref="Interfaces.IEventDispatcher"/> when a transaction, relevant to the wallet, reaches a certain confirmation threshold on the blockchain.
    /// Subscribers can listen for this event(e.g. to Update UI) to be notified when a transaction's status changes to confirmed, indicating a higher degree of security and finality.
    /// </summary>
    public class TransactionConfirmedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the transaction ID (TxId) of the confirmed transaction.
        /// </summary>
        public string TxId { get; }

        public TransactionConfirmedEventArgs(string txId)
        {
            TxId = txId ?? throw new ArgumentNullException(nameof(txId));
        }
    }
}