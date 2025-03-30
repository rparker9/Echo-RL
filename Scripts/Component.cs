using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// Base abstract class for all components in the Entity Component System.
    /// Components store data and state for entities but contain no game logic.
    /// </summary>
    public class Component
    {
        /// <summary>
        /// Gets or sets the Entity this component is attached to.
        /// </summary>
        public Entity Entity { get; set; }

        /// <summary>
        /// Gets whether this component has been initialized.
        /// Protected to allow derived classes to check initialization status.
        /// </summary>
        protected bool IsInitialized { get; private set; } = false;

        /// <summary>
        /// Initializes the component. Called automatically when the component is added to an entity.
        /// Override this method in derived classes to perform initialization logic.
        /// </summary>
        public virtual void Initialize()
        {
            // Mark the component as initialized
            IsInitialized = true;
        }
    }
}
