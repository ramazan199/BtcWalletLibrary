using BtcWalletLibrary.Models;

namespace BtcWalletLibrary.Events.Arguments
{
    /// <summary>
    /// Event arguments for when an input address in a transaction is identified as belonging to the user's wallet addresses.
    /// This event is published via the <see cref="Interfaces.IEventDispatcher"/> when the wallet's services determine that an input address of a specific transaction corresponds to an address managed by this wallet.
    ///
    /// **UI Usage:** User interfaces can subscribe to this event to:
    /// <list type="bullet">
    ///     <item>Identify transactions where funds are being sent from the user's wallet.</item>
    ///     <item>Update transaction displays to visually distinguish outgoing transactions based on user-owned input addresses.</item>
    ///     <item>Potentially calculate and display the amount sent in outgoing transactions.</item>
    /// </list>
    ///
    /// Subscribers can listen for this event to track and react to transactions where the wallet's addresses are used as inputs, indicating funds are being spent from the wallet.
    /// </summary>
    public class TxInputAddrMarkedAsUserAddrEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the transaction ID (TxId) of the transaction containing the input address that was marked as a user address.
        /// </summary>
        public string TxId { get; }
        /// <summary>
        /// Gets the <see cref="TransactionInput"/> object representing the input that was marked as a user address.
        /// This object provides details about the transaction input, including the input address and related information.
        /// </summary>
        public TransactionInput Input { get; }

        internal TxInputAddrMarkedAsUserAddrEventArgs(string txId, TransactionInput input)
        {
            TxId = txId;
            Input = input;
        }
    }
}