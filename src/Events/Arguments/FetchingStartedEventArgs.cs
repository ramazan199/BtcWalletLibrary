namespace BtcWalletLibrary.Events.Arguments
{
    /// <summary>
    /// Event arguments for when a process of  fetching all addresses inside wallet is completed.
    /// This event is published via the <see cref="Interfaces.IEventDispatcher"/> to signal the beginning of a data fetching / synchronizing  operation of the wallet transactions.
    /// Subscribers can listen for this event to be notified when a fetching process commences.
    /// </summary>
    public class FetchingStartedEventArgs : System.EventArgs
    {
        internal FetchingStartedEventArgs()
        {
            // No specific event data to initialize.
        }
    }
}