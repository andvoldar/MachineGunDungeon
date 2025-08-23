using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using UnityEngine.UI;

public enum GameState
{
    MainMenu,
    Playing,
    Paused,
    Options,
    Controls
}

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Referencias a pantallas UI")]
    [SerializeField] private GameObject startMenuUI;

    [Header("Fade Settings")]
    [SerializeField] private Image fadeImage; // Imagen negra que está como hijo del UIManager
    [SerializeField] private float fadeInDuration = 1.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
        HandleGameStateChanged(GameManager.Instance.CurrentState);

        SceneManager.sceneLoaded += OnSceneLoaded;

        if (fadeImage == null)
        {
            Debug.LogWarning("➔ No se asignó la imagen de Fade en el UIManager.");
        }
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void HandleGameStateChanged(GameState state)
    {
        startMenuUI?.SetActive(state == GameState.MainMenu);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"➔ Escena cargada: {scene.name}");

        if (fadeImage == null)
        {
            Debug.LogWarning("➔ No se encontró fadeImage.");
            return;
        }

        SetAlpha(1f); // Empezamos negro
        StartCoroutine(FadeIn());
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeInDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(0f);
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }
}
