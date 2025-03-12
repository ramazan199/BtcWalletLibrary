using System;

namespace BtcWalletLibrary.Models
{
    /// <summary>
    /// Represents an input within a Bitcoin transaction.
    /// Transaction inputs detail the source of funds for a transaction, referencing outputs from previous transactions.
    /// This class encapsulates the specifics of a transaction input, including the originating transaction, the output index being spent,
    /// the associated address, user address status, and the amount contributed as input. Implements the <see cref="ICloneable"/> interface and is <see cref="SerializableAttribute"/>.
    /// </summary>
    [Serializable]
    public class TransactionInput : ICloneable
    {
        /// <summary>
        /// Identifies the transaction from which this input is spending an output.
        /// This property holds the transaction ID (TxId) of the prior transaction being referenced.
        /// </summary>
        public string TrId { get; set; }
        /// <summary>
        /// Specifies the index of the output within the referenced transaction that is being utilized as input.
        /// This index points to a specific output in the outputs collection of the transaction identified by <see cref="TrId"/>.
        /// </summary>
        public int OutputIdx { get; set; }
        /// <summary>
        /// Bitcoin address associated with this transaction input.
        /// This address typically corresponds to the recipient of the bitcoins in the referenced transaction output.
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Indicates whether the address of this input is one managed by the user's wallet.
        /// When true, it signifies that the <see cref="Address"/> belongs to the wallet owner; otherwise, it's false.
        /// </summary>
        public bool IsUsersAddress { get; set; }
        /// <summary>
        /// Represents the Bitcoin amount being spent by this transaction input.
        /// This value reflects the worth of the referenced output (<see cref="TrId"/>:<see cref="OutputIdx"/>) being used as an input in the current transaction.
        /// </summary>
        public double Amount { get; set; }


        /// <summary>
        /// Creates a new object that is a deep copy of the current <see cref="TransactionInput"/> instance.
        /// Implements the <see cref="ICloneable.Clone"/> method to facilitate the creation of exact replicas of <see cref="TransactionInput"/> objects.
        /// </summary>
        /// <returns>A new <see cref="TransactionInput"/> object that is a clone of this instance.</returns>
        public object Clone()
        {
            return new TransactionInput
            {
                TrId = TrId,
                OutputIdx = OutputIdx,
                Address = Address,
                IsUsersAddress = IsUsersAddress,
                Amount = Amount
            };
        }
    }
}