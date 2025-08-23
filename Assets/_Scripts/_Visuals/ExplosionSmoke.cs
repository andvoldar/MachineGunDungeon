using UnityEngine;
using DG.Tweening;
using System.Collections;

public class ExplosionSmokeEffect : MonoBehaviour
{
    private SpriteRenderer sr;

    [SerializeField] private float riseDistance = 0.5f;
    [SerializeField] private float duration = .7f;
    [SerializeField] private float fadeStartDelay = 0.3f;
    [SerializeField] private float finalAlpha = 0f;
    [SerializeField] private float startAlpha = 0.5f;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        StartCoroutine(PlayEffectAndDestroyParent());
    }

    private IEnumerator PlayEffectAndDestroyParent()
    {
        // Inicializa transparencia y escala
        Color c = sr.color;
        sr.color = new Color(c.r, c.g, c.b, startAlpha);
        transform.localScale = Vector3.one * 0.8f;

        // Movimiento hacia arriba (local)
        transform.DOLocalMoveY(transform.localPosition.y + riseDistance, duration)
            .SetEase(Ease.OutQuad);

        // Expansión
        transform.DOScale(1.2f, duration * 0.8f).SetEase(Ease.OutQuad);

        // Fade out
        sr.DOFade(finalAlpha, duration * 0.7f)
            .SetEase(Ease.InQuad)
            .SetDelay(fadeStartDelay);

        // Esperar el tiempo total antes de destruir al padre
        yield return new WaitForSeconds(duration);

        if (transform.parent != null)
            Destroy(transform.parent.gameObject);
        else
            Destroy(gameObject); // Fallback por si no tiene padre
    }
}
