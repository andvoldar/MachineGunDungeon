// Assets/_Scripts/UI/OptionsMenuUI.cs
using UnityEngine;
using UnityEngine.UI;

public class OptionsMenuUI : MonoBehaviour
{
    [Header("Botones del menú de opciones")]
    [SerializeField] private Button audioSettingsButton;
    [SerializeField] private Button graphicsSettingsButton;
    [SerializeField] private Button backButton;

    [Header("Panel principal de botones (Start, Options, Exit...)")]
    [SerializeField] private GameObject mainMenuButtonsPanel;


    [Header("Submenús")]
    [SerializeField] private GameObject audioMenuPanel;
    [SerializeField] private GameObject graphicsMenuPanel;


    private void Start()
    {
        audioSettingsButton.onClick.AddListener(HandleAudioSettingsPressed);
        graphicsSettingsButton.onClick.AddListener(HandleGraphicsSettingsPressed);
        backButton.onClick.AddListener(HandleBackPressed);
    }

    private void HandleAudioSettingsPressed()
    {
        Debug.Log("➔ Audio Settings pulsado");
        gameObject.SetActive(false);
        audioMenuPanel.SetActive(true);
    }

    private void HandleGraphicsSettingsPressed()
    {
        gameObject.SetActive(false);
        graphicsMenuPanel.SetActive(true); // <-- ABRIR SUBMENÚ GRAPHICS
    }

    private void HandleBackPressed()
    {
        Debug.Log("➔ Volver al menú principal");
        gameObject.SetActive(false);               // Oculta este panel
        mainMenuButtonsPanel.SetActive(true);      // Reactiva los botones del menú principal
    }
}
