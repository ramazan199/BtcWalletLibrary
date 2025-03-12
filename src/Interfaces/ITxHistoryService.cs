using System.Collections.Generic;
using System.Threading.Tasks;
using BtcWalletLibrary.DTOs.Responses;
using BtcWalletLibrary.Models;

namespace BtcWalletLibrary.Interfaces
{
    /// <summary>
    /// Interface for services responsible for managing and synchronizing the wallet's transaction history.
    /// This service provides access to the list of transactions and functionality to synchronize transaction history with the blockchain.
    /// </summary>
    public interface ITxHistoryService
    {
        /// <summary>
        /// Gets the list of transactions currently stored in the wallet's history.
        /// This list is updated during synchronization operations. Note that during transaction fetching, individual transactions and updates are published in real-time via the <see cref="IEventDispatcher"/>, not directly through this property for immediate consumption.
        /// </summary>
        IReadOnlyList<Transaction> Transactions { get; }

        /// <summary>
        /// Asynchronously synchronizes the wallet's transaction history with the blockchain (for main and change addresses).
        /// During synchronization, new transactions related to the wallet's addresses are fetched and added to the history.
        ///
        /// **Real-time Updates via Events:**
        /// As the synchronization process runs, the <see cref="IEventDispatcher"/> publishes the following events to provide real-time updates and process lifecycle notifications:
        /// <list type="bullet">
        ///     <item><see cref="Events.Arguments.FetchingStartedEventArgs"/>:  Published when the transaction fetching process begins.</item>
        ///     <item><see cref="Events.Arguments.TransactionAddedEventArgs"/>: For each newly discovered transaction.</item>
        ///     <item><see cref="Events.Arguments.TransactionConfirmedEventArgs"/>: When a transaction reaches a confirmation threshold.</item>
        ///     <item><see cref="Events.Arguments.TransactionDateUpdatedEventArgs"/>: If a transaction's date information is updated.</item>
        ///     <item><see cref="Events.Arguments.FetchingCompletedEventArgs"/>: Published when the entire transaction fetching and synchronization process is complete.</item>
        /// </list>
        ///
        /// These events allow subscribers to receive immediate notifications of transaction activity and the overall synchronization status.
        /// The <see cref="Transactions"/> property provides access to the complete, synchronized transaction history *after* the synchronization process finishes.
        /// </summary>
        /// <returns>A <see cref="Task{TransactionFetchResult}"/> representing the asynchronous operation.
        /// The task result contains a <see cref="TransactionFetchResult"/> indicating whether the synchronization process encountered network errors.
        /// To access the updated list of transactions after synchronization, use the <see cref="Transactions"/> property.
        /// </returns>
        Task<TransactionFetchResult> SyncTransactionsAsync();
    }
}