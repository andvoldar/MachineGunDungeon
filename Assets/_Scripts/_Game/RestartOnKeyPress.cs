using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Este script recarga la escena actual cuando se pulsa la tecla definida.
/// </summary>
public class RestartOnKeyPress : MonoBehaviour
{
    [Tooltip("Tecla para reiniciar el nivel")]
    public KeyCode restartKey = KeyCode.K;

    void Update()
    {
        // Detectar pulsación de la tecla
        if (Input.GetKeyDown(restartKey))
        {
            ReloadCurrentScene();
        }
    }

    /// <summary>
    /// Recarga la escena activa usando su índice en el Build Settings.
    /// </summary>
    private void ReloadCurrentScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.buildIndex);
    }
}
