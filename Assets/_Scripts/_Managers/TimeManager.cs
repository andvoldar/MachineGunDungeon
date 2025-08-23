using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static TimeManager Instance { get; private set; }

    [Range(0.1f, 1f)] public float slowMotionScale = 0.3f;
    public float slowMotionDuration = 0.3f;

    private float defaultFixedDeltaTime;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        defaultFixedDeltaTime = Time.fixedDeltaTime;
    }

    public void DoSlowMotion(float duration = -1f)
    {
        Time.timeScale = slowMotionScale;
        Time.fixedDeltaTime = defaultFixedDeltaTime * slowMotionScale;

        CancelInvoke(nameof(RestoreTimeScale));
        Invoke(nameof(RestoreTimeScale), duration > 0 ? duration : slowMotionDuration);
    }

    public void RestoreTimeScale()
    {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = defaultFixedDeltaTime;
    }
}
