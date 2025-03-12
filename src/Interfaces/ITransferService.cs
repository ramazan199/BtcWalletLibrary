using System.Threading.Tasks;
using BtcWalletLibrary.DTOs.Responses;
using Transaction = NBitcoin.Transaction;
using BtcWalletLibrary.Models;

namespace BtcWalletLibrary.Interfaces
{
    /// <summary>
    /// Interface for services responsible for transferring Bitcoin, specifically broadcasting transactions to the network.
    /// </summary>
    public interface ITransferService
    {
        /// <summary>
        /// Broadcasts a Bitcoin transaction to the network asynchronously.
        /// </summary>
        /// <param name="tx">The <see cref="Transaction"/> to broadcast.</param>
        /// <returns>A <see cref="Task{TransferResult}"/> representing the asynchronous operation.
        /// The task result contains a <see cref="TransferResult"/> indicating the success or failure of the broadcast.
        /// Potential failures could be due to network issues or transaction rejection by the network.
        /// </returns>
        Task<TransferResult> BroadcastTransactionAsync(Transaction tx);
    }
}