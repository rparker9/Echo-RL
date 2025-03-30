using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// Base class for all faction goals. Each goal knows how to evaluate itself and execute behavior.
    /// </summary>
    public abstract class FactionGoal
    {
        /// <summary>
        /// A human-readable name for the goal.
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// Determines if this goal is viable for the given faction.
        /// </summary>
        public abstract bool IsViable(FactionComponent faction);

        /// <summary>
        /// Executes the goal's behavior for the given faction entity.
        /// </summary>
        public abstract void Execute(Entity faction, FactionComponent factionData);
    }
}