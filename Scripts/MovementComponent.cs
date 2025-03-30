using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace EchoRL
{
    /// <summary>
    /// Component that handles movement data for an entity.
    /// Stores destination and movement speed information.
    /// </summary>
    public class MovementComponent : Component
    {
        /// <summary>
        /// Gets or sets the destination position the entity is moving towards.
        /// </summary>
        public Vector2Int Destination { get; set; }

        /// <summary>
        /// Gets or sets the movement speed of the entity.
        /// </summary>
        public float MovementSpeed { get; set; } = 1.0f;

        /// <summary>
        /// Gets whether the entity is currently moving.
        /// Returns true if the current position is different from the destination.
        /// </summary>
        public bool IsMoving => IsInitialized ? Position.Position != Destination : false;

        /// <summary>
        /// Reference to the PositionComponent of the entity.
        /// </summary>
        private PositionComponent Position;

        /// <summary>
        /// Initializes the component by retrieving the required PositionComponent.
        /// </summary>
        /// <exception cref="Exception">Thrown when the entity doesn't have a PositionComponent.</exception>
        public override void Initialize()
        {
            // Get the PositionComponent from the entity
            Position = Entity.GetComponent<PositionComponent>();

            // Ensure the PositionComponent exists
            if (Position == null)
                throw new Exception("MovementComponent requires a PositionComponent");

            // Set the initial destination to the current position
            Destination = Position.Position;

            // Call the base Initialize method to set IsInitialized
            base.Initialize();
        }
    }
}
