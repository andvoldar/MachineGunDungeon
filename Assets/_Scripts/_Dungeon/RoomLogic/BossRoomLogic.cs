// Assets/_Scripts/Rooms/BossRoomLogic.cs
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
        else
        {
            // El boss arranca dormant; definimos sus límites de wander aquí
            bossInstance.SetWanderBounds(ComputeWanderBounds());
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
            Debug.LogWarning("[BossRoomLogic] El prefab de boss no tiene componente MinotaurBoss.");
            Destroy(go);
        }
    }

    // === Activación al levantar barreras: activa boss + CAMBIO DE MUSICA ===
    protected override void OnBarriersActivated()
    {
        if (IsRoomCleared) return;

        if (bossInstance != null)
        {
            // 1) Cambiar música a Boss (crossfade)
            if (BackgroundMusicController.Instance != null)
                BackgroundMusicController.Instance.CrossfadeToBoss();

            // 2) Activar IA + grito de entrada
            bossInstance.ActivateBoss(playScream: true);
        }
    }

    private void SpawnFloorTraps()
    {
        if (floorTrapPrefab == null) return;

        var floorSet = new List<Vector2Int>(generator.FloorPositions);
        if (floorSet.Count == 0) return;

        int trapCount = Random.Range(trapCountMin, trapCountMax + 1);
        int spawned = 0;

        // Shuffle
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
            bossInstance.OnDeath.AddListener(() =>
            {
                // Al morir el boss, volver a música ambient
                if (BackgroundMusicController.Instance != null)
                    BackgroundMusicController.Instance.CrossfadeToAmbient();

                RaiseRoomCleared();
            });
    }

    public override void UnregisterListeners()
    {
        if (bossInstance != null)
            bossInstance.OnDeath.RemoveAllListeners();
    }

    private Rect ComputeWanderBounds()
    {
        if (generator == null || generator.FloorPositions == null || generator.FloorPositions.Count == 0 || bossInstance == null)
            return new Rect(bossInstance != null ? bossInstance.transform.position : Vector3.zero, Vector2.one * 6f);

        int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
        foreach (var cell in generator.FloorPositions)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y < minY) minY = cell.y;
            if (cell.y > maxY) maxY = cell.y;
        }

        Vector3 wMin = generator.floorTilemap.CellToWorld(new Vector3Int(minX, minY, 0)) + new Vector3(0.5f, 0.5f, 0f);
        Vector3 wMax = generator.floorTilemap.CellToWorld(new Vector3Int(maxX, maxY, 0)) + new Vector3(0.5f, 0.5f, 0f);

        float width = Mathf.Abs(wMax.x - wMin.x);
        float height = Mathf.Abs(wMax.y - wMin.y);
        float x = Mathf.Min(wMin.x, wMax.x);
        float y = Mathf.Min(wMin.y, wMax.y);

        float margin = 0.8f;
        return new Rect(x + margin, y + margin, Mathf.Max(0.5f, width - margin * 2f), Mathf.Max(0.5f, height - margin * 2f));
    }
}
