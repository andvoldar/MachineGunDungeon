using UnityEngine;

[RequireComponent(typeof(RandomShapeRoomGenerator))]
[RequireComponent(typeof(BoxCollider2D))]
public class RoomController : MonoBehaviour
{
    public enum RoomType
    {
        Start,
        Enemy,
        Boss
    }

    [Tooltip("Tipo de sala: Start, Enemy o Boss.")]
    public RoomType roomType = RoomType.Enemy;

    [Header("Prefabs de Jugador y Arma")]
    public GameObject playerPrefab;
    public GameObject initialWeaponPrefab;

    [Header("Prefabs de Enemigos/Boss")]
    public GameObject[] enemyPrefabs;
    public int enemyCount = 3;
    public GameObject bossPrefab;

    [Header("Prefab de Antorcha")]
    public GameObject torchPrefab;
    public int torchOffsetMin = 2;
    public int torchOffsetMax = 5;

    [Header("Configuración de Trampas")]
    public GameObject floorTrapPrefab;
    public int trapCountMin = 2;
    public int trapCountMax = 5;
    [Range(0f, 1f)] public float trapProbability = 0.75f;

    [Header("Objetos Destruibles")]
    public GameObject[] destructiblePrefabs;
    public int destructibleCountMin = 3;
    public int destructibleCountMax = 8;

    [Header("Barreras de Energía")]
    public GameObject energyBarrierPrefab;

    [HideInInspector] public RoomLogicBase roomLogic;

    private bool weaponSpawned = false;
    private bool playerInside = false;
    private bool roomVisited = false;

    private void Start()
    {
        SetupRoomBoundsTrigger();
    }

    public void InitRoomLogic()
    {
        if (roomLogic != null) return;

        var gen = GetComponent<RandomShapeRoomGenerator>();

        switch (roomType)
        {
            case RoomType.Start:
                var startLogic = gameObject.AddComponent<StartRoomLogic>();
                startLogic.playerPrefab = playerPrefab;
                startLogic.destructiblePrefabs = destructiblePrefabs;
                startLogic.destructibleCountMin = destructibleCountMin;
                startLogic.destructibleCountMax = destructibleCountMax;
                startLogic.torchPrefab = torchPrefab;
                startLogic.torchOffsetMin = torchOffsetMin;
                startLogic.torchOffsetMax = torchOffsetMax;
                startLogic.energyBarrierPrefab = null;
                startLogic.generator = gen;
                roomLogic = startLogic;
                break;

            case RoomType.Enemy:
                var enemyLogic = gameObject.AddComponent<EnemyRoomLogic>();
                enemyLogic.enemyPrefabs = enemyPrefabs;
                enemyLogic.enemyCount = enemyCount;
                enemyLogic.destructiblePrefabs = destructiblePrefabs;
                enemyLogic.destructibleCountMin = destructibleCountMin;
                enemyLogic.destructibleCountMax = destructibleCountMax;
                enemyLogic.torchPrefab = torchPrefab;
                enemyLogic.torchOffsetMin = torchOffsetMin;
                enemyLogic.torchOffsetMax = torchOffsetMax;
                enemyLogic.floorTrapPrefab = floorTrapPrefab;
                enemyLogic.trapCountMin = trapCountMin;
                enemyLogic.trapCountMax = trapCountMax;
                enemyLogic.trapProbability = trapProbability;
                enemyLogic.energyBarrierPrefab = energyBarrierPrefab;
                enemyLogic.generator = gen;
                roomLogic = enemyLogic;
                break;

            case RoomType.Boss:
                var bossLogic = gameObject.AddComponent<BossRoomLogic>();
                bossLogic.bossPrefab = bossPrefab;
                bossLogic.destructiblePrefabs = destructiblePrefabs;
                bossLogic.destructibleCountMin = destructibleCountMin;
                bossLogic.destructibleCountMax = destructibleCountMax;
                bossLogic.torchPrefab = torchPrefab;
                bossLogic.torchOffsetMin = torchOffsetMin;
                bossLogic.torchOffsetMax = torchOffsetMax;
                bossLogic.floorTrapPrefab = floorTrapPrefab;
                bossLogic.trapCountMin = trapCountMin;
                bossLogic.trapCountMax = trapCountMax;
                bossLogic.trapProbability = trapProbability;
                bossLogic.energyBarrierPrefab = energyBarrierPrefab;
                bossLogic.generator = gen;
                roomLogic = bossLogic;
                break;
        }

        if (roomType == RoomType.Start && roomLogic != null)
            roomLogic.OnRoomGeneratedEvent += SpawnInitialWeapon;
    }

    private void SpawnInitialWeapon()
    {
        if (weaponSpawned || initialWeaponPrefab == null) return;
        var gen = GetComponent<RandomShapeRoomGenerator>();
        Vector2Int center = gen.RoomCenter;
        Vector3 world = gen.floorTilemap.CellToWorld(new Vector3Int(center.x, center.y, 0)) + new Vector3(0.5f, 0.5f, 0);
        Instantiate(initialWeaponPrefab, world, Quaternion.identity, transform);
        weaponSpawned = true;
    }

    private void SetupRoomBoundsTrigger()
    {
        var gen = GetComponent<RandomShapeRoomGenerator>();
        if (gen == null || gen.FloorPositions == null || gen.FloorPositions.Count == 0)
            return;

        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var cell in gen.FloorPositions)
        {
            if (cell.x < minX) minX = cell.x;
            if (cell.x > maxX) maxX = cell.x;
            if (cell.y < minY) minY = cell.y;
            if (cell.y > maxY) maxY = cell.y;
        }

        Vector3Int cellMin = new Vector3Int(minX, minY, 0);
        Vector3Int cellMax = new Vector3Int(maxX + 1, maxY + 1, 0); // +1 para incluir última fila/col

        Vector3 worldMin = gen.floorTilemap.CellToWorld(cellMin);
        Vector3 worldMax = gen.floorTilemap.CellToWorld(cellMax);

        Vector3 center = (worldMin + worldMax) / 2f;
        Vector3 size = worldMax - worldMin;

        var collider = GetComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.offset = transform.InverseTransformPoint(center);
        collider.size = size;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !playerInside)
        {
            playerInside = true;

            if (!roomVisited && roomLogic != null)
                roomLogic.OnPlayerEnteredRoom();

            roomVisited = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInside = false;
        }
    }

    private void OnDestroy()
    {
        if (roomLogic != null)
        {
            roomLogic.OnRoomGeneratedEvent -= SpawnInitialWeapon;
            roomLogic.UnregisterListeners();
        }
    }
}
