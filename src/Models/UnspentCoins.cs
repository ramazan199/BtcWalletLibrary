namespace BtcWalletLibrary.Models
{
    /// <summary>
    /// Represents an Unspent Transaction Output (UTXO), also known as an unspent coin, in the Bitcoin system.
    /// UTXOs are the fundamental units of spendable Bitcoin. Each UTXO represents a certain amount of bitcoin
    /// that is associated with a specific address and can be used as input in a new transaction.
    /// This class encapsulates the key details of a UTXO, including its value, the transaction it originated from,
    /// the address it is associated with, and its confirmation status on the blockchain.
    /// </summary>
    public class UnspentCoin
    {
        /// <summary>
        /// Amount of bitcoins this UTXO represents.
        /// This value is typically expressed in satoshis (the smallest unit of Bitcoin) as a decimal.
        /// </summary>
        public decimal Amount { get; set; }
        /// <summary>
        /// Transaction ID (TxId) of the transaction that created this UTXO.
        /// This property refers to the transaction hash of the transaction where this unspent output was originally created.
        /// </summary>
        public string TransactionId { get; set; }
        /// <summary>
        /// Bitcoin address associated with this UTXO.
        /// This address is the recipient address to which the bitcoins were sent in the output that created this UTXO.
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Boolean indicating whether the transaction that created this UTXO is confirmed on the blockchain.
        /// True if the transaction has reached a sufficient number of confirmations, making this UTXO spendable with a higher degree of certainty, false otherwise.
        /// </summary>
        public bool Confirmed { get; set; }
    }
}