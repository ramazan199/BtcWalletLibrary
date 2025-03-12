using NBitcoin;
using System.Collections.Generic;

namespace BtcWalletLibrary.Interfaces
{
    internal interface ISigningKeyService
    {
        List<Key> PrepareSigningKeys(IEnumerable<Coin> selectedUnspentCoins);
    }
}