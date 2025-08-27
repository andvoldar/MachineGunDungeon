using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using System.Collections;
using UnityEngine.SceneManagement;

public class StartMenuSoundController : MonoBehaviour
{
    [Header("Sonidos (SoundManager)")]
    [SerializeField] private SoundType hoverSound;
    [SerializeField] private SoundType clickSound;
    [SerializeField] private SoundType menuMusicSound;
    [SerializeField] private SoundType startGameSound;

    [Header("Autoplay Música de Menú")]
    [Tooltip("Si está activo, intentará reproducir la música del menú al iniciar este objeto.")]
    [SerializeField] private bool autoPlayMenuMusic = true;

    [Tooltip("Nombre EXACTO de la escena de menú principal donde sí debe sonar la música.")]
    [SerializeField] private string mainMenuSceneName = "StartMenu";

    [Tooltip("Si está activo, NO auto-reproducirá música si el juego está en pausa.")]
    [SerializeField] private bool blockAutoPlayIfPaused = true;

    private EventInstance musicInstance;
    private bool isFadingOut = false;
    private bool musicStarted = false;
    private bool soundLocked = false; // bloquea hover/click/start si lo decides (p.ej., al pulsar START)

    private void Start()
    {
        TryAutoPlayMenuMusic();
    }

    // ---------- Autoplay seguro ----------
    private void TryAutoPlayMenuMusic()
    {
        if (!autoPlayMenuMusic) return;

        // Reproducir solo en la escena del menú principal
        string scene = SceneManager.GetActiveScene().name;
        if (!string.Equals(scene, mainMenuSceneName, System.StringComparison.Ordinal))
            return;

        // Evitar autoplay si estamos en PAUSA (por ejemplo, si este controller existe en un variant del Pause)
        if (blockAutoPlayIfPaused && PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            return;

        PlayMenuMusic();
    }

    // ---------- Música de menú ----------
    private void PlayMenuMusic()
    {
        if (musicStarted) return;

        // Si no tienes asignado un tipo válido para música en SoundLibrary, simplemente no intentes crearla.
        // (Asumimos que SoundManager se encarga de validar internamente)
        musicInstance = SoundManager.Instance.CreateEventInstance(menuMusicSound, Vector3.zero);
        musicInstance.start();
        musicStarted = true;
        isFadingOut = false;
    }

    public void FadeOutMusic(float fadeDuration = 1.5f)
    {
        if (isFadingOut || !musicStarted) return;
        isFadingOut = true;
        StartCoroutine(FadeOutAndStopCoroutine(fadeDuration));
    }

    private IEnumerator FadeOutAndStopCoroutine(float duration)
    {
        if (!musicInstance.isValid()) yield break;

        float timer = 0f;
        musicInstance.getVolume(out float startVolume);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, 0f, timer / duration);
            musicInstance.setVolume(newVolume);
            yield return null;
        }

        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
        musicStarted = false;
        isFadingOut = false;
    }

    // ---------- SFX UI ----------
    public void PlayHoverSound()
    {
        if (soundLocked) return;
        SoundManager.Instance.PlaySound(hoverSound, Vector3.zero);
    }

    public void PlayClickSound()
    {
        if (soundLocked) return;
        SoundManager.Instance.PlaySound(clickSound, Vector3.zero);
    }

    public void PlayStartGameSound()
    {
        if (soundLocked) return;
        SoundManager.Instance.PlaySound(startGameSound, Vector3.zero);
    }

    // Permite bloquear todos los SFX de UI (lo usabas al pulsar START)
    public void LockSounds()
    {
        soundLocked = true;
    }
}
