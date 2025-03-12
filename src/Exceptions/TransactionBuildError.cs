namespace BtcWalletLibrary.Exceptions
{
    /// <summary>
    /// Represents an error message specific to Bitcoin transaction building operations.
    /// This class inherits from <see cref="OperationError"/> and is used to provide detailed error information when the process of building a Bitcoin transaction (e.g., coin selection, output creation, fee calculation) fails.
    /// Instances of this class are used to indicate specific transaction building errors and can be used to provide user-friendly error messages or for logging and debugging purposes.
    /// </summary>
    public class TransactionBuildError : OperationError
    {
        
        public TransactionBuildError(string message) : base(message)
        {
        }
    }
}