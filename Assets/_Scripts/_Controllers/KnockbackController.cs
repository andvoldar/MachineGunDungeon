using UnityEngine;

public class KnockbackController : MonoBehaviour
{
    private Rigidbody2D rb;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;

    public bool IsKnockedBack => isKnockedBack;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0)
            {
                isKnockedBack = false;
                rb.velocity = Vector2.zero;
            }
        }
    }

    // Se utiliza AddForce con ForceMode2D.Impulse para aplicar un impulso suave.
    public void ApplyKnockback(Vector2 knockbackDirection, float force, float duration)
    {
        if (!isKnockedBack)
        {
            rb.AddForce(knockbackDirection * force, ForceMode2D.Impulse);
            isKnockedBack = true;
            knockbackTimer = duration;
        }
    }
}
