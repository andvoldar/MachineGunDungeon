// Assets/_Scripts/_Managers/AudioSettings.cs
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// �NICA fuente de verdad de vol�menes (Music/FX) para todo el juego.
/// - Lee/escribe PlayerPrefs
/// - Aplica a FMOD seg�n el contexto (en pausa multiplica por el ducking, fuera de pausa aplica base).
/// - Llama AudioSettings.Instance.ApplyAll() siempre que cambie un slider.
/// </summary>
public class AudioSettings : MonoBehaviour
{
    public static AudioSettings Instance { get; private set; }

    // Claves compartidas
    public const string KEY_MUSIC_VOL = "settings_music_volume";
    public const string KEY_FX_VOL = "settings_fx_volume";

    // Rutas de bus
    private const string BUS_MUSIC_PATH = "bus:/Music";
    private const string BUS_FX_PATH = "bus:/FX";

    private Bus _musicBus;
    private Bus _fxBus;

    // �ltimos valores base (sin duck)
    public float MusicBaseVolume { get; private set; }
    public float FxBaseVolume { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        _musicBus = RuntimeManager.GetBus(BUS_MUSIC_PATH);
        _fxBus = RuntimeManager.GetBus(BUS_FX_PATH);

        // Cargar base
        MusicBaseVolume = PlayerPrefs.GetFloat(KEY_MUSIC_VOL, 1f);
        FxBaseVolume = PlayerPrefs.GetFloat(KEY_FX_VOL, 1f);

        ApplyAll(); // aplica al inicio seg�n si hay pausa o no
    }

    /// <summary> Llamar cuando el usuario cambie sliders o al abrir un men� para sincronizar. </summary>
    public void ApplyAll()
    {
        // Asegurar lectura �ltima de PlayerPrefs (por si otro men� los actualiz�)
        MusicBaseVolume = PlayerPrefs.GetFloat(KEY_MUSIC_VOL, 1f);
        FxBaseVolume = PlayerPrefs.GetFloat(KEY_FX_VOL, 1f);

        // Volumen efectivo de m�sica = base * (duck si est� en pausa)
        float duckMul = (PauseManager.Instance != null && PauseManager.Instance.IsPaused)
            ? PauseManager.Instance.MusicDuckMultiplier
            : 1f;

        if (_musicBus.isValid()) _musicBus.setVolume(Mathf.Clamp01(MusicBaseVolume * duckMul));
        if (_fxBus.isValid()) _fxBus.setVolume(Mathf.Clamp01(FxBaseVolume));
    }

    // Helpers para que los men�s actualicen y guarden:
    public void SetMusicBaseVolume(float v)
    {
        MusicBaseVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KEY_MUSIC_VOL, MusicBaseVolume);
        PlayerPrefs.Save();
        ApplyAll();
    }

    public void SetFxBaseVolume(float v)
    {
        FxBaseVolume = Mathf.Clamp01(v);
        PlayerPrefs.SetFloat(KEY_FX_VOL, FxBaseVolume);
        PlayerPrefs.Save();
        ApplyAll();
    }
}
