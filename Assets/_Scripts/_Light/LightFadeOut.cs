using UnityEngine;
using UnityEngine.Rendering.Universal; // Asegúrate de tener esta referencia para trabajar con Light2D.

public class LightFadeOut : MonoBehaviour
{
    private Light2D light2D;
    private float targetIntensity;
    private float fadeDuration = 2f; // Duración del fade
    private float currentFadeTime = 0f;
    private bool isFadingOut = false;

    private void Awake()
    {
        // Asegúrate de obtener el Light2D del objeto.
        light2D = GetComponent<Light2D>();
        targetIntensity = light2D.intensity; // Guardamos la intensidad actual.
    }

    private void Update()
    {
        // Si estamos desvaneciendo la luz, reducimos su intensidad gradualmente.
        if (isFadingOut)
        {
            currentFadeTime += Time.deltaTime;

            // Calculamos la nueva intensidad usando Lerp (de la intensidad actual a 0).
            float newIntensity = Mathf.Lerp(targetIntensity, 0f, currentFadeTime / fadeDuration);

            // Aplicamos la nueva intensidad.
            light2D.intensity = newIntensity;

            // Cuando el fade se complete, detenemos el proceso.
            if (currentFadeTime >= fadeDuration)
            {
                isFadingOut = false;
                light2D.intensity = 0f; // Nos aseguramos de que llegue a 0
            }
        }
    }

    // Método para iniciar el fade out.
    public void StartFadeOut()
    {
        isFadingOut = true;
        currentFadeTime = 0f; // Resetamos el contador del tiempo de fade
    }
}
