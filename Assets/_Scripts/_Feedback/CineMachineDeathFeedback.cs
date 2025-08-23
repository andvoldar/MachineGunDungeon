using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CinemachineDeathFeedback : Feedback
{
    [Header("Cinemachine Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Header("Shake Settings")]
    [SerializeField] private float amplitude = 2f;
    [SerializeField] private float frequency = 2f;
    [SerializeField] private float shakeDuration = 0.3f;

    [Header("Vignette Settings")]
    [SerializeField] private Volume volume;
    [SerializeField] private float vignetteIntensity = 0.5f;
    [SerializeField] private float vignetteDuration = 1.5f;

    private CinemachineBasicMultiChannelPerlin noise;
    private Tween shakeTween;
    private Tween vignetteTween;
    private Vignette vignette;

    private void Awake()
    {
        if (virtualCamera == null)
        {
            virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
        }

        if (virtualCamera != null)
        {
            noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        if (volume != null && volume.profile.TryGet(out Vignette v))
        {
            vignette = v;
        }
    }

    public override void CreateFeedback()
    {
        CompletePreviousFeedback();

        // Temblor sincronizado con slowmo
        if (noise != null)
        {
            noise.m_AmplitudeGain = amplitude;
            noise.m_FrequencyGain = frequency;

            shakeTween = DOTween.To(() => noise.m_AmplitudeGain,
                                    x => noise.m_AmplitudeGain = x,
                                    0f, shakeDuration)
                                .SetEase(Ease.OutSine);
        }

        // Oscurecimiento con viñeta
        if (vignette != null)
        {
            vignette.intensity.value = 0f;
            vignette.active = true;

            vignetteTween = DOTween.To(() => vignette.intensity.value,
                                       x => vignette.intensity.value = x,
                                       vignetteIntensity,
                                       vignetteDuration)
                                   .SetEase(Ease.OutQuad);
        }
    }

    public override void CompletePreviousFeedback()
    {
        shakeTween?.Kill();
        vignetteTween?.Kill();

        if (noise != null)
        {
            noise.m_AmplitudeGain = 0f;
            noise.m_FrequencyGain = 0f;
        }

        if (vignette != null)
        {
            vignette.intensity.value = 0f;
            vignette.active = false;
        }
    }
}
