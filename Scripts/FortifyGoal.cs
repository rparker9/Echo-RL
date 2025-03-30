using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// A fallback goal where the faction builds up its military strength passively.
    /// </summary>
    public class FortifyGoal : FactionGoal
    {
        public override string Name => "Fortify";

        public override bool IsViable(FactionComponent faction)
        {
            // Always viable fallback
            return true;
        }

        public override void Execute(Entity faction, FactionComponent factionData)
        {
            // Increase strength and resources passively
            factionData.MilitaryStrength += 5;
            factionData.Resources += 10;
            Debug.Log($"{factionData.FactionName} fortifies (strength now {factionData.MilitaryStrength})");
        }
    }
}
