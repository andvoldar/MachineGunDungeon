// Assets/_Scripts/UI/AudioMenuUI.cs
using UnityEngine;
using UnityEngine.UI;
using FMODUnity;
using FMOD.Studio;

public class AudioMenuUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider fxSlider;

    [Header("Panel al que volver")]
    [SerializeField] private GameObject optionsMenuPanel;

    [Header("Botón de regreso")]
    [SerializeField] private Button backButton;

    // Referencias FMOD
    private Bus musicBus;
    private Bus fxBus;

    private const string BUS_MUSIC_PATH = "bus:/Music";
    private const string BUS_FX_PATH = "bus:/FX";

    private const string KEY_MUSIC_VOL = "settings_music_volume";
    private const string KEY_FX_VOL = "settings_fx_volume";

    private void Awake()
    {
        musicBus = RuntimeManager.GetBus(BUS_MUSIC_PATH);
        fxBus = RuntimeManager.GetBus(BUS_FX_PATH);
    }

    private void Start()
    {
        float musicVol = PlayerPrefs.GetFloat(KEY_MUSIC_VOL, 1f);
        float fxVol = PlayerPrefs.GetFloat(KEY_FX_VOL, 1f);

        musicSlider.SetValueWithoutNotify(musicVol);
        fxSlider.SetValueWithoutNotify(fxVol);

        ApplyMusicVolume(musicVol);
        ApplyFXVolume(fxVol);

        musicSlider.onValueChanged.AddListener(ApplyMusicVolume);
        fxSlider.onValueChanged.AddListener(ApplyFXVolume);

        backButton.onClick.AddListener(HandleBackPressed);
    }

    private void ApplyMusicVolume(float value)
    {
        musicBus.setVolume(value);
        PlayerPrefs.SetFloat(KEY_MUSIC_VOL, value);
    }

    private void ApplyFXVolume(float value)
    {
        fxBus.setVolume(value);
        PlayerPrefs.SetFloat(KEY_FX_VOL, value);
    }

    private void HandleBackPressed()
    {
        optionsMenuPanel.SetActive(true);
        gameObject.SetActive(false);
    }
}
