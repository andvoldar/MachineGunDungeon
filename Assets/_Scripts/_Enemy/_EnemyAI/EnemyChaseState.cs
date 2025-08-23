// EnemyChaseState.cs
using UnityEngine;

public class EnemyChaseState : IEnemyState
{
    private EnemyAISystem enemyAISystem;
    private EnemyController enemyController;
    private Transform playerTransform;
    private float chaseSpeed;
    private float detectionRange;
    private float disengageRange;

    public EnemyChaseState(EnemyAISystem enemyAISystem)
    {
        this.enemyAISystem = enemyAISystem;
        this.enemyController = enemyAISystem.GetComponent<EnemyController>();
    }

    public void OnEnter()
    {
        playerTransform = enemyAISystem.PlayerTransform;
        chaseSpeed = enemyAISystem.EnemyData.ChaseSpeed;
        detectionRange = enemyAISystem.EnemyData.DetectionRange;
        disengageRange = enemyAISystem.EnemyData.DisengageRange;

        enemyController.SetMoveSpeed(chaseSpeed);
    }

    public void OnUpdate()
    {
        if (playerTransform == null)
        {
            enemyAISystem.ChangeState(enemyAISystem.IdleState);
            return;
        }

        float distanceToPlayer = Vector2.Distance(
            enemyController.transform.position,
            playerTransform.position);

        if (enemyAISystem.Perception.IsPlayerOutOfDisengageRange())
        {
            enemyController.StopMovement();
            enemyAISystem.ChangeState(enemyAISystem.WanderState);
            return;
        }

        if (distanceToPlayer <= enemyAISystem.EnemyData.AttackRange)
        {
            enemyAISystem.ChangeState(enemyAISystem.AttackState);
            return;
        }

        enemyController.MoveTo(playerTransform.position);
    }

    public void OnExit()
    {
        enemyController.StopMovement();
        float defaultSpeed = enemyAISystem.EnemyData.MoveSpeed;
        enemyController.SetMoveSpeed(defaultSpeed);
    }
}
