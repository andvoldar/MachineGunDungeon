using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class StartMenuUI : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button exitButton;
    [SerializeField] private GameObject menuButtonsContainer;
    [SerializeField] private Image fadeImage;

    [Header("Configuración")]
    [SerializeField] private string gameplaySceneName = "SandBox";
    [SerializeField] private float fadeDuration = 1.5f;

    [Header("Sonidos (opcional)")]
    [SerializeField] private StartMenuSoundController soundController;

    private bool isTransitioning = false;
    private List<Button> allMenuButtons = new List<Button>();

    private void Start()
    {
        if (fadeImage != null)
        {
            SetAlpha(0f); // Fade empieza invisible
        }

        if (startButton != null)
        {
            startButton.onClick.AddListener(HandleStartPressed);
        }
        else
        {
            Debug.LogError("No está asignado el StartButton en StartMenuUI.");
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(HandleExitPressed);
        }
        else
        {
            Debug.LogError("No está asignado el ExitButton en StartMenuUI.");
        }

        if (menuButtonsContainer != null)
        {
            allMenuButtons.AddRange(menuButtonsContainer.GetComponentsInChildren<Button>(true));
        }
        else
        {
            Debug.LogWarning("No se asignó MenuButtonsContainer en StartMenuUI.");
        }
    }

    private void HandleStartPressed()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        Debug.Log("➔ StartButton pulsado");

        if (soundController != null)
        {
            soundController.PlayClickSound();       // Primero suena el click
            soundController.PlayStartGameSound();   // Luego el sonido especial de empezar
            soundController.FadeOutMusic(fadeDuration); // Mientras, empieza a hacer fade la música
            soundController.LockSounds();           // 🔥 SOLO DESPUÉS, bloqueamos nuevos sonidos
        }

        DisableMenuButtons(); // Desactivamos interacción (pero ya sonaron los clicks)

        if (fadeImage != null)
        {
            StartCoroutine(FadeAndLoadScene());
        }
        else
        {
            Debug.LogWarning("No se encontró el FadeImage, cargando directamente.");
            GameManager.Instance.LoadScene(gameplaySceneName);
        }
    }

    private void HandleExitPressed()
    {
        if (isTransitioning) return;
        isTransitioning = true;

        Debug.Log("➔ ExitButton pulsado");

        if (soundController != null)
        {
            soundController.PlayClickSound();
            soundController.LockSounds(); // 🔥 Después de sonar el click
        }

        DisableMenuButtons();
        StartCoroutine(ExitGameCoroutine());
    }

    private IEnumerator FadeAndLoadScene()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(1f);

        yield return new WaitForSeconds(0.2f); // Pequeño delay

        GameManager.Instance.LoadScene(gameplaySceneName);
    }

    private IEnumerator ExitGameCoroutine()
    {
        if (fadeImage != null)
        {
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Clamp01(elapsedTime / fadeDuration);
                SetAlpha(alpha);
                yield return null;
            }

            SetAlpha(1f);
        }

        yield return new WaitForSeconds(0.2f);

        Debug.Log("➔ Cerrando el juego...");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    private void SetAlpha(float alpha)
    {
        if (fadeImage == null) return;

        Color color = fadeImage.color;
        color.a = alpha;
        fadeImage.color = color;
    }

    private void DisableMenuButtons()
    {
        foreach (var button in allMenuButtons)
        {
            button.interactable = false;
        }
    }
}
