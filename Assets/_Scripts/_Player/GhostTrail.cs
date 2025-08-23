using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class GhostTrail : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Color startColor;

    [SerializeField] private float fadeDuration = 0.2f;
    private float timer;

    public void Init(Sprite sprite, Vector3 position, Vector3 scale, bool flipX, Material ghostMaterial = null)
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        spriteRenderer.sprite = sprite;
        spriteRenderer.flipX = flipX;
        transform.position = position;
        transform.localScale = scale;

        // Asigna un material alternativo si se proporciona (ej: dissolve, ghost, etc.)
        if (ghostMaterial != null)
        {
            spriteRenderer.material = new Material(ghostMaterial); // instancia única
        }

        startColor = spriteRenderer.color;
        timer = fadeDuration;
    }

    private void Update()
    {
        if (spriteRenderer == null) return; // <- Añadir esta línea de seguridad

        timer -= Time.unscaledDeltaTime; // Usar Time.unscaledDeltaTime para desacoplar el tiempo de la ralentización
        float alpha = Mathf.Clamp01(timer / fadeDuration);

        // Actualiza transparencia
        Color color = startColor;
        color.a = alpha;
        spriteRenderer.color = color;

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
