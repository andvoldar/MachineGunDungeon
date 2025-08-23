using UnityEngine;
using FMOD.Studio;
using FMODUnity;
using System.Collections;

public class StartMenuSoundController : MonoBehaviour
{
    [Header("Sonidos")]
    [SerializeField] private SoundType hoverSound;
    [SerializeField] private SoundType clickSound;
    [SerializeField] private SoundType menuMusicSound;
    [SerializeField] private SoundType startGameSound;

    private EventInstance musicInstance;
    private bool isFadingOut = false;
    private bool musicStarted = false;
    private bool soundLocked = false; // 🔥 Aquí lo declaramos

    private void Start()
    {
        PlayMenuMusic();
    }

    private void PlayMenuMusic()
    {
        if (musicStarted) return;
        musicInstance = SoundManager.Instance.CreateEventInstance(menuMusicSound, Vector3.zero);
        musicInstance.start();
        musicStarted = true;
    }

    public void PlayHoverSound()
    {
        if (soundLocked) return; // 🔥 No reproducir si está bloqueado
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
        float startVolume = 1f;
        musicInstance.getVolume(out startVolume);

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float newVolume = Mathf.Lerp(startVolume, 0f, timer / duration);
            musicInstance.setVolume(newVolume);
            yield return null;
        }

        musicInstance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
        musicInstance.release();
    }

    // 🔥 NUEVO método que faltaba
    public void LockSounds()
    {
        soundLocked = true;
    }
}
