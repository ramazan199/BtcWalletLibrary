using NBitcoin;

namespace BtcWalletLibrary.Interfaces
{
    /// <summary>
    /// Interface for common wallet services providing access to shared configurations and resources.
    /// </summary>
    public interface ICommonService
    {
        /// <summary>
        /// Gets the extended private key used as the parent for deriving change addresses.
        /// </summary>
        ExtKey ChangeAddressesParentExtKey { get; }
        /// <summary>
        /// Gets the extended public key derived from the change addresses parent extended private key.
        /// This public key can be used for watch-only wallets or sharing purposes.
        /// </summary>
        ExtPubKey ChangeAddressesParentExtPubKey { get; }
        /// <summary>
        /// Gets the extended private key used as the parent for deriving main (receiving) addresses.
        /// </summary>
        ExtKey MainAddressesParentExtKey { get; }
        /// <summary>
        /// Gets the extended public key derived from the main addresses parent extended private key.
        /// This public key can be used for watch-only wallets or sharing purposes.
        /// </summary>
        ExtPubKey MainAddressesParentExtPubKey { get; }
        /// <summary>
        /// Gets the Bitcoin network the wallet is configured to operate on (e.g., Mainnet, Testnet).
        /// </summary>
        Network BitcoinNetwork { get; }
        /// <summary>
        /// Gets the maximum range of consecutive empty addresses to check during address discovery.
        /// This is used to determine when to stop searching for used addresses.
        /// </summary>
        int MaxEmptyAddrRange { get; }
    }
}