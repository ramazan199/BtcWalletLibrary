using System.Threading.Tasks;
using BtcWalletLibrary.Models;
using Transaction = NBitcoin.Transaction;


namespace BtcWalletLibrary.Interfaces
{
    /// <summary>
        /// Interface for mapping Bitcoin transaction data between different formats and models.
        /// This interface defines methods for converting transaction representations from external libraries (like NBitcoin)
        /// into the library's internal models optimized for storage and processing (<see cref="Models.Transaction"/>) and for working with UTXOs.
        /// Implementations of this interface are responsible for handling the data transformation logic.
        /// </summary>
    public interface ITxMapper
    {
        /// <summary>
        /// Maps a <see cref="UtxoDetailsElectrumx"/> object (representing UTXO details from Electrumx) to an NBitcoin <see cref="Coin"/> object.
        /// This method converts UTXO data retrieved from an Electrumx server into the NBitcoin <see cref="Coin"/> format,
        /// which is commonly used within the NBitcoin library for representing transaction inputs and outputs.
        /// </summary>
        /// <param name="utxo">The <see cref="UtxoDetailsElectrumx"/> object containing UTXO details from Electrumx.</param>
        /// <returns>An NBitcoin <see cref="Coin"/> object representing the UTXO, suitable for use with NBitcoin transaction operations.</returns>
        Coin UtxoToCoin(UtxoDetailsElectrumx utxo);

        /// <summary>
                /// Asynchronously maps an NBitcoin <see cref="Transaction"/> object to a <see cref="global::BtcWalletLibrary.Models.Transaction"/> object.
                /// This method is used to convert a transaction representation from the NBitcoin library into the library's internal
                /// format suitable for storage in the wallet's transaction history and further processing within the application.
                /// </summary>
                /// <param name="tx">The NBitcoin <see cref="Transaction"/> object to be mapped.</param>
                /// <returns>A <see cref="Task{TransactionForStorage}"/> representing the asynchronous operation.
                /// The task result is a <see cref="global::BtcWalletLibrary.Models.Transaction"/> object that is the mapped representation of the input NBitcoin transaction.
                /// </returns>
        Task<Models.Transaction> NBitcoinTxToBtcTxForStorage(Transaction tx);

        
    }
}