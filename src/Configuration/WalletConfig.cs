namespace BtcWalletLibrary.Configuration
{
    /// <summary>
    /// Represents the overall wallet configuration.
    /// </summary>
    public class WalletConfig
    {
        /// <summary>
        /// Configuration section for the Bitcoin node.
        /// </summary>
        public NodeConfigSection NodeConfiguration { get; set; }
        /// <summary>
        /// Configuration section for the Blockchain transaction fee API.
        /// </summary>
        public BlockchainTxFeeApiConfigSection BlockchainTxFeeApi { get; set; }
    }


    /// <summary>
        /// Represents the configuration section for the Bitcoin node.
        /// </summary>
    public class NodeConfigSection
    {
        /// <summary>
        /// URL of the Bitcoin node.
        /// </summary>
        public string Url { get; set; }
        /// <summary>
        /// Port number of the Bitcoin node.
        /// </summary>
        public int Port { get; set; }
        /// <summary>
        /// Indicates whether to use SSL for connecting to the Bitcoin node.
        /// </summary>
        public bool UseSsl { get; set; }
        /// <summary>
        /// Bitcoin network type (Main, TestNet, RegTest).
        /// </summary>
        public NetworkType Network { get; set; }
        /// <summary>
        /// Maximum range of empty addresses to scan during address discovery.
        /// </summary>
        public int MaxRangeEmptyAddr { get; set; }
    }

    /// <summary>
    /// Represents the configuration section for the Blockchain transaction fee API.
    /// </summary>
    public class BlockchainTxFeeApiConfigSection
    {
        /// <summary>
        /// Base URL of the Blockchain transaction fee API.
        /// </summary>
        public string BaseUrl { get; set; }
        /// <summary>
        /// Path to the fee endpoint in the Blockchain transaction fee API.
        /// </summary>
        public string FeePath { get; set; }
    }

    /// <summary>
    /// Enum representing Bitcoin network types.
    /// </summary>
    public enum NetworkType
    {
        Main,
        TestNet,
        RegTest
    }
}