using BtcWalletLibrary.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace BtcWalletLibrary.Events
{
    /// <summary>
    /// A thread-safe event dispatcher that allows objects (Services, ViewModels etc.) to subscribe to and publish events.
    /// Events are processed asynchronously in a background task.
    /// </summary>
    internal class EventDispatcher : IEventDispatcher
    {
        // Thread-safe queue for pending events.  Tuples store (sender, event type, event data).
        private readonly ConcurrentQueue<(object sender, Type eventType, EventArgs eventData)> _eventQueue = new();
        // Thread-safe dictionary to store event handlers. Key is event type, value is combined delegate.
        private readonly ConcurrentDictionary<Type, Delegate> _eventHandlers = new();
        private readonly CancellationTokenSource _cancellationTokenSource = new();
        // Background task that processes events from the queue.
        private readonly Task _eventProcessingTask;

        public EventDispatcher()
        {
            _eventProcessingTask = Task.Run(() => ProcessEventsAsync(_cancellationTokenSource.Token));
        }

        public void Subscribe<TEventArgs>(EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
        {
            _eventHandlers.AddOrUpdate(
                typeof(TEventArgs),
                handler,
                (_, existingHandlers) => Delegate.Combine(existingHandlers, handler));
        }

        public void Unsubscribe<TEventArgs>(EventHandler<TEventArgs> handler) where TEventArgs : EventArgs
        {
            if (!_eventHandlers.TryGetValue(typeof(TEventArgs), out var existingHandlers)) return;
            var newHandlers = Delegate.Remove(existingHandlers, handler);
            if (newHandlers != null)
                _eventHandlers[typeof(TEventArgs)] = newHandlers;
            else
                _eventHandlers.TryRemove(typeof(TEventArgs), out _);
        }

        public void Publish<TEventArgs>(object sender, TEventArgs eventData) where TEventArgs : EventArgs
        {
            _eventQueue.Enqueue((sender, typeof(TEventArgs), eventData));
        }

        // Processes events from the queue in a background task
        private async Task ProcessEventsAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_eventQueue.TryDequeue(out var eventTuple))
                {
                    var (sender, eventType, eventData) = eventTuple;

                    if (_eventHandlers.TryGetValue(eventType, out var handlers))
                    {
                        foreach (var handler in handlers.GetInvocationList())
                        {
                            // Dynamically invoke the handler with the correct type and sender
                            var handlerType = typeof(EventHandler<>).MakeGenericType(eventType);
                            var invokeMethod = handlerType.GetMethod("Invoke");
                            invokeMethod?.Invoke(handler, new[] { sender, eventData });
                        }
                    }
                }

                await Task.Delay(100, cancellationToken); // Reduce CPU usage
            }
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            _eventProcessingTask.Wait();
        }
    }
}
