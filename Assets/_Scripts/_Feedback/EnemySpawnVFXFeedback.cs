using UnityEngine;
using DG.Tweening;

public class EnemySpawnVFXFeedback : Feedback
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float duration = 1f;
    [SerializeField] private string dissolveProperty = "_DissolveAmount";
    [SerializeField] private float startValue = 0f;
    [SerializeField] private float endValue = 1f;

    private MaterialPropertyBlock propBlock;
    private Tween dissolveTween;
    private float currentValue;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInParent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.material = new Material(spriteRenderer.material);

        propBlock = new MaterialPropertyBlock();
        SetDissolve(startValue);
    }

    public override void CreateFeedback()
    {
        CompletePreviousFeedback();

        currentValue = startValue;

        dissolveTween = DOTween.To(() => currentValue, x =>
        {
            currentValue = x;
            SetDissolve(x);
        }, endValue, duration)
        .SetEase(Ease.InOutCubic)
        .SetUpdate(true)
        .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    public override void CompletePreviousFeedback()
    {
        if (dissolveTween != null && dissolveTween.IsActive())
        {
            dissolveTween.Kill();
        }
    }

    private void SetDissolve(float value)
    {
        if (spriteRenderer == null) return;

        spriteRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(dissolveProperty, value);
        spriteRenderer.SetPropertyBlock(propBlock);
    }

    private void OnDestroy()
    {
        CompletePreviousFeedback();
        DOTween.Kill(gameObject);
    }
}
