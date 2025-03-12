namespace BtcWalletLibrary.Events.Arguments
{
    /// <summary>
    /// Event arguments for when the wallet's balance is updated.
    /// This event is published via the <see cref="Interfaces.IEventDispatcher"/> **after the wallet's balance has been recalculated.**
    /// Balance recalculation is triggered by processing transaction fetching events, such as <see cref="AddressTxsFetchedEventArgs"/>.
    /// The event is published by services like the <see cref="Interfaces.IBalanceService"/> when it has updated the wallet's balance based on new transaction data.
    /// Subscribers can listen for this event to update their UI or application state to reflect the latest confirmed and unconfirmed wallet balances.
    /// </summary>
    public class BalanceUpdatedEventArgs : System.EventArgs
    {
        /// <summary>
        /// Gets the new confirmed balance of the wallet.
        /// </summary>
        public double NewConfirmedBalance { get; }
        /// <summary>
        /// Gets the new unconfirmed balance of the wallet.
        /// </summary>
        public double NewUnconfirmedBalance { get; }

        internal BalanceUpdatedEventArgs(double newConfirmedBalance, double newUnconfirmedBalance)
        {
            NewConfirmedBalance = newConfirmedBalance;
            NewUnconfirmedBalance = newUnconfirmedBalance;
        }
    }
}