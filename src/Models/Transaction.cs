using System;
using System.Collections.Generic;
using System.Linq;

namespace BtcWalletLibrary.Models
{
    /// <summary>
    /// Represents a Bitcoin transaction within the wallet library.
    /// This class models a Bitcoin transaction, encapsulating key details such as the raw transaction hex, date, transaction ID,
    /// outputs, inputs, and confirmation status. It implements the <see cref="ICloneable"/> interface to allow for creating copies of transaction objects and is marked as <see cref="SerializableAttribute"/> to enable serialization.
    /// </summary>
    [Serializable]
    public class Transaction : ICloneable
    {
        /// <summary>
        /// Provides the raw transaction data in hexadecimal format.
        /// This string holds the serialized Bitcoin transaction in its hexadecimal encoding.
        /// </summary>
        public string TransactionHex { get; set; }
        /// <summary>
        /// Represents the date and time when the transaction was first seen or included in a block.
        /// This <see cref="DateTime"/> object stores the transaction's timestamp.
        /// </summary>
        public DateTime Date { get; set; }
        /// <summary>
        /// Uniquely identifies the transaction within the Bitcoin blockchain.
        /// This is the transaction hash (TxId) represented as a string.
        /// </summary>
        public string TransactionId { get; set; }
        /// <summary>
        /// Contains a list of transaction outputs.
        /// Each <see cref="TransactionOutput"/> object in this list represents an output of this transaction,
        /// detailing recipient addresses and amounts.
        /// </summary>
        public List<TransactionOutput> Outputs { get; set; }
        /// <summary>
        /// Includes a list of transaction inputs.
        /// This list comprises <see cref="TransactionInput"/> objects, each signifying an input to this transaction,
        /// and referencing the UTXOs (Unspent Transaction Outputs) being spent.
        /// </summary>
        public List<TransactionInput> Inputs { get; set; }
        /// <summary>
        /// Indicates if the transaction has reached a sufficient number of confirmations.
        /// When true, the transaction is considered confirmed on the blockchain; otherwise, it is false.
        /// </summary>
        public bool Confirmed { get; set; }

        /// <summary>
                /// Creates a deep clone of the <see cref="Transaction"/> object.
                /// This method implements the <see cref="ICloneable.Clone"/> interface and creates a new <see cref="Transaction"/> object
                /// with all properties copied from the original, including deep clones of the <see cref="Outputs"/> and <see cref="Inputs"/> lists and their contents.
                /// </summary>
                /// <returns>A new <see cref="Transaction"/> object that is a clone of the current instance.</returns>
        public object Clone()
        {
            return new Transaction
            {
                TransactionHex = TransactionHex,
                Date = Date,
                TransactionId = TransactionId,
                Outputs = Outputs?.Select(o => (TransactionOutput)o.Clone()).ToList() ?? new List<TransactionOutput>(),
                Inputs = Inputs?.Select(i => (TransactionInput)i.Clone()).ToList() ?? new List<TransactionInput>(),
                Confirmed = Confirmed
            };
        }
    }
}