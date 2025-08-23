using UnityEngine;
using DG.Tweening;

public class GetHitVisualFeedback : Feedback
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float dissolveDuration = 0.1f;
    [SerializeField] private string dissolveProperty = "_DissolveAmount";
    [SerializeField] private float startValue = 1f;
    [SerializeField] private float flashValue = 0.5f;

    private MaterialPropertyBlock propBlock;
    private Tween dissolveTween;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.material = new Material(spriteRenderer.material);
        propBlock = new MaterialPropertyBlock();
        SetDissolve(startValue);
    }

    public override void CreateFeedback()
    {
        CompletePreviousFeedback();

        dissolveTween = DOTween.Sequence()
            .Append(DOTween.To(() => startValue, SetDissolve, flashValue, dissolveDuration / 2f))
            .Append(DOTween.To(() => flashValue, SetDissolve, startValue, dissolveDuration / 2f))
            .SetEase(Ease.InOutQuad);
    }

    public override void CompletePreviousFeedback() 
    {
        if (dissolveTween != null && dissolveTween.IsActive())
        {
            dissolveTween.Kill();
            SetDissolve(startValue);
        }
    }

    private void SetDissolve(float value)
    {
        spriteRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(dissolveProperty, value);
        spriteRenderer.SetPropertyBlock(propBlock);
    }
}
