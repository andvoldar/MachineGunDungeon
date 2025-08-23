// Assets/Scripts/Weapons/LaserWeaponController.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Weapon), typeof(LineRenderer))]
public class LaserWeaponController : WeaponController
{
    protected LineRenderer laserBeam;
    protected LaserWeaponDataSO laserData;
    protected bool isFiring = false;

    protected override void Awake()
    {
        base.Awake();
        laserBeam = GetComponent<LineRenderer>();
        laserData = weapon.GetWeaponData() as LaserWeaponDataSO;

        laserBeam.positionCount = 2;
        laserBeam.enabled = false;
    }

    public override void HandleTriggerPressed()
    {
        if (!isFiring && CanShoot)
        {
            isFiring = true;
            StartCoroutine(FireLaserLoop());
        }
    }

    public override void HandleTriggerReleased()
    {
        StopAllFire();
    }

    protected virtual IEnumerator FireLaserLoop()
    {
        laserBeam.enabled = true;

        while (isFiring && weapon.Ammo > 0)
        {
            float delta = Time.deltaTime;
            Vector3 origin = weapon.muzzle.transform.position;
            Vector3 direction = weapon.muzzle.transform.right;
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, laserData.Range, laserData.HitLayers);
            Vector3 endPoint = hit.collider != null
                ? (Vector3)hit.point
                : origin + direction * laserData.Range;

            laserBeam.SetPosition(0, origin);
            laserBeam.SetPosition(1, endPoint);

            if (hit.collider != null && hit.collider.TryGetComponent<IHittable>(out var target))
            {
                int dmg = Mathf.CeilToInt(laserData.DamagePerSecond * delta);
                target.GetHit(dmg, gameObject);
                target.OnGetHit?.Invoke();
            }

            int ammoUsed = Mathf.CeilToInt(laserData.AmmoPerSecond * delta);
            weapon.Ammo -= ammoUsed;

            yield return null;
        }

        StopAllFire();
    }

    public override void StopAllFire()
    {

        isFiring = false;
        StopAllCoroutines();
        laserBeam.enabled = false;
    }
}
