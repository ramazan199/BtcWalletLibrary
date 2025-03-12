namespace BtcWalletLibrary.Exceptions
{
    /// <summary>
    /// Base class for representing operation error messages within the BtcWalletLibrary.
    /// This class provides a common structure for encapsulating error details as objects, allowing for more type-safe and organized error handling compared to using plain strings for error messages.
    /// Derived classes, such as <see cref="TransactionFeeError"/> and <see cref="TransferError"/>, can inherit from this class to represent specific types of errors with potentially additional properties in the future.
    /// </summary>
    public class OperationError
    {
        /// <summary>
        /// Gets the error message string.
        /// This property contains the human-readable description of the error that occurred.
        /// </summary>
        public string Message { get; }

        internal OperationError(string message)
        {
            Message = message;
        }
    }
}