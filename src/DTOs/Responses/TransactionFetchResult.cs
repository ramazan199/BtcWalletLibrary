namespace BtcWalletLibrary.DTOs.Responses
{
    /// <summary>
    /// Represents the result of a wallet transaction fetching operation.
    /// Fetched transactions are published individually via the EventDispatcher (since they are processed in parallel).
    /// To access complete transaction lists, use the TxHistoryService's Transactions property.
    /// </summary>
    public class TransactionFetchResult
    {
        /// <summary>
        /// Indicates whether network errors occurred during the transaction fetch operation.
        /// </summary>
        public bool HasNetworkErrors { get; }

        internal TransactionFetchResult(bool hasNetworkErrors)
        {
            HasNetworkErrors = hasNetworkErrors;
        }
    }
}