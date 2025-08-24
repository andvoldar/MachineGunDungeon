// Assets/_Scripts/UI/GraphicsMenuUI.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems; // para click al abrir el dropdown

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

    [Header("Filtro (solo 16:9, de 720p a 4K)")]
    [SerializeField] private int minWidth = 1280;
    [SerializeField] private int minHeight = 720;
    [SerializeField] private int maxWidth = 3840;   // 4K
    [SerializeField] private int maxHeight = 2160;  // 4K
    [SerializeField] private bool onlyExactSixteenByNine = true; // SOLO 16:9 exacto

    // Lista final de opciones (solo pares WxH)
    private List<Vector2Int> resOptions = new List<Vector2Int>();

    // Para SFX al abrir dropdown
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
        RebuildResolutionOptions(fullscreenToggle != null ? fullscreenToggle.isOn : Screen.fullScreen);
        BuildResolutionDropdown();
        SyncFromPrefs();

        // Listeners (limpiamos y re-asignamos)
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionChanged);

        fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullscreenChanged);

        backButton.onClick.RemoveListener(HandleBack);
        backButton.onClick.AddListener(HandleBack);

        EnsureDropdownOpenClickSFX(); // sonido al abrir el desplegable
    }

    private void OnDisable()
    {
        resolutionDropdown.onValueChanged.RemoveListener(OnResolutionChanged);
        fullscreenToggle.onValueChanged.RemoveListener(OnFullscreenChanged);
        backButton.onClick.RemoveListener(HandleBack);
    }

    // ------------------- RES LIST -------------------

    /// <summary>
    /// Reconstruye la lista de resoluciones 16:9 desde 720p hasta 4K.
    /// Combina: resoluciones físicas del sistema (si las hay) + una lista canónica.
    /// En FULLSCREEN, limita a la capacidad del display; en Windowed, permite hasta 4K.
    /// </summary>
    private void RebuildResolutionOptions(bool fullscreenMode)
    {
        var set = new HashSet<Vector2Int>();

        // 1) Aporta las resoluciones que reporta el sistema (filtradas a 16:9 exacto y rango)
        foreach (var r in Screen.resolutions)
        {
            if (!PassesAspectFilter(r.width, r.height)) continue;
            if (!PassesRange(r.width, r.height)) continue;
            set.Add(new Vector2Int(r.width, r.height));
        }

        // 2) Aporta la lista canónica 16:9 de 720p a 4K (por si el SO no reporta alguna)
        foreach (var v in GetCanonical16x9UpTo4K())
        {
            if (!PassesRange(v.x, v.y)) continue;
            set.Add(v);
        }

        // 3) En FULLSCREEN, recorta a la capacidad del display (no tiene sentido ofrecer > nativo)
        if (fullscreenMode)
        {
            // Máximo físico (aprox): usa la mayor WxH del sistema
            int sysMaxW = Screen.resolutions.Length > 0 ? Screen.resolutions.Max(r => r.width) : Display.main.systemWidth;
            int sysMaxH = Screen.resolutions.Length > 0 ? Screen.resolutions.Max(r => r.height) : Display.main.systemHeight;
            set.RemoveWhere(v => v.x > sysMaxW || v.y > sysMaxH);
        }
        else
        {
            // En Windowed permitimos hasta el tope 4K (controlado por maxWidth/maxHeight)
            set.RemoveWhere(v => v.x > maxWidth || v.y > maxHeight);
        }

        // 4) Ordena ascendente
        resOptions = set.OrderBy(v => v.x).ThenBy(v => v.y).ToList();

        // Fallback si la lista quedara vacía
        if (resOptions.Count == 0)
        {
            var cur = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
            resOptions.Add(ClampTo16x9Range(cur));
        }
    }

    private bool PassesAspectFilter(int w, int h)
    {
        if (!onlyExactSixteenByNine) return true;
        return w * 9 == h * 16; // 16:9 exacto
    }

    private bool PassesRange(int w, int h)
    {
        return w >= minWidth && h >= minHeight && w <= maxWidth && h <= maxHeight;
    }

    private Vector2Int ClampTo16x9Range(Vector2Int v)
    {
        int w = Mathf.Clamp(v.x, minWidth, maxWidth);
        int h = Mathf.Clamp(v.y, minHeight, maxHeight);
        // fuerza 16:9 por altura
        h = Mathf.RoundToInt(w * 9f / 16f);
        return new Vector2Int(w, h);
    }

    private IEnumerable<Vector2Int> GetCanonical16x9UpTo4K()
    {
        // Lista canónica 16:9 desde 720p hasta 4K (puedes añadir 5K si quieres)
        yield return new Vector2Int(1280, 720);   // 720p
        yield return new Vector2Int(1600, 900);   // 900p
        yield return new Vector2Int(1920, 1080);  // 1080p
        yield return new Vector2Int(2560, 1440);  // 1440p
        yield return new Vector2Int(3200, 1800);  // 1800p
        yield return new Vector2Int(3840, 2160);  // 4K UHD
    }

    // ------------------- UI BUILD / SYNC -------------------

    private void BuildResolutionDropdown()
    {
        resolutionDropdown.ClearOptions();
        var opts = resOptions
            .Select(v => new TMP_Dropdown.OptionData($"{v.x} x {v.y}"))
            .ToList();
        resolutionDropdown.AddOptions(opts);
    }

    private void SyncFromPrefs()
    {
        // Sincroniza con PlayerPrefs y/o con la resolución actual si el índice guardado no sirve
        int idxSaved = PlayerPrefs.GetInt(KEY_RES_INDEX, -1);
        int idx;

        if (idxSaved >= 0 && idxSaved < resOptions.Count)
        {
            idx = idxSaved;
        }
        else
        {
            // Si la actual no está en la lista (p.ej. 1366x768), busca la más cercana por área dentro del set válido
            var cur = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
            idx = GetClosestIndexTo(cur);
        }

        bool fs = PlayerPrefs.GetInt(KEY_FULLSCREEN, Screen.fullScreen ? 1 : 0) == 1;

        resolutionDropdown.SetValueWithoutNotify(idx);
        fullscreenToggle.SetIsOnWithoutNotify(fs);
    }

    private int GetClosestIndexTo(Vector2Int target)
    {
        long targetArea = (long)target.x * (long)target.y;

        int bestIndex = 0;
        long bestDelta = long.MaxValue;

        for (int i = 0; i < resOptions.Count; i++)
        {
            var v = resOptions[i];
            long area = (long)v.x * (long)v.y;
            long delta = System.Math.Abs(area - targetArea);
            if (delta < bestDelta)
            {
                bestDelta = delta;
                bestIndex = i;
            }
        }
        return bestIndex;
    }

    // ------------------- APPLY -------------------

    private void Apply(int resIndex, bool fullscreen)
    {
        resIndex = Mathf.Clamp(resIndex, 0, resOptions.Count - 1);
        var v = resOptions[resIndex];

        Screen.SetResolution(v.x, v.y, fullscreen);

        PlayerPrefs.SetInt(KEY_RES_INDEX, resIndex);
        PlayerPrefs.SetInt(KEY_FULLSCREEN, fullscreen ? 1 : 0);
        PlayerPrefs.Save();
    }

    // ------------------- CALLBACKS -------------------

    private void OnResolutionChanged(int index)
    {
        Apply(index, fullscreenToggle.isOn);
        soundController?.PlayClickSound(); // sonido al seleccionar opción
    }

    private void OnFullscreenChanged(bool on)
    {
        // Al cambiar de modo regeneramos la lista (ej.: en fullscreen no ofrecer > nativo)
        var prevRes = resOptions[Mathf.Clamp(resolutionDropdown.value, 0, Mathf.Max(0, resOptions.Count - 1))];

        RebuildResolutionOptions(on);
        BuildResolutionDropdown();

        // Selecciona la más cercana a la anterior
        int newIndex = GetClosestIndexTo(prevRes);
        resolutionDropdown.SetValueWithoutNotify(newIndex);

        Apply(newIndex, on);
        soundController?.PlayClickSound(); // sonido al cambiar toggle
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
