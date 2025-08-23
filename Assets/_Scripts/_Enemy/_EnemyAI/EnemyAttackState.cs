// EnemyAttackState.cs
using UnityEngine;

public class EnemyAttackState : IEnemyState
{
    private EnemyAISystem enemyAISystem;
    private EnemyController enemyController;
    private EnemyAttackModule enemyAttackModule;
    private Transform playerTransform;
    private float attackRange;

    public EnemyAttackState(EnemyAISystem enemyAISystem, EnemyAttackModule enemyAttackModule)
    {
        this.enemyAISystem = enemyAISystem;
        this.enemyController = enemyAISystem.GetComponent<EnemyController>();
        this.enemyAttackModule = enemyAttackModule;
    }

    public void OnEnter()
    {
        playerTransform = enemyAISystem.PlayerTransform;
        attackRange = enemyAISystem.EnemyData.AttackRange;

        enemyController.StopMovement();
        enemyController.DisableSeparation();
        enemyController.ResetVisualsEnemyAvatar();

        enemyAttackModule.StartPreparingAttack();
    }

    public void OnUpdate()
    {
        if (enemyAISystem.enemyIsDead || playerTransform == null)
        {
            enemyAISystem.ChangeState(enemyAISystem.IdleState);
            return;
        }

        float distance = Vector2.Distance(
            enemyAISystem.transform.position,
            playerTransform.position);

        if (distance > attackRange)
        {
            enemyAISystem.ChangeState(enemyAISystem.ChaseState);
            return;
        }

        if (distance <= attackRange && enemyAttackModule.IsAttackCooldownReady())
        {
            enemyAttackModule.StartPreparingAttack();
        }
    }

    public void OnExit()
    {
        enemyController.EnableSeparation();
        enemyController.ResetVisualsEnemyAvatar();
    }
}
