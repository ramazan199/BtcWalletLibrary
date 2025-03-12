namespace BtcWalletLibrary.Exceptions
{
    /// <summary>
    /// Represents an error message specific to Bitcoin transfer operations.
    /// This class inherits from <see cref="OperationError"/> and is used to provide detailed error information when a Bitcoin transfer (sending funds) fails.
    /// Instances of this class are returned within the <see cref="DTOs.Responses.TransferResult"/> class, specifically in its <see cref="DTOs.Responses.TransferResult.OperationError"/> property, when a transfer operation is unsuccessful.
    /// </summary>
    public class TransferError : OperationError
    {
        internal TransferError(string message) : base(message)
        {
        }
    }
}