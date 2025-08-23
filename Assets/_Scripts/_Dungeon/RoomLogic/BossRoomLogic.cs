using System.Collections.Generic;
using UnityEngine;

public class BossRoomLogic : RoomLogicBase
{
    [Header("Prefab del Boss")]
    public GameObject bossPrefab;

    [Header("Trampas de Suelo")]
    public GameObject floorTrapPrefab;
    public int trapCountMin = 2;
    public int trapCountMax = 5;
    [Range(0f, 1f)]
    public float trapProbability = 0.75f;

    [Header("Objetos Destruibles")]
    public GameObject[] destructiblePrefabs;
    public int destructibleCountMin = 3;
    public int destructibleCountMax = 8;

    private MinotaurBoss bossInstance;

    public override void OnRoomGenerated()
    {
        base.OnRoomGenerated();

        SpawnFloorTraps();
        SpawnDestructibleObjects();
        SpawnBoss();
        RegisterListeners();

        if (bossInstance == null)
        {
            RaiseRoomCleared(); // si no hay boss, sala limpia
        }
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogWarning("[BossRoomLogic] bossPrefab no asignado.");
            return;
        }

        Vector2Int centerRel = generator.RoomCenter;
        Vector3Int cell = new Vector3Int(centerRel.x, centerRel.y, 0);
        Vector3 origin = generator.floorTilemap.CellToWorld(cell);
        Vector3 spawnPos = origin + new Vector3(0.5f, 0.5f, 0f);

        GameObject go = Instantiate(bossPrefab, spawnPos, Quaternion.identity, transform);
        go.name = "Boss";
        bossInstance = go.GetComponent<MinotaurBoss>();

        if (bossInstance == null)
        {
            Debug.LogWarning("[BossRoomLogic] El prefab de boss no tiene componente Enemy.");
            Destroy(go);
        }
    }

    private void SpawnFloorTraps()
    {
        if (floorTrapPrefab == null) return;

        var floorSet = new List<Vector2Int>(generator.FloorPositions);
        if (floorSet.Count == 0) return;

        int trapCount = Random.Range(trapCountMin, trapCountMax + 1);
        int spawned = 0;

        for (int i = 0; i < floorSet.Count; i++)
        {
            int r = Random.Range(i, floorSet.Count);
            (floorSet[i], floorSet[r]) = (floorSet[r], floorSet[i]);
        }

        foreach (var cell in floorSet)
        {
            if (spawned >= trapCount) break;
            if (Random.value > trapProbability) continue;

            if (generator.UpDoors.Contains(cell) ||
                generator.DownDoors.Contains(cell) ||
                generator.LeftDoors.Contains(cell) ||
                generator.RightDoors.Contains(cell))
                continue;

            Vector3 origin = generator.floorTilemap.CellToWorld(new Vector3Int(cell.x, cell.y, 0));
            Vector3 spawnPos = origin + new Vector3(0.5f, 0.5f, 0f);
            GameObject trapGO = Instantiate(floorTrapPrefab, spawnPos, Quaternion.identity, transform);
            trapGO.name = $"FloorTrap_{cell.x}_{cell.y}";
            spawned++;
        }
    }

    private void SpawnDestructibleObjects()
    {
        if (destructiblePrefabs == null || destructiblePrefabs.Length == 0) return;

        var floorSet = new List<Vector2Int>(generator.FloorPositions);
        if (floorSet.Count == 0) return;

        int style = Random.Range(0, 2);
        List<Vector2Int> candidates = new List<Vector2Int>(floorSet);

        if (style == 0)
        {
            var walls = ComputeWallCells(floorSet);
            candidates.Clear();
            foreach (var cellPos in floorSet)
            {
                foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
                {
                    if (walls.Contains(cellPos + dir))
                    {
                        candidates.Add(cellPos);
                        break;
                    }
                }
            }
            if (candidates.Count == 0)
                candidates = new List<Vector2Int>(floorSet);
        }
        else
        {
            Vector2Int center = floorSet[Random.Range(0, floorSet.Count)];
            List<Vector2Int> cluster = new List<Vector2Int> { center };
            foreach (var dir in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int n = center + dir;
                if (floorSet.Contains(n)) cluster.Add(n);
            }
            candidates = cluster;
        }

        for (int i = 0; i < candidates.Count; i++)
        {
            int r = Random.Range(i, candidates.Count);
            (candidates[i], candidates[r]) = (candidates[r], candidates[i]);
        }

        int destructibleCount = Random.Range(destructibleCountMin, destructibleCountMax + 1);
        int spawned = 0;
        foreach (var cellPos in candidates)
        {
            if (spawned >= destructibleCount) break;
            var prefab = destructiblePrefabs[Random.Range(0, destructiblePrefabs.Length)];
            Vector3 origin = generator.floorTilemap.CellToWorld(new Vector3Int(cellPos.x, cellPos.y, 0));
            Vector3 spawnPos = origin + new Vector3(0.5f, 0.5f, 0f);
            GameObject objGO = Instantiate(prefab, spawnPos, Quaternion.identity, transform);
            objGO.name = $"Destructible_{cellPos.x}_{cellPos.y}";
            spawned++;
        }
    }

    private HashSet<Vector2Int> ComputeWallCells(List<Vector2Int> floorSet)
    {
        var wallCells = new HashSet<Vector2Int>();
        Vector2Int[] dirs = {
            Vector2Int.up, Vector2Int.down,
            Vector2Int.left, Vector2Int.right,
            Vector2Int.up + Vector2Int.left, Vector2Int.up + Vector2Int.right,
            Vector2Int.down + Vector2Int.left, Vector2Int.down + Vector2Int.right
        };
        var floorHash = new HashSet<Vector2Int>(floorSet);
        foreach (var cellPos in floorSet)
        {
            foreach (var d in dirs)
            {
                Vector2Int neighbor = cellPos + d;
                if (!floorHash.Contains(neighbor))
                    wallCells.Add(neighbor);
            }
        }
        return wallCells;
    }

    public override void RegisterListeners()
    {
        if (bossInstance != null)
            bossInstance.OnDeath.AddListener(() => RaiseRoomCleared());
    }

    public override void UnregisterListeners()
    {
        if (bossInstance != null)
            bossInstance.OnDeath.RemoveAllListeners();
    }
}
