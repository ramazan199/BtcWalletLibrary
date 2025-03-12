namespace BtcWalletLibrary.Events.Arguments
{
    /// <summary>
    /// Event arguments base class for events indicating that new addresses have been found (derived and added to the wallet's address lists).
    /// This is an abstract base class; concrete implementations like <see cref="NewMainAddressesFoundEventArgs"/> and <see cref="NewChangeAddressesFoundEventArgs"/> are used for specific address types.
    /// These events are published via the <see cref="Interfaces.IEventDispatcher"/>
    /// </summary>
    public abstract class NewAddressesFoundEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the index of the last newly found address in the respective address list (main or change).
        /// This index typically represents the highest index that has been derived and added during the address discovery process.
        /// </summary>
        public uint NewLastAddrIdx { get; }

        protected NewAddressesFoundEventArgs(uint newIdx)
        {
            NewLastAddrIdx = newIdx;
        }
    }
}