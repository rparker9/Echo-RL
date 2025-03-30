using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// Base class for all game events
    /// </summary>
    public abstract class GameEvent
    {
        public Entity Sender { get; protected set; }

        public GameEvent(Entity sender)
        {
            Sender = sender;
        }
    }
}
