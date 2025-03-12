namespace BtcWalletLibrary.Models
{
    /// <summary>
    /// Represents detailed information about an Unspent Transaction Output (UTXO) as retrieved from an ElectrumX server.
    /// ElectrumX is an open-source Electrum server implementation that provides APIs to query Bitcoin blockchain data.
    /// This class encapsulates specific UTXO details obtained from ElectrumX, such as the transaction ID and position of the output,
    /// the raw transaction hex, confirmation status, and the associated address.
    /// It is used to represent UTXOs in a format compatible with data returned by ElectrumX.
    /// </summary>
    public class UtxoDetailsElectrumx
    {
        /// <summary>
        /// Transaction ID (TxId) of the transaction that created this UTXO, as reported by ElectrumX.
        /// This is the hash of the transaction in which this UTXO was an output.
        /// </summary>
        public string TransactionId { get; internal set; }
        /// <summary>
        /// Output position (index) within the transaction (<see cref="TransactionId"/>) that created this UTXO, as reported by ElectrumX.
        /// This index identifies the specific output in the transaction's output list that corresponds to this UTXO.
        /// </summary>
        public int TransactionPos { get; internal set; }
        /// <summary>
        /// Raw transaction data in hexadecimal format for the transaction that created this UTXO, as reported by ElectrumX.
        /// This can be used to access the full transaction details if needed.
        /// </summary>
        public string TransactionHex { get; internal set; }
        /// <summary>
        /// Value indicating whether the transaction that created this UTXO is considered confirmed by ElectrumX.
        /// True if the transaction has reached a sufficient number of confirmations as determined by the ElectrumX server, false otherwise.
        /// </summary>
        public bool Confirmed { get; internal set; }
        /// <summary>
        /// Bitcoin address associated with this UTXO, as reported by ElectrumX.
        /// This is the address to which the bitcoins in this UTXO were sent.
        /// </summary>
        public string Address { get; internal set; }
    }
}