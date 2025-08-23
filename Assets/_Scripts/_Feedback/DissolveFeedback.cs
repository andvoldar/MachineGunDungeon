using UnityEngine;
using DG.Tweening;
using System;

public class DissolveFeedback : Feedback
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float dissolveDuration = 1f;
    [SerializeField] private string dissolveProperty = "_DissolveAmount";
    [SerializeField] private float startValue = 1f;
    [SerializeField] private float endValue = 0f;

    [SerializeField] private string emissionProperty = "_EmissionColor";
    [SerializeField] private Color startEmissionColor = Color.white;
    [SerializeField] private Color endEmissionColor = Color.red;
    [SerializeField] private float emissionIntensityMultiplier = 2f;

    [SerializeField] private float flashValue = 0.5f;
    [SerializeField] private float flashDuration = 0.2f;

    public Action OnDissolveComplete;

    private MaterialPropertyBlock propBlock;
    private Tween dissolveTween;
    private Tween emissionTween;  // Nuevo Tween para la emisión
    private float currentDissolveValue;

    private void Awake()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInParent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.material = new Material(spriteRenderer.material);

        propBlock = new MaterialPropertyBlock();
        SetDissolve(startValue);
        SetEmission(startEmissionColor);
    }

    // Método para verificar la validez del spriteRenderer antes de aplicar efectos
    private bool IsValid()
    {
        if (spriteRenderer == null || spriteRenderer.material == null)
        {
            Debug.LogWarning("SpriteRenderer is null or material is missing.");
            return false;
        }
        return true;
    }

    public override void CreateFeedback()
    {
        CompletePreviousFeedback();

        if (!IsValid()) return;  // Verificamos que el spriteRenderer es válido antes de crear el feedback

        currentDissolveValue = startValue;

        dissolveTween = DOTween.Sequence()
            .Append(DOTween.To(() => currentDissolveValue, x =>
            {
                currentDissolveValue = x;
                SetDissolve(x);
            }, flashValue, flashDuration / 2f))
            .Append(DOTween.To(() => currentDissolveValue, x =>
            {
                currentDissolveValue = x;
                SetDissolve(x);
            }, startValue, flashDuration / 2f))
            .Append(DOTween.To(() => currentDissolveValue, x =>
            {
                currentDissolveValue = x;
                SetDissolve(x);
            }, endValue, dissolveDuration))
            .SetEase(Ease.InOutCubic)
            .OnUpdate(UpdateEmission)
            .OnComplete(() => OnDissolveComplete?.Invoke())
            .SetUpdate(true)    
            .SetLink(gameObject, LinkBehaviour.KillOnDestroy); // <--- NUEVO

        emissionTween = DOTween.To(() => startEmissionColor, x =>
        {
            startEmissionColor = x;
            SetEmission(x);
        }, endEmissionColor, dissolveDuration)
        .SetEase(Ease.InOutCubic)
        .SetUpdate(true)
        .SetLink(gameObject, LinkBehaviour.KillOnDestroy);   // Asegura que el tiempo real se use aquí
    }

    public override void CompletePreviousFeedback()
    {
        if (dissolveTween != null && dissolveTween.IsActive())
        {
            dissolveTween.Kill();
        }

        // Matamos el Tween de emisión también si está activo
        emissionTween?.Kill();
    }

    private void SetDissolve(float value)
    {
        if (!IsValid()) return;  // Verificamos que el spriteRenderer es válido antes de modificar el dissolve

        spriteRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(dissolveProperty, value);
        spriteRenderer.SetPropertyBlock(propBlock);
    }

    private void SetEmission(Color emissionColor)
    {
        if (!IsValid()) return;  // Verificamos que el spriteRenderer es válido antes de modificar la emisión

        spriteRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor(emissionProperty, emissionColor);
        spriteRenderer.SetPropertyBlock(propBlock);
    }

    private void UpdateEmission()
    {
        if (!IsValid()) return;  // Verificamos que el spriteRenderer es válido antes de actualizar la emisión

        float progress = Mathf.InverseLerp(startValue, endValue, currentDissolveValue);
        Color currentEmission = Color.Lerp(startEmissionColor, endEmissionColor, progress) * emissionIntensityMultiplier;
        SetEmission(currentEmission);
    }

    private void OnDestroy()
    {
        CompletePreviousFeedback();
        
        // Matamos el Tween de emisión si existe
        emissionTween?.Kill();
        DOTween.Kill(gameObject);
    }
}
