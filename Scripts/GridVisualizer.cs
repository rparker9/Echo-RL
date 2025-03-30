using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace EchoRL
{
    /// <summary>
    /// Specifies the type of data to visualize as an overlay.
    /// </summary>
    public enum OverlayMode
    {
        None,
        Faction
    }

    /// <summary>
    /// Responsible for rendering the tile-based grid map including biomes, features,
    /// and debug overlays such as climate and biome data.
    /// </summary>
    public class GridVisualizer : MonoBehaviour
    {
        [Header("Tilemaps")]
        public Tilemap floorTilemap;         // Main terrain layer (based on biome)
        public Tilemap featureTilemap;       // Objects or obstacles (e.g. walls)

        [Header("Tile Assets")]
        public TileBase[] floorTiles;        // One tile per biome
        public TileBase[] featureTiles;      // Visuals for unwalkable cells

        [Header("Debug Overlays")]
        public Tilemap overlayTilemap;       // Rendered above all other tilemaps
        public TileBase overlayTile;         // Transparent base tile for overlays

        [Header("Debug Settings")]
        public OverlayMode overlayMode = OverlayMode.None;
        public bool showGridLines = true;
        public Color gridLineColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);

        private Grid grid;

        /// <summary>
        /// Initializes the visualizer with the simulation grid.
        /// </summary>
        /// <param name="grid">The simulation grid to visualize.</param>
        public void Initialize(Grid grid)
        {
            this.grid = grid;
            grid.OnCellChanged += OnCellChanged;
            RefreshTilemap();
        }

        /// <summary>
        /// Called when a cell is modified; triggers visual update.
        /// </summary>
        private void OnCellChanged(int x, int y)
        {
            UpdateTile(x, y);
        }

        /// <summary>
        /// Updates the visual representation of a single cell.
        /// This method is still used for individual cell updates.
        /// </summary>
        /// <param name="x">The X coordinate of the cell.</param>
        /// <param name="y">The Y coordinate of the cell.</param>
        private void UpdateTile(int x, int y)
        {
            // Check bounds first to prevent out-of-range errors.
            if (x < 0 || x >= grid.Width || y < 0 || y >= grid.Height)
                return;

            Vector3Int cellPos = new Vector3Int(x, y, 0);
            Cell cell = grid.GetCell(x, y);

            if (cell == null)
            {
                Debug.LogWarning($"Missing cell at ({x},{y})");
                return;
            }
            
            floorTilemap.SetTile(cellPos, floorTiles[0]);

            // Set a feature tile if the cell is not walkable.
            if (!cell.IsWalkable)
            {
                int featureType = GetFeatureType(x, y);
                featureTilemap.SetTile(cellPos, featureTiles[featureType]);
            }
            else
            {
                featureTilemap.SetTile(cellPos, null);
            }

            if (overlayTilemap != null && overlayTile != null && overlayMode != OverlayMode.None)
            {
                // Update overlay tile (if applicable).
            }
        }

        /// <summary>
        /// Fully redraws all tiles in the grid and overlays using bulk updates and asynchronous processing.
        /// </summary>
        public void RefreshTilemap()
        {
            if (grid == null)
            {
                Debug.LogWarning("GridVisualizer: Grid not initialized.");
                return;
            }

            // Cache the bounds for better performance with large grids
            int width = grid.Width;
            int height = grid.Height;

            // Clear all tiles before updating.
            floorTilemap.ClearAllTiles();
            featureTilemap.ClearAllTiles();
            if (overlayTilemap != null)
                overlayTilemap.ClearAllTiles();

            // Temporarily disable the tilemap GameObjects to prevent multiple redraws.
            floorTilemap.gameObject.SetActive(false);
            featureTilemap.gameObject.SetActive(false);
            if (overlayTilemap != null)
                overlayTilemap.gameObject.SetActive(false);

            // Bulk update floor and feature tilemaps.
            RefreshFloorAndFeatureTilemapsBulk();

            // Start asynchronous overlay update
            if (overlayTilemap != null && overlayTile != null && overlayMode != OverlayMode.None)
            {
                StartCoroutine(RefreshOverlayTilemapCoroutine());
            }

            // Re-enable the tilemap GameObjects.
            floorTilemap.gameObject.SetActive(true);
            featureTilemap.gameObject.SetActive(true);
            if (overlayTilemap != null)
                overlayTilemap.gameObject.SetActive(true);
        }

        /// <summary>
        /// Updates the floor and feature tilemaps in bulk using SetTilesBlock.
        /// This avoids per-tile updates and minimizes draw calls.
        /// </summary>
        private void RefreshFloorAndFeatureTilemapsBulk()
        {
            int width = grid.Width;
            int height = grid.Height;
            
            // Create a BoundsInt for the entire grid (z-dimension is 1 for 2D tilemaps).
            BoundsInt region = new BoundsInt(0, 0, 0, width, height, 1);

            // --- Bulk update for Floor Tilemap ---
            TileBase[] floorTileArray = new TileBase[width * height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    Cell cell = grid.GetCell(x, y);
                    if (cell != null)
                    {
                        floorTileArray[index] = floorTiles[0];
                    }
                }
            }

            // Update the entire floor tilemap block in one call.
            floorTilemap.SetTilesBlock(region, floorTileArray);

            // --- Bulk update for Feature Tilemap ---
            TileBase[] featureTileArray = new TileBase[width * height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int index = y * width + x;
                    Cell cell = grid.GetCell(x, y);
                    if (cell != null)
                    {
                        // Set a feature tile if the cell is not walkable.
                        if (!cell.IsWalkable)
                        {
                            int featureIndex = GetFeatureType(x, y);
                            featureTileArray[index] = featureTiles[featureIndex];
                        }
                        else
                        {
                            featureTileArray[index] = null;
                        }
                    }
                }
            }
            // Update the entire feature tilemap block in one call.
            featureTilemap.SetTilesBlock(region, featureTileArray);
        }

        /// <summary>
        /// Asynchronously updates the overlay tilemap's colors in small chunks.
        /// This prevents a long frame hitch when updating a large number of cells.
        /// </summary>
        /// <returns>An IEnumerator for use with StartCoroutine.</returns>
        private IEnumerator RefreshOverlayTilemapCoroutine()
        {
            int width = grid.Width;
            int height = grid.Height;

            // Create a BoundsInt for the entire grid (z-dimension is 1 for 2D tilemaps).
            BoundsInt region = new BoundsInt(0, 0, 0, width, height, 1);

            // Since the overlay tile is the same for every cell, create an array for bulk tile assignment.
            TileBase[] overlayTileArray = new TileBase[width * height];
            
            for (int i = 0; i < overlayTileArray.Length; i++)
            {
                overlayTileArray[i] = overlayTile;
            }

            // Bulk update the overlay tiles.
            overlayTilemap.SetTilesBlock(region, overlayTileArray);

            // Now update each tile's color in small chunks.
            const int chunkSize = 1024;
            int count = 0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Vector3Int cellPos = new Vector3Int(x, y, 0);
                    Cell cell = grid.GetCell(x, y);
                    
                    if (cell == null) 
                        continue;

                    // Get region and faction
                    Region regionOwner = SimulationManager.Instance.RegionGrid.GetRegionAtWorldPos(new Vector2Int(x, y));
                    if (overlayMode == OverlayMode.Faction && regionOwner != null && regionOwner.Faction != null)
                    {
                        var factionComp = regionOwner.Faction.GetComponent<FactionComponent>();
                        overlayTilemap.SetColor(cellPos, factionComp.FactionColor);
                    }
                    else
                    {
                        overlayTilemap.SetColor(cellPos, Color.clear);
                    }

                    count++;
                    
                    // Yield every chunkSize iterations to spread the work over multiple frames.
                    if (count % chunkSize == 0)
                        yield return null;
                }
            }
        }

        /// <summary>
        /// Updates overlay color only for cells in a region.
        /// </summary>
        public void UpdateOverlayForRegion(Region region)
        {
            if (overlayTilemap == null || overlayTile == null || overlayMode != OverlayMode.Faction)
                return;

            Color color = region.Faction != null
                ? region.Faction.GetComponent<FactionComponent>().FactionColor
                : Color.clear;

            foreach (var cell in region.LocalCells)
            {
                if (cell == null) continue;
                Vector3Int pos = new Vector3Int(cell.X, cell.Y, 0);

                overlayTilemap.SetTile(pos, overlayTile);       // Ensure tile exists
                overlayTilemap.SetColor(pos, color);            // Tint with faction color
            }
        }

        /// <summary>
        /// Changes the currently active overlay mode and refreshes the display.
        /// </summary>
        /// <param name="mode">The new overlay mode to use.</param>
        public void SetOverlayMode(OverlayMode mode)
        {
            overlayMode = mode;
            RefreshTilemap();
        }

        /// <summary>
        /// Gets a random feature tile index (can be replaced with smarter logic).
        /// </summary>
        private int GetFeatureType(int x, int y)
        {
            return Random.Range(0, featureTiles.Length);
        }

        /// <summary>
        /// Draws debug grid lines in the scene view for visual debugging.
        /// </summary>
        private void OnDrawGizmos()
        {
            if (grid == null || !showGridLines)
                return;

            Gizmos.color = gridLineColor;

            for (int x = 0; x <= grid.Width; x++)
                Gizmos.DrawLine(new Vector3(x, 0, 0), new Vector3(x, grid.Height, 0));

            for (int y = 0; y <= grid.Height; y++)
                Gizmos.DrawLine(new Vector3(0, y, 0), new Vector3(grid.Width, y, 0));
        }
    }
}
