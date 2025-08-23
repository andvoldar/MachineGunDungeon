using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer))]
public class AgentRenderer : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private Color originalColor;
    private bool isShuttingDown = false;

    [Header("Dissolve Flash")]
    [SerializeField] private float dissolveDuration = 0.3f;
    [SerializeField] private string dissolveProperty = "_DissolveAmount";
    [SerializeField] private float startValue = 1f;
    [SerializeField] private float flashValue = 0.5f;

    [Header("Ghost Trail Effect")]
    [SerializeField] private GameObject ghostTrailPrefab;
    [SerializeField] private float trailDuration = 0.5f;

    [field: SerializeField]
    public UnityEvent<int> OnMovingBackwards { get; set; }

    private MaterialPropertyBlock propBlock;
    private Tween dissolveTween;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        originalColor = spriteRenderer.color;

        spriteRenderer.material = new Material(spriteRenderer.material);
        propBlock = new MaterialPropertyBlock();
        SetDissolve(startValue);
    }

    private void Update()
    {
        if (isShuttingDown) return;

        if (Time.frameCount % 2 == 0)
            FaceDirection();
    }

    private void FaceDirection()
    {
        Vector2 mouseWorldPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 playerPosition = transform.position;
        spriteRenderer.flipX = mouseWorldPosition.x < playerPosition.x;
    }

    public void LookAt(Vector2 pointerWorldPosition)
    {
        Vector2 playerPosition = transform.position;
        spriteRenderer.flipX = pointerWorldPosition.x < playerPosition.x;
    }

    public void CheckIfMovingBackwards(Vector2 movementVector)
    {
        if (isShuttingDown) return;

        float angle = 0;
        if (spriteRenderer.flipX)
        {
            angle = Vector2.Angle(-transform.right, movementVector);
        }
        else
        {
            angle = Vector2.Angle(transform.right, movementVector);
        }

        if (angle > 90)
            OnMovingBackwards?.Invoke(-1);
        else
            OnMovingBackwards?.Invoke(1);
    }

    public void PlayGetHitPlayerVisuals()
    {
        if (isShuttingDown) return;

        StartCoroutine(BlinkRedOnHit());
        PlayDissolveFlash();
    }

    public void PlayHealVisualFeedback()
    {
        if (isShuttingDown) return;

        StartCoroutine(BlinkGreenOnHeal());
        PlayDissolveFlash(Color.green);
        PlayScalePop();
    }

    private IEnumerator BlinkRedOnHit()
    {
        int blinkCount = 3;
        float blinkDuration = 0.1f;

        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(blinkDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }
    }

    private IEnumerator BlinkGreenOnHeal()
    {
        int blinkCount = 2;
        float blinkDuration = 0.1f;
        Color healColor = Color.green;

        for (int i = 0; i < blinkCount; i++)
        {
            spriteRenderer.color = healColor;
            yield return new WaitForSeconds(blinkDuration);
            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(blinkDuration);
        }
    }

    public void PlayDissolveFlash()
    {
        if (isShuttingDown) return;
        PlayDissolveFlash(Color.white);
    }

    public void PlayDissolveFlash(Color colorOverride)
    {
        if (isShuttingDown) return;

        CompleteDissolveTween();

        dissolveTween = DOTween.Sequence()
            .Append(DOTween.To(() => startValue, SetDissolve, flashValue, dissolveDuration / 2f))
            .AppendCallback(() => spriteRenderer.color = colorOverride)
            .Append(DOTween.To(() => flashValue, SetDissolve, startValue, dissolveDuration / 2f))
            .AppendCallback(() => spriteRenderer.color = originalColor)
            .SetEase(Ease.InOutQuad);
    }

    private void PlayScalePop()
    {
        if (isShuttingDown) return;

        Transform t = transform;
        t.DOKill();
        t.localScale = Vector3.one;
        t.DOPunchScale(Vector3.one * 0.15f, 0.3f, 8, 0.4f)
         .SetEase(Ease.OutBack)
         .SetLink(gameObject);
    }

    private void CompleteDissolveTween()
    {
        if (dissolveTween != null && dissolveTween.IsActive())
        {
            dissolveTween.Kill();
            dissolveTween = null;
        }
    }

    public void SetDissolve(float value)
    {
        if (spriteRenderer == null) return;

        spriteRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat(dissolveProperty, value);
        spriteRenderer.SetPropertyBlock(propBlock);
    }

    public void ResetVisualsPlayerAvatar()
    {
        if (isShuttingDown) return;

        spriteRenderer.color = originalColor;
        SetDissolve(startValue);
    }

    public void CreateGhostTrail(Vector3 position, Sprite sprite, bool flipX)
    {
        if (ghostTrailPrefab == null) return;

        GameObject ghost = Instantiate(ghostTrailPrefab, position, Quaternion.identity);
        GhostTrail ghostScript = ghost.GetComponent<GhostTrail>();
        ghostScript.Init(sprite, position, transform.localScale, flipX);
        Destroy(ghost, trailDuration);
    }

    private void OnDestroy()
    {
        CompleteDissolveTween();

        if (spriteRenderer != null)
            DOTween.Kill(spriteRenderer);

        DOTween.Kill(transform);
        DOTween.Kill(gameObject);
    }

    public void ShutdownRenderer()
    {
        isShuttingDown = true;

        CompleteDissolveTween();

        if (spriteRenderer != null)
            DOTween.Kill(spriteRenderer);

        DOTween.Kill(transform);
        DOTween.Kill(gameObject);
    }
}
