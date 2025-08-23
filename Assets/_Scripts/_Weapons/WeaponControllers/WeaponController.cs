// WeaponController.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Weapon))]
public abstract class WeaponController : MonoBehaviour, IWeaponController
{
    protected Weapon weapon;
    protected WeaponRenderer weaponRenderer;
    protected float nextAvailableFireTime = 0f;

    [SerializeField, Tooltip("Cuánto tarda en interpolar al nuevo ángulo")]
    private float aimSmoothSpeed = 15f;

    /// <summary>Puede disparar si el cooldown ha expirado y hay ammo.</summary>
    public bool CanShoot => Time.time >= nextAvailableFireTime && weapon.Ammo > 0;

    protected virtual void Awake()
    {
        weapon = GetComponent<Weapon>();
        weaponRenderer = GetComponent<WeaponRenderer>();
    }

    /// <summary>Apunta suavemente hacia el puntero.</summary>
    public void AimWeapon(Vector2 pointerPosition)
    {
        Vector3 parentPos = transform.parent.position;
        Vector2 dir = (pointerPosition - (Vector2)parentPos).normalized;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        float current = transform.localEulerAngles.z;
        if (current > 180) current -= 360;
        // ▶️ Aquí cambiamos para usar aimSmoothSpeed, no SpreadAngle:
        float sm = Mathf.LerpAngle(current, targetAngle, Time.deltaTime * aimSmoothSpeed);

        transform.localRotation = Quaternion.Euler(0, 0, sm);

        bool behind = sm > 0 && sm < 180;
        weaponRenderer?.RenderWeaponBehindPlayer(behind);
        bool flipY = sm > 90 || sm < -90;
        weaponRenderer?.FlipWeaponSprite(flipY);
    }

    /// <summary>
    /// El “core” de disparo: decrementa munición, invoca eventos,
    /// spawnea todas las balas y programa el cooldown.
    /// </summary>
    protected void FireCore()
    {
        if (!CanShoot) return;

        weapon.Ammo--;
        weapon.OnShoot?.Invoke();

        Vector3 pos = weapon.muzzle.transform.position;
        for (int i = 0; i < weapon.GetBulletCountToSpawn(); i++)
            weapon.SpawnBullet(pos, weapon.CalculateBulletRotation());

        nextAvailableFireTime = Time.time + weapon.weaponData.WeaponDelay;
    }

    /// <summary>Cancela todo feedback activo.</summary>
    public virtual void StopAllFire()
    {
        // Por si los feedbacks necesitan cancelarse
    }

    public void FullReset()
    {
        StopAllFire();
        nextAvailableFireTime = 0f;
        StopAllCoroutines();
    }

    /// <summary>Disparador: pulsación.</summary>
    public abstract void HandleTriggerPressed();

    /// <summary>Disparador: liberación.</summary>
    public abstract void HandleTriggerReleased();

    void IWeaponController.HandleAltPressed() { }
    void IWeaponController.HandleAltReleased() { }
}
