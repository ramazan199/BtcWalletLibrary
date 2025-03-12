using BtcWalletLibrary.Models;

namespace BtcWalletLibrary.Events.Arguments
{
    /// <summary>
    /// Event arguments for when an output address in a transaction is identified as belonging to the user's wallet addresses.
    /// This event is published via the <see cref="Interfaces.IEventDispatcher"/> when the wallet's services determine that an output address of a specific transaction corresponds to an address managed by this wallet.
    ///
    /// **UI Usage:** User interfaces can subscribe to this event to:
    /// <list type="bullet">
    ///     <item>Identify transactions where funds are being sent to the user's wallet (incoming transactions).</item>
    ///     <item>Update transaction displays to visually distinguish incoming transactions based on user-owned output addresses.</item>
    ///     <item>Calculate and display the amount received in incoming transactions.</item>
    /// </list>
    ///
    /// Subscribers can listen for this event to track and react to transactions where the wallet's addresses are used as outputs, indicating funds are being received by the wallet.
    /// </summary>
    public class TxOutputAddrMarkedAsUserAddrEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the transaction ID (TxId) of the transaction containing the output address that was marked as a user address.
        /// </summary>
        public string TxId { get; }
        /// <summary>
        /// Gets the <see cref="TransactionOutput"/> object representing the output that was marked as a user address.
        /// This object provides details about the transaction output, including the output address, value, and related information.
        /// </summary>
        public TransactionOutput Output { get; }

        internal TxOutputAddrMarkedAsUserAddrEventArgs(string txId, TransactionOutput output)
        {
            TxId = txId;
            Output = output;
        }
    }
}