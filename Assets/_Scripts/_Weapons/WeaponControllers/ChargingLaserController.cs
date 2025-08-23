// Assets/Scripts/Weapons/ChargingLaserController.cs
using System.Collections;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(LineRenderer), typeof(Weapon))]
public class ChargingLaserController : LaserWeaponController
{
    [Header("Charge VFX")]
    [SerializeField] private Light2D chargeLight;
    [SerializeField] private SpriteRenderer chargeSprite;

    [Header("Glow Flash")]
    [SerializeField] private string glowProperty = "_Glow";
    [SerializeField] private float baseGlow = 1f;
    [SerializeField] private float flashGlow = 2f;
    [SerializeField] private float glowDuration = 0.5f;

    [Header("Dissolve")]
    [SerializeField] private string dissolveProperty = "_DissolveAmount";
    [SerializeField] private float baseDissolve = 1f;

    private Material beamMaterial;
    private Tween glowTween;

    private Coroutine chargeRoutine;
    private Coroutine fireRoutine;
    private bool isCharging;

    // ----------------------------------------------------------------------------
    // ① Flag para suprimir el siguiente sonido de release (sólo al dropear o swap)
    // ----------------------------------------------------------------------------
    private bool _suppressNextRelease = false;
    public void SuppressNextReleaseSound()
    {
        _suppressNextRelease = true;
    }

    protected override void Awake()
    {
        base.Awake();
        chargeLight = chargeLight ?? GetComponentInChildren<Light2D>();
        chargeSprite = chargeSprite ?? GetComponentInChildren<SpriteRenderer>();
        ResetChargeVFX();

        beamMaterial = laserBeam.material;
        beamMaterial.SetFloat(glowProperty, baseGlow);
        beamMaterial.SetFloat(dissolveProperty, baseDissolve);
    }

    public override void HandleTriggerPressed()
    {
        if (!CanShoot || chargeRoutine != null)
            return;

        chargeRoutine = StartCoroutine(ChargeAndFire());
    }

    public override void HandleTriggerReleased()
    {
        // 1) Cancelar coroutines
        isCharging = false;
        if (chargeRoutine != null) { StopCoroutine(chargeRoutine); chargeRoutine = null; }
        if (fireRoutine != null) { StopCoroutine(fireRoutine); fireRoutine = null; }

        // 2) Parar loops de carga y fuego
        SoundManager.Instance.StopLoop(SoundType.LaserCharge);
        SoundManager.Instance.StopLoop(SoundType.LaserFire);

        // 3) Sonido de release **solo** si NO lo hemos suprimido
        if (!_suppressNextRelease)
            SoundManager.Instance.PlaySound(SoundType.LaserStop, transform.position);

        // 4) Resetar el flag para la próxima vez
        _suppressNextRelease = false;

        // 5) Parar el beam y resetear VFX
        StopAllFire();
        ResetChargeVFX();
        ResetGlow();
        ResetDissolve();
    }

    private IEnumerator ChargeAndFire()
    {
        isCharging = true;

        // 1) Inicia loop de carga
        SoundManager.Instance.PlaySound(SoundType.LaserCharge, transform.position);
        chargeSprite.enabled = true;

        // 2) Animación de carga
        float t = 0f;
        while (isCharging && t < laserData.ChargeTime && weapon.Ammo > 0)
        {
            t += Time.deltaTime;
            float flick = Mathf.Sin(t * laserData.FlickerSpeed * Mathf.PI * 2f)
                        * laserData.FlickerIntensity;
            if (chargeLight) chargeLight.intensity = Mathf.Max(0f, flick);
            if (chargeSprite) chargeSprite.transform.Rotate(0, 0, 360f * Time.deltaTime / laserData.ChargeTime);
            yield return null;
        }

        // Si se canceló, limpiamos y salimos sin disparar
        if (!isCharging)
        {
            SoundManager.Instance.StopLoop(SoundType.LaserCharge);
            chargeRoutine = null;
            yield break;
        }

        // 3) Terminó carga: arranca loop de fuego
        SoundManager.Instance.StopLoop(SoundType.LaserCharge);
        SoundManager.Instance.PlaySound(SoundType.LaserFire, transform.position);

        ResetChargeVFX();
        isFiring = true;

        StartGlowFlash();
        StartDissolveReset();
        fireRoutine = StartCoroutine(FireLaserLoop());

        chargeRoutine = null;
    }

    public override void StopAllFire()
    {
        base.StopAllFire();
        SoundManager.Instance.StopLoop(SoundType.LaserFire);
    }

    private void ResetChargeVFX()
    {
        if (chargeLight) chargeLight.intensity = 0f;
        if (chargeSprite) chargeSprite.enabled = false;
    }

    private void StartGlowFlash()
    {
        glowTween?.Kill();
        glowTween = DOTween.Sequence()
            .Append(DOTween.To(() => baseGlow, x => beamMaterial.SetFloat(glowProperty, x), flashGlow, glowDuration * .5f))
            .Append(DOTween.To(() => flashGlow, x => beamMaterial.SetFloat(glowProperty, x), baseGlow, glowDuration * .5f))
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutQuad);
    }

    private void ResetGlow()
    {
        glowTween?.Kill();
        beamMaterial.SetFloat(glowProperty, baseGlow);
    }

    private void StartDissolveReset()
        => beamMaterial.SetFloat(dissolveProperty, baseDissolve);

    private void ResetDissolve()
        => beamMaterial.SetFloat(dissolveProperty, baseDissolve);
}
