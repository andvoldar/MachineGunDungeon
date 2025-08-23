using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class StaminaBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private float refillDuration = 0.4f;

    public void SetStamina(float current, float max)
    {
        float percent = Mathf.Clamp01(current / max);
        fillImage.DOFillAmount(percent, refillDuration)
            .SetEase(Ease.OutQuad)
            .SetTarget(fillImage);
    }

    public void FlashEmpty()
    {
        fillImage.DOKill(true);
        fillImage.color = Color.yellow;
        fillImage.DOColor(Color.white, 0.3f);
    }
}
