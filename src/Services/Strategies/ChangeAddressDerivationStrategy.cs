using NBitcoin;
using System;
using BtcWalletLibrary.Events.Arguments;
using BtcWalletLibrary.Interfaces;

namespace BtcWalletLibrary.Services.Strategies
{
    internal class ChangeAddressDerivationStrategy : IAddressDerivationStrategy
    {
        private readonly IAddressService _btcAddressService;
        private readonly IEventDispatcher _eventDispatcher;
        public ChangeAddressDerivationStrategy(IAddressService btcAddressService, IEventDispatcher eventDispatcher)
        {
            _btcAddressService = btcAddressService ?? throw new ArgumentNullException(nameof(btcAddressService));
            _eventDispatcher = eventDispatcher ?? throw new ArgumentNullException(nameof(eventDispatcher));
        }

        public long LastKnownIdx => _btcAddressService.LastChangeAddrIdx;
        public BitcoinAddress DeriveAddr(uint index) => _btcAddressService.DerivChangeAddr(index);
        public void PublishNewFoundAddr(object sender, uint lastAddrIdx) => _eventDispatcher.Publish(this, new NewChangeAddressesFoundEventArgs(lastAddrIdx));
        public void AddAddressToQueriedAddrLst(uint index) => _btcAddressService.AddAddressToQueriedChangeAddrLst(DeriveAddr(index));
    }
}