using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

[RequireComponent(typeof(CanvasGroup))]
public class GrenadeChargeBarUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Child Image with Type=Filled and Fill Method=Vertical")]
    [SerializeField] private Image fillImage;
    [SerializeField] private float fillDuration = 0.1f;

    private CanvasGroup canvasGroup;
    private bool isShowing = false;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (fillImage == null)
            Debug.LogError($"[{nameof(GrenadeChargeBarUI)}] El campo fillImage no está asignado en '{gameObject.name}'.");

        // Aseguramos estado inicial
        canvasGroup.alpha = 0f;
        if (fillImage != null)
            fillImage.fillAmount = 0f;
    }

    /// <summary>
    /// Ajusta el porcentaje de carga con un tween.
    /// </summary>
    public void SetCharge(float current, float max)
    {
        if (fillImage == null) return;

        float percent = Mathf.Clamp01(current / max);
        fillImage.DOKill();
        fillImage.DOFillAmount(percent, fillDuration)
                 .SetEase(Ease.OutQuad)
                 .SetTarget(fillImage);
    }

    /// <summary>
    /// Muestra u oculta la barra. Si acaba de mostrarse, resetea el fill a 0.
    /// </summary>
    public void Show(bool visible)
    {
        // Mata cualquier tween previo
        canvasGroup.DOKill();

        if (visible)
        {
            // Si antes estaba oculta, reiniciamos la barra
            if (!isShowing && fillImage != null)
                fillImage.fillAmount = 0f;

            // Activamos alpha con tween de 0s (puedes cambiar la duración aquí si quieres un fade)
            canvasGroup.DOFade(1f, 0f)
                       .SetEase(Ease.Linear)
                       .SetTarget(canvasGroup);
        }
        else
        {
            canvasGroup.DOFade(0f, 0f)
                       .SetEase(Ease.Linear)
                       .SetTarget(canvasGroup);
        }

        isShowing = visible;
    }

    private void OnDestroy()
    {
        // Nos aseguramos de matar todos los tweens al destruir
        if (canvasGroup != null)
            canvasGroup.DOKill();
        if (fillImage != null)
            fillImage.DOKill();
    }
}
