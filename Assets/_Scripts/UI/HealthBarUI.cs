using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HealthBarUI : MonoBehaviour
{
    [Header("Bar Images")]
    [SerializeField] private Image fillImage;
    [SerializeField] private Image ghostFillImage;

    [Header("UI Feedback")]
    [SerializeField] private Image boxGlowImage;

    [Header("Color Config")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color damageColor = Color.red;
    [SerializeField] private Color lowHealthColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] private Color dangerBoxColor = new Color(1f, 0.1f, 0.1f, 0.6f);
    [SerializeField] private float lowHealthThreshold = 0.25f;

    [Header("Timing")]
    [SerializeField] private float ghostDelay = 0.2f;
    [SerializeField] private float ghostDuration = 0.5f;

    private Tween blinkTween;
    private Tween boxTween;
    private Tween punchTween;

    public void SetHealth(float current, float max)
    {
        float percent = Mathf.Clamp01(current / max);
        Debug.Log($"➤ SetHealth: {current}/{max} = {percent}");

        fillImage.DOKill(true);
        ghostFillImage.DOKill(true);

        fillImage.fillAmount = percent;

        ghostFillImage.DOFillAmount(percent, ghostDuration)
            .SetDelay(ghostDelay)
            .SetEase(Ease.OutCubic)
            .SetTarget(ghostFillImage);

        fillImage.color = normalColor;

        if (percent <= lowHealthThreshold)
            ActivateLowHealthWarning();
        else
            DeactivateLowHealthWarning();
    }

    public void PlayDamageFeedback()
    {
        transform.DOKill(true);
        transform.localScale = Vector3.one;

        transform.DOShakeScale(0.2f, strength: 0.15f, vibrato: 10)
            .SetTarget(transform);

        fillImage.DOKill(true);
        fillImage.color = damageColor;

        fillImage.DOColor(normalColor, 0.3f)
            .SetTarget(fillImage)
            .OnComplete(() =>
            {
                if (fillImage.fillAmount <= lowHealthThreshold)
                    StartBarBlink();
            });
    }

    private void ActivateLowHealthWarning()
    {
        StartBarBlink();
        StartBoxBlink();

        punchTween?.Kill();
        punchTween = transform
            .DOPunchScale(Vector3.one * 0.1f, 0.4f, 4, 0.5f)
            .SetLoops(-1, LoopType.Restart)
            .SetEase(Ease.InOutQuad)
            .SetTarget(transform);
    }

    private void DeactivateLowHealthWarning()
    {
        StopBarBlink();
        StopBoxBlink();

        punchTween?.Kill();
        transform.localScale = Vector3.one;
    }

    private void StartBarBlink()
    {
        if (blinkTween != null && blinkTween.IsActive()) return;

        blinkTween = fillImage
            .DOColor(lowHealthColor, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopBarBlink()
    {
        if (blinkTween != null)
        {
            blinkTween.Kill();
            blinkTween = null;
            fillImage.color = normalColor;
        }
    }

    private void StartBoxBlink()
    {
        if (boxGlowImage == null || (boxTween != null && boxTween.IsActive())) return;

        boxTween = boxGlowImage
            .DOColor(dangerBoxColor, 0.6f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);
    }

    private void StopBoxBlink()
    {
        if (boxTween != null)
        {
            boxTween.Kill();
            boxTween = null;
            if (boxGlowImage != null)
                boxGlowImage.color = Color.clear;
        }
    }
}
