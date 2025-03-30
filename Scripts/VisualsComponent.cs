using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// Visual representation component
    /// </summary>
    public class VisualsComponent : Component
    {
        public GameObject gameObject { get; private set; }
        public SpriteRenderer spriteRenderer { get; private set; }

        public VisualsComponent(GameObject prefab, Transform parent = null)
        {
            gameObject = GameObject.Instantiate(prefab, parent);
            spriteRenderer = gameObject.GetComponentInChildren<SpriteRenderer>();
        }

        public void UpdatePosition(Vector2 position)
        {
            gameObject.transform.position = new Vector3(position.x, position.y, 0);
        }
    }
}
