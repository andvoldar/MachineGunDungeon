using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DroppedWeaponVisuals : MonoBehaviour
{
    [Header("Floating Animation")]
    [SerializeField] private float floatAmplitude = 0.1f;
    [SerializeField] private float floatFrequency = 1f;

    [Header("Light Settings")]
    [SerializeField] private Light2D weaponLight;
    [SerializeField] private float lightFluctuationSpeed = 1f;
    [SerializeField] private float lightFluctuationRange = 0.5f;
    [SerializeField] private float lightBaseIntensity = 2f;

    [Header("Throw Settings")]
    [SerializeField] private float throwDistance = 2.5f;
    [SerializeField] private float throwHeight = 1.2f;
    [SerializeField] private float throwDuration = 0.4f;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private Vector3 baseLocalPosition;
    private float baseY;
    private bool isActive = false;
    private bool isThrowing = false;

    private Vector3 originalLightOffset;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        baseLocalPosition = transform.localPosition;
        baseY = transform.localPosition.y;

        if (weaponLight != null)
        {
            originalLightOffset = weaponLight.transform.localPosition;
        }

        ActivateVisuals();
    }

    private void Update()
    {
        if (!isActive || isThrowing) return;

        AnimateFloating();
        AnimateLight();
    }

    private void AnimateFloating()
    {
        float floatOffset = Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        Vector3 localPos = baseLocalPosition;
        localPos.y = baseY + floatOffset;
        transform.localPosition = localPos;
    }

    private void AnimateLight()
    {
        if (weaponLight == null) return;

        float fluctuation = Mathf.PingPong(Time.time * lightFluctuationSpeed, lightFluctuationRange);
        weaponLight.intensity = lightBaseIntensity + fluctuation;
    }

    private void UpdateLightPosition()
    {
        if (weaponLight == null) return;
        Vector3 offset = originalLightOffset;
        // Flip X offset when sprite is flipped
        offset.x = spriteRenderer.flipX ? -Mathf.Abs(originalLightOffset.x) : Mathf.Abs(originalLightOffset.x);
        weaponLight.transform.localPosition = offset;
    }

    public void ActivateVisuals()
    {
        isActive = true;

        if (weaponLight != null)
        {
            weaponLight.enabled = true;
            UpdateLightPosition();
        }

        SetInteractable(true);
    }

    public void DeactivateVisuals()
    {
        isActive = false;

        if (weaponLight != null)
            weaponLight.enabled = false;

        transform.localPosition = baseLocalPosition;
        transform.rotation = Quaternion.identity;

        SetInteractable(false);
    }

    public void Throw(Vector2 direction)
    {
        if (isThrowing) return;
        isThrowing = true;

        // Calculate angle and flip
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        spriteRenderer.flipX = IsFacingLeft(angle);
        UpdateLightPosition();

        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + (Vector3)(direction.normalized * throwDistance);

        SetInteractable(false);
        DeactivateVisuals();

        transform.DOKill();
        rb.velocity = Vector2.zero;

        transform.DOJump(targetPos, throwHeight, 1, throwDuration)
            .SetEase(Ease.OutQuad)
            .OnKill(() =>
            {
                transform.position = targetPos;
                transform.rotation = Quaternion.identity;
                baseLocalPosition = transform.localPosition;
                baseY = transform.localPosition.y;

                ActivateVisuals();
                isThrowing = false;
            });
    }

    private bool IsFacingLeft(float zAngle)
    {
        return zAngle > 90f && zAngle < 270f;
    }

    private void SetInteractable(bool value)
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = value;

        gameObject.tag = value ? "Pickup" : "Untagged";
    }

    public void SetCanPickup(bool canPickup)
    {
        if (canPickup)
        {
            ActivateVisuals();
        }
        else
        {
            DeactivateVisuals();
        }
    }

    private void OnDisable()
    {
        transform.DOKill();
    }
}
