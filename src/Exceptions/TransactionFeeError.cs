namespace BtcWalletLibrary.Exceptions
{
    /// <summary>
    /// Represents an error message specific to transaction fee retrieval operations.
    /// This class inherits from <see cref="OperationError"/> and is used to provide more specific error details when fetching or calculating transaction fees fails.
    /// Instances of this class are returned within the <see cref="DTOs.Responses.TxFeeResult"/> class, specifically in its <see cref="DTOs.Responses.TxFeeResult.OperationError"/> property, when a fee retrieval operation is unsuccessful.
    /// </summary>
    public class TransactionFeeError : OperationError
    {
        internal TransactionFeeError(string message) : base(message)
        {
        }
    }
}