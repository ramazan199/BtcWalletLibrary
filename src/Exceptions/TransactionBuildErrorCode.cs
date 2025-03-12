namespace BtcWalletLibrary.Exceptions
{
    /// <summary>
    /// Enumeration of error codes related to transaction building process.
    /// </summary>
    public enum TransactionBuildErrorCode
    {
        /// <summary>
        /// No error. Default value indicating no specific transaction build error.
        /// </summary>
        None = 0,
        /// <summary>
        /// Error code indicating an invalid transaction amount.
        /// </summary>
        InvalidAmount = 1001,
        /// <summary>
        /// Error code indicating an invalid custom fee value.
        /// </summary>
        InvalidCustomFee = 1002,
        /// <summary>
        /// Error code indicating an invalid Bitcoin address format.
        /// </summary>
        InvalidAddress = 1003,
        /// <summary>
        /// Error code indicating that no Unspent Transaction Outputs (UTXOs) were selected for the transaction.
        /// </summary>
        NoUtxosSelected = 1004,
        /// <summary>
        /// Error code indicating insufficient funds to build the transaction.
        /// </summary>
        InsufficientFunds = 1005,
        /// <summary>
        /// Error code indicating a general failure during transaction building process.
        /// </summary>
        TransactionBuildFailed = 1006
    }
}