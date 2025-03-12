using BtcWalletLibrary.Exceptions;

namespace BtcWalletLibrary.DTOs.Responses
{
    /// <summary>
    /// Represents the result of a transfer operation after sending bitcoins to a recipient.
    /// </summary>
    public class TransferResult
    {
        /// <summary>
        /// Indicates whether the transfer operation was successful.
        /// </summary>
        public bool Success { get; }
        /// <summary>
        /// The transaction ID of the transfer, if successful. Null or empty if the transfer failed.
        /// </summary>
        public string TransactionId { get; }
        /// <summary>
        /// Error information in case the transfer operation failed. Null if the transfer was successful.
        /// If not null, this property will contain a <see cref="TransferError"/> object providing details about the error.
        /// </summary>
        public TransferError OperationError { get; }

        internal TransferResult(bool success, string transactionId = null, TransferError errorMessage = null)
        {
            Success = success;
            TransactionId = transactionId;
            OperationError = errorMessage;
        }
    }
}