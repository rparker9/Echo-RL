using System.Collections.Generic;
using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// A goal that allows a faction to expand into unclaimed territory by spending resources.
    /// </summary>
    public class ExpandGoal : FactionGoal
    {
        private const int ExpansionCost = 50; // Cost in resources to expand

        public override string Name => "Expand";

        public override bool IsViable(FactionComponent faction)
        {
            if (faction.Resources < ExpansionCost)
                return false;

            // Check if any unclaimed neighbors exist
            foreach (Vector2Int owned in faction.ControlledRegions)
            {
                foreach (Region neighbor in GetNeighbors(owned))
                {
                    if (neighbor.Faction == null)
                        return true;
                }
            }

            return false;
        }

        public override void Execute(Entity faction, FactionComponent data)
        {
            RegionGrid grid = SimulationManager.Instance.RegionGrid;

            // Loop through each owned region
            foreach (Vector2Int owned in data.ControlledRegions)
            {
                // Check all neighbors for unclaimed region
                foreach (Region neighbor in GetNeighbors(owned))
                {
                    if (neighbor.Faction == null)
                    {
                        // Spend resources and claim region
                        data.Resources -= ExpansionCost;
                        AssignFactionToRegion(faction, neighbor);
                        Debug.Log($"{data.FactionName} expands into {neighbor.RegionCoord} (cost {ExpansionCost})");
                        return; // Only expand one region per tick
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
