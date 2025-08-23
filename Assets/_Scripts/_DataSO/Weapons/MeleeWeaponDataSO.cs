// Assets/Scripts/Weapons/MeleeWeaponDataSO.cs
using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Weapons/MeleeWeaponData")]
public class MeleeWeaponDataSO : WeaponDataSO
{
    [Header("Melee Settings")]
    public float Damage = 1f;
    public float Cooldown = 0.5f;
    public float SwingAngle = 120f;
    public float SwingDuration = 0.2f;

    [Header("Knockback Settings")]
    [Tooltip("Fuerza del knockback aplicado al enemigo")]
    public float KnockbackForce = 5f;
    [Tooltip("Duración del knockback en segundos")]
    public float KnockbackDuration = 0.2f;

    [Header("FMOD Events")]
    public EventReference SwingSound;
    public EventReference HitSound;
}
