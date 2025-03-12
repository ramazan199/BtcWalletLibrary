using NBitcoin;
using System.Collections.Generic;
using System.Threading.Tasks;
using Coin = NBitcoin.Coin;
using BtcWalletLibrary.DTOs.Responses;

namespace BtcWalletLibrary.Interfaces
{
    /// <summary>
    /// Interface for services responsible for calculating Bitcoin transaction fees.
    /// This service provides functionalities to estimate transaction fees based on different parameters and retrieve fee recommendations.
    /// </summary>
    public interface ITxFeeService
    {
        /// <summary>
        /// Calculates the transaction fee for a given set of inputs and outputs.
        /// </summary>
        /// <param name="selectedUnspentCoins">List of <see cref="Coin"/> coins selected as inputs for the transaction.</param>
        /// <param name="signingKeys">List of <see cref="Key"/> keys corresponding to the input coins, used for fee calculation based on signature size.</param>
        /// <param name="destinationAddress">The <see cref="IDestination"/> address to which the funds are being sent. Used for output size estimation.</param>
        /// <param name="amount">The amount of Bitcoin to send. Used to determine the output value.</param>
        /// <returns>A <see cref="Money"/> object representing the calculated transaction fee.</returns>
        Money CalculateTransactionFee(List<Coin> selectedUnspentCoins, List<Key> signingKeys,
      IDestination destinationAddress, Money amount);

        /// <summary>
        /// Asynchronously retrieves the recommended transaction fee rate from an external source.
        /// </summary>
        /// <returns>A <see cref="Task{TxFeeResult}"/> representing the asynchronous operation.
        /// The task result contains a <see cref="TxFeeResult"/> object with recommended fee rates in satoshis per byte.
        /// This method might involve network requests and could potentially fail, resulting in a failed task or exceptions encapsulated in the <see cref="TxFeeResult"/>.</returns>
        Task<TxFeeResult> GetRecommendedBitFeeAsync();
    }
}