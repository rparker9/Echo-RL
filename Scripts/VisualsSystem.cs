using System.Collections.Generic;
using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// Visual update system
    /// </summary>
    public class VisualsSystem : ISystem
    {
        public void Initialize() { }

        public void Process(List<Entity> entities)
        {
            foreach (var entity in entities)
            {
                // Entity needs Position and Visual components for processing
                if (!entity.HasComponent<PositionComponent>() || !entity.HasComponent<VisualsComponent>())
                    continue;

                PositionComponent position = entity.GetComponent<PositionComponent>();
                VisualsComponent visuals = entity.GetComponent<VisualsComponent>();

                visuals.UpdatePosition(position.Position);
            }
        }
    }
}
