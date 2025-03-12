namespace BtcWalletLibrary.Events.Arguments
{
    /// <summary>
    /// Event arguments for when new change addresses have been found and added to the wallet.
    /// This event is published via the <see cref="Interfaces.IEventDispatcher"/>  during initial address discovery or gap limit handling.
    /// Subscribers can listen for this event to be notified when the wallet expands its set of available change addresses.
    /// </summary>
    public class NewChangeAddressesFoundEventArgs : NewAddressesFoundEventArgs
    {
        internal NewChangeAddressesFoundEventArgs(uint newLastAddrIdx) : base(newLastAddrIdx) { }
    }
}