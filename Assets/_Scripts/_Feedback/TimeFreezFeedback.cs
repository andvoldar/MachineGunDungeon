using System.Collections;
using UnityEngine;

public class TimeFreezeFeedback : Feedback
{
    [SerializeField]
    private float freezeDuration = 1f; // Duración del Time Freeze
    [SerializeField]
    private float freezeTimescale = 0.1f; // Escala de tiempo durante el congelamiento
    [SerializeField]
    private float smoothTransitionDuration = 0.5f; // Duración de la transición suave

    private float originalTimeScale;
    private float targetTimeScale;
    private float currentTimeScale;

    private bool isTimeFreezing = false;

    private void Awake()
    {
        originalTimeScale = Time.timeScale;
        currentTimeScale = originalTimeScale;
    }

    public override void CompletePreviousFeedback()
    {
        StopAllCoroutines();

        if (isTimeFreezing)
        {
            // Restauramos el tiempo a su estado original con una transición suave
            StartCoroutine(TransitionTimeScale(originalTimeScale, smoothTransitionDuration));
        }
    }

    public override void CreateFeedback()
    {
        // Iniciar el efecto de Time Freeze
        isTimeFreezing = true;
        targetTimeScale = freezeTimescale;

        // Suavizamos el cambio del TimeScale
        StartCoroutine(TransitionTimeScale(targetTimeScale, smoothTransitionDuration));

        // Restauramos el tiempo normal después de la duración
        StartCoroutine(RestoreNormalTimeScaleAfterDelay());
    }

    private IEnumerator TransitionTimeScale(float targetScale, float duration)
    {
        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            currentTimeScale = Mathf.Lerp(Time.timeScale, targetScale, timeElapsed / duration);
            Time.timeScale = currentTimeScale;
            Time.fixedDeltaTime = 0.02f * Time.timeScale;
            timeElapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // Asegurar que el time scale final sea exactamente el deseado
        Time.timeScale = targetScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
    }

    private IEnumerator RestoreNormalTimeScaleAfterDelay()
    {
        // Esperamos la duración del congelamiento
        yield return new WaitForSecondsRealtime(freezeDuration);

        // Restauramos el tiempo normal después de la duración con transición suave
        isTimeFreezing = false;
        StartCoroutine(TransitionTimeScale(originalTimeScale, smoothTransitionDuration));
    }
}
