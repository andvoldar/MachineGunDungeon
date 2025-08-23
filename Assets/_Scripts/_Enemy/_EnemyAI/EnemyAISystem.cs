// EnemyAISystem.cs
using UnityEngine;

public class EnemyAISystem : MonoBehaviour
{
    public IEnemyState currentState;
    public IEnemyState IdleState { get; private set; }
    public IEnemyState ChaseState { get; private set; }
    public IEnemyState WanderState { get; private set; }
    public IEnemyState AttackState { get; private set; }
    public IEnemyState FuriousState { get; private set; }

    public Transform PlayerTransform { get; private set; }
    public EnemyDataSO EnemyData { get; private set; }
    public EnemyPerception Perception { get; private set; }

    public EnemyAttackModule enemyAttackModule;
    public EnemyWeaponController WeaponController { get; private set; }

    public bool enemyIsDead = false;
    internal float knockBackPower;
    internal float knockBackDelay;

    private void Awake()
    {
        PlayerTransform = FindObjectOfType<PlayerController>()?.transform;
        if (PlayerTransform == null)
            Debug.LogWarning("Jugador no encontrado. EnemyAISystem no tendrá target.");

        EnemyData = GetComponent<Enemy>().EnemyData;
        knockBackPower = EnemyData.KnockBackPower;
        knockBackDelay = EnemyData.KnockBackDelay;
        WeaponController = GetComponentInChildren<EnemyWeaponController>();

        switch (EnemyData.AttackType)
        {
            case EnemyAttackType.Melee:
                enemyAttackModule = new EnemyMeleeAttackModule(this);
                break;
            case EnemyAttackType.Ranged:
                enemyAttackModule = new EnemyRangedAttackModule(this);
                break;
            default:
                Debug.LogError("Tipo de ataque no definido en EnemyData.");
                break;
        }

        enemyAttackModule?.SetPlayerTransform(PlayerTransform);

        AttackState = new EnemyAttackState(this, enemyAttackModule);
        IdleState = new EnemyIdleState(this);
        WanderState = new EnemyWanderState(this);
        ChaseState = new EnemyChaseState(this);
        FuriousState = new EnemyFuriousState(this);

        Perception = new EnemyPerception(
            transform,
            PlayerTransform,
            EnemyData.DetectionRange,
            EnemyData.DisengageRange,
            EnemyData.furiousChaseSpeed,
            EnemyData.furiousDetectionRange
        );
    }

    private void Start()
    {
        if (PlayerTransform == null)
        {
            Debug.LogWarning("Jugador no encontrado. No se puede iniciar el sistema de IA.");
            return;
        }

        ChangeState(IdleState);
    }

    private void Update()
    {
        if (PlayerTransform == null) return;

        Perception.UpdatePerception();
        currentState.OnUpdate();
    }

    public void ChangeState(IEnemyState newState)
    {
        currentState?.OnExit();
        currentState = newState;
        currentState.OnEnter();
    }

    public void EnterFuriousState()
    {
        Perception.SetDetectionRange(EnemyData.furiousDetectionRange);
        ChangeState(FuriousState);
    }

    internal void EnemyIsDead(bool v)
    {
        enemyIsDead = v;
    }
}
