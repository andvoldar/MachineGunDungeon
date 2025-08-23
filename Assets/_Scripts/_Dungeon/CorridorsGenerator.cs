// CorridorsGenerator.cs
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
public class CorridorsGenerator : MonoBehaviour
{
    [Header("Corridor Tilemaps")]
    public Tilemap floorTilemap;
    public Tilemap wallBackTilemap;
    public Tilemap wallFrontTilemap;
    public Tilemap wallVerticalTilemap;      // para paredes verticales
    public Tilemap exteriorWallTilemapA;
    public Tilemap exteriorWallTilemapB;

    [Header("Tile Variants")]
    public TileBase[] floorTiles;
    public TileBase[] wallBackTiles;
    public TileBase[] wallFrontTiles;
    public TileBase[] wallVerticalTiles;     // variantes verticales
    public TileBase[] exteriorCoverTiles;

    [Header("Settings")]
    public int corridorWidth = 1;
    public int doorOpeningWidth = 3;

    private Vector2Int startDoor, endDoor;

    public void SetEndpoints(Vector2Int a, Vector2Int b)
    {
        startDoor = a;
        endDoor = b;
    }

    [ContextMenu("Generate Corridor")]
    public void GenerateCorridor()
    {
        ClearAll();

        // 1) dirección cardinal
        Vector2Int delta = endDoor - startDoor;
        Vector2Int dir = Math.Abs(delta.x) > Math.Abs(delta.y)
            ? new Vector2Int(Math.Sign(delta.x), 0)
            : new Vector2Int(0, Math.Sign(delta.y));
        bool horiz = dir.x != 0;

        // 2) aperturas de puerta
        HashSet<Vector2Int> doorOpen = new HashSet<Vector2Int>();
        Vector2Int perp = new Vector2Int(dir.y, dir.x);
        int halfOpen = doorOpeningWidth / 2;
        for (int i = -halfOpen; i <= halfOpen; i++)
        {
            doorOpen.Add(startDoor + perp * i);
            doorOpen.Add(endDoor + perp * i);
        }

        // 3) path fuera de salas
        Vector2Int start = startDoor + dir;
        Vector2Int end = endDoor - dir;
        int len = horiz
            ? Math.Abs(end.x - start.x)
            : Math.Abs(end.y - start.y);
        List<Vector2Int> raw = new List<Vector2Int>();
        for (int i = 0; i <= len; i++)
            raw.Add(start + dir * i);

        // 4) truncar al chocar
        List<Vector2Int> path = new List<Vector2Int>();
        foreach (Vector2Int p in raw)
        {
            Vector3Int c3 = (Vector3Int)p;
            if (floorTilemap.HasTile(c3)
             || wallBackTilemap.HasTile(c3)
             || wallFrontTilemap.HasTile(c3)
             || wallVerticalTilemap.HasTile(c3))
            {
                break;
            }
            path.Add(p);
        }
        if (path.Count == 0) return;

        // 5) pintar suelo
        HashSet<Vector2Int> floorSet = new HashSet<Vector2Int>();
        int half = corridorWidth / 2;
        foreach (Vector2Int p in path)
        {
            for (int o = -half; o <= half; o++)
            {
                Vector2Int cell = horiz
                    ? new Vector2Int(p.x, p.y + o)
                    : new Vector2Int(p.x + o, p.y);
                floorSet.Add(cell);
            }
        }
        foreach (Vector2Int c in floorSet)
            floorTilemap.SetTile((Vector3Int)c, RandomTile(floorTiles));

        // 6) calcular muros
        HashSet<Vector2Int> walls = new HashSet<Vector2Int>();
        Vector2Int first = path[0];
        Vector2Int last = path[path.Count - 1];
        foreach (Vector2Int p in floorSet)
        {
            foreach (Vector2Int d in new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right })
            {
                Vector2Int n = p + d;
                if (floorSet.Contains(n)) continue;
                if (doorOpen.Contains(n)) continue;
                if (p == first && d == -dir) continue;
                if (p == last && d == dir) continue;
                walls.Add(n);
            }
        }

        // 7) pintar muros con layering y vertical/horizontal
        int midY = (start.y + end.y) / 2;
        foreach (Vector2Int w in walls)
        {
            Vector3Int c3 = (Vector3Int)w;
            bool isVerticalWall = floorSet.Contains(w + Vector2Int.left) || floorSet.Contains(w + Vector2Int.right);

            if (isVerticalWall)
            {
                // pared vertical
                wallVerticalTilemap.SetTile(c3, RandomTile(wallVerticalTiles));
                wallBackTilemap.SetTile(c3, null);
                wallFrontTilemap.SetTile(c3, null);
            }
            else
            {
                // pared horizontal
                if (w.y < midY)
                {
                    wallFrontTilemap.SetTile(c3, RandomTile(wallFrontTiles));
                    wallBackTilemap.SetTile(c3, null);
                }
                else
                {
                    wallBackTilemap.SetTile(c3, RandomTile(wallBackTiles));
                    wallFrontTilemap.SetTile(c3, null);
                }
                wallVerticalTilemap.SetTile(c3, null);
            }
        }

        // 8) recubrimiento exterior
        PlaceExteriorCover(walls, horiz, doorOpen, first, last, dir);

        // 9) limpiar bocas
        foreach (Vector2Int d in doorOpen)
        {
            Vector3Int c3 = (Vector3Int)d;
            wallBackTilemap.SetTile(c3, null);
            wallFrontTilemap.SetTile(c3, null);
            wallVerticalTilemap.SetTile(c3, null);
            exteriorWallTilemapA.SetTile(c3, null);
            exteriorWallTilemapB.SetTile(c3, null);
        }
    }

    private void PlaceExteriorCover(
        HashSet<Vector2Int> walls,
        bool horiz,
        HashSet<Vector2Int> doorOpen,
        Vector2Int first,
        Vector2Int last,
        Vector2Int dir)
    {
        Dictionary<Vector2Int, float> angles = new Dictionary<Vector2Int, float>
        {
            { Vector2Int.up,     0f },
            { Vector2Int.right, -90f },
            { Vector2Int.down,  180f },
            { Vector2Int.left,   90f },
        };

        HashSet<Vector2Int> placedA = new HashSet<Vector2Int>();
        HashSet<Vector2Int> placedB = new HashSet<Vector2Int>();

        Vector2Int wallStart = first - dir;
        Vector2Int wallEnd = last + dir;

        // capa A (cardinal)
        foreach (Vector2Int w in walls)
        {
            foreach (KeyValuePair<Vector2Int, float> kv in angles)
            {
                Vector2Int n = w + kv.Key;
                if (!IsEmpty(n) || doorOpen.Contains(n) || placedA.Contains(n))
                    continue;
                // en extremos no pintar perpendicular
                if ((w == wallStart || w == wallEnd) &&
                    ((horiz && (kv.Key == Vector2Int.up || kv.Key == Vector2Int.down)) ||
                     (!horiz && (kv.Key == Vector2Int.left || kv.Key == Vector2Int.right))))
                    continue;

                TileBase t = RandomTile(exteriorCoverTiles);
                exteriorWallTilemapA.SetTile((Vector3Int)n, t);
                exteriorWallTilemapA.SetTransformMatrix(
                    (Vector3Int)n,
                    Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, kv.Value), Vector3.one)
                );
                placedA.Add(n);
            }
        }

        // capa B (diagonales)
        Vector2Int[][] diag = {
            new[]{ Vector2Int.up,   Vector2Int.left  },
            new[]{ Vector2Int.up,   Vector2Int.right },
            new[]{ Vector2Int.down, Vector2Int.right },
            new[]{ Vector2Int.down, Vector2Int.left  },
        };
        foreach (Vector2Int w in walls)
        {
            if (w == wallStart || w == wallEnd) continue;
            foreach (Vector2Int[] pair in diag)
            {
                Vector2Int n1 = w + pair[0];
                Vector2Int n2 = w + pair[1];
                Vector2Int nd = w + pair[0] + pair[1];
                if (!walls.Contains(n1) || !walls.Contains(n2) || !IsEmpty(nd) || placedB.Contains(nd) || doorOpen.Contains(nd))
                    continue;

                float angle = angles[pair[0]];
                TileBase t = RandomTile(exteriorCoverTiles);
                exteriorWallTilemapB.SetTile((Vector3Int)nd, t);
                exteriorWallTilemapB.SetTransformMatrix(
                    (Vector3Int)nd,
                    Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, angle), Vector3.one)
                );
                placedB.Add(nd);
            }
        }
    }

    private bool IsEmpty(Vector2Int p)
    {
        Vector3Int c = (Vector3Int)p;
        return !floorTilemap.HasTile(c)
            && !wallBackTilemap.HasTile(c)
            && !wallFrontTilemap.HasTile(c)
            && !wallVerticalTilemap.HasTile(c)
            && !exteriorWallTilemapA.HasTile(c)
            && !exteriorWallTilemapB.HasTile(c);
    }

    private void ClearAll()
    {
        floorTilemap.ClearAllTiles();
        wallBackTilemap.ClearAllTiles();
        wallFrontTilemap.ClearAllTiles();
        wallVerticalTilemap.ClearAllTiles();
        exteriorWallTilemapA.ClearAllTiles();
        exteriorWallTilemapB.ClearAllTiles();
    }

    private TileBase RandomTile(TileBase[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        return arr[UnityEngine.Random.Range(0, arr.Length)];
    }
}
