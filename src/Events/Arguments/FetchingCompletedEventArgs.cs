namespace BtcWalletLibrary.Events.Arguments
{
    /// <summary>
    /// Event arguments for when a fetching process of all addresses inside wallet is completed.
    /// This event is published via the <see cref="Interfaces.IEventDispatcher"/> to signal the completion of a data fetching / synchronizing  operation of the wallet transactions.
    /// Subscribers can listen for this event to be notified when a fetching process has finished.
    /// !!! Note that you need to wait for this event to be raised before sending a new transaction, to ensure that the wallet is up-to-date.
    /// </summary>
    public class FetchingCompletedEventArgs : System.EventArgs
    {
        
        internal FetchingCompletedEventArgs()
        {
            // No specific event data to initialize.
        }
    }
}