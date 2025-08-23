using System.Collections;
using UnityEngine;

public class EnemyAvatar : MonoBehaviour
{

    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private Color originalColor;
    private Vector3 originalScale;
    public bool IsFacingLeft => spriteRenderer.flipX;
    private FeedbackPlayer feedbackPlayer;
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        feedbackPlayer = GetComponent<FeedbackPlayer>();
        originalColor = spriteRenderer.color;
        originalScale = transform.localScale;
    }

    public void LookAtMovement(Vector2 moveDirection)
    {
        if (moveDirection.x > 0) spriteRenderer.flipX = false;
        else if (moveDirection.x < 0) spriteRenderer.flipX = true;
    }


    public void PlayAttackEffectEnemyAvatar()
    {
        spriteRenderer.color = new Color(1f, 0.6f, 0f); // naranja
        transform.localScale = originalScale * 1.2f;
        transform.localPosition = new Vector3(0f, 0.5f, 0f); // pequeño salto visual
        CancelInvoke(nameof(ResetVisualsEnemyAvatar));
        Invoke(nameof(ResetVisualsEnemyAvatar), 0.2f);
    }

    public void PlayGetHitVisuals()
    {

        StartCoroutine(BlinkRedOnHit());

    }

    public void PlayDeathVisuals()
    {
        feedbackPlayer.PlayFeedback();
    }

    private IEnumerator BlinkRedOnHit()
    {
        // Número de parpadeos y duración
        int blinkCount = 2;
        float blinkDuration = 0.1f; // Duración de cada parpadeo

        for (int i = 0; i < blinkCount; i++)
        {
            // Cambiar color a rojo
            spriteRenderer.color = Color.red;

            // Esperar un breve momento
            yield return new WaitForSeconds(blinkDuration);

            // Volver al color original
            spriteRenderer.color = originalColor;

            // Esperar otro breve momento antes de parpadear de nuevo
            yield return new WaitForSeconds(blinkDuration);
        }
    }


    public void ResetVisualsEnemyAvatar()
    {
        spriteRenderer.color = originalColor;
        transform.localScale = originalScale;
        transform.localPosition = Vector3.zero;
    }
}
