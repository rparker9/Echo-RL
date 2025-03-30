using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// Represents a single region (chunk) in the world map.
    /// Each region has a grid of local cells.
    /// </summary>
    public class Region
    {
        public Vector2Int RegionCoord { get; private set; }
        public int RegionSize { get; private set; }
        public Cell[,] LocalCells { get; private set; }
        public Entity Faction { get; set; } // Owning faction entity

        public Region(Vector2Int coord, int regionSize)
        {
            RegionCoord = coord;
            RegionSize = regionSize;
            LocalCells = new Cell[regionSize, regionSize];

            for (int x = 0; x < regionSize; x++)
            {
                for (int y = 0; y < regionSize; y++)
                {
                    LocalCells[x, y] = new Cell(x, y);
                }
            }
        }

        /// <summary>
        /// Converts a local cell coordinate in this region to global world coordinates.
        /// </summary>
        public Vector2Int ToWorldCoord(int localX, int localY)
        {
            return new Vector2Int(
                RegionCoord.x * RegionSize + localX,
                RegionCoord.y * RegionSize + localY
            );
        }
    }
}
