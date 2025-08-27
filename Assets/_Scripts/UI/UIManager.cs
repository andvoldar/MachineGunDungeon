// Assets/_Scripts/UI/UIManager.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Options,
    Controls
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Referencias a pantallas UI (auto-rebind)")]
    [SerializeField] private GameObject startMenuUI;
    [SerializeField] private string startMenuRootName = "StartMenuUI";

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage;       // NEGRA opaca
    [SerializeField] private string fadeImageName = "FadeImage";
    [SerializeField] private float fadeInDuration = 0.6f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;

        RebindSceneReferences();
        HandleGameStateChanged(GameManager.Instance != null ? GameManager.Instance.CurrentState : GameState.MainMenu);

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (fadeImage == null)
            Debug.LogWarning("➔ [UIManager] fadeImage no asignado (usa FadeOutAndLoadScene para transición).");
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RebindSceneReferences();

        // F A D E   I N   SIEMPRE al entrar en una escena
        if (fadeImage != null)
        {
            SetAlpha(1f);
            StartCoroutine(FadeIn());
        }
    }

    private void RebindSceneReferences()
    {
        if (startMenuUI == null)
        {
            var byName = GameObject.Find(startMenuRootName);
            if (byName != null) startMenuUI = byName;
            else
            {
                var sm = FindObjectOfType<StartMenuUI>(true);
                if (sm != null) startMenuUI = sm.gameObject;
            }
        }

        if (fadeImage == null && !string.IsNullOrEmpty(fadeImageName))
        {
            var fi = GameObject.Find(fadeImageName);
            if (fi != null) fadeImage = fi.GetComponent<Image>();
        }
    }

    private void HandleGameStateChanged(GameState state)
    {
        if (state == GameState.MainMenu) SafeSetActive(startMenuUI, true);
        else SafeSetActive(startMenuUI, false);
    }

    private void SafeSetActive(GameObject go, bool active)
    {
        if (go == null || go.Equals(null)) return;
        if (go.activeSelf == active) return;
        go.SetActive(active);
    }

    private IEnumerator FadeIn()
    {
        float t = 0f;
        while (t < fadeInDuration)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(1f, 0f, t / fadeInDuration));
            yield return null;
        }
        SetAlpha(0f);
    }

    private void SetAlpha(float a)
    {
        if (fadeImage == null) return;
        var c = fadeImage.color; c.a = a; fadeImage.color = c;
    }

    // --------- API pública para fade out + load scene ---------
    public void FadeOutAndLoadScene(string sceneName, float fadeOutDuration)
    {
        if (fadeImage == null)
        {
            Debug.LogWarning("[UIManager] No hay fadeImage asignada. Cargando sin transición.");
            SceneManager.LoadScene(sceneName);
            return;
        }
        StartCoroutine(FadeOutThenLoad(sceneName, fadeOutDuration));
    }

    private IEnumerator FadeOutThenLoad(string sceneName, float d)
    {
        // Asegurar negro puro, no “cuadro azul”
        var c = fadeImage.color; c.r = 0; c.g = 0; c.b = 0; fadeImage.color = c;

        float t = 0f;
        while (t < d)
        {
            t += Time.deltaTime;
            SetAlpha(Mathf.Lerp(0f, 1f, t / d));
            yield return null;
        }
        SetAlpha(1f);

        SceneManager.LoadScene(sceneName);
        // El Fade IN se hará en OnSceneLoaded automáticamente
    }
}
