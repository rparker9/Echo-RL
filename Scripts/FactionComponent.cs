using System.Collections.Generic;
using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// Component that holds core data for a faction entity.
    /// </summary>
    public class FactionComponent : Component
    {
        public string FactionName;
        public Color FactionColor;
        public int Resources = 100;
        public int MilitaryStrength = 10;

        // Region coords this faction currently owns
        public List<Vector2Int> ControlledRegions = new();

        public FactionComponent(string name, Color color)
        {
            FactionName = name;
            FactionColor = color;
        }

        public override void Initialize()
        {
            base.Initialize();
        }
    }
}