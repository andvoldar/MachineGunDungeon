using UnityEngine;
using Cinemachine;
using DG.Tweening;

public class CineMachineShakeFeedback : Feedback
{
    [Header("Cinemachine Virtual Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Header("Shake Settings")]
    [SerializeField] private float amplitude = 1f;
    [SerializeField] private float frequency = 2f;
    [SerializeField] private float duration = 0.15f;

    [Header("Reduce Shake While Running")]
    [SerializeField] private bool reduceShakeWhileRunning = true;
    [SerializeField] private float runningShakeMultiplier = 0.5f;

    private CinemachineBasicMultiChannelPerlin noise;
    private Tween shakeTween;

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
    }

    public override void CreateFeedback()
    {
        CompletePreviousFeedback();

        if (noise == null) return;

        float finalAmplitude = amplitude;

        if (reduceShakeWhileRunning && IsPlayerRunning())
        {
            finalAmplitude *= runningShakeMultiplier;
        }

        noise.m_AmplitudeGain = finalAmplitude;
        noise.m_FrequencyGain = frequency;

        shakeTween = DOTween.To(() => noise.m_AmplitudeGain,
                                x => noise.m_AmplitudeGain = x,
                                0f, duration)
                            .SetEase(Ease.OutQuad);
    }

    public override void CompletePreviousFeedback()
    {
        if (shakeTween != null && shakeTween.IsActive())
            shakeTween.Kill();

        if (noise != null)
        {
            noise.m_AmplitudeGain = 0f;
            noise.m_FrequencyGain = 0f;
        }
    }

    private bool IsPlayerRunning()
    {
        PlayerController player = FindObjectOfType<PlayerController>();
        return player != null && player.playerIsMoving;
    }
}
