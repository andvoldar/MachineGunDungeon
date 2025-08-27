// Assets/_Scripts/UI/PauseManager.cs
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Pausa con ducking RELATIVO al volumen base, LPF Snapshot y overlay.
/// Bloquea la pausa en escenas no jugables. Integra con UIManager para fades.
/// </summary>
public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance { get; private set; }

    [Header("Activation")]
    [SerializeField] private KeyCode pauseKey = KeyCode.P;

    [Header("Disable Pause In These Scenes")]
    [SerializeField] private string[] pauseDisabledScenes = new[] { "StartMenu" };

    [Header("UI References")]
    [SerializeField] private Canvas pauseCanvas;
    [SerializeField] private GameObject pauseMenuRoot;
    [SerializeField] private Image darkenOverlay;
    [SerializeField] private CanvasGroup pauseMenuCanvasGroup;

    [Header("Look & Feel")]
    [SerializeField, Range(0f, 1f)] private float darkenTargetAlpha = 0.7f;
    [SerializeField] private float fadeDuration = 0.25f;

    [Header("FMOD Music Ducking (relative)")]
    [SerializeField] private string musicBusPath = "bus:/Music";
    [SerializeField, Range(0f, 1f)] private float musicDuckMultiplier = 0.25f; // relativo al base
    [SerializeField] private float musicFadeTime = 0.25f;

    [Header("FMOD Low-Pass Snapshot")]
    [SerializeField] private EventReference pauseSnapshot; // snapshot:/PauseLPF
    [SerializeField] private bool usePauseSnapshot = true;

    [Header("Main Menu")]
    [SerializeField] private string mainMenuSceneName = "StartMenu";
    [SerializeField] private float sceneFadeOutTime = 0.6f;

    public bool IsPaused { get; private set; }
    public float MusicDuckMultiplier => musicDuckMultiplier;

    private Bus _musicBus;
    private Coroutine _fadeRoutine;

    private EventInstance _snapshotInstance;
    private bool _snapshotPlaying;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicBus = RuntimeManager.GetBus(musicBusPath);

        InitUIStates();
        EnsurePauseCanvasOnTop();

        // Asegura que tras cualquier carga de escena el pause queda oculto
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // Por si cambiamos de escena en medio de una pausa o quedaron refs activas
        ForceUnpauseAndHide();
        EnsurePauseCanvasOnTop();
    }

    private void InitUIStates()
    {
        if (darkenOverlay) SetImageAlpha(darkenOverlay, 0f);
        if (pauseMenuRoot) pauseMenuRoot.SetActive(false);
        if (pauseMenuCanvasGroup)
        {
            pauseMenuCanvasGroup.alpha = 0f;
            pauseMenuCanvasGroup.interactable = false;
            pauseMenuCanvasGroup.blocksRaycasts = false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pauseKey))
        {
            if (!IsPauseAllowed()) return;
            TogglePause();
        }
    }

    private bool IsPauseAllowed()
    {
        var scene = SceneManager.GetActiveScene().name;
        for (int i = 0; i < pauseDisabledScenes.Length; i++)
            if (string.Equals(scene, pauseDisabledScenes[i], System.StringComparison.Ordinal))
                return false;
        return true;
    }

    public void TogglePause()
    {
        if (IsPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        if (IsPaused) return;
        IsPaused = true;

        EnsurePauseCanvasOnTop();

        // UI (overlay+menú) con fade
        if (pauseMenuRoot) pauseMenuRoot.SetActive(true);
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeUI(true));

        // Volúmenes efectivos (base * duck)
        AudioSettings.Instance?.ApplyAll();

        // LPF snapshot
        StartPauseSnapshot();

        // Congelar
        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ResumeGame()
    {
        if (!IsPaused) return;
        IsPaused = false;

        // UI fade out
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeUI(false));

        // Restaurar volúmenes efectivos (base)
        AudioSettings.Instance?.ApplyAll();

        // Parar snapshot
        StopPauseSnapshot();

        // Reanudar
        Time.timeScale = 1f;
        // Cursor.visible = false;
        // Cursor.lockState = CursorLockMode.Locked;
    }


    public void ForceUnpauseAndHide()
    {
        // Estado lógico
        IsPaused = false;

        // Tiempo normal
        if (Time.timeScale == 0f) Time.timeScale = 1f;

        // Audio: parar snapshot y aplicar base
        StopPauseSnapshot();
        AudioSettings.Instance?.ApplyAll();

        // UI: ocultar y resetear
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);

        if (darkenOverlay) SetImageAlpha(darkenOverlay, 0f);

        if (pauseMenuCanvasGroup)
        {
            pauseMenuCanvasGroup.alpha = 0f;
            pauseMenuCanvasGroup.interactable = false;
            pauseMenuCanvasGroup.blocksRaycasts = false;
        }

        if (pauseMenuRoot && pauseMenuRoot.activeSelf)
            pauseMenuRoot.SetActive(false);
    }

    private IEnumerator FadeUI(bool show)
    {
        float t = 0f;
        float startOverlay = darkenOverlay ? darkenOverlay.color.a : 0f;
        float endOverlay = show ? darkenTargetAlpha : 0f;

        float startMenu = pauseMenuCanvasGroup ? pauseMenuCanvasGroup.alpha : 0f;
        float endMenu = show ? 1f : 0f;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);

            if (darkenOverlay)
            {
                var c = darkenOverlay.color;
                c.a = Mathf.Lerp(startOverlay, endOverlay, k);
                darkenOverlay.color = c;
            }

            if (pauseMenuCanvasGroup)
            {
                pauseMenuCanvasGroup.alpha = Mathf.Lerp(startMenu, endMenu, k);
                pauseMenuCanvasGroup.interactable = show;
                pauseMenuCanvasGroup.blocksRaycasts = show;
            }

            yield return null;
        }

        if (darkenOverlay) { var c = darkenOverlay.color; c.a = endOverlay; darkenOverlay.color = c; }
        if (pauseMenuCanvasGroup) { pauseMenuCanvasGroup.alpha = endMenu; pauseMenuCanvasGroup.interactable = show; pauseMenuCanvasGroup.blocksRaycasts = show; }

        if (!show && pauseMenuRoot) pauseMenuRoot.SetActive(false);
    }

    private void EnsurePauseCanvasOnTop()
    {
        if (!pauseCanvas) return;
        pauseCanvas.overrideSorting = true;
        pauseCanvas.sortingOrder = 9999;

        if (darkenOverlay) darkenOverlay.raycastTarget = false;
        if (darkenOverlay) darkenOverlay.transform.SetAsFirstSibling();
        if (pauseMenuRoot) pauseMenuRoot.transform.SetAsLastSibling();
    }

    private void StartPauseSnapshot()
    {
        if (!usePauseSnapshot || pauseSnapshot.IsNull) return;
        if (_snapshotPlaying) return;
        _snapshotInstance = RuntimeManager.CreateInstance(pauseSnapshot);
        _snapshotInstance.start();
        _snapshotPlaying = true;
    }

    private void StopPauseSnapshot()
    {
        if (!_snapshotPlaying) return;
        if (_snapshotInstance.isValid())
        {
            _snapshotInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            _snapshotInstance.release();
        }
        _snapshotPlaying = false;
    }

    // ---- Botones ----
    public void OnContinuePressed() => ResumeGame();

    public void OnOptionsPressed(GameObject pauseButtonsPanel, GameObject optionsPanel)
    {
        if (optionsPanel == null) return;
        pauseButtonsPanel?.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OnMainMenuPressed()
    {
        // 🔒 Limpia estado de pausa ANTES de salir
        ForceUnpauseAndHide();

        // Fade out + Load scene (UIManager hará fade-in al entrar)
        if (UIManager.Instance != null)
        {
            UIManager.Instance.FadeOutAndLoadScene(mainMenuSceneName, sceneFadeOutTime);
        }
        else
        {
            SceneManager.LoadScene(mainMenuSceneName);
        }
    }

    // ---- Utils ----
    private void SetImageAlpha(Image img, float a)
    {
        var c = img.color; c.a = a; img.color = c;
    }
}
