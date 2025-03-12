using NBitcoin;
using System;
using System.Collections.Generic;
using System.Text;

namespace BtcWalletLibrary.Interfaces
{
    internal interface ICoinSelectionService
    {
        bool AutoSelectCoinsWithNoFeeSelected(out List<Coin> coinsToSpend, Money amount, out Money fee,
           List<Coin> unspentCoins, IDestination destinationAddress);
        bool AutoSelectCoinsWithFeeSeleceted(out List<Coin> coinsToSpend, Money amount, Money fee, List<Coin> unspentCoins);
    }
}
