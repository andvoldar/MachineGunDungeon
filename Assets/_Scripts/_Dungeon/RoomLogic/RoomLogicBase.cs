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

    [Header("SFX")]
    [SerializeField, Tooltip("Sonido al 'aparecer' cada barrera (por puerta).")]
    private SoundType barrierAppearSound = SoundType.EnergyBarrierSFX;

    [SerializeField, Tooltip("Sonido al limpiar la habitación.")]
    private SoundType roomClearedSound = SoundType.RoomClearedSFX;

    protected List<EnergyBarrier> spawnedBarriers = new List<EnergyBarrier>();

    // Estado de limpieza
    private bool roomCleared = false;
    public bool IsRoomCleared => roomCleared;

    // ✅ Control: solo sonará RoomCleared si hubo enemigos reales
    private bool hadEnemies = false;
    private int aliveEnemies = 0;

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

    // ===== API para lógica de enemigos =====

    /// <summary>
    /// Llama esto cada vez que **instancies** un enemigo en esta sala.
    /// </summary>
    public void NotifyEnemySpawned()
    {
        hadEnemies = true;
        aliveEnemies++;
    }

    /// <summary>
    /// Llama esto cuando un enemigo de esta sala **muera**.
    /// </summary>
    public void NotifyEnemyDied()
    {
        if (aliveEnemies > 0) aliveEnemies--;
        if (aliveEnemies == 0) RaiseRoomCleared();
    }

    /// <summary>
    /// Si tu generador determina que **no habrá** enemigos, puedes marcar como despejada sin SFX.
    /// </summary>
    protected void MarkNoEnemiesAndClearSilently()
    {
        if (roomCleared) return;
        hadEnemies = false;
        roomCleared = true;
        foreach (var b in spawnedBarriers) b.DisableBarrier();
        OnRoomCleared?.Invoke(this);
    }

    /// <summary>
    /// Dispara el "room cleared" con guardas internas.
    /// Solo reproduce SFX si hubo combate real.
    /// </summary>
    protected void RaiseRoomCleared()
    {
        if (roomCleared) return;
        roomCleared = true;

        if (hadEnemies)
        {
            // SFX de sala despejada en el centro de la sala
            if (SoundManager.Instance != null && generator != null && generator.floorTilemap != null)
            {
                Vector3 centerWorld = generator.floorTilemap.CellToWorld(
                    new Vector3Int(generator.RoomCenter.x, generator.RoomCenter.y, 0)
                ) + new Vector3(0.5f, 0.5f, 0f);

                SoundManager.Instance.PlaySound(roomClearedSound, centerWorld);
            }
        }

        foreach (var b in spawnedBarriers) b.DisableBarrier();
        OnRoomCleared?.Invoke(this);
    }

    // ===== Entrada del jugador y barreras =====

    public virtual void OnPlayerEnteredRoom(Transform playerTransform)
    {
        if (roomCleared || _barriersActivated) return;
        StartCoroutine(ActivateBarriersWhenPlayerIsInsideRoutine(playerTransform));
    }

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

        Vector3 doorCenterWorld, inwardDir;
        if (!TryGetNearestDoorCenterAndInward(playerT.position, out doorCenterWorld, out inwardDir))
        {
            yield return StartCoroutine(AppearAllBarriersRoutine());
            yield break;
        }

        float tileWorld = Mathf.Abs(generator.floorTilemap.cellSize.x) > 0.0001f
            ? generator.floorTilemap.cellSize.x
            : 1f;
        float threshold = tileWorld;

        while (playerT != null)
        {
            Vector3 toPlayer = playerT.position - doorCenterWorld;
            float along = Vector3.Dot(toPlayer, inwardDir);
            if (along >= threshold) break;
            yield return null;
        }

        yield return StartCoroutine(AppearAllBarriersRoutine());
        _barriersActivated = true;
        OnBarriersActivated();
    }

    private IEnumerator AppearAllBarriersRoutine()
    {
        foreach (var b in spawnedBarriers)
        {
            if (b != null)
            {
                if (SoundManager.Instance != null)
                    SoundManager.Instance.PlaySound(barrierAppearSound, b.transform.position);

                StartCoroutine(b.Appear(barrierAppearDuration, true, barrierSpawnVFXPrefab));
            }
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

        Vector3 centerWorld = generator.floorTilemap.CellToWorld(
            new Vector3Int(generator.RoomCenter.x, generator.RoomCenter.y, 0)
        ) + new Vector3(0.5f, 0.5f, 0f);

        var doorGroups = new List<List<Vector2Int>> {
            generator.UpDoors, generator.DownDoors, generator.LeftDoors, generator.RightDoors
        };

        float bestDistSqr = float.MaxValue;
        bool found = false;

        foreach (var group in doorGroups)
        {
            if (group == null || group.Count == 0) continue;

            Vector3 avg = Vector3.zero;
            foreach (var c in group)
                avg += generator.floorTilemap.CellToWorld((Vector3Int)c) + new Vector3(0.5f, 0.5f, 0f);
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

            Vector3 avgPos = Vector3.zero;
            foreach (var cell in doorCells)
            {
                Vector3 world = generator.floorTilemap.CellToWorld((Vector3Int)cell);
                avgPos += world + new Vector3(0.5f, 0.5f, 0f);
            }
            avgPos /= doorCells.Count;

            GameObject go = Instantiate(energyBarrierPrefab, avgPos, Quaternion.identity, transform);
            go.name = $"Barrier_{pair.Key}";

            go.transform.localScale = new Vector3(3f, 1f, 1f);
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
