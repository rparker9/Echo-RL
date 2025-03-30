using System;
using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// Represents a single cell in the grid world
    /// Stores terrain information and pathfinding data
    /// </summary>
    public class Cell : IComparable<Cell>
    {
        /// <summary>
        /// 
        /// </summary>
        public int X {  get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public int Y { get; private set; }
        
        /// <summary>
        /// 
        /// </summary>
        public bool IsWalkable { get; set; } = true;

        /// <summary>
        /// Gets or sets the priority of this cell for pathfinding algorithms.
        /// </summary>
        public float Priority { get; set; }

        /// <summary>
        /// Gets or sets the movement cost for traversing this cell.
        /// Higher values make the cell more expensive to move through.
        /// </summary>
        public float MovementCost { get; set; } = 1.0f;

        public BiomeType Biome { get; private set; }

        public float Altitude { get; private set; }

        public float Humidity { get; private set; }

        public Cell(int x, int y)
        {
            X = x; Y = y;
        }

        public void SetBiome(BiomeType biome)
        {
            Biome = biome;
        }

        public void SetAltitude(float value)
        {
            Altitude = value;
        }

        public void SetHumidity(float value)
        {
            Humidity = value;
        }

        /// <summary>
        /// Compares this cell with another cell based on priority.
        /// Used for priority queues in pathfinding.
        /// </summary>
        /// <param name="other">The other cell to compare with.</param>
        /// <returns>-1 if this cell has lower priority, 0 if equal, 1 if higher priority.</returns>
        public int CompareTo(Cell other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }
}
