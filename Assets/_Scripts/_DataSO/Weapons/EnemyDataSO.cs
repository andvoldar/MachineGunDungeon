using UnityEngine;



public enum EnemyAttackType { Melee, Ranged }


[CreateAssetMenu(fileName = "NewEnemyData", menuName = "ScriptableObjects/EnemyData")]
public class EnemyDataSO : ScriptableObject
{
    [field: SerializeField, Range(1, 10)]
    public float MoveSpeed = 2f;

    [field: SerializeField, Range(1, 10)]
    public float ChaseSpeed = 4f;

    [field: SerializeField, Range(1, 10)]
    public float furiousChaseSpeed = 5f;

    [field: SerializeField, Range(1, 20)]
    public float furiousDetectionRange = 10f;


    [field: SerializeField, Range(1, 100)]
    public float DetectionRange = 5f;

    [field: SerializeField, Range(1, 100)]
    public float DisengageRange = 7f;

    [field: SerializeField, Range(1, 10)]
    public float WanderTime = 3f;

    [field: SerializeField, Range(1, 10)]
    public float IdleTime = 2f;

    [field: SerializeField, Range(1, 100)]
    public int MaxHealth = 10;

    [field: SerializeField, Range(1, 10)]
    public int Damage = 1;

    [field: SerializeField, Range(1, 10)]
    private float attackRange = 1.2f;

    [field: SerializeField, Range(1, 10)]
    private int attackDamage = 1;


    [field: SerializeField]
    public GameObject MeleeAttackVFXPrefab; // ← Nuevo campo para el prefab del efecto visual

    public EnemyAttackType AttackType;

    public float AttackRange => attackRange;
    public int AttackDamage => attackDamage;

    [field: SerializeField]
    [Range(0.1f, 5f)]
    public float AttackCooldown = 0.1f;


    [field: SerializeField]
    [field: Range(1, 500)]
    public float KnockBackPower { get; set; } = 50;

    [field: SerializeField]
    [field: Range(0.01f, 1f)]
    public float KnockBackDelay { get; set; } = .1f;

}
