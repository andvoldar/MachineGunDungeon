using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

[RequireComponent(typeof(Light2D))]
public class ExplosionLightFlash : MonoBehaviour
{
    [Header("Expansión de Luz")]
    [SerializeField] private float startRadius = 1f;
    [SerializeField] private float endRadius = 8f;
    [SerializeField] private float duration = 0.15f;
    [SerializeField] private Ease ease = Ease.OutCubic;

    private Light2D light2D;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();
    }

    private void OnEnable()
    {
        light2D.pointLightOuterRadius = startRadius;

        // Expansión rápida
        DOTween.To(
            () => light2D.pointLightOuterRadius,
            r => light2D.pointLightOuterRadius = r,
            endRadius,
            duration
        )
        .SetEase(ease)
        .SetLink(gameObject);
    }
}
