using UnityEngine;

namespace EchoRL
{
    public class RegionGrid
    {
        public int RegionWidth { get; private set; }
        public int RegionHeight { get; private set; }
        public int RegionSize { get; private set; }

        private Region[,] regions;

        public RegionGrid(int regionWidth, int regionHeight, int regionSize)
        {
            RegionWidth = regionWidth;
            RegionHeight = regionHeight;
            RegionSize = regionSize;

            regions = new Region[regionWidth, regionHeight];

            for (int x = 0; x < regionWidth; x++)
            {
                for (int y = 0; y < regionHeight; y++)
                {
                    Vector2Int coord = new Vector2Int(x, y);
                    regions[x, y] = new Region(coord, regionSize);
                }
            }
        }

        /// <summary>
        /// Gets the region at a given grid coordinate.
        /// </summary>
        public Region GetRegion(int rx, int ry)
        {
            if (rx >= 0 && rx < RegionWidth && ry >= 0 && ry < RegionHeight)
                return regions[rx, ry];
            return null;
        }

        /// <summary>
        /// Gets the region containing a specific world position.
        /// </summary>
        public Region GetRegionAtWorldPos(Vector2Int worldPos)
        {
            int rx = worldPos.x / RegionSize;
            int ry = worldPos.y / RegionSize;
            return GetRegion(rx, ry);
        }
    }
}
