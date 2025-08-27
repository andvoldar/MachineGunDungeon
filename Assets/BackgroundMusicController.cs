// Assets/_Scripts/Audio/BackgroundMusicController.cs
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using FMODUnity;
using FMOD.Studio;
using STOP_MODE = FMOD.Studio.STOP_MODE;

public class BackgroundMusicController : MonoBehaviour
{
    public static BackgroundMusicController Instance { get; private set; }

    [Header("FMOD Events")]
    [SerializeField] private EventReference ambientEvent;  // Música genérica (gameplay / dungeon)
    [SerializeField] private EventReference bossEvent;     // Música Boss

    [Header("Crossfade")]
    [SerializeField, Min(0f)] private float crossfadeSeconds = 1.5f;

    [Header("Persistencia")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    [Header("Scenes")]
    [Tooltip("Si el nombre de la escena contiene alguno de estos términos, se considera Main Menu y se para la música.")]
    [SerializeField] private string[] mainMenuSceneKeywords = new[] { "MainMenu", "StartMenu", "Title" };

    [Tooltip("Si el nombre de la escena contiene alguno de estos términos, se considera Gameplay y se reproduce la ambient.")]
    [SerializeField] private string[] gameplaySceneKeywords = new[] { "Level", "Dungeon", "Game", "Room" };

    private EventInstance _ambientInst;
    private EventInstance _bossInst;
    private Coroutine _fadeRoutine;
    private bool _bossActive = false;    // si true: queremos Boss como pista principal
    private bool _isInMainMenu = false;

    private void Awake()
    {
        // Singleton
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        // Crear instancia de Ambient (arranca sonando si estamos en gameplay; si caemos en menu la paramos)
        if (!ambientEvent.IsNull)
        {
            _ambientInst = RuntimeManager.CreateInstance(ambientEvent);
            _ambientInst.setVolume(1f);
            _ambientInst.start(); // arranca por defecto
        }

        // Boss en silencio (se arranca al crossfade)
        if (!bossEvent.IsNull)
        {
            _bossInst = RuntimeManager.CreateInstance(bossEvent);
            _bossInst.setVolume(0f);
            // no start aún
        }

        // Hook de escenas
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.activeSceneChanged += OnActiveSceneChanged;

        // Ajuste inicial por la escena actual
        var active = SceneManager.GetActiveScene();
        ApplySceneMusicPolicy(active);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;

        StopAndRelease(ref _ambientInst);
        StopAndRelease(ref _bossInst);
        if (Instance == this) Instance = null;
    }

    private void StopAndRelease(ref EventInstance inst)
    {
        if (inst.isValid())
        {
            inst.stop(STOP_MODE.IMMEDIATE);
            inst.release();
            inst.clearHandle();
        }
    }

    // ===== Scene policy =====

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySceneMusicPolicy(scene);
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        ApplySceneMusicPolicy(newScene);
    }

    private void ApplySceneMusicPolicy(Scene scene)
    {
        string name = scene.name ?? "";
        _isInMainMenu = StringContainsAny(name, mainMenuSceneKeywords);

        if (_isInMainMenu)
        {
            // Estamos en main menu: todo parado.
            StopAllMusicImmediate();
            _bossActive = false;
            return;
        }

        // Asumimos gameplay
        bool isGameplay = StringContainsAny(name, gameplaySceneKeywords) || !_isInMainMenu;

        if (isGameplay)
        {
            // Queremos ambient sonando (en 1.0), boss apagado (a 0 / parado)
            _bossActive = false;
            EnsureAmbientStarted(volume: 1f);
            StopBossIfRunning();
        }
    }

    private bool StringContainsAny(string haystack, string[] needles)
    {
        if (needles == null || needles.Length == 0) return false;
        string h = haystack.ToLowerInvariant();
        foreach (var n in needles)
        {
            if (string.IsNullOrWhiteSpace(n)) continue;
            if (h.Contains(n.ToLowerInvariant())) return true;
        }
        return false;
    }

    // ===== Public API =====

    /// <summary>Crossfade a música de Boss. Llamar al entrar a boss room.</summary>
    public void CrossfadeToBoss(float? durationOverride = null)
    {
        if (_isInMainMenu) return; // en menu no hacemos nada
        if (_bossActive) return;
        _bossActive = true;

        // Asegura ambient corriendo (por si estaba parada)
        EnsureAmbientStarted(volume: 1f);

        // Arranca Boss si estaba parado
        if (_bossInst.isValid())
        {
            _bossInst.getPlaybackState(out var ps);
            if (ps == PLAYBACK_STATE.STOPPED || ps == PLAYBACK_STATE.STOPPING)
                _bossInst.start();
        }

        StartFadeRoutine(toBoss: true, durationOverride ?? crossfadeSeconds);
    }

    /// <summary>Crossfade de vuelta a Ambient. Llamar al morir boss o abandonar boss room.</summary>
    public void CrossfadeToAmbient(float? durationOverride = null)
    {
        if (_isInMainMenu) return;

        // Arranca ambient si estaba parada (y mutea para hacer fade in)
        EnsureAmbientStarted(volume: 0f);

        _bossActive = false;
        StartFadeRoutine(toBoss: false, durationOverride ?? crossfadeSeconds);
    }

    /// <summary>Parar todo al ir a Main Menu (puedes llamarlo desde tu Pause Menu antes de cargar la escena).</summary>
    public void StopForMainMenu()
    {
        _isInMainMenu = true;
        StopAllMusicImmediate();
        _bossActive = false;
    }

    /// <summary>Reinicia Ambient de forma inmediata (por ejemplo tras morir/reintentar).</summary>
    public void RestartAmbientImmediate()
    {
        _isInMainMenu = false;
        _bossActive = false;
        EnsureAmbientStarted(volume: 1f);
        StopBossIfRunning();
    }

    /// <summary>Reinicia Ambient con fade-in (útil tras muerte o reload).</summary>
    public void RestartAmbientFadeIn(float seconds = 1.0f)
    {
        _isInMainMenu = false;
        _bossActive = false;

        EnsureAmbientStarted(volume: 0f);
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeAmbientInRoutine(seconds));
    }

    // ===== Internals =====

    private void EnsureAmbientStarted(float volume)
    {
        if (!_ambientInst.isValid()) return;

        _ambientInst.getPlaybackState(out var ps);
        if (ps == PLAYBACK_STATE.STOPPED || ps == PLAYBACK_STATE.STOPPING)
            _ambientInst.start();

        _ambientInst.setVolume(Mathf.Clamp01(volume));
    }

    private void StopBossIfRunning()
    {
        if (_bossInst.isValid())
        {
            _bossInst.setVolume(0f);
            _bossInst.stop(STOP_MODE.IMMEDIATE); // fuera del boss no lo necesitamos sonando
        }
    }

    private void StartFadeRoutine(bool toBoss, float duration)
    {
        if (_fadeRoutine != null) StopCoroutine(_fadeRoutine);
        _fadeRoutine = StartCoroutine(FadeRoutine(toBoss, Mathf.Max(0.05f, duration)));
    }

    private IEnumerator FadeRoutine(bool toBoss, float duration)
    {
        // Lee volúmenes iniciales
        float aStart = 0f, bStart = 0f;
        if (_ambientInst.isValid()) _ambientInst.getVolume(out aStart);
        if (_bossInst.isValid()) _bossInst.getVolume(out bStart);

        float aFrom = aStart;
        float bFrom = bStart;

        float aTo = toBoss ? 0f : 1f;
        float bTo = toBoss ? 1f : 0f;

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / duration);

            if (_ambientInst.isValid()) _ambientInst.setVolume(Mathf.Lerp(aFrom, aTo, k));
            if (_bossInst.isValid()) _bossInst.setVolume(Mathf.Lerp(bFrom, bTo, k));

            yield return null;
        }

        if (_ambientInst.isValid()) _ambientInst.setVolume(aTo);
        if (_bossInst.isValid()) _bossInst.setVolume(bTo);

        if (toBoss)
        {
            // Estamos en Boss: para Ambient suavemente (opcional, CPU)
            if (_ambientInst.isValid())
                _ambientInst.stop(STOP_MODE.ALLOWFADEOUT);
        }
        else
        {
            // Volvemos a Ambient: para Boss
            StopBossIfRunning();
        }

        _fadeRoutine = null;
    }

    private IEnumerator FadeAmbientInRoutine(float seconds)
    {
        float t = 0f;
        float from = 0f;
        float to = 1f;

        while (t < seconds)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / seconds);
            if (_ambientInst.isValid()) _ambientInst.setVolume(Mathf.Lerp(from, to, k));
            yield return null;
        }

        if (_ambientInst.isValid()) _ambientInst.setVolume(1f);
        _fadeRoutine = null;
    }

    public void StopAllMusicImmediate()
    {
        if (_fadeRoutine != null) { StopCoroutine(_fadeRoutine); _fadeRoutine = null; }

        if (_ambientInst.isValid())
        {
            _ambientInst.stop(STOP_MODE.IMMEDIATE);
            _ambientInst.setVolume(0f);
        }
        if (_bossInst.isValid())
        {
            _bossInst.stop(STOP_MODE.IMMEDIATE);
            _bossInst.setVolume(0f);
        }
    }
}
