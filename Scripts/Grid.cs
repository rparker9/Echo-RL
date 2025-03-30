using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// Represents a grid of walkable/non-walkable cells for pathfinding
    /// </summary>
    public class Grid
    {
        public int Width { get; private set; }
        public int Height { get; private set; }

        public event Action<int, int> OnCellChanged;

        private Cell[,] cells;

        // Cache for recently calculated paths
        private Dictionary<Vector2Int, List<Vector2Int>> pathCache = new();
        private int pathCacheMaxSize = 1000;

        private HashSet<Vector2Int> occupiedCells = new();

        public Grid(int width, int height)
        {
            Width = width;
            Height = height;
            cells = new Cell[width, height];

            // Create all cells in the grid
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    cells[x, y] = new Cell(x, y);
                }
            }
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public bool IsWalkable(int x, int y)
        {
            return IsInBounds(x, y) && cells[x, y].IsWalkable;
        }

        public void SetWalkable(int x, int y, bool walkable)
        {
            if (IsInBounds(x, y))
            {
                bool oldValue = cells[x, y].IsWalkable;
                cells[x, y].IsWalkable = walkable;

                // If the value actually changed, trigger the event
                if (oldValue != walkable)
                {
                    OnCellChanged?.Invoke(x, y);
                }

                // Clear cached paths when walkability changes
                InvalidatePathsForCell(x, y);
            }
        }

        public Cell GetCell(int x, int y)
        {
            if (IsInBounds(x, y))
                return cells[x, y];
            return null;
        }

        public bool IsOccupied(int x, int y)
        {
            return occupiedCells.Contains(new Vector2Int(x, y));
        }

        public void SetOccupied(int x, int y, bool occupied)
        {
            var pos = new Vector2Int(x, y);
            if (occupied)
                occupiedCells.Add(pos);
            else
                occupiedCells.Remove(pos);
        }

        public void ClearOccupied()
        {
            occupiedCells.Clear();
        }

        private void InvalidatePathsForCell(int x, int y)
        {
            // Simple solution: clear all cached paths when one cell changes
            pathCache.Clear();
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            // Combine start and end into a unique key for the cache
            Vector2Int cacheKey = new Vector2Int(
                start.x * Width + end.x,
                start.y * Height + end.y
            );

            // If this path is already cached, return a copy of it
            if (pathCache.TryGetValue(cacheKey, out var cachedPath))
                return new List<Vector2Int>(cachedPath);

            // Otherwise, calculate a new path
            var path = AStarPathfinding(start, end);

            if (path != null)
            {
                if (pathCache.Count >= pathCacheMaxSize)
                {
                    var oldestKey = pathCache.Keys.First();
                    pathCache.Remove(oldestKey);
                }

                pathCache[cacheKey] = new List<Vector2Int>(path);
            }

            return path;
        }

        /// <summary>
        /// A* Pathfinding algorithm
        /// </summary>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        private List<Vector2Int> AStarPathfinding(Vector2Int start, Vector2Int end)
        {
            // Setup data structures
            var openSet = new PriorityQueue<Cell>();                    // Nodes to explore
            var closedSet = new HashSet<Vector2Int>();                  // Already explored nodes
            var cameFrom = new Dictionary<Vector2Int, Vector2Int>();    // Best path to reach a node
            var gScore = new Dictionary<Vector2Int, float>();           // Cost from start to each node
            var fScore = new Dictionary<Vector2Int, float>();           // Estimated total cost

            Cell startCell = GetCell(start.x, start.y);
            Cell goalCell = GetCell(end.x, end.y);

            // If start or end is invalid or not walkable, return null
            if (startCell == null || goalCell == null || !startCell.IsWalkable || !goalCell.IsWalkable)
                return null;

            // Add the start cell to the open set
            openSet.Enqueue(startCell, 0);
            gScore[start] = 0;
            fScore[start] = HeuristicCost(start, end);

            while (openSet.Count > 0)
            {
                // Pick the cell with the lowest estimated total cost
                var current = openSet.Dequeue();
                var currentPos = new Vector2Int(current.X, current.Y);

                // If we've reached the goal, reconstruct the path
                if (currentPos == end)
                    return ReconstructPath(cameFrom, currentPos);

                closedSet.Add(currentPos);

                // Check each neighbor of the current cell
                foreach (var neighbor in GetNeighbors(currentPos))
                {
                    if (closedSet.Contains(neighbor))
                        continue;

                    float tentativeG = gScore[currentPos] + 1; // 1 is the cost to move to a neighbor

                    if (!gScore.ContainsKey(neighbor) || tentativeG < gScore[neighbor])
                    {
                        // This path to neighbor is better
                        cameFrom[neighbor] = currentPos;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + HeuristicCost(neighbor, end);

                        var neighborCell = GetCell(neighbor.x, neighbor.y);
                        if (!openSet.Contains(neighborCell))
                            openSet.Enqueue(neighborCell, fScore[neighbor]);
                    }
                }
            }

            // No path was found
            return null;
        }

        /// <summary>
        /// Heuristic function: estimated distance to goal (Manhattan distance)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private float HeuristicCost(Vector2Int a, Vector2Int b)
        {
            return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
        }

        /// <summary>
        /// Returns walkable neighbors (up, down, left, right)
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        private List<Vector2Int> GetNeighbors(Vector2Int position)
        {
            List<Vector2Int> neighbors = new();
            int[] dx = { 0, 1, 0, -1 };
            int[] dy = { 1, 0, -1, 0 };

            for (int i = 0; i < 4; i++)
            {
                int nx = position.x + dx[i];
                int ny = position.y + dy[i];

                if (IsInBounds(nx, ny) && IsWalkable(nx, ny) && !IsOccupied(nx, ny))
                    neighbors.Add(new Vector2Int(nx, ny));

            }

            return neighbors;
        }

        /// <summary>
        /// Reconstructs the full path from end to start using the cameFrom map
        /// </summary>
        /// <param name="cameFrom"></param>
        /// <param name="current"></param>
        /// <returns></returns>
        private List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int current)
        {
            List<Vector2Int> path = new();
            path.Add(current);

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }

            // Optional: remove the start cell if not needed
            if (path.Count > 0)
                path.RemoveAt(0);

            return path;
        }
    }

    /// <summary>
    /// Simple priority queue for A* (min-heap based)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> data = new();

        public void Enqueue(T item, float priority)
        {
            // Assumes T is Cell, sets the priority value for comparison
            if (item is Cell cell)
                cell.Priority = priority;

            data.Add(item);
            int ci = data.Count - 1;

            // Bubble up
            while (ci > 0)
            {
                int pi = (ci - 1) / 2;
                if (data[ci].CompareTo(data[pi]) >= 0)
                    break;

                (data[ci], data[pi]) = (data[pi], data[ci]);
                ci = pi;
            }
        }

        public T Dequeue()
        {
            int li = data.Count - 1;
            T frontItem = data[0];
            data[0] = data[li];
            data.RemoveAt(li);
            li--;

            int pi = 0;
            while (true)
            {
                int ci = pi * 2 + 1;
                if (ci > li) break;

                int rc = ci + 1;
                if (rc <= li && data[rc].CompareTo(data[ci]) < 0)
                    ci = rc;

                if (data[pi].CompareTo(data[ci]) <= 0)
                    break;

                (data[pi], data[ci]) = (data[ci], data[pi]);
                pi = ci;
            }

            return frontItem;
        }

        public int Count => data.Count;

        public bool Contains(T item) => data.Contains(item);
    }
}
