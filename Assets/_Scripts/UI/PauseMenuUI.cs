// Assets/_Scripts/UI/PauseMenuUI.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI del menú de pausa: botones Continue / Options / Main Menu.
/// Reutiliza tu OptionsMenuUI (mismo panel/submenús que en StartMenu).
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button continueButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button mainMenuButton;

    [Header("Panels")]
    [Tooltip("Panel que contiene los 3 botones (Continue/Options/MainMenu). Se oculta al abrir Options.")]
    [SerializeField] private GameObject pauseButtonsPanel;
    [Tooltip("Panel del OptionsMenuUI (instancia duplicada para in‑game).")]
    [SerializeField] private GameObject optionsMenuPanel;

    private void Awake()
    {
        if (continueButton) continueButton.onClick.AddListener(HandleContinue);
        if (optionsButton) optionsButton.onClick.AddListener(HandleOptions);
        if (mainMenuButton) mainMenuButton.onClick.AddListener(HandleMainMenu);
    }

    private void OnDestroy()
    {
        if (continueButton) continueButton.onClick.RemoveListener(HandleContinue);
        if (optionsButton) optionsButton.onClick.RemoveListener(HandleOptions);
        if (mainMenuButton) mainMenuButton.onClick.RemoveListener(HandleMainMenu);
    }

    private void HandleContinue()
    {
        PauseManager.Instance?.OnContinuePressed();
    }

    private void HandleOptions()
    {
        PauseManager.Instance?.OnOptionsPressed(pauseButtonsPanel, optionsMenuPanel);
    }

    private void HandleMainMenu()
    {
        PauseManager.Instance?.OnMainMenuPressed();
    }
}
