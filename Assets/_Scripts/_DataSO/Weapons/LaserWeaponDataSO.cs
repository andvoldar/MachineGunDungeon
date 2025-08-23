// Assets/Scripts/Weapons/LaserWeaponDataSO.cs
using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Weapons/LaserWeaponData")]
public class LaserWeaponDataSO : WeaponDataSO
{
    [Header("Laser Settings")]
    public float DamagePerSecond = 10f;
    public float AmmoPerSecond = 5f;
    public float Range = 10f;
    public LayerMask HitLayers = ~0;

    [Header("Charge Settings")]
    public float ChargeTime = 1f;
    public float FlickerIntensity = 2f;
    public float FlickerSpeed = 8f;

    [Header("FMOD Events")]
    [Tooltip("Evento FMOD para el loop de carga")]
    public EventReference ChargeEvent;
    [Tooltip("Evento FMOD para el loop de disparo")]
    public EventReference FireEvent;
    [Tooltip("Evento FMOD para soltar sin disparar")]
    public EventReference ReleaseEvent;
}
