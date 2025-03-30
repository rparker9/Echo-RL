using System.Collections.Generic;
using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// A goal that allows a faction to conquer enemy territory if stronger.
    /// </summary>
    public class ConquerGoal : FactionGoal
    {
        private const int ConquestCost = 30; // Cost in military strength to conquer

        public override string Name => "Conquer";

        public override bool IsViable(FactionComponent faction)
        {
            if (faction.MilitaryStrength < ConquestCost)
                return false;

            // Check if any neighbors are enemies
            foreach (Vector2Int owned in faction.ControlledRegions)
            {
                foreach (Region neighbor in GetNeighbors(owned))
                {
                    if (neighbor.Faction != null && neighbor.Faction != faction.Entity)
                        return true;
                }
            }

            return false;
        }

        public override void Execute(Entity faction, FactionComponent data)
        {
            RegionGrid grid = SimulationManager.Instance.RegionGrid;

            // Loop through each region controlled by this faction
            foreach (Vector2Int owned in data.ControlledRegions)
            {
                // Get neighbors of this region
                foreach (Region neighbor in GetNeighbors(owned))
                {
                    // If region is owned by a different faction
                    if (neighbor.Faction != null && neighbor.Faction != faction)
                    {
                        var enemyData = neighbor.Faction.GetComponent<FactionComponent>();

                        // Only conquer if stronger than the enemy
                        if (data.MilitaryStrength > enemyData.MilitaryStrength)
                        {
                            // Spend strength to conquer
                            data.MilitaryStrength -= ConquestCost;
                            enemyData.MilitaryStrength -= 10; // Damage to defender

                            // Transfer region ownership
                            AssignFactionToRegion(faction, neighbor);

                            Debug.Log($"{data.FactionName} conquers {neighbor.RegionCoord} from {enemyData.FactionName}!");
                            return; // Only conquer one region per tick
                        }
                        else
                        {
                            Debug.Log($"{data.FactionName} wants to conquer {neighbor.RegionCoord}, but has {data.MilitaryStrength} vs enemy's {enemyData.MilitaryStrength}");
                        }
                    }
                }
            }
        }

        // Returns cardinal neighboring regions
        private List<Region> GetNeighbors(Vector2Int coord)
        {
            RegionGrid grid = SimulationManager.Instance.RegionGrid;
            Vector2Int[] offsets = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };
            List<Region> neighbors = new();

            foreach (var offset in offsets)
            {
                var region = grid.GetRegion(coord.x + offset.x, coord.y + offset.y);
                if (region != null)
                    neighbors.Add(region);
            }

            return neighbors;
        }

        // Assigns a region to the faction and updates visuals
        private void AssignFactionToRegion(Entity faction, Region region)
        {
            region.Faction = faction;
            var data = faction.GetComponent<FactionComponent>();

            // Avoid duplicates
            if (!data.ControlledRegions.Contains(region.RegionCoord))
                data.ControlledRegions.Add(region.RegionCoord);

            // Refresh overlay for visualization
            SimulationManager.Instance.GridVisualizer?.UpdateOverlayForRegion(region);
        }
    }
}
