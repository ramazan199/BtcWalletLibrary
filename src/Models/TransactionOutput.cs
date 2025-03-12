using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BtcWalletLibrary.Models
{
    /// <summary>
    /// Represents an output of a Bitcoin transaction.
    /// Transaction outputs define where bitcoins are sent as a result of a transaction. Each output specifies a destination address and the amount of bitcoins being transferred.
    /// This class details the components of a transaction output, including the recipient's address, the value of bitcoins being sent, and whether the address belongs to the user's wallet.
    /// Implements the <see cref="ICloneable"/> interface and is marked as <see cref="SerializableAttribute"/>.
    /// </summary>
    [Serializable]
    public class TransactionOutput : ICloneable
    {
        /// <summary>
        /// Bitcoin address to which bitcoins are being sent.
        /// This property contains the recipient's Bitcoin address for this output.
        /// </summary>
        public string Address { get; set; }
        /// <summary>
        /// Defines the quantity of bitcoins being transferred in this output.
        /// This value represents the amount of bitcoins, typically in satoshis, designated for the recipient address.
        /// </summary>
        public double Amount { get; set; }
        /// <summary>
        /// Flags if the output address is associated with the user's wallet.
        /// When true, it indicates that the <see cref="Address"/> is an address managed by the current wallet; otherwise, it is false.
        /// </summary>
        public bool IsUsersAddress { get; set; }


        // ICloneable interface method
        /// <summary>
        /// Creates a new object that is a deep copy of the current <see cref="TransactionOutput"/> instance.
        /// Implements the <see cref="ICloneable.Clone"/> method to enable the creation of exact copies of <see cref="TransactionOutput"/> objects.
        /// </summary>
        /// <returns>A new <see cref="TransactionOutput"/> object that is a clone of this instance.</returns>
        public object Clone()
        {
            return new TransactionOutput
            {
                Address = Address,
                Amount = Amount,
                IsUsersAddress = IsUsersAddress
            };
        }
    }
}