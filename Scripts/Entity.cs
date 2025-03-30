using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// Core entity class in the Entity Component System.
    /// Entities are containers for components and have no behavior of their own.
    /// </summary>
    public class Entity
    {
        /// <summary>
        /// Gets the unique identifier for this entity.
        /// </summary>
        public int ID { get; private set; }

        /// <summary>
        /// Dictionary storing all components attached to this entity, indexed by their type.
        /// </summary>
        private Dictionary<Type, Component> components = new Dictionary<Type, Component>();

        /// <summary>
        /// Creates a new entity with the specified ID.
        /// </summary>
        /// <param name="id">Unique identifier for this entity.</param>
        public Entity(int id)
        {
            ID = id;
        }

        /// <summary>
        /// Adds a component to this entity.
        /// </summary>
        /// <typeparam name="T">The type of component to add.</typeparam>
        /// <param name="component">The component instance to add.</param>
        /// <returns>The added component instance.</returns>
        public T AddComponent<T>(T component) where T : Component
        {
            // Store the component in the dictionary with its type as the key
            components[typeof(T)] = component;

            // Set the component's entity reference to this entity
            component.Entity = this;

            // Initialize the component immediately
            component.Initialize();

            return component;
        }

        /// <summary>
        /// Gets a component of the specified type from this entity.
        /// </summary>
        /// <typeparam name="T">The type of component to retrieve.</typeparam>
        /// <returns>The component if found, null otherwise.</returns>
        public T GetComponent<T>() where T : Component
        {
            // Try to get the component from the dictionary
            if (components.TryGetValue(typeof(T), out Component component))
                return (T)component;

            // Return null if the component wasn't found
            return null;
        }

        /// <summary>
        /// Checks if this entity has a component of the specified type.
        /// </summary>
        /// <typeparam name="T">The type of component to check for.</typeparam>
        /// <returns>True if the entity has the component, false otherwise.</returns>
        public bool HasComponent<T>() where T : Component
        {
            return components.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Removes a component of the specified type from this entity.
        /// </summary>
        /// <typeparam name="T">The type of component to remove.</typeparam>
        public void RemoveComponent<T>() where T : Component
        {
            // Remove the component if it exists
            if (components.ContainsKey(typeof(T)))
                components.Remove(typeof(T));
        }
    }
}
