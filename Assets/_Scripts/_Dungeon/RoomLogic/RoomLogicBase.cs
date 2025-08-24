// Assets/_Scripts/Rooms/RoomLogicBase.cs
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RandomShapeRoomGenerator))]
public abstract class RoomLogicBase : MonoBehaviour
{
    public RandomShapeRoomGenerator generator;

    [HideInInspector] public GameObject torchPrefab;
    [HideInInspector] public int torchOffsetMin;
    [HideInInspector] public int torchOffsetMax;

    [Header("Barrera de energía")]
    [Tooltip("Prefab que se instancia en cada apertura de puerta.")]
    public GameObject energyBarrierPrefab;

    [Tooltip("Duración del 'appear' visual antes de habilitar el collider.")]
    public float barrierAppearDuration = 0.3f;

    [Tooltip("VFX opcional al aparecer cada barrera.")]
    public GameObject barrierSpawnVFXPrefab;

    protected List<EnergyBarrier> spawnedBarriers = new List<EnergyBarrier>();
    private bool roomCleared = false;
    public bool IsRoomCleared => roomCleared;

    public event Action OnRoomGeneratedEvent;
    public event Action<RoomLogicBase> OnRoomCleared;

    protected HashSet<Vector2Int> occupiedCells;

    // Control interno para no reactivar en la misma visita
    private bool _barriersActivated = false;

    protected virtual void Awake()
    {
        generator = GetComponent<RandomShapeRoomGenerator>();
    }

    public virtual void OnRoomGenerated()
    {
        occupiedCells = new HashSet<Vector2Int>();
        SpawnTorchesInCorners();
        SpawnEnergyBarriers(); // crea pero deja desactivadas
        OnRoomGeneratedEvent?.Invoke();
    }

    public virtual void RegisterListeners() { }
    public virtual void UnregisterListeners() { }

    protected void RaiseRoomCleared()
    {
        roomCleared = true;
        foreach (var b in spawnedBarriers) b.DisableBarrier();
        OnRoomCleared?.Invoke(this);
    }

    /// <summary>
    /// Versión nueva con referencia al player. Espera a que pase 1 tile hacia dentro
    /// desde la puerta más cercana y entonces aparece la barrera y se llama al hook.
    /// </summary>
    public virtual void OnPlayerEnteredRoom(Transform playerTransform)
    {
        if (roomCleared || _barriersActivated) return;
        StartCoroutine(ActivateBarriersWhenPlayerIsInsideRoutine(playerTransform));
    }

    /// <summary>
    /// Conserva compatibilidad si alguien llama la versión antigua sin parámetro.
    /// Usará la posición actual del objeto "Player" en escena si existe.
    /// </summary>
    public virtual void OnPlayerEnteredRoom()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null) OnPlayerEnteredRoom(player.transform);
    }

    /// <summary>
    /// Hook que se dispara justo después de que TODAS las barreras
    /// hayan completado su 'appear' (colliders ON).
    /// </summary>
    protected virtual void OnBarriersActivated() { }

    private IEnumerator ActivateBarriersWhenPlayerIsInsideRoutine(Transform playerT)
    {
        if (playerT == null || generator == null || generator.floorTilemap == null)
            yield break;

        // 1) Localizar puerta más cercana al player (en mundo)
        Vector3 doorCenterWorld, inwardDir;
        if (!TryGetNearestDoorCenterAndInward(playerT.position, out doorCenterWorld, out inwardDir))
        {
            // Si no hay puertas (raro), activa sin esperar
            yield return StartCoroutine(AppearAllBarriersRoutine());
            yield break;
        }

        // 2) Esperar a que cruce 1 tile hacia dentro de la sala
        float tileWorld = Mathf.Abs(generator.floorTilemap.cellSize.x) > 0.0001f
            ? generator.floorTilemap.cellSize.x
            : 1f;
        float threshold = tileWorld; // 1 tile exacto

        while (playerT != null)
        {
            Vector3 toPlayer = playerT.position - doorCenterWorld;
            float along = Vector3.Dot(toPlayer, inwardDir); // proyección dentro
            if (along >= threshold) break;
            yield return null;
        }

        // 3) Aparecer TODAS las barreras (render ON inmediato, collider al final)
        yield return StartCoroutine(AppearAllBarriersRoutine());

        // 4) Hook: ahora ya podemos spawnear enemigos, etc.
        _barriersActivated = true;
        OnBarriersActivated();
    }

    private IEnumerator AppearAllBarriersRoutine()
    {
        foreach (var b in spawnedBarriers)
        {
            if (b != null)
                StartCoroutine(b.Appear(barrierAppearDuration, true, barrierSpawnVFXPrefab));
        }
        if (barrierAppearDuration > 0f)
            yield return new WaitForSeconds(barrierAppearDuration);
        else
            yield return null;
    }

    private bool TryGetNearestDoorCenterAndInward(Vector3 playerWorldPos, out Vector3 doorCenterWorld, out Vector3 inwardDir)
    {
        doorCenterWorld = Vector3.zero;
        inwardDir = Vector3.zero;

        // Centro de la sala en mundo
        Vector3 centerWorld = generator.floorTilemap.CellToWorld(
            new Vector3Int(generator.RoomCenter.x, generator.RoomCenter.y, 0)
        ) + new Vector3(0.5f, 0.5f, 0f);

        // Todas las puertas
        var doorGroups = new List<List<Vector2Int>> {
            generator.UpDoors, generator.DownDoors, generator.LeftDoors, generator.RightDoors
        };

        float bestDistSqr = float.MaxValue;
        bool found = false;

        foreach (var group in doorGroups)
        {
            if (group == null || group.Count == 0) continue;

            // Centro del hueco (promedio de 3 celdas) en mundo
            Vector3 avg = Vector3.zero;
            foreach (var c in group)
            {
                avg += generator.floorTilemap.CellToWorld((Vector3Int)c) + new Vector3(0.5f, 0.5f, 0f);
            }
            avg /= group.Count;

            float d2 = (playerWorldPos - avg).sqrMagnitude;
            if (d2 < bestDistSqr)
            {
                bestDistSqr = d2;
                doorCenterWorld = avg;
                inwardDir = (centerWorld - avg);
                inwardDir.z = 0f;
                if (inwardDir.sqrMagnitude > 0.0001f) inwardDir.Normalize();
                found = true;
            }
        }

        return found;
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

            // Centro del hueco (3 celdas)
            Vector3 avgPos = Vector3.zero;
            foreach (var cell in doorCells)
            {
                Vector3 world = generator.floorTilemap.CellToWorld((Vector3Int)cell);
                avgPos += world + new Vector3(0.5f, 0.5f, 0f);
            }
            avgPos /= doorCells.Count;

            GameObject go = Instantiate(energyBarrierPrefab, avgPos, Quaternion.identity, transform);
            go.name = $"Barrier_{pair.Key}";

            // Escala 3x1; rotación para vertical
            go.transform.localScale = new Vector3(3f, 1f, 1f);
            if (pair.Key == "Left" || pair.Key == "Right")
                go.transform.rotation = Quaternion.Euler(0f, 0f, 90f);

            var barrier = go.GetComponent<EnergyBarrier>();
            if (barrier != null)
            {
                barrier.DisableBarrier(); // arrancan ocultas
                spawnedBarriers.Add(barrier);
            }
        }
    }

    private void SpawnTorchesInCorners()
    {
        if (torchPrefab == null || generator == null || generator.FloorPositions == null || generator.floorTilemap == null)
            return;

        var floorSet = generator.FloorPositions;
        if (floorSet.Count == 0) return;

        int minX = int.MaxValue, maxX = int.MinValue, minY = int.MaxValue, maxY = int.MinValue;
        foreach (var cell in floorSet)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y < minY) minY = cell.y;
            if (cell.y > maxY) maxY = cell.y;
        }

        int offset = UnityEngine.Random.Range(torchOffsetMin, torchOffsetMax + 1);

        Vector2Int[] baseCorners = {
            new Vector2Int(minX, minY),
            new Vector2Int(maxX, minY),
            new Vector2Int(minX, maxY),
            new Vector2Int(maxX, maxY)
        };

        foreach (var corner in baseCorners)
        {
            Vector2Int dirX = (corner.x == minX) ? Vector2Int.right : Vector2Int.left;
            Vector2Int dirY = (corner.y == minY) ? Vector2Int.up : Vector2Int.down;

            Vector2Int torchCell = corner + dirX * offset + dirY * offset;

            if (!floorSet.Contains(torchCell))
            {
                torchCell.x = Mathf.Clamp(torchCell.x, minX, maxX);
                torchCell.y = Mathf.Clamp(torchCell.y, minY, maxY);
            }

            Vector3 worldOrigin = generator.floorTilemap.CellToWorld(new Vector3Int(torchCell.x, torchCell.y, 0));
            Vector3 spawnPos = worldOrigin + new Vector3(0.5f, 0.5f, 0f);

            GameObject torchGO = Instantiate(torchPrefab, spawnPos, Quaternion.identity, transform);
            torchGO.name = $"Torch_{torchCell.x}_{torchCell.y}";

            if (occupiedCells == null) occupiedCells = new HashSet<Vector2Int>();
            occupiedCells.Add(torchCell);
        }
    }
}
