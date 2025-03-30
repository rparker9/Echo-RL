using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// Position component
    /// </summary>
    public class PositionComponent : Component
    {
        public Vector2Int Position { get; set; }

        public PositionComponent(int x, int y)
        {
            Position = new Vector2Int(x, y);
        }

        public override void Initialize()
        {
            SimulationManager.Instance.Grid.SetOccupied(Position.x, Position.y, true);

            base.Initialize();
        }
    }
}
