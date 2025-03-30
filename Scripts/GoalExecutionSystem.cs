using System.Collections.Generic;
using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// Executes the current goal for each faction, if one is assigned.
    /// </summary>
    public class GoalExecutionSystem : ISystem
    {
        public void Initialize() { }

        public void Process(List<Entity> entities)
        {
            foreach (var faction in entities)
            {
                if (!faction.HasComponent<FactionComponent>() || !faction.HasComponent<GoalComponent>())
                    continue;

                var goal = faction.GetComponent<GoalComponent>().CurrentGoal;
                var data = faction.GetComponent<FactionComponent>();

                goal?.Execute(faction, data);
            }
        }
    }
}