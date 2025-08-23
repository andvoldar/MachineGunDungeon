// Assets/Scripts/Weapons/Weapon.cs
using FMODUnity;
using UnityEngine;
using UnityEngine.Events;
using System;

public class Weapon : MonoBehaviour
{
    [SerializeField] public GameObject muzzle;
    [SerializeField] protected int ammo = 10;
    [SerializeField] public WeaponDataSO weaponData;

    [field: SerializeField] public UnityEvent OnShoot { get; set; }
    [field: SerializeField] public UnityEvent OnShootNoAmmo { get; set; }

    private WeaponState currentState;

    // <-- NUEVO: evento para equip/unequip
    public event Action<bool> OnEquippedChanged;

    public int Ammo
    {
        get => ammo;
        set => ammo = Mathf.Clamp(value, 0, weaponData?.AmmoCapacity ?? 999);
    }

    public bool AmmoFull => Ammo >= (weaponData?.AmmoCapacity ?? 999);

    public WeaponDataSO GetWeaponData() => weaponData;

    private void Start()
    {
        if (weaponData != null && currentState == null)
            LoadState(null);

        if (TryGetComponent(out WeaponStateHolder h) && h.StoredState == null)
            h.SetState(GetCurrentState());
    }

    public void SpawnBullet(Vector3 position, Quaternion rotation)
    {
        RuntimeManager.PlayOneShot(weaponData.gunSoundFMOD, transform.position);
        var bulletGO = Instantiate(weaponData.BulletData.bulletPrefab, position, rotation);
        if (bulletGO.TryGetComponent(out Bullet b))
            b.bulletData = weaponData.BulletData;
    }

    public Quaternion CalculateBulletRotation()
    {
        float spread = UnityEngine.Random.Range(-weaponData.SpreadAngle, weaponData.SpreadAngleRandomizer);
        return muzzle.transform.rotation * Quaternion.Euler(0, 0, spread);
    }

    public int GetBulletCountToSpawn() => weaponData.GetBulletCountToSpawn();

    public void AssignWeaponData(WeaponDataSO data) => weaponData = data;

    public void LoadState(WeaponState state)
    {
        if (state == null)
            ammo = weaponData.AmmoCapacity;
        else
            ammo = state.ammo;
        currentState = state ?? new WeaponState(ammo, weaponData.AmmoCapacity);
    }

    public WeaponState GetCurrentState() => new WeaponState(ammo, weaponData.AmmoCapacity);

    public void SetEquipped(bool equipped)
    {
        if (TryGetComponent(out DroppedWeaponVisuals v))
            v.SetCanPickup(!equipped);
        if (equipped && ammo == 0 && weaponData != null)
            ammo = weaponData.AmmoCapacity;

        // <-- DISPARAMOS EL EVENTO
        OnEquippedChanged?.Invoke(equipped);
    }
}
