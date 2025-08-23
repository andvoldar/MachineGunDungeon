using UnityEngine;

[CreateAssetMenu(fileName = "NewGrenadeData", menuName = "Items/Grenade Data")]
public class GrenadeDataSO : ScriptableObject
{
    [Header("General")]
    public string GrenadeName;
    public GameObject GrenadePrefab;

    [Header("Explosion Settings")]
    public float ExplosionRadius = 2.5f;
    public float ExplosionDamage = 50f;
    public float FuseTime = 3f;

    [Header("Throw Settings")]
    public float LaunchForceMin = 5f;
    public float LaunchForceMax = 15f;

    [Header("Explosion Physics")]
    public float KnockbackForce = 8f;

    [Header("Explosion Visuals")]
    public GameObject ExplosionPrefab;
}
