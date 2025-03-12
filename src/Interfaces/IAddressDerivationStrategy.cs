using NBitcoin;

namespace BtcWalletLibrary.Interfaces
{
    internal interface IAddressDerivationStrategy
    {
        long LastKnownIdx { get; }
        BitcoinAddress DeriveAddr(uint index);
        void PublishNewFoundAddr(object sender, uint lastAddrIdx);
        void AddAddressToQueriedAddrLst(uint index);
    }
}