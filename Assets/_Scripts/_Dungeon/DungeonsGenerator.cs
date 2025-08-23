using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Genera habitaciones de forma orgánica en un "cluster" sin seguir una grilla fija,
/// y conecta habitaciones adyacentes con pasillos automáticos.
/// </summary>
[ExecuteAlways]
public class DungeonsGenerator : MonoBehaviour
{
    [Header("Prefabs & Settings")]
    [Tooltip("Prefab que contiene RandomShapeRoomGenerator, RoomController y sus Tilemaps")]
    public GameObject roomPrefab;
    [Tooltip("Prefab que contiene CorridorsGenerator")]
    public GameObject corridorPrefab;

    [Tooltip("Número total de habitaciones a generar")]
    public int roomCount = 9;

    [Tooltip("Espacio en unidades mundo entre el centro de cada habitación")]
    public Vector2 spacing = new Vector2(20f, 16f);

    [Header("Ejecución")]
    [Tooltip("¿Generar automáticamente al iniciar la escena?")]
    public bool generateOnStart = true;

    // Mapa de coordenadas (Vector2Int) a la instancia de RandomShapeRoomGenerator
    private Dictionary<Vector2Int, RandomShapeRoomGenerator> placedRooms = new Dictionary<Vector2Int, RandomShapeRoomGenerator>();

    private void Start()
    {
        if (generateOnStart)
        {
            ClearDungeon();
            GenerateDungeon();
        }
    }

    [ContextMenu("Generate Dungeon")]
    public void GenerateDungeon()
    {
        ClearDungeon();
        placedRooms.Clear();

        if (roomPrefab == null || corridorPrefab == null)
        {
            Debug.LogError("Asignar roomPrefab y corridorPrefab en el inspector.");
            return;
        }

        // 1) Generar un conjunto de coordenadas orgánicas en un "cluster"
        var occupied = new HashSet<Vector2Int>();
        var frontier = new HashSet<Vector2Int>();

        occupied.Add(Vector2Int.zero);
        AddNeighborsToFrontier(Vector2Int.zero, occupied, frontier);

        for (int i = 1; i < roomCount; i++)
        {
            if (frontier.Count == 0)
            {
                Debug.LogWarning("No quedan posiciones libres para nuevas habitaciones.");
                break;
            }

            Vector2Int[] frontierArray = new Vector2Int[frontier.Count];
            frontier.CopyTo(frontierArray);
            Vector2Int chosen = frontierArray[Random.Range(0, frontierArray.Length)];

            occupied.Add(chosen);
            frontier.Remove(chosen);
            AddNeighborsToFrontier(chosen, occupied, frontier);
        }

        // 2) Instanciar habitaciones en cada coordenada generada
        foreach (var coord in occupied)
        {
            Vector3 worldPos = new Vector3(coord.x * spacing.x, coord.y * spacing.y, 0f);
            GameObject roomGO = Instantiate(roomPrefab, worldPos, Quaternion.identity, transform);
            roomGO.name = $"Room_{coord.x}_{coord.y}";

            var gen = roomGO.GetComponent<RandomShapeRoomGenerator>();
            if (gen == null)
            {
                Debug.LogWarning($"El prefab de sala no tiene RandomShapeRoomGenerator en {roomGO.name}");
                DestroyImmediate(roomGO);
                continue;
            }

            var controller = roomGO.GetComponent<RoomController>();
            if (controller == null)
            {
                Debug.LogWarning($"El prefab de sala no tiene RoomController en {roomGO.name}");
                DestroyImmediate(roomGO);
                continue;
            }

            // Asignamos el tipo de sala ANTES de llamar a InitRoomLogic() y GenerateRoom()
            if (coord == Vector2Int.zero)
                controller.roomType = RoomController.RoomType.Start;
            else
                controller.roomType = RoomController.RoomType.Enemy;

            placedRooms[coord] = gen;
        }

        // Identificar la “última sala” para poner el boss (la más lejana de (0,0))
        Vector2Int bossCoord = Vector2Int.zero;
        int maxDist = -1;
        foreach (var coord in placedRooms.Keys)
        {
            int dist = Mathf.Abs(coord.x) + Mathf.Abs(coord.y);
            if (dist > maxDist && coord != Vector2Int.zero)
            {
                maxDist = dist;
                bossCoord = coord;
            }
        }
        if (bossCoord != Vector2Int.zero && placedRooms.ContainsKey(bossCoord))
        {
            var bossRoomGO = placedRooms[bossCoord].gameObject;
            var bossController = bossRoomGO.GetComponent<RoomController>();
            if (bossController != null)
                bossController.roomType = RoomController.RoomType.Boss;
        }

        // 3) Configurar puertas de cada habitación según vecinos adyacentes y GENERAR cada sala
        foreach (var kv in placedRooms)
        {
            Vector2Int coord = kv.Key;
            var gen = kv.Value;
            var controller = gen.GetComponent<RoomController>();

            // Abrir puertas según vecinos
            gen.openUp = placedRooms.ContainsKey(coord + Vector2Int.up);
            gen.openDown = placedRooms.ContainsKey(coord + Vector2Int.down);
            gen.openLeft = placedRooms.ContainsKey(coord + Vector2Int.left);
            gen.openRight = placedRooms.ContainsKey(coord + Vector2Int.right);

            // 3.1) Inicializar lógica de la sala (Start/Enemy/Boss)
            controller.InitRoomLogic();

            // 3.2) Generar la geometría de la sala (muros, puertas, suelo, etc.)
            gen.GenerateRoom();

            // 3.3) Registrar listeners (solo en Enemy/Boss)
            if (controller.roomLogic != null)
                controller.roomLogic.RegisterListeners();
        }

        // 4) Forzar spawn del jugador en la Start Room (por si OnRoomGenerated no se disparó)
        foreach (var kv in placedRooms)
        {
            if (kv.Key == Vector2Int.zero) // sala (0,0) es Start
            {
                var controller = kv.Value.GetComponent<RoomController>();
                var startLogic = controller.roomLogic as StartRoomLogic;
                if (startLogic != null)
                {
                    startLogic.SpawnPlayerInside();
                }
                break;
            }
        }

        // 5) Instanciar pasillos entre cada par de habitaciones adyacentes
        foreach (var kv in placedRooms)
        {
            Vector2Int coord = kv.Key;
            var srcGen = kv.Value;

            Vector2Int rightCoord = coord + Vector2Int.right;
            if (placedRooms.ContainsKey(rightCoord))
            {
                var dstGen = placedRooms[rightCoord];
                CreateCorridorBetween(srcGen, dstGen);
            }

            Vector2Int upCoord = coord + Vector2Int.up;
            if (placedRooms.ContainsKey(upCoord))
            {
                var dstGen = placedRooms[upCoord];
                CreateCorridorBetween(srcGen, dstGen);
            }
        }
    }

    private void AddNeighborsToFrontier(Vector2Int pos, HashSet<Vector2Int> occupied, HashSet<Vector2Int> frontier)
    {
        var directions = new[] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var d in directions)
        {
            Vector2Int neighbor = pos + d;
            if (!occupied.Contains(neighbor))
                frontier.Add(neighbor);
        }
    }

    private void CreateCorridorBetween(RandomShapeRoomGenerator src, RandomShapeRoomGenerator dst)
    {
        Vector2Int coordA = new Vector2Int(
            Mathf.RoundToInt(src.transform.position.x / spacing.x),
            Mathf.RoundToInt(src.transform.position.y / spacing.y));
        Vector2Int coordB = new Vector2Int(
            Mathf.RoundToInt(dst.transform.position.x / spacing.x),
            Mathf.RoundToInt(dst.transform.position.y / spacing.y));
        Vector2Int delta = coordB - coordA;

        List<Vector2Int> doorsA = null, doorsB = null;
        if (delta == Vector2Int.right)
        {
            doorsA = src.RightDoors;
            doorsB = dst.LeftDoors;
        }
        else if (delta == Vector2Int.left)
        {
            doorsA = src.LeftDoors;
            doorsB = dst.RightDoors;
        }
        else if (delta == Vector2Int.up)
        {
            doorsA = src.UpDoors;
            doorsB = dst.DownDoors;
        }
        else if (delta == Vector2Int.down)
        {
            doorsA = src.DownDoors;
            doorsB = dst.UpDoors;
        }
        else
        {
            return;
        }

        if (doorsA == null || doorsB == null || doorsA.Count == 0 || doorsB.Count == 0)
            return;

        Vector2Int offsetA = new Vector2Int(
            Mathf.RoundToInt(src.transform.position.x),
            Mathf.RoundToInt(src.transform.position.y));
        Vector2Int offsetB = new Vector2Int(
            Mathf.RoundToInt(dst.transform.position.x),
            Mathf.RoundToInt(dst.transform.position.y));

        int midA = doorsA.Count / 2;
        int midB = doorsB.Count / 2;
        Vector2Int localA = doorsA[midA];
        Vector2Int localB = doorsB[midB];

        Vector2Int globalA = offsetA + localA;
        Vector2Int globalB = offsetB + localB;

        GameObject corridorGO = Instantiate(corridorPrefab, Vector3.zero, Quaternion.identity, transform);
        corridorGO.name = $"Corridor_{globalA.x}_{globalA.y}_to_{globalB.x}_{globalB.y}";

        var corridorGen = corridorGO.GetComponent<CorridorsGenerator>();
        if (corridorGen == null)
        {
            Debug.LogWarning($"El prefab de pasillo no tiene CorridorsGenerator en {corridorGO.name}");
            DestroyImmediate(corridorGO);
            return;
        }

        corridorGen.SetEndpoints(globalA, globalB);
        corridorGen.GenerateCorridor();
    }

    [ContextMenu("Clear Dungeon")]
    public void ClearDungeon()
    {
        // 1) Borrar habitaciones y pasillos
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i).gameObject);
        }

        // 2) Borrar jugador si existe
        GameObject player = GameObject.Find("Player");
        if (player != null)
        {
#if UNITY_EDITOR
            DestroyImmediate(player);
#else
        Destroy(player);
#endif
        }
    }

}
