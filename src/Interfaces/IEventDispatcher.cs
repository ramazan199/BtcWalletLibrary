using System;

namespace BtcWalletLibrary.Interfaces
{
    /// <summary>
    /// Interface for an event dispatcher, enabling loosely coupled communication between components.
    /// This interface allows for  subscribing/unsubscribing to specific event types in your ViewModels or other classes.
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// Publishes an event of type TEventArgs to all subscribers.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event arguments, must inherit from EventArgs.</typeparam>
        /// <param name="sender">The object that is raising the event.</param>
        /// <param name="eventData">The event arguments to be passed to subscribers.</param>
        /// <exception cref="ArgumentNullException">Thrown if eventData is null.</exception>
        void Publish<TEventArgs>(object sender, TEventArgs eventData) where TEventArgs : EventArgs;

        /// <summary>
        /// Subscribes a handler to events of type TEventArgs.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event arguments to subscribe to, must inherit from EventArgs.</typeparam>
        /// <param name="handler">The event handler delegate to be invoked when an event of type TEventArgs is published.</param>
        void Subscribe<TEventArgs>(EventHandler<TEventArgs> handler) where TEventArgs : EventArgs;

        /// <summary>
        /// Unsubscribes a handler from events of type TEventArgs.
        /// </summary>
        /// <typeparam name="TEventArgs">The type of the event arguments to unsubscribe from, must inherit from EventArgs.</typeparam>
        /// <param name="handler">The event handler delegate to be removed from the subscribers list for events of type TEventArgs.</param>
        void Unsubscribe<TEventArgs>(EventHandler<TEventArgs> handler) where TEventArgs : EventArgs;
    }
}