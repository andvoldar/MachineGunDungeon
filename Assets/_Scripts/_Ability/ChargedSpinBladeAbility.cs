using UnityEngine;
using DG.Tweening;
using FMODUnity;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(SpriteRenderer))]
public class ChargedSpinBladeAbility : MonoBehaviour
{
    [Header("Carga Visual")]
    [SerializeField] private float chargeTime = 1f;
    [SerializeField] private ParticleSystem chargeParticles;
    [SerializeField] private Light2D glowLight;
    [SerializeField] private float maxGlowIntensity = 1f;

    [SerializeField] private SpriteRenderer swordSprite;

    [Header("Spin—Daño y Efectos")]
    [SerializeField] private int spinCount = 3;
    [SerializeField] private float spinDurationPerTurn = 0.3f;

    [Header("Ghost Trail")]
    [SerializeField] private GameObject ghostTrailPrefab;
    [SerializeField] private float ghostInterval = 0.05f;
    [SerializeField] private float ghostLifetime = 0.5f;

    private MeleeHitbox hitbox;
    private Transform swordTf;
    private Color originalColor;
    private float originalGlow;
    private Tween glowTween;
    private bool isCharging;

    private Transform holder;
    private Vector3 offsetLocal;
    private float initialLocalRotZ;
    private bool isSpinning;
    private float ghostTimer;

    private void OnEnable()
    {
        swordTf = swordSprite != null ? swordSprite.transform : transform;
        originalColor = swordSprite != null ? swordSprite.color : Color.white;
        originalGlow = glowLight != null ? glowLight.intensity : maxGlowIntensity;

        holder = transform.parent != null ? transform.parent : transform;
        offsetLocal = swordTf.localPosition;
        initialLocalRotZ = swordTf.localEulerAngles.z;

        hitbox = GetComponentInChildren<MeleeHitbox>();
        if (hitbox == null)
            Debug.LogWarning("ChargedSpinBladeAbility: no se encontró MeleeHitbox en hijos.");
        else
            hitbox.SetEnabled(false);

        if (chargeParticles) chargeParticles.Stop();
        if (glowLight)
        {
            glowLight.intensity = 0f;
            glowLight.gameObject.SetActive(false);
        }
    }


    private void Update()
    {
        if (!enabled) return;

        if (!isCharging && !isSpinning && Input.GetMouseButtonDown(1))
            TriggerSpinAttack();

        if (isCharging && Input.GetMouseButtonUp(1))
            CancelCharge();

        if (isSpinning)
        {
            ghostTimer -= Time.deltaTime;
            if (ghostTimer <= 0f)
            {
                SpawnGhost();
                ghostTimer = ghostInterval;
            }
        }
    }

    public void TriggerSpinAttack()
    {
        if (swordTf == null || swordSprite == null)
        {
            Debug.LogWarning("ChargedSpinBladeAbility: referencias no inicializadas.");
            return;
        }

        isCharging = true;

        if (glowLight) glowLight.gameObject.SetActive(true);
        if (chargeParticles) chargeParticles.Play();
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound(SoundType.SwordChargeSFX, swordTf.position);

        if (glowLight)
        {
            glowLight.intensity = 0f;
            glowTween = DOTween.To(
                () => glowLight.intensity,
                x => glowLight.intensity = x,
                maxGlowIntensity,
                chargeTime
            ).SetEase(Ease.InOutSine);
        }

        DOVirtual.DelayedCall(chargeTime, () => {
            if (isCharging)
                BeginSpin();
        });
    }


    private void CancelCharge()
    {
        isCharging = false;
        if (chargeParticles) chargeParticles.Stop();
        if (glowTween != null) glowTween.Kill();

        if (glowLight)
        {
            DOTween.To(
                () => glowLight.intensity,
                x => glowLight.intensity = x,
                0f,
                0.2f
            ).SetEase(Ease.Linear)
             .OnComplete(() => glowLight.gameObject.SetActive(false));
        }
    }

    private void BeginSpin()
    {
        isCharging = false;
        isSpinning = true;

        if (chargeParticles) chargeParticles.Stop();
        if (glowTween != null) glowTween.Kill(true);
        if (glowLight) glowLight.intensity = maxGlowIntensity;
        swordSprite.color = Color.red;

        FaceMouse();

        offsetLocal = holder.InverseTransformPoint(swordTf.position);
        initialLocalRotZ = swordTf.localEulerAngles.z;

        if (hitbox != null)
            hitbox.SetEnabled(true);

        SoundManager.Instance.PlaySound(SoundType.SwordSpinSFX, swordTf.position);

        ghostTimer = 0f;

        float totalAngle = -360f * spinCount;
        DOVirtual.Float(
            0f,
            totalAngle,
            spinDurationPerTurn * spinCount,
            angle =>
            {
                swordTf.localPosition = Quaternion.Euler(0, 0, angle) * offsetLocal;
                swordTf.localEulerAngles = new Vector3(0, 0, initialLocalRotZ + angle);
            }
        )
        .SetEase(Ease.Linear)
        .OnComplete(FinishSpin);
    }

    private void FinishSpin()
    {
        isSpinning = false;

        if (hitbox != null)
            hitbox.SetEnabled(false);

        if (glowLight)
        {
            DOTween.To(
                () => glowLight.intensity,
                x => glowLight.intensity = x,
                0f,
                0.2f
            ).SetEase(Ease.Linear)
             .OnComplete(() => glowLight.gameObject.SetActive(false));
        }

        swordSprite.DOColor(originalColor, 0.2f).SetEase(Ease.Linear);
        swordTf.localPosition = offsetLocal;
        FaceMouse();
    }

    private void SpawnGhost()
    {
        if (ghostTrailPrefab == null) return;
        GameObject ghost = Instantiate(ghostTrailPrefab, swordTf.position, swordTf.rotation);
        if (ghost.TryGetComponent<GhostTrail>(out var trail))
        {
            trail.Init(
                swordSprite.sprite,
                swordTf.position,
                swordTf.localScale,
                swordSprite.flipX,
                swordSprite.material
            );
        }
        Destroy(ghost, ghostLifetime);
    }

    private void FaceMouse()
    {
        Vector3 mouse = Input.mousePosition;
        Vector3 worldMouse = Camera.main.ScreenToWorldPoint(
            new Vector3(mouse.x, mouse.y, Camera.main.nearClipPlane)
        );
        Vector3 dir = worldMouse - holder.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        swordTf.localEulerAngles = new Vector3(0, 0, angle);
    }
}
