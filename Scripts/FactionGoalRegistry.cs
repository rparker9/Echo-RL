using System.Collections.Generic;
using UnityEngine;

namespace EchoRL
{
    public static class FactionGoalRegistry
    {
        public static List<FactionGoal> AllGoals = new List<FactionGoal>
        {
            new ConquerGoal(),
            new ExpandGoal(),
            new FortifyGoal()
        };
    }
}
