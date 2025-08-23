// RoomLogicBase.cs
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RandomShapeRoomGenerator))]
public abstract class RoomLogicBase : MonoBehaviour
{
    public RandomShapeRoomGenerator generator;

    /// <summary>
    /// Prefab de antorcha y offsets, heredados del RoomController.
    /// </summary>
    [HideInInspector] public GameObject torchPrefab;
    [HideInInspector] public int torchOffsetMin;
    [HideInInspector] public int torchOffsetMax;


    [Header("Barrera de energía")]
    [Tooltip("Prefab que se instancia en cada apertura de puerta.")]
    public GameObject energyBarrierPrefab;

    protected List<EnergyBarrier> spawnedBarriers = new List<EnergyBarrier>();
    private bool roomCleared = false;
    public bool IsRoomCleared => roomCleared;

    /// <summary>
    /// Evento que se dispara justo después de que la sala termine de generarse.
    /// </summary>
    public event Action OnRoomGeneratedEvent;

    /// <summary>
    /// Evento que se dispara cuando la sala queda limpia de enemigos/boss.
    /// </summary>
    public event Action<RoomLogicBase> OnRoomCleared;

    /// <summary>
    /// Conjunto de celdas ocupadas (por antorchas, trampas, enemigos, etc.)
    /// </summary>
    protected HashSet<Vector2Int> occupiedCells;

    protected virtual void Awake()
    {
        generator = GetComponent<RandomShapeRoomGenerator>();
    }

    public virtual void OnRoomGenerated()
    {
        occupiedCells = new HashSet<Vector2Int>();
        SpawnTorchesInCorners();
        SpawnEnergyBarriers();
        OnRoomGeneratedEvent?.Invoke();
    }

    public virtual void RegisterListeners() { }
    public virtual void UnregisterListeners() { }

    protected void RaiseRoomCleared()
    {
        roomCleared = true;
        foreach (var b in spawnedBarriers)
            b.DisableBarrier();

        OnRoomCleared?.Invoke(this);
    }

    public virtual void OnPlayerEnteredRoom()
    {
        if (roomCleared) return;
        foreach (var b in spawnedBarriers)
            b.EnableBarrier();
    }

    private void SpawnEnergyBarriers()
    {
        if (energyBarrierPrefab == null || generator == null) return;

        Dictionary<string, List<Vector2Int>> doorGroups = new()
        {
            { "Up", generator.UpDoors },
            { "Down", generator.DownDoors },
            { "Left", generator.LeftDoors },
            { "Right", generator.RightDoors },
        };

        foreach (var pair in doorGroups)
        {
            List<Vector2Int> doorCells = pair.Value;
            if (doorCells == null || doorCells.Count == 0) continue;

            // Calcular centro del hueco de puerta (3 celdas)
            Vector3 avgPos = Vector3.zero;
            foreach (var cell in doorCells)
            {
                Vector3 world = generator.floorTilemap.CellToWorld((Vector3Int)cell);
                avgPos += world + new Vector3(0.5f, 0.5f, 0f);
            }
            avgPos /= doorCells.Count;

            GameObject go = Instantiate(energyBarrierPrefab, avgPos, Quaternion.identity, transform);
            go.name = $"Barrier_{pair.Key}";

            // Escala siempre 3x1
            go.transform.localScale = new Vector3(3f, 1f, 1f);

            // Rotar si es vertical
            if (pair.Key == "Left" || pair.Key == "Right")
                go.transform.rotation = Quaternion.Euler(0f, 0f, 90f);

            var barrier = go.GetComponent<EnergyBarrier>();
            if (barrier != null)
            {
                barrier.DisableBarrier();
                spawnedBarriers.Add(barrier);
            }
        }
    }



    /// <summary>
    /// Spawnea antorchas en las cuatro esquinas de la habitación con un mismo offset aleatorio
    /// entre torchOffsetMin y torchOffsetMax. Todas las antorchas usan el mismo offset.
    /// Marca las celdas donde se colocan para que no se instancien otros objetos encima.
    /// </summary>
    private void SpawnTorchesInCorners()
    {
        if (torchPrefab == null || generator == null || generator.FloorPositions == null || generator.floorTilemap == null)
            return;

        var floorSet = generator.FloorPositions;
        if (floorSet.Count == 0) return;

        // Calcular bounds en coordenadas de celda
        int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
        foreach (var cell in floorSet)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y < minY) minY = cell.y;
            if (cell.y > maxY) maxY = cell.y;
        }

        // Escoger offset único para todas las esquinas
        int offset = UnityEngine.Random.Range(torchOffsetMin, torchOffsetMax + 1);

        // Definir esquinas base (sin offset)
        Vector2Int[] baseCorners = {
            new Vector2Int(minX, minY), // bottom-left
            new Vector2Int(maxX, minY), // bottom-right
            new Vector2Int(minX, maxY), // top-left
            new Vector2Int(maxX, maxY)  // top-right
        };

        foreach (var corner in baseCorners)
        {
            Vector2Int dirX = (corner.x == minX) ? Vector2Int.right : Vector2Int.left;
            Vector2Int dirY = (corner.y == minY) ? Vector2Int.up : Vector2Int.down;

            Vector2Int torchCell = corner + dirX * offset + dirY * offset;

            // Si torchCell no está en floorSet, recortarlo dentro de bounds
            if (!floorSet.Contains(torchCell))
            {
                torchCell.x = Mathf.Clamp(torchCell.x, minX, maxX);
                torchCell.y = Mathf.Clamp(torchCell.y, minY, maxY);
            }

            Vector3 worldOrigin = generator.floorTilemap.CellToWorld(new Vector3Int(torchCell.x, torchCell.y, 0));
            Vector3 spawnPos = worldOrigin + new Vector3(0.5f, 0.5f, 0f);

            GameObject torchGO = Instantiate(torchPrefab, spawnPos, Quaternion.identity, transform);
            torchGO.name = $"Torch_{torchCell.x}_{torchCell.y}";

            occupiedCells.Add(torchCell);
        }
    }
}
