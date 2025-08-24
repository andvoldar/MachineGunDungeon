// Assets/_Scripts/UI/UIAlwaysVisible.cs
using UnityEngine;
using UnityEngine.UI;

/// Mantiene la UI SIEMPRE visible ajustando din�micamente el CanvasScaler.
/// - Sin GameObjects extra.
/// - Sin "safe frames".
/// - Funciona en Editor y Build.
/// Coloca este script EN EL MISMO OBJETO que el Canvas de tu men� (StartMenu, Options, etc.).
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(Canvas))]
public class UIAlwaysVisible : MonoBehaviour
{
    [Header("Resoluci�n de referencia (tu UI fue maquetada para esto)")]
    public Vector2 referenceResolution = new Vector2(3840, 2160); // UHD por defecto

    [Tooltip("Si est� activo, forzar� que toda la UI de referencia quepa dentro de la pantalla, sin recortarse. " +
             "Equivale a seleccionar autom�ticamente 'Match Width' o 'Match Height' seg�n el aspecto actual.")]
    public bool forceFitInside = true;

    [Header("Opcional")]
    [Tooltip("Log informativo en consola para depuraci�n.")]
    public bool debugLogs = false;

    private Canvas _canvas;
    private CanvasScaler _scaler;
    private int _prevScreenW = -1, _prevScreenH = -1;
    private Vector2 _prevReference;

    void Reset()
    {
        referenceResolution = new Vector2(3840, 2160);
        forceFitInside = true;
    }

    void OnEnable()
    {
        EnsureScaler();
        ApplySettings();
        Canvas.willRenderCanvases += OnWillRenderCanvases;
    }

    void OnDisable()
    {
        Canvas.willRenderCanvases -= OnWillRenderCanvases;
    }

    void OnValidate()
    {
        // Aplicar inmediatamente cuando cambian valores en el inspector
        EnsureScaler();
        ApplySettings();
    }

    void OnRectTransformDimensionsChange()
    {
        // En Editor: cambia cuando ajustas la GameView; en runtime: al cambiar resoluci�n/ventana
        ApplySettingsIfScreenChanged();
    }

    private void OnWillRenderCanvases()
    {
        // �ltima oportunidad antes de dibujar para reaccionar a cambios de tama�o/escala
        ApplySettingsIfScreenChanged();
    }

    private void EnsureScaler()
    {
        if (_canvas == null) _canvas = GetComponent<Canvas>();
        if (_scaler == null) _scaler = GetComponent<CanvasScaler>();

        if (_scaler == null)
        {
            _scaler = gameObject.AddComponent<CanvasScaler>();
            if (debugLogs) Debug.Log("[UIAlwaysVisible] CanvasScaler a�adido autom�ticamente.", this);
        }

        // Configuraci�n base recomendada
        _scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        _scaler.referenceResolution = referenceResolution;
        _scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        // El match se ajustar� din�micamente en ApplySettings()
    }

    private void ApplySettingsIfScreenChanged()
    {
        if (Screen.width != _prevScreenW || Screen.height != _prevScreenH || _prevReference != referenceResolution)
        {
            ApplySettings();
        }
    }

    private void ApplySettings()
    {
        if (_scaler == null) return;

        _scaler.referenceResolution = referenceResolution;

        if (forceFitInside)
        {
            // Calcula el aspecto actual vs el de referencia
            float currentAspect = Screen.width / Mathf.Max(1f, (float)Screen.height);
            float refAspect = referenceResolution.x / Mathf.Max(1f, referenceResolution.y);

            // Regla:
            // - Si la pantalla es M�S ANCHA que la referencia -> escalamos por ALTURA (match=1)
            //   (as� la altura entra y la UI no se recorta verticalmente)
            // - Si la pantalla es M�S ESTRECHA que la referencia -> escalamos por ANCHURA (match=0)
            //   (as� el ancho entra y la UI no se recorta horizontalmente)
            _scaler.matchWidthOrHeight = (currentAspect > refAspect) ? 1f : 0f;
        }

        _prevScreenW = Screen.width;
        _prevScreenH = Screen.height;
        _prevReference = referenceResolution;

        if (debugLogs)
        {
            Debug.Log($"[UIAlwaysVisible] Applied. Screen: {Screen.width}x{Screen.height}, " +
                      $"Ref: {referenceResolution.x}x{referenceResolution.y}, " +
                      $"Match (0=W / 1=H): {_scaler.matchWidthOrHeight:0.00}", this);
        }
    }
}
