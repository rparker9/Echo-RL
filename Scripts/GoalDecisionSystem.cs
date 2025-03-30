using System.Collections.Generic;
using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// Assigns the most viable goal from the registry to each faction.
    /// </summary>
    public class GoalDecisionSystem : ISystem
    {
        public void Initialize() { }

        public void Process(List<Entity> entities)
        {
            foreach (var faction in entities)
            {
                if (!faction.HasComponent<FactionComponent>())
                    continue;

                var data = faction.GetComponent<FactionComponent>();

                // Ensure GoalComponent exists
                GoalComponent goalComp = faction.HasComponent<GoalComponent>()
                    ? faction.GetComponent<GoalComponent>()
                    : faction.AddComponent(new GoalComponent());

                // Pick first viable goal
                foreach (var goal in FactionGoalRegistry.AllGoals)
                {
                    if (goal.IsViable(data))
                    {
                        goalComp.CurrentGoal = goal;
                        Debug.Log($"{data.FactionName} selects goal: {goal.Name}");
                        break;
                    }
                }
            }
        }
    }
}