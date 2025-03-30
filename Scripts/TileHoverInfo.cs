using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;
using EchoRL;
using Unity.VisualScripting;

public class TileHoverInfo : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;                   // Assign your main camera.
    public TextMeshProUGUI biomeInfoText;       // Your UI TextMeshPro element.

    [Header("Tile Settings")]
    public float tileSize = 1f; // World units per tile. If your tile sprites use 16 pixels per unit, then each tile is 1 unit.

    private EchoRL.Grid simulationGrid;

    private void Start()
    {
        simulationGrid = SimulationManager.Instance.Grid;
    }

    private void Update()
    {
        if (simulationGrid == null)
        {
            simulationGrid = SimulationManager.Instance.Grid;
        }

        // Optional: if the mouse is over UI elements, don't update the info.
        if (UnityEngine.EventSystems.EventSystem.current != null && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
        {
            biomeInfoText.text = "";
            return;
        }

        // Convert the mouse position to world coordinates.
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);

        // Adjust for tile size: divide world coordinates by tileSize.
        int x = Mathf.FloorToInt(worldPos.x / tileSize);
        int y = Mathf.FloorToInt(worldPos.y / tileSize);

        // Check if the grid contains these coordinates.
        if (simulationGrid != null && simulationGrid.IsInBounds(x, y))
        {
            Cell cell = simulationGrid.GetCell(x, y);
            if (cell != null)
            {
                // Get a friendly biome name from your BiomeColorMap.
                //string biomeName = BiomeColorMap.GetBiomeName(cell.Biome);
                //biomeInfoText.text = $"Tile: ({x}, {y})\nBiome: {biomeName}\nAltitude: {cell.Altitude:F2}";
            }
            else
            {
                biomeInfoText.text = "Cell data unavailable.";
            }
        }
        else
        {
            // Clear text if the pointer is outside the grid.
            biomeInfoText.text = "";
        }
    }
}
