// GameManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using UnityEngine.UI;
using DG.Tweening;

public enum UIGameState
{
    MainMenu,
    Playing,
    Paused,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Prefab del cursor")]
    [SerializeField] private GameObject cursorPrefab;

    private GameObject currentCursor;
    private Canvas pointerCanvas;

    public GameState CurrentState { get; private set; }
    public event Action<GameState> OnGameStateChanged;

    // --- Estas dos referencias las podremos usar para spawnear al jugador ---
    private StartRoomLogic startRoom;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        DOTween.SetTweensCapacity(1000, 200);
    }

    private void Start()
    {
        CreatePointerCanvas();
        SpawnCursor();
        ChangeState(GameState.MainMenu);
    }

    private void CreatePointerCanvas()
    {
        if (pointerCanvas != null) return;

        GameObject canvasGO = new GameObject("PointerCanvas");
        pointerCanvas = canvasGO.AddComponent<Canvas>();
        pointerCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // Muy importante: que realmente ordene por encima
        pointerCanvas.overrideSorting = true;
        pointerCanvas.sortingOrder = 32760;


        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);


        var raycaster = canvasGO.AddComponent<GraphicRaycaster>();
        raycaster.enabled = false;

        DontDestroyOnLoad(canvasGO);
    }

    private void SpawnCursor()
    {
        if (cursorPrefab == null)
        {
            Debug.LogWarning("Cursor Prefab no asignado en GameManager.");
            return;
        }

        currentCursor = Instantiate(cursorPrefab, pointerCanvas.transform);
        currentCursor.transform.SetAsLastSibling();

        Cursor.visible = false;
    }

    private void Update()
    {
        MousePosition();
    }

    void MousePosition()
    {
        if (currentCursor != null)
        {
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 0f;

            RectTransform rectTransform = currentCursor.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.position = mousePos;
            }
            else
            {
                Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
                worldPos.z = 0f;
                currentCursor.transform.position = worldPos;
            }
        }
    }

    public void ChangeState(GameState newState)
    {
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);
    }

    public void LoadScene(string sceneName)
    {
        Debug.Log("➔ Cargando escena: " + sceneName);
        ChangeState(GameState.Playing);
        SceneManager.LoadSceneAsync(sceneName);
    }

    // ---------------------------------------------------------------------------
    // NUEVAS FUNCIONALIDADES: gestión de sala de inicio y lecturas de salas liberadas
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Llamado por StartRoomLogic.OnRoomGenerated() para avisar que esta sala es la de inicio.
    /// Aquí almacenamos la referencia y spawneamos al jugador de inmediato.
    /// </summary>
    public void SetStartRoom(StartRoomLogic start)
    {
        startRoom = start;

        // Tan pronto se recibe la sala inicial, instanciamos al jugador.
        startRoom.SpawnPlayerInside();

        // Nos suscribimos a su evento OnRoomCleared por si más adelante queremos reaccionar.
        startRoom.OnRoomCleared += OnRoomClearedGlobal;
    }

    /// <summary>
    /// Método público para que cualquier sala (EnemyRoomLogic o BossRoomLogic) invoque
    /// RaiseRoomCleared(), que acaba llamando a esto. Podemos usarlo para, por ejemplo,
    /// abrir puertas bloqueadas en toda la mazmorra o chequear si fue la sala del boss.
    /// </summary>
    public void OnRoomClearedGlobal(RoomLogicBase room)
    {
        // Si la sala que se limpió es BossRoomLogic, podemos finalizar la mazmorra
        if (room is BossRoomLogic)
        {
            Debug.Log("¡Mazmorra completada! El boss ha muerto.");
            // Aquí podrías cambiar de escena, mostrar pantalla de victoria, etc.
        }
        else if (room is EnemyRoomLogic)
        {
            Debug.Log("Sala de enemigos limpia: " + room.gameObject.name);
            // Por ejemplo, habilitar recompensas locales o puertas específicas.
        }
        else if (room is StartRoomLogic)
        {
            Debug.Log("Sala inicial marcada como limpia.");
        }
    }
}
