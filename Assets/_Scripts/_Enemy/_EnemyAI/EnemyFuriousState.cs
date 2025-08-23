// EnemyFuriousState.cs
using UnityEngine;

public class EnemyFuriousState : IEnemyState
{
    private EnemyAISystem enemyAISystem;
    private EnemyController enemyController;
    private Transform playerTransform;
    private float furiousChaseSpeed;

    public EnemyFuriousState(EnemyAISystem enemyAISystem)
    {
        this.enemyAISystem = enemyAISystem;
        this.enemyController = enemyAISystem.GetComponent<EnemyController>();
    }

    public void OnEnter()
    {
        playerTransform = enemyAISystem.PlayerTransform;
        furiousChaseSpeed = enemyAISystem.EnemyData.furiousChaseSpeed;
        enemyAISystem.Perception.SetDetectionRange(enemyAISystem.EnemyData.furiousDetectionRange);
        enemyController.SetMoveSpeed(furiousChaseSpeed);
    }

    public void OnUpdate()
    {
        if (enemyAISystem.enemyIsDead || playerTransform == null)
        {
            enemyAISystem.ChangeState(enemyAISystem.IdleState);
            return;
        }

        float distanceToPlayer = Vector2.Distance(
            enemyAISystem.transform.position,
            playerTransform.position);

        if (distanceToPlayer <= enemyAISystem.EnemyData.AttackRange)
        {
            enemyAISystem.ChangeState(enemyAISystem.AttackState);
            return;
        }

        enemyController.MoveTo(playerTransform.position);
    }

    public void OnExit()
    {
        enemyController.SetMoveSpeed(enemyAISystem.EnemyData.MoveSpeed);
        enemyAISystem.Perception.SetDetectionRange(enemyAISystem.EnemyData.DetectionRange);
    }
}
