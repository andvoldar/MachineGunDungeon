// Assets/Scripts/Audio/SoundEventSO.cs
using UnityEngine;
using FMODUnity;

public enum SoundType
{
    PlayerFootstep,
    PlayerHitVoice,
    PlayerHitSFX,
    PlayerDeath,
    PlayerDash,
    PlayerSlowMotion,

    EnemyFootstep,
    EnemyHit,
    EnemyDeathVoice,
    EnemyDeathVFX,
    EnemyHitVoice,
    EnemyAttackVoice,
    EnemyMeleeAttack,

    BulletShell,

    SwordChargeSFX,
    SwordSpecialAttackSFX,
    SwordSpinSFX,
    SwordAttackSFX,
    SwordHitSFX,

    LaserCharge,
    LaserFire,
    LaserStop,

    ButtonHoverSFX,
    ButtonClickSFX,
    ButtonStartClickSFX,

    StartMenuMusic,
    PickupWeaponSFX,
    DropWeaponSFX,
    PickupItemSFX,
    PickupHealthSFX,

    GrenadePrepareThrow,
    GrenadeThrow,
    GrenadeExplosion,

    BoxDestroyedSFX,
    HitDestructibleSFX,
    PickupAmmoSFX
}

/// <summary>
/// Un ScriptableObject que encapsula un solo sonido FMOD
/// junto con su configuración de bucle y límite de voces.
/// </summary>

[CreateAssetMenu(fileName = "SoundEvent", menuName = "Audio/Sound Event")]
public class SoundEventSO : ScriptableObject
{
    public SoundType type;

    [Tooltip("Referencia al evento de FMOD")]
    public EventReference fmodEvent;

    [Tooltip("¿Es un sonido de loop?")]
    public bool isLoop = false;

    [Tooltip("Número máximo de instancias simultáneas. 0 = sin límite")]
    public int maxVoices = 0;

    [Tooltip("Distancia máxima en la que este sonido debería oírse")]
    public float audibleRange = 25f;

    [Tooltip("Ignorar filtro de distancia y sonar siempre (ideal para jugador, UI, pickups, etc.)")]
    public bool ignoreHearingRange = false;
}

