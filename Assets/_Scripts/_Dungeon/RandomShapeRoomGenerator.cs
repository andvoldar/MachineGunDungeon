using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Grid))]
public class RandomShapeRoomGenerator : MonoBehaviour
{
    [Header("Tilemaps")]
    public Tilemap floorTilemap;
    public Tilemap wallBackTilemap;
    public Tilemap wallFrontTilemap;
    public Tilemap wallVerticalTilemap;
    public Tilemap decorationTilemap;
    public Tilemap exteriorWallTilemapA;
    public Tilemap exteriorWallTilemapB;
    public Tilemap cornerTilemap;

    [Header("Tile Variants")]
    public TileBase[] floorTiles;
    public TileBase[] wallBackTiles;
    public TileBase[] wallFrontTiles;
    public TileBase[] wallVerticalTiles;
    public TileBase[] decorationTiles;
    public TileBase[] exteriorCoverTiles;

    [Header("Corner Tiles")]
    public TileBase cornerTopLeft, cornerTopRight, cornerBottomLeft, cornerBottomRight;

    [Header("Room Bounds & Doors")]
    public int maxWidth = 16, maxHeight = 12;
    public int minRoomWidth = 6, minRoomHeight = 6;
    public int doorWidth = 3;

    [Header("Decorations")]
    [Range(0f, 1f)] public float decorationChance = 0.05f;

    [Header("Enable Doors")]
    public bool openUp = true, openDown = true, openLeft = true, openRight = true;

    private HashSet<Vector2Int> floorPositions;
    private HashSet<Vector2Int> doorPositions;

    // Para exponer posiciones de suelo a otras lógicas
    public HashSet<Vector2Int> FloorPositions => floorPositions;
    public Vector2Int RoomCenter { get; private set; }
    public List<Vector2Int> UpDoors { get; private set; }
    public List<Vector2Int> DownDoors { get; private set; }
    public List<Vector2Int> LeftDoors { get; private set; }
    public List<Vector2Int> RightDoors { get; private set; }

    [ContextMenu("Generate Room")]
    public void GenerateRoom()
    {
        ClearAll();

        UpDoors = new List<Vector2Int>();
        DownDoors = new List<Vector2Int>();
        LeftDoors = new List<Vector2Int>();
        RightDoors = new List<Vector2Int>();
        floorPositions = new HashSet<Vector2Int>();
        doorPositions = new HashSet<Vector2Int>();

        // 1) Rectángulo base
        int w = UnityEngine.Random.Range(minRoomWidth, maxWidth + 1);
        int h = UnityEngine.Random.Range(minRoomHeight, maxHeight + 1);
        int ox = (maxWidth - w) / 2;
        int oy = (maxHeight - h) / 2;
        RoomCenter = new Vector2Int(ox + w / 2, oy + h / 2);

        for (int x = 0; x < w; x++)
        {
            for (int y = 0; y < h; y++)
            {
                floorPositions.Add(new Vector2Int(ox + x, oy + y));
            }
        }

        // 2) Puertas
        for (int i = 0; i < doorWidth; i++)
        {
            if (openUp)
            {
                var p = new Vector2Int(RoomCenter.x - doorWidth / 2 + i, oy + h);
                doorPositions.Add(p);
                UpDoors.Add(p);
                var c3 = (Vector3Int)p;
                wallBackTilemap.SetTile(c3, null);
                wallFrontTilemap.SetTile(c3, null);
                wallVerticalTilemap.SetTile(c3, null);
                floorTilemap.SetTile(c3, RandomTile(floorTiles));
            }
            if (openDown)
            {
                var p = new Vector2Int(RoomCenter.x - doorWidth / 2 + i, oy - 1);
                doorPositions.Add(p);
                DownDoors.Add(p);
                var c3 = (Vector3Int)p;
                wallBackTilemap.SetTile(c3, null);
                wallFrontTilemap.SetTile(c3, null);
                wallVerticalTilemap.SetTile(c3, null);
                floorTilemap.SetTile(c3, RandomTile(floorTiles));
            }
            if (openRight)
            {
                var p = new Vector2Int(ox + w, RoomCenter.y - doorWidth / 2 + i);
                doorPositions.Add(p);
                RightDoors.Add(p);
                var c3 = (Vector3Int)p;
                wallBackTilemap.SetTile(c3, null);
                wallFrontTilemap.SetTile(c3, null);
                wallVerticalTilemap.SetTile(c3, null);
                floorTilemap.SetTile(c3, RandomTile(floorTiles));
            }
            if (openLeft)
            {
                var p = new Vector2Int(ox - 1, RoomCenter.y - doorWidth / 2 + i);
                doorPositions.Add(p);
                LeftDoors.Add(p);
                var c3 = (Vector3Int)p;
                wallBackTilemap.SetTile(c3, null);
                wallFrontTilemap.SetTile(c3, null);
                wallVerticalTilemap.SetTile(c3, null);
                floorTilemap.SetTile(c3, RandomTile(floorTiles));
            }
        }

        // 3) Pintar suelo
        foreach (var p in floorPositions)
        {
            floorTilemap.SetTile((Vector3Int)p, RandomTile(floorTiles));
        }

        // 4) Muros
        var walls = ComputeWallPositions();
        int midY = oy + h / 2;
        foreach (var wpos in walls)
        {
            var c3 = (Vector3Int)wpos;
            bool isVerticalWall = floorPositions.Contains(wpos + Vector2Int.left)
                                || floorPositions.Contains(wpos + Vector2Int.right);
            if (isVerticalWall)
            {
                wallVerticalTilemap.SetTile(c3, RandomTile(wallVerticalTiles));
                wallBackTilemap.SetTile(c3, null);
                wallFrontTilemap.SetTile(c3, null);
            }
            else
            {
                if (wpos.y < midY)
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

        // 5) Abrir puertas (vuelve a pintar suelo en puerta)
        foreach (var d in doorPositions)
        {
            var c3 = (Vector3Int)d;
            wallBackTilemap.SetTile(c3, null);
            wallFrontTilemap.SetTile(c3, null);
            wallVerticalTilemap.SetTile(c3, null);
            floorTilemap.SetTile(c3, RandomTile(floorTiles));
        }

        // 6) Decoraciones básicas
        foreach (var p in floorPositions)
        {
            if (!doorPositions.Contains(p) && UnityEngine.Random.value < decorationChance)
            {
                decorationTilemap.SetTile((Vector3Int)p, RandomTile(decorationTiles));
            }
        }

        // 7) Recubrimiento exterior capa A y B, y esquinas
        var placedA = PlaceExteriorCover(walls);
        PlaceCornerTiles(placedA);

        // Lógica de sala: avisamos que terminó de generar
        GetComponent<RoomLogicBase>()?.OnRoomGenerated();
    }

    private HashSet<Vector2Int> ComputeWallPositions()
    {
        HashSet<Vector2Int> s = new HashSet<Vector2Int>();
        Vector2Int[] dirs = {
            Vector2Int.up, Vector2Int.down,
            Vector2Int.left, Vector2Int.right,
            Vector2Int.up + Vector2Int.left, Vector2Int.up + Vector2Int.right,
            Vector2Int.down + Vector2Int.left, Vector2Int.down + Vector2Int.right
        };
        foreach (Vector2Int p in floorPositions)
        {
            foreach (Vector2Int d in dirs)
            {
                Vector2Int n = p + d;
                if (!floorPositions.Contains(n) && !doorPositions.Contains(n))
                {
                    s.Add(n);
                }
            }
        }
        return s;
    }

    private HashSet<Vector2Int> PlaceExteriorCover(HashSet<Vector2Int> walls)
    {
        Dictionary<Vector2Int, float> angles = new Dictionary<Vector2Int, float> {
            { Vector2Int.up,     0f },
            { Vector2Int.right, -90f },
            { Vector2Int.down,  180f },
            { Vector2Int.left,   90f },
        };

        HashSet<Vector2Int> placedA = new HashSet<Vector2Int>();
        HashSet<Vector2Int> placedB = new HashSet<Vector2Int>();

        // Capa A (cardinal)
        foreach (Vector2Int wpos in walls)
        {
            foreach (KeyValuePair<Vector2Int, float> kv in angles)
            {
                Vector2Int n = wpos + kv.Key;
                if (IsEmpty(n) && !doorPositions.Contains(n) && !placedA.Contains(n))
                {
                    SetCover(exteriorWallTilemapA, n, RandomTile(exteriorCoverTiles), kv.Value);
                    placedA.Add(n);
                }
            }
        }

        // Capa B (diagonal)
        Vector2Int[][] diag = {
            new[]{ Vector2Int.up,    Vector2Int.left  },
            new[]{ Vector2Int.up,    Vector2Int.right },
            new[]{ Vector2Int.down,  Vector2Int.right },
            new[]{ Vector2Int.down,  Vector2Int.left  },
        };
        foreach (Vector2Int wpos in walls)
        {
            foreach (Vector2Int[] pair in diag)
            {
                Vector2Int n1 = wpos + pair[0];
                Vector2Int n2 = wpos + pair[1];
                Vector2Int nd = wpos + pair[0] + pair[1];
                if (walls.Contains(n1) && walls.Contains(n2)
                 && IsEmpty(nd)
                 && !doorPositions.Contains(nd)
                 && !placedB.Contains(nd))
                {
                    SetCover(exteriorWallTilemapB, nd, RandomTile(exteriorCoverTiles), angles[pair[0]]);
                    placedB.Add(nd);
                }
            }
        }
        return placedA;
    }

    private void PlaceCornerTiles(HashSet<Vector2Int> placedA)
    {
        if (placedA.Count == 0) return;
        int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
        foreach (Vector2Int p in placedA)
        {
            minX = Math.Min(minX, p.x);
            maxX = Math.Max(maxX, p.x);
            minY = Math.Min(minY, p.y);
            maxY = Math.Max(maxY, p.y);
        }
        Vector3Int tl = new Vector3Int(minX, maxY, 0);
        Vector3Int tr = new Vector3Int(maxX, maxY, 0);
        Vector3Int bl = new Vector3Int(minX, minY, 0);
        Vector3Int br = new Vector3Int(maxX, minY, 0);

        cornerTilemap.SetTile(tl, cornerTopLeft);
        cornerTilemap.SetTile(tr, cornerTopRight);
        cornerTilemap.SetTile(bl, cornerBottomLeft);
        cornerTilemap.SetTile(br, cornerBottomRight);
        cornerTilemap.SetTransformMatrix(br, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, 180f), Vector3.one));
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

    private void SetCover(Tilemap m, Vector2Int p, TileBase t, float angle)
    {
        Vector3Int c = (Vector3Int)p;
        m.SetTile(c, t);
        m.SetTransformMatrix(c, Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(0, 0, angle), Vector3.one));
    }

    private void ClearAll()
    {
        floorTilemap.ClearAllTiles();
        wallBackTilemap.ClearAllTiles();
        wallFrontTilemap.ClearAllTiles();
        wallVerticalTilemap.ClearAllTiles();
        exteriorWallTilemapA.ClearAllTiles();
        exteriorWallTilemapB.ClearAllTiles();
        cornerTilemap.ClearAllTiles();
        decorationTilemap.ClearAllTiles();
    }

    private TileBase RandomTile(TileBase[] arr)
    {
        if (arr == null || arr.Length == 0) return null;
        return arr[UnityEngine.Random.Range(0, arr.Length)];
    }
}
