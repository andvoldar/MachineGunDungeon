// Assets/_Scripts/UI/GraphicsMenuUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems; // para sonido al abrir el dropdown

public class GraphicsMenuUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private Button backButton;

    [Header("Panel al que volver")]
    [SerializeField] private GameObject optionsMenuPanel;

    [Header("Sonidos (opcional)")]
    [SerializeField] private StartMenuSoundController soundController; // usa el mismo que en StartMenuUI

    [Header("Filtro de resoluciones (solo 16:9)")]
    [SerializeField] private int minWidth = 1280; // mínimo 720p
    [SerializeField] private int minHeight = 720;
    [SerializeField] private int maxWidth = 3840; // 4K
    [SerializeField] private int maxHeight = 2160; // 4K
    [SerializeField] private bool onlyExactSixteenByNine = true; // mantener 16:9 exacto

    // Lista canónica 16:9 que SIEMPRE se muestra en el dropdown
    private readonly List<Vector2Int> canonical16x9 =
        new List<Vector2Int>
        {
            new Vector2Int(1280,  720),   // 720p
            new Vector2Int(1600,  900),   // 900p
            new Vector2Int(1920, 1080),   // 1080p
            new Vector2Int(2560, 1440),   // 1440p
            new Vector2Int(3200, 1800),   // 1800p
            new Vector2Int(3840, 2160),   // 4K UHD
        };

    // Opciones finales mostradas (filtradas por rango/16:9)
    private List<Vector2Int> resOptions = new List<Vector2Int>();

    // SFX al abrir el desplegable
    private bool dropdownClickHooked = false;

    // PlayerPrefs
    private const string KEY_RES_INDEX = "settings_res_index";
    private const string KEY_FULLSCREEN = "settings_fullscreen";

    private void Awake()
    {
        if (soundController == null)
            soundController = FindObjectOfType<StartMenuSoundController>(true);
    }

    private void OnEnable()
    {
        BuildCanonicalOptions();
        BuildResolutionDropdown();
        SyncFromPrefs();

        // Listeners
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        backButton.onClick.RemoveListener(HandleBack);
        backButton.onClick.AddListener(HandleBack);

        EnsureDropdownOpenClickSFX();
    }

    private void OnDisable()
    {
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        backButton.onClick.RemoveListener(HandleBack);
    }

    // ------------------- Opciones 16:9 -------------------

    private void BuildCanonicalOptions()
    {
        resOptions = canonical16x9
            .Where(v => v.x >= minWidth && v.y >= minHeight)
            .Where(v => v.x <= maxWidth && v.y <= maxHeight)
            .Where(v => !onlyExactSixteenByNine || v.x * 9 == v.y * 16)
            .OrderBy(v => v.x).ThenBy(v => v.y)
            .ToList();

        if (resOptions.Count == 0)
        {
            // Fallback: fuerza 16:9 dentro de rango
            var cur = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
            int w = Mathf.Clamp(cur.x, minWidth, maxWidth);
            int h = Mathf.RoundToInt(w * 9f / 16f);
            resOptions.Add(new Vector2Int(w, Mathf.Clamp(h, minHeight, maxHeight)));
        }
    }

    private void BuildResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        var opts = resOptions.Select(v => new TMP_Dropdown.OptionData($"{v.x} x {v.y}")).ToList();
        resolutionDropdown.AddOptions(opts);
    }

    private void SyncFromPrefs()
    {
        int idxSaved = PlayerPrefs.GetInt(KEY_RES_INDEX, -1);
        int idx;

        if (idxSaved >= 0 && idxSaved < resOptions.Count)
        {
            idx = idxSaved;
        }
        else
        {
            var cur = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
            idx = GetClosestIndexTo(cur);
        }

        bool fs = PlayerPrefs.GetInt(KEY_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;

        resolutionDropdown.SetValueWithoutNotify(idx);
        fullscreenToggle.SetIsOnWithoutNotify(fs);
    }

    private int GetClosestIndexTo(Vector2Int target)
    {
        long targetArea = (long)target.x * target.y;
        int bestIndex = 0; long bestDelta = long.MaxValue;

        for (int i = 0; i < resOptions.Count; i++)
        {
            var v = resOptions[i];
            long area = (long)v.x * v.y;
            long delta = System.Math.Abs(area - targetArea);
            if (delta < bestDelta) { bestDelta = delta; bestIndex = i; }
        }
        return bestIndex;
    }

    private Vector2Int GetSystemMaxResolution()
    {
        // Usa Screen.resolutions si está poblado; si no, Display.main.systemWidth/Height
        int w = Screen.resolutions.Length > 0 ? Screen.resolutions.Max(r => r.width) : Display.main.systemWidth;
        int h = Screen.resolutions.Length > 0 ? Screen.resolutions.Max(r => r.height) : Display.main.systemHeight;
        return new Vector2Int(w, h);
    }

    private int GetBestIndexUnderCap(Vector2Int cap)
    {
        // Devuelve la mayor resolución <= cap (por área)
        int best = 0; long bestArea = -1;
        for (int i = 0; i < resOptions.Count; i++)
        {
            var v = resOptions[i];
            if (v.x <= cap.x && v.y <= cap.y)
            {
                long area = (long)v.x * v.y;
                if (area > bestArea) { bestArea = area; best = i; }
            }
        }
        return best;
    }

    // ------------------- Aplicación -------------------

    private void Apply(int resIndex, bool fullscreen)
    {
        resIndex = Mathf.Clamp(resIndex, 0, resOptions.Count - 1);
        var desired = resOptions[resIndex];

        if (fullscreen)
        {
            // Si el monitor no soporta esa resolución en fullscreen, baja a la más alta válida
            Vector2Int cap = GetSystemMaxResolution();
            if (desired.x > cap.x || desired.y > cap.y)
            {
                int fallback = GetBestIndexUnderCap(cap);
                desired = resOptions[fallback];
                // Refleja el ajuste en el dropdown
                resolutionDropdown.SetValueWithoutNotify(fallback);
                resIndex = fallback;
            }

            Screen.SetResolution(desired.x, desired.y, true);
        }
        else
        {
            Screen.SetResolution(desired.x, desired.y, false);
        }

        PlayerPrefs.SetInt(KEY_RES_INDEX, resIndex);
        PlayerPrefs.SetInt(KEY_FULLSCREEN, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ------------------- Callbacks -------------------

    private void OnResolutionChanged(int index)
    {
        Apply(index, fullscreenToggle.isOn);
        soundController?.PlayClickSound();
    }

    private void OnFullscreenChanged(bool on)
    {
        // No cambiamos lista ni etiquetas; solo aplicamos la resolución actual en el modo nuevo
        int curIndex = Mathf.Clamp(resolutionDropdown.value, 0, Mathf.Max(0, resOptions.Count - 1));
        Apply(curIndex, on);
        soundController?.PlayClickSound();
    }

    private void HandleBack()
    {
        soundController?.PlayClickSound();
        optionsMenuPanel.SetActive(true);
        gameObject.SetActive(false);
    }

    // ------------------- SFX al abrir dropdown -------------------

    private void EnsureDropdownOpenClickSFX()
    {
        if (dropdownClickHooked || resolutionDropdown == null) return;

        var go = resolutionDropdown.gameObject;
        var trigger = go.GetComponent<EventTrigger>();
        if (trigger == null) trigger = go.AddComponent<EventTrigger>();

        var entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
        entry.callback.AddListener(OnDropdownPointerClick);
        trigger.triggers.Add(entry);

        dropdownClickHooked = true;
    }

    private void OnDropdownPointerClick(BaseEventData _)
    {
        soundController?.PlayClickSound();
    }
}
