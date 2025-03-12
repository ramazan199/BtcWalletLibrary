using System.Collections.Generic;
using static ElectrumXClient.Response.BlockchainScripthashGetHistoryResponse;

namespace BtcWalletLibrary.DTOs.Responses
{
    internal class AddressTxIdsFetchResult
    {
        public bool HasNetworkError { get; internal set; }
        public List<BlockchainScripthashGetHistoryResult> AddressTxIds { get; internal set; }
    }
}