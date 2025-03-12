using BtcWalletLibrary.Models;

using System.Collections.Generic;
using Coin = NBitcoin.Coin;

namespace BtcWalletLibrary.Interfaces
{
    internal interface ICoinMapper
    {
        List<Coin> UnspentCoinsToNbitcoinCoin(List<UnspentCoin> selectedUnspentCoins);
        List<Coin> UtxoDetailsToNBitcoinCoin(List<UtxoDetailsElectrumx> utxoDetails);
    }
}
