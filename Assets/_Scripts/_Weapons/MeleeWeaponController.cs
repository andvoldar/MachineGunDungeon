// Assets/Scripts/Weapons/MeleeWeaponController.cs
using UnityEngine;
using FMODUnity;
using DG.Tweening;

[RequireComponent(typeof(MeleeWeapon))]
public class MeleeWeaponController : MonoBehaviour, IWeaponController
{
    private MeleeWeapon meleeWeapon;
    private WeaponRenderer weaponRenderer;
    private MeleeHitbox hitboxComp;
    private Tween swingTween;
    private IWeaponAbility specialAbility;

    [Header("Aiming Settings")]
    [SerializeField, Tooltip("Speed smoothing rotation toward the pointer")]
    private float aimSmoothSpeed = 15f;

    [Header("Swing Settings")]
    [SerializeField, Tooltip("Total arc of the swing in degrees")]
    private float swingAngle = 120f;
    [SerializeField, Tooltip("Wind-up duration before the main swing")]
    private float windupDuration = 0.1f;
    [SerializeField, Tooltip("Duration of the main swing motion")]
    private float swingDuration = 0.15f;
    [SerializeField, Tooltip("Recovery time returning to idle")]
    private float recoverDuration = 0.1f;


    private bool isFlippedY = false;


    private void Awake()
    {
        meleeWeapon = GetComponent<MeleeWeapon>();
        weaponRenderer = GetComponent<WeaponRenderer>() ?? GetComponentInChildren<WeaponRenderer>();
        hitboxComp = GetComponentInChildren<MeleeHitbox>();
        // may be null if this weapon has no special ability attached
        specialAbility = GetComponent<IWeaponAbility>();
    }

    /// <summary>Soft-rotate the sword to point at the mouse.</summary>
    public void AimWeapon(Vector2 pointerPosition)
    {
        Vector3 parentPos = transform.parent.position;
        Vector2 toMouse = (pointerPosition - (Vector2)parentPos).normalized;
        float rawAngle = Mathf.Atan2(toMouse.y, toMouse.x) * Mathf.Rad2Deg;

        // Flip sprite on Y when pointing left

        isFlippedY = rawAngle > 90f || rawAngle < -90f;
        weaponRenderer?.FlipWeaponSprite(isFlippedY);


        // Sorting order: behind when pointing up
        bool behind = rawAngle > 0f && rawAngle < 180f;
        weaponRenderer?.RenderWeaponBehindPlayer(behind);

        // Smooth rotation
        float currZ = transform.localEulerAngles.z;
        if (currZ > 180f) currZ -= 360f;
        float smoothed = Mathf.LerpAngle(currZ, rawAngle, Time.deltaTime * aimSmoothSpeed);
        transform.localRotation = Quaternion.Euler(0f, 0f, smoothed);
    }

    /// <summary>Standard left-click swing.</summary>
    public void HandleTriggerPressed()
    {
        if (swingTween != null || !meleeWeapon.CanAttack)
            return;


        // Invoke swing
        meleeWeapon.StartSwing();
        RuntimeManager.PlayOneShot(meleeWeapon.weaponData.SwingSound, transform.position);
        swingTween = CreateSwingSequence();

    }

    public void HandleTriggerReleased()
    {
        // no-op for melee
    }

    /// <summary>Right-click begins special ability.</summary>
    public void HandleAltPressed()
    {
        specialAbility?.OnAbilityPressed();
    }

    /// <summary>Releasing right-click ends or cancels special.</summary>
    public void HandleAltReleased()
    {
        specialAbility?.OnAbilityReleased();
    }

    /// <summary>Kill any active tweens and reset state.</summary>
    public void StopAllFire()
    {
        swingTween?.Kill();
        swingTween = null;
        hitboxComp?.SetEnabled(false);
        specialAbility?.CancelAbility();
    }

    /// <summary>Full reset called when weapon is (re)equipped.</summary>
    public void FullReset()
    {
        StopAllFire();
        transform.localRotation = Quaternion.identity;
    }

    /// <summary>Builds the wind-up ➔ swing ➔ recovery sequence.</summary>
    private Sequence CreateSwingSequence()
    {
        float baseZ = transform.localEulerAngles.z;
        if (baseZ > 180f) baseZ -= 360f;

        float halfArc = swingAngle * 0.5f;

        // Si está volteado (mirando a la izquierda), invertimos el arco
        float startZ, endZ;
        if (isFlippedY)
        {
            startZ = baseZ - halfArc; // empieza arriba (más negativo)
            endZ = baseZ + halfArc;   // termina abajo (más positivo)
        }
        else
        {
            startZ = baseZ + halfArc;
            endZ = baseZ - halfArc;
        }

        hitboxComp?.SetEnabled(false);

        var seq = DOTween.Sequence();
        seq.Append(transform.DOLocalRotate(Vector3.forward * startZ, windupDuration)
                     .SetEase(Ease.OutCubic));
        seq.AppendCallback(() => hitboxComp?.SetEnabled(true));
        seq.Append(transform.DOLocalRotate(Vector3.forward * endZ, swingDuration)
                     .SetEase(Ease.OutQuad));
        seq.AppendCallback(() => hitboxComp?.SetEnabled(false));
        seq.Append(transform.DOLocalRotate(Vector3.forward * baseZ, recoverDuration)
                     .SetEase(Ease.InQuad));
        seq.OnComplete(() => swingTween = null);

        return seq;
    }

}
