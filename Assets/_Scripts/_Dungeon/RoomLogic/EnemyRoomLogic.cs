// Assets/_Scripts/Rooms/EnemyRoomLogic.cs
using System.Collections.Generic;
using UnityEngine;

public class EnemyRoomLogic : RoomLogicBase
{
    [Header("Spawn de Enemigos")]
    public GameObject[] enemyPrefabs;
    public int enemyCount = 3;

    [Header("Trampas de Suelo")]
    public GameObject floorTrapPrefab;
    public int trapCountMin = 2;
    public int trapCountMax = 5;
    [Range(0f, 1f)] public float trapProbability = 0.75f;

    [Header("Objetos Destruibles")]
    public GameObject[] destructiblePrefabs;
    public int destructibleCountMin = 3;
    public int destructibleCountMax = 8;

    private readonly List<Enemy> enemiesInRoom = new List<Enemy>();
    private List<Vector2Int> pendingEnemySpawns;
    private bool enemiesSpawned = false;

    public override void OnRoomGenerated()
    {
        base.OnRoomGenerated(); // crea barreras y antorchas

        SpawnFloorTraps();
        SpawnDestructibleObjects();
        PrepareEnemySpawns(); // Aún NO instanciamos

        // Si no habrá enemigos en esta sala, marcamos despejado silencioso (SIN SFX)
        if (pendingEnemySpawns.Count == 0)
        {
            MarkNoEnemiesAndClearSilently();
        }
    }

    /// <summary>
    /// El spawn REAL sucede cuando las barreras ya están arriba.
    /// </summary>
    protected override void OnBarriersActivated()
    {
        if (IsRoomCleared) return;
        SpawnPendingEnemies();
    }

    private void PrepareEnemySpawns()
    {
        pendingEnemySpawns = new List<Vector2Int>();

        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || generator?.FloorPositions == null)
            return;

        var cells = new List<Vector2Int>(generator.FloorPositions);
        Shuffle(cells);

        int placed = 0;
        foreach (var cell in cells)
        {
            if (placed >= enemyCount) break;
            if (occupiedCells != null && occupiedCells.Contains(cell)) continue;

            pendingEnemySpawns.Add(cell);
            placed++;
        }
    }

    private void SpawnPendingEnemies()
    {
        if (enemiesSpawned || pendingEnemySpawns == null) return;

        foreach (var cell in pendingEnemySpawns)
        {
            Vector3 world = generator.floorTilemap.CellToWorld((Vector3Int)cell) + new Vector3(0.5f, 0.5f, 0f);
            GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject go = Instantiate(prefab, world, Quaternion.identity, transform);
            go.name = $"Enemy";
            var e = go.GetComponent<Enemy>();
            if (e != null)
            {
                // ✅ Informamos a la sala que hay un enemigo vivo
                NotifyEnemySpawned();

                enemiesInRoom.Add(e);
                if (occupiedCells == null) occupiedCells = new HashSet<Vector2Int>();
                occupiedCells.Add(cell);

                // Gancho de muerte -> sacar del listado + notificar a la sala
                e.OnDeath.AddListener(() =>
                {
                    OnEnemyDied(e);
                    NotifyEnemyDied(); // ✅ cuando llega a 0, la base llamará RaiseRoomCleared con SFX si hubo combate
                });

                e.TriggerSpawnVFX();
            }
            else
            {
                Destroy(go);
            }
        }

        enemiesSpawned = true;

        // Si por lo que sea no se añadieron (prefabs sin Enemy component), limpiamos silencioso
        if (enemiesInRoom.Count == 0)
        {
            MarkNoEnemiesAndClearSilently();
        }
    }

    private void SpawnFloorTraps()
    {
        if (floorTrapPrefab == null || generator?.FloorPositions == null) return;

        var cells = new List<Vector2Int>(generator.FloorPositions);
        Shuffle(cells);

        int toSpawn = Random.Range(trapCountMin, trapCountMax + 1);
        int spawned = 0;

        foreach (var cell in cells)
        {
            if (spawned >= toSpawn) break;
            if (Random.value > trapProbability) continue;
            if (occupiedCells != null && occupiedCells.Contains(cell)) continue;
            if (generator.UpDoors.Contains(cell) || generator.DownDoors.Contains(cell)
             || generator.LeftDoors.Contains(cell) || generator.RightDoors.Contains(cell))
                continue;

            Vector3 world = generator.floorTilemap.CellToWorld((Vector3Int)cell) + new Vector3(0.5f, 0.5f, 0f);
            var go = Instantiate(floorTrapPrefab, world, Quaternion.identity, transform);
            go.name = $"FloorTrap_{cell.x}_{cell.y}";
            if (occupiedCells == null) occupiedCells = new HashSet<Vector2Int>();
            occupiedCells.Add(cell);
            spawned++;
        }
    }

    private void SpawnDestructibleObjects()
    {
        if (destructiblePrefabs == null || destructiblePrefabs.Length == 0 || generator?.FloorPositions == null)
            return;

        var floorSet = new List<Vector2Int>(generator.FloorPositions);
        var available = new List<Vector2Int>();
        foreach (var cell in floorSet)
            if (occupiedCells == null || !occupiedCells.Contains(cell))
                available.Add(cell);
        if (available.Count == 0) return;

        Shuffle(available);

        int toSpawn = Random.Range(destructibleCountMin, destructibleCountMax + 1);
        int spawned = 0;
        foreach (var cell in available)
        {
            if (spawned >= toSpawn) break;
            if (occupiedCells != null && occupiedCells.Contains(cell)) continue;

            Vector3 world = generator.floorTilemap.CellToWorld((Vector3Int)cell) + new Vector3(0.5f, 0.5f, 0f);
            var prefab = destructiblePrefabs[Random.Range(0, destructiblePrefabs.Length)];
            var go = Instantiate(prefab, world, Quaternion.identity, transform);
            go.name = $"Destructible_{cell.x}_{cell.y}";
            if (occupiedCells == null) occupiedCells = new HashSet<Vector2Int>();
            occupiedCells.Add(cell);
            spawned++;
        }
    }

    private void OnEnemyDied(Enemy e)
    {
        enemiesInRoom.Remove(e);
        // NOTA: la limpieza y SFX se gestionan en NotifyEnemyDied() de la base.
    }

    public override void RegisterListeners()
    {
        foreach (var e in enemiesInRoom)
        {
            e.OnDeath.AddListener(() =>
            {
                OnEnemyDied(e);
                NotifyEnemyDied();
            });
        }
    }

    public override void UnregisterListeners()
    {
        foreach (var e in enemiesInRoom)
            e.OnDeath.RemoveAllListeners();
    }

    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }
}
