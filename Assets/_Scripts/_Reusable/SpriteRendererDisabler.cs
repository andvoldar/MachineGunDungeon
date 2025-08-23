using UnityEngine;

public class SpriteRendererDisabler : MonoBehaviour
{
    private SpriteRenderer sr;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning($"No SpriteRenderer found on {gameObject.name}");
        }
    }

    // Esta función se llama desde un Animation Event
    public void DisableSpriteRenderer()
    {
        if (sr != null)
        {
            sr.enabled = false;
        }
    }
}
