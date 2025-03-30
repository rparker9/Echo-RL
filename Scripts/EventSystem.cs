using System.Collections.Generic;
using System;
using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// This system allows different parts of the game (like enemies, items, or the player)
    /// to communicate by sending and receiving events.
    /// It uses the Singleton pattern, meaning only one instance of this system exists at a time.
    /// </summary>
    public class EventSystem
    {
        /// <summary>
        /// The one and only instance of the EventSystem.
        /// </summary>
        private static EventSystem instance;

        /// <summary>
        /// Access point to the EventSystem instance.
        /// If it doesn't exist yet, we create it here.
        /// </summary>
        public static EventSystem Instance
        {
            get
            {
                if (instance == null)
                    instance = new EventSystem();
                return instance;
            }
        }

        /// <summary>
        /// This dictionary keeps track of who wants to listen to which type of event.
        /// For example, enemies might listen for a "PlayerMoved" event.
        /// </summary>
        private Dictionary<Type, List<Action<GameEvent>>> eventSubscriptions =
            new Dictionary<Type, List<Action<GameEvent>>>();

        /// <summary>
        /// A list of events waiting to be processed right now, during this frame or tick.
        /// </summary>
        private Queue<GameEvent> eventQueue = new Queue<GameEvent>();

        /// <summary>
        /// A list of events that will be processed in the next game frame or tick.
        /// Useful for delaying reactions until the next update.
        /// </summary>
        private Queue<GameEvent> nextTickQueue = new Queue<GameEvent>();

        /// <summary>
        /// This lets a system (like AI or UI) say: \"Hey, I want to know when this event happens.\"
        /// </summary>
        /// <typeparam name="T">The type of event to listen for.</typeparam>
        /// <param name="handler">The function to call when that event happens.</param>
        public void Subscribe<T>(Action<GameEvent> handler) where T : GameEvent
        {
            Type eventType = typeof(T);

            // If we’ve never seen this event type before, set up a new list for it
            if (!eventSubscriptions.ContainsKey(eventType))
                eventSubscriptions[eventType] = new List<Action<GameEvent>>();

            // Add this handler (listener) to the list
            eventSubscriptions[eventType].Add(handler);
        }

        /// <summary>
        /// This stops a system from listening to an event.
        /// </summary>
        /// <typeparam name="T">The type of event to stop listening for.</typeparam>
        /// <param name="handler">The specific function that should be removed.</param>
        public void Unsubscribe<T>(Action<GameEvent> handler) where T : GameEvent
        {
            Type eventType = typeof(T);

            // If we’re tracking this type of event, remove the handler from its list
            if (eventSubscriptions.ContainsKey(eventType))
                eventSubscriptions[eventType].Remove(handler);
        }

        /// <summary>
        /// Instantly sends out an event to all systems that are listening for it.
        /// This is like shouting: \"Hey, this thing just happened!\" and everyone who's interested hears it.
        /// </summary>
        /// <param name="gameEvent">The event to send out.</param>
        public void RaiseEvent(GameEvent gameEvent)
        {
            Type eventType = gameEvent.GetType();

            // If anyone is listening for this event, call their handlers
            if (eventSubscriptions.ContainsKey(eventType))
            {
                foreach (var handler in eventSubscriptions[eventType])
                {
                    handler(gameEvent); // Notify each listener
                }
            }
        }

        /// <summary>
        /// Adds an event to the queue to be processed later during this update/tick.
        /// This is useful when events need to be collected and processed in order.
        /// </summary>
        /// <param name="gameEvent">The event to add to the queue.</param>
        public void QueueEvent(GameEvent gameEvent)
        {
            eventQueue.Enqueue(gameEvent);
        }

        /// <summary>
        /// Adds an event to the \"next tick\" queue — it won't be handled until the next frame.
        /// This helps prevent issues when an event triggers something too early.
        /// </summary>
        /// <param name="gameEvent">The event to delay until the next tick.</param>
        public void QueueEventNextTick(GameEvent gameEvent)
        {
            nextTickQueue.Enqueue(gameEvent);
        }

        /// <summary>
        /// This method is called at the end of each tick (or game frame) to process all events.
        /// First, it handles all current events, then it prepares the next set of delayed events.
        /// </summary>
        public void ProcessEvents()
        {
            // Handle everything in the current event queue
            while (eventQueue.Count > 0)
            {
                GameEvent gameEvent = eventQueue.Dequeue();
                RaiseEvent(gameEvent); // Send it out to listeners
            }

            // Move the \"next tick\" events into the main queue so they're ready next time
            while (nextTickQueue.Count > 0)
            {
                eventQueue.Enqueue(nextTickQueue.Dequeue());
            }
        }
    }
}
