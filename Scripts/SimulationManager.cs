using System;
using System.Collections.Generic;
using UnityEngine;

namespace EchoRL
{
    /// <summary>
    /// Central manager for initializing and updating the simulation world.
    /// Handles terrain generation, entity spawning, ticking, and rerolling.
    /// </summary>
    public class SimulationManager : MonoBehaviour
    {
        [Header("World Region Settings")]
        public int RegionWidth = 10;
        public int RegionHeight = 10;
        public int RegionSize = 16;

        [Header("Entity Settings")]
        public GameObject EntityPrefab;
        public int EntityCount = 5;

        [Header("Simulation Settings")]
        public float TickRate = 0.1f;
        public GridVisualizer GridVisualizer;

        [Space(10)]
        public CameraController CameraController;

        /// <summary>
        /// Singleton instance of the simulation manager.
        /// </summary>
        public static SimulationManager Instance => instance;

        /// <summary>
        /// Number of ticks elapsed since simulation start.
        /// </summary>
        public int TickCount { get; private set; } = 0;
        public Grid Grid { get; private set; }
        public RegionGrid RegionGrid { get; private set; }

        private List<Entity> entities = new List<Entity>();
        private List<ISystem> systems = new List<ISystem>();
        private static SimulationManager instance;
        private int nextEntityID = 0;
        private float tickTimer = 0;

        /// <summary>
        /// Initializes the singleton instance.
        /// </summary>
        private void Awake()
        {
            if (instance == null)
                instance = this;
            else
                Destroy(gameObject);
        }

        /// <summary>
        /// Called once at simulation start. Initializes world and spawns entities.
        /// </summary>
        private void Start()
        {
            Initialize(RegionWidth, RegionHeight, RegionSize);
            CreateFactions();
            RegisterSystem(new GoalDecisionSystem());
            RegisterSystem(new GoalExecutionSystem());
            RegisterSystem(new VisualsSystem());
            SpawnEntities(EntityCount);
        }

        /// <summary>
        /// Unity update loop. Ticks the simulation and checks for reroll input.
        /// </summary>
        private void Update()
        {
            tickTimer += Time.deltaTime;

            if (tickTimer >= TickRate)
            {
                Tick();
                tickTimer = 0;
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                RegenerateWorld();
            }
        }

        /// <summary>
        /// Initializes the simulation world with terrain, climate, and region data.
        /// </summary>
        public void Initialize(int regionWidth, int regionHeight, int regionSize)
        {
            RegionGrid = new RegionGrid(regionWidth, regionHeight, regionSize);

            int totalWidth = regionWidth * regionSize;
            int totalHeight = regionHeight * regionSize;

            Grid = new Grid(totalWidth, totalHeight);

            // Generate terrain and assign biomes
            TerrainGenerator.Generate(Grid);

            // Assign cells to regions
            AssignCellsToRegions();

            // Initialize visuals
            GridVisualizer?.Initialize(Grid);
        }
        

        /// <summary>
        /// Runs one simulation tick and processes all systems.
        /// </summary>
        private void Tick()
        {
            TickCount++;

            foreach (var system in systems)
            {
                system.Process(entities);
            }

            EventSystem.Instance.ProcessEvents();
        }

        /// <summary>
        /// Regenerates.
        /// </summary>
        public void RegenerateWorld()
        {
            Debug.Log("Regenerating terrain...");

            TickCount = 0;

            // Clear all existing entities, including factions
            ClearEntities();

            // Reset occupied grid cells
            int totalWidth = RegionWidth * RegionSize;
            int totalHeight = RegionHeight * RegionSize;

            for (int x = 0; x < totalWidth; x++)
            {
                for (int y = 0; y < totalHeight; y++)
                {
                    Cell cell = Grid.GetCell(x, y);
                    Grid.SetOccupied(x, y, false);
                }
            }

            // Regenerate terrain and regions
            TerrainGenerator.Generate(Grid);
            AssignCellsToRegions();

            // Clear region faction ownership
            for (int rx = 0; rx < RegionWidth; rx++)
            {
                for (int ry = 0; ry < RegionHeight; ry++)
                {
                    var region = RegionGrid.GetRegion(rx, ry);
                    region.Faction = null;
                }
            }

            // Recreate factions
            CreateFactions();

            // Refresh Tilemap
            GridVisualizer?.RefreshTilemap();

            // Spawn Entities
            SpawnEntities(EntityCount);
        }

        private void AssignCellsToRegions()
        {
            for (int rx = 0; rx < RegionWidth; rx++)
            {
                for (int ry = 0; ry < RegionHeight; ry++)
                {
                    Region region = RegionGrid.GetRegion(rx, ry);

                    for (int lx = 0; lx < RegionSize; lx++)
                    {
                        for (int ly = 0; ly < RegionSize; ly++)
                        {
                            int wx = rx * RegionSize + lx;
                            int wy = ry * RegionSize + ly;
                            region.LocalCells[lx, ly] = Grid.GetCell(wx, wy);
                        }
                    }
                }
            }
        }

        private void CreateFactions()
        {
            string[] names = { "Crimson Empire", "Azure Dominion", "Verdant Pact", "Obsidian Horde" };
            Color[] colors = { Color.red, Color.blue, Color.green, Color.black };

            // Gather all unclaimed regions
            List<Region> candidates = new();
            for (int x = 0; x < RegionGrid.RegionWidth; x++)
            {
                for (int y = 0; y < RegionGrid.RegionHeight; y++)
                {
                    Region region = RegionGrid.GetRegion(x, y);
                    if (region.Faction == null)
                        candidates.Add(region);
                }
            }

            // Shuffle the list
            for (int i = 0; i < candidates.Count; i++)
            {
                int j = UnityEngine.Random.Range(i, candidates.Count);
                (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
            }

            int factionCount = Mathf.Min(names.Length, candidates.Count);
            for (int i = 0; i < factionCount; i++)
            {
                Entity faction = CreateEntity();
                FactionComponent component = new FactionComponent(names[i], colors[i]);

                // Introduce slight randomness to make AI diverge
                component.MilitaryStrength = UnityEngine.Random.Range(10, 30);
                component.Resources = UnityEngine.Random.Range(75, 150);

                faction.AddComponent(component);

                var region = candidates[i];
                region.Faction = faction;

                faction.GetComponent<FactionComponent>().ControlledRegions.Add(region.RegionCoord);
            }
        }


        /// <summary>
        /// Spawns a number of entities at valid random locations.
        /// </summary>
        private void SpawnEntities(int count)
        {
            int totalWidth = RegionWidth * RegionSize;
            int totalHeight = RegionHeight * RegionSize;
            List<Cell> candidates = new List<Cell>();

            // Gather all walkable, unoccupied cells
            for (int x = 0; x < totalWidth; x++)
            {
                for (int y = 0; y < totalHeight; y++)
                {
                    if (Grid.IsWalkable(x, y) && !Grid.IsOccupied(x, y))
                    {
                        candidates.Add(new Cell(x, y));
                    }
                }
            }

            if (candidates.Count == 0)
            {
                Debug.LogWarning("No valid spawn locations.");
                return;
            }

            int spawnCount = Mathf.Min(count, candidates.Count);

            for (int i = 0; i < spawnCount; i++)
            {
                int index = UnityEngine.Random.Range(0, candidates.Count);
                Cell cell = candidates[index];
                candidates.RemoveAt(index);

                Grid.SetOccupied(cell.X, cell.Y, true);

                Entity entity = CreateEntity();
                entity.AddComponent(new NameComponent($"Individual_{i + 1}"));
                entity.AddComponent(new PositionComponent(cell.X, cell.Y));
                entity.AddComponent(new MovementComponent());
                entity.AddComponent(new VisualsComponent(EntityPrefab, transform));

                var vis = entity.GetComponent<VisualsComponent>();
                var refComp = vis.gameObject.GetComponent<EntityReference>() ?? vis.gameObject.AddComponent<EntityReference>();
                refComp.Entity = entity;
            }

            Debug.Log($"Spawned {spawnCount} entities.");
        }

        /// <summary>
        /// Removes all current entities and their visual objects.
        /// </summary>
        public void ClearEntities()
        {
            foreach (var entity in new List<Entity>(entities))
            {
                DestroyEntity(entity);
            }

            entities.Clear();
            nextEntityID = 0;
        }

        /// <summary>
        /// Destroys an entity and cleans up its data and visuals.
        /// </summary>
        public void DestroyEntity(Entity entity)
        {
            var posComp = entity.GetComponent<PositionComponent>();
            if (posComp != null)
            {
                Grid.SetOccupied(posComp.Position.x, posComp.Position.y, false);
            }

            var visComp = entity.GetComponent<VisualsComponent>();
            if (visComp != null && visComp.gameObject != null)
            {
                Destroy(visComp.gameObject);
            }

            entities.Remove(entity);
        }

        /// <summary>
        /// Creates a new entity and registers it with the simulation.
        /// </summary>
        public Entity CreateEntity()
        {
            var entity = new Entity(nextEntityID++);
            entities.Add(entity);
            return entity;
        }

        /// <summary>
        /// Checks if an entity still exists in the simulation.
        /// </summary>
        public bool EntityExists(Entity entity)
        {
            return entity != null && entities.Contains(entity);
        }

        /// <summary>
        /// Returns all entities that have a specific component type.
        /// </summary>
        public IEnumerable<Entity> GetEntitiesWithComponent<T>() where T : Component
        {
            foreach (var entity in entities)
            {
                if (entity.HasComponent<T>())
                    yield return entity;
            }
        }

        /// <summary>
        /// Registers a new system to be processed each simulation tick.
        /// </summary>
        public void RegisterSystem(ISystem system)
        {
            systems.Add(system);
            system.Initialize();
        }
    }
}
