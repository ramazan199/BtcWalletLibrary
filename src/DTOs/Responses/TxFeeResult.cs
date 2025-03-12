using BtcWalletLibrary.Exceptions;
using NBitcoin;

namespace BtcWalletLibrary.DTOs.Responses
{
    /// <summary>
    /// Represents the result of a transaction fee retrieval operation.
    /// This class encapsulates the outcome of attempting to fetch or calculate a transaction fee, including success status, whether a default fee was used, the fee amount, and any error messages.
    /// </summary>
    public class TxFeeResult
    {
        /// <summary>
        /// Gets or internal sets a value indicating whether the network fee retrieval operation was successful.
        /// True if the fee was successfully fetched from the network, false otherwise (e.g., due to network errors).
        /// </summary>
        public bool IsSuccess { get; internal set; } // Was network fetch successful?
        /// <summary>
        /// Gets or internal sets a value indicating whether a default fee value was used.
        /// True if a default or fallback fee was used (typically when network fee retrieval fails), false if a network-derived fee was used.
        /// </summary>
        public bool IsDefault { get; internal set; } // Was default value used?
        /// <summary>
        /// Gets or internal sets the transaction fee amount.
        /// This property contains the recommended or calculated transaction fee as a <see cref="Money"/> object.
        /// The fee unit is typically in satoshis.
        /// </summary>
        public Money Fee { get; internal set; }
        /// <summary>
        /// Gets or internal sets the error information in case the fee retrieval operation was not successful (<see cref="IsSuccess"/> is false).
        /// Null if the operation was successful or if no specific error occurred.
        /// If not null, this property will contain a <see cref="TransactionFeeError"/> object providing details about the error.
        /// </summary>
        public TransactionFeeError OperationError { get; internal set; }
    }
}