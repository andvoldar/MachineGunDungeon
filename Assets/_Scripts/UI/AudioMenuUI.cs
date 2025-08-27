// Assets/_Scripts/UI/AudioMenuUI.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Menú de audio UNIFICADO:
/// - No toca buses directamente.
/// - Sincroniza sliders en OnEnable desde PlayerPrefs.
/// - Al cambiar slider, guarda y delega en AudioSettings.ApplyAll().
/// </summary>
public class AudioMenuUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider fxSlider;

    [Header("Panel al que volver")]
    [SerializeField] private GameObject optionsMenuPanel;

    [Header("Botón de regreso")]
    [SerializeField] private Button backButton;

    private bool _hooked;

    private void OnEnable()
    {
        // Asegurar singleton
        if (AudioSettings.Instance == null)
        {
            Debug.LogWarning("[AudioMenuUI] No hay AudioSettings en la escena. Añade el prefab/singleton.");
            return;
        }

        // Sincroniza sliders con PlayerPrefs (sin disparar eventos)
        float music = PlayerPrefs.GetFloat(AudioSettings.KEY_MUSIC_VOL, 1f);
        float fx = PlayerPrefs.GetFloat(AudioSettings.KEY_FX_VOL, 1f);

        if (musicSlider) musicSlider.SetValueWithoutNotify(music);
        if (fxSlider) fxSlider.SetValueWithoutNotify(fx);

        // Aplica a buses según contexto actual
        AudioSettings.Instance.ApplyAll();

        HookListeners();
    }

    private void HookListeners()
    {
        if (_hooked) return;

        if (musicSlider) musicSlider.onValueChanged.AddListener(OnMusicChanged);
        if (fxSlider) fxSlider.onValueChanged.AddListener(OnFxChanged);
        if (backButton) backButton.onClick.AddListener(HandleBack);

        _hooked = true;
    }

    private void OnMusicChanged(float v)
    {
        AudioSettings.Instance?.SetMusicBaseVolume(v);
    }

    private void OnFxChanged(float v)
    {
        AudioSettings.Instance?.SetFxBaseVolume(v);
    }

    private void HandleBack()
    {
        if (optionsMenuPanel)
        {
            optionsMenuPanel.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
