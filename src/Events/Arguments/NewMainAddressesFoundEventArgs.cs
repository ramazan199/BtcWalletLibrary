namespace BtcWalletLibrary.Events.Arguments
{
    /// <summary>
    /// Event arguments for when new main (receiving) addresses have been found and added to the wallet.
    /// This event is published via the <see cref="Interfaces.IEventDispatcher"/> during initial address discovery or gap limit handling.
    /// Subscribers can listen for this event to be notified when the wallet expands its set of available main addresses.
    /// </summary>
    public class NewMainAddressesFoundEventArgs : NewAddressesFoundEventArgs
    {
        internal NewMainAddressesFoundEventArgs(uint newLastAddrIdx) : base(newLastAddrIdx) { }
    }
}