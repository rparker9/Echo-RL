using TMPro;
using UnityEngine;

namespace EchoRL
{
    public class EntityInspector : MonoBehaviour
    {
        public TextMeshProUGUI individualName;

        private Entity selectedEntity;

        private void Update()
        {
            // Example: on left mouse click, try to select an entity.
            if (Input.GetMouseButtonDown(0))
            {
                SelectEntityUnderMouse();
            }

            // If an entity is selected and it has needs, update UI
            if (selectedEntity != null)
            {
                var individualNameComp = selectedEntity.GetComponent<NameComponent>();

                if (individualNameComp != null)
                {
                    individualName.text = $"Name: {individualNameComp.Name}";
                }
                else
                {
                    Debug.LogError("Selected Entity does not have Name Component!");
                }
            }
        }

        private void SelectEntityUnderMouse()
        {
            // Raycast from the main camera using the mouse position
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);
            if (hit.collider != null)
            {
                EntityReference entityRef = hit.collider.GetComponent<EntityReference>();
                if (entityRef != null)
                {
                    SetSelectedEntity(entityRef.Entity);
                }
            }
        }

        public void SetSelectedEntity(Entity entity)
        {
            selectedEntity = entity;
        }
    }
}
