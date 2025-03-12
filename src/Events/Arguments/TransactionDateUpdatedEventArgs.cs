using System;

namespace BtcWalletLibrary.Events.Arguments
{
    /// <summary>
    /// Event arguments for when the date of a transaction is updated in the wallet's transaction history.
    /// This event is published via the <see cref="Interfaces.IEventDispatcher"/> when the date associated with a transaction is updated  due to synchronization with the blockchain.
    /// Subscribers can listen for this event to update transaction displays or time-sensitive logic based on the corrected transaction date.
    /// </summary>
    public class TransactionDateUpdatedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the transaction ID (TxId) of the transaction that has its date updated.
        /// </summary>
        public string TxId { get; }
        /// <summary>
        /// Gets the new <see cref="DateTime"/> representing the updated date of the transaction.
        /// </summary>
        public DateTime Date { get; }
      
        public TransactionDateUpdatedEventArgs(string txId, DateTime date)
        {
            TxId = txId;
            Date = date;
        }
    }
}