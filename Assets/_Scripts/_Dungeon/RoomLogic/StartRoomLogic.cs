// StartRoomLogic.cs
using System.Collections.Generic;
using UnityEngine;

public class StartRoomLogic : RoomLogicBase
{
    [HideInInspector] public GameObject playerPrefab;
    [HideInInspector] public GameObject[] destructiblePrefabs;
    [HideInInspector] public int destructibleCountMin = 3;
    [HideInInspector] public int destructibleCountMax = 8;

    private bool playerSpawned = false;

    public override void OnRoomGenerated()
    {
        // 1) Spawnear antorchas (implementado en RoomLogicBase)
        base.OnRoomGenerated();

        // 2) Spawnear objetos destruibles en Start Room
        SpawnDestructibleObjects();

        // 3) Instanciar al jugador en el centro de la sala
        SpawnPlayerInside();
    }

    public void SpawnPlayerInside()
    {
        if (playerSpawned || playerPrefab == null) return;

        Vector2Int centerRel = generator.RoomCenter;
        Vector3Int cell = new Vector3Int(centerRel.x, centerRel.y, 0);
        Vector3 origin = generator.floorTilemap.CellToWorld(cell);
        Vector3 spawnPos = origin + new Vector3(0.5f, 0.5f, 0f);

        GameObject playerGO = Instantiate(playerPrefab, spawnPos, Quaternion.identity);
        playerGO.name = "Player";

        playerSpawned = true;

        // Marcar la sala como limpia (no hay enemigos)
        RaiseRoomCleared();
    }

    private void SpawnDestructibleObjects()
    {
        if (destructiblePrefabs == null || destructiblePrefabs.Length == 0) return;

        var floorSet = new List<Vector2Int>(generator.FloorPositions);
        if (floorSet.Count == 0) return;

        // Decidir estilo: 0 = pegados a pared, 1 = agrupados
        int style = Random.Range(0, 2);
        List<Vector2Int> candidates = new List<Vector2Int>(floorSet);

        if (style == 0) // pegados a pared
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
        else // agrupados en cluster
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

        // Mezclar candidatos
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

    public override void RegisterListeners() { }
    public override void UnregisterListeners() { }
}
