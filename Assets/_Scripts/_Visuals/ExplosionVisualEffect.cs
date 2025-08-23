using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;
using System.Collections;

public class ExplosionVisualEffect : MonoBehaviour
{
    [Header("Light Flash")]
    [SerializeField] private Light2D light2D;
    [SerializeField] private float lightStartRadius = 1f;
    [SerializeField] private float lightEndRadius = 8f;
    [SerializeField] private float lightDuration = 0.1f;
    [SerializeField] private float lightFadeDelay = 0.05f;

    [Header("Smoke Rise")]
    [SerializeField] private SpriteRenderer smokeRenderer;
    [SerializeField] private float smokeRiseDistance = 0.5f;
    [SerializeField] private float smokeDuration = 0.7f;
    [SerializeField] private float smokeFadeStartDelay = 0.3f;
    [SerializeField] private float smokeFinalAlpha = 0f;
    [SerializeField] private float smokeStartAlpha = 0.5f;

    [Header("Explosion Sprite (opcional)")]
    [SerializeField] private SpriteRenderer explosionSpriteRenderer;

    private void OnEnable()
    {
        StartCoroutine(PlayEffectSequence());
    }

    private IEnumerator PlayEffectSequence()
    {

        SoundManager.Instance.PlaySound(SoundType.GrenadeExplosion, transform.position);
        // ⚡ LIGHT
        if (light2D != null)
        {
            light2D.pointLightOuterRadius = lightStartRadius;
            light2D.enabled = true;

            DOTween.To(
                () => light2D.pointLightOuterRadius,
                r => light2D.pointLightOuterRadius = r,
                lightEndRadius,
                lightDuration
            )
            .SetEase(Ease.OutCubic)
            .SetLink(gameObject);

            yield return new WaitForSeconds(lightDuration + lightFadeDelay);
            light2D.enabled = false;
        }

        // ✅ OCULTAR el sprite principal de explosión
        if (explosionSpriteRenderer != null)
            explosionSpriteRenderer.enabled = false;

        // ⚡ SMOKE
        if (smokeRenderer != null)
        {
            Color c = smokeRenderer.color;
            smokeRenderer.color = new Color(c.r, c.g, c.b, smokeStartAlpha);
            Transform t = smokeRenderer.transform;
            t.localScale = Vector3.one * 0.8f;

            t.DOLocalMoveY(t.localPosition.y + smokeRiseDistance, smokeDuration)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);

            t.DOScale(1.2f, smokeDuration * 0.8f)
                .SetEase(Ease.OutQuad)
                .SetLink(gameObject);

            smokeRenderer.DOFade(smokeFinalAlpha, smokeDuration * 0.7f)
                .SetEase(Ease.InQuad)
                .SetDelay(smokeFadeStartDelay)
                .SetLink(gameObject);
        }

        yield return new WaitForSeconds(smokeDuration);
        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        // 🧹 Limpieza segura de todos los tweens vinculados al objeto
        DOTween.Kill(gameObject);
    }
}
