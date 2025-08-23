// EnemyWanderState.cs
using UnityEngine;

public class EnemyWanderState : IEnemyState
{
    private EnemyAISystem enemyAISystem;
    private EnemyController enemyController;
    private float wanderTime;
    private float pauseTime;

    public EnemyWanderState(EnemyAISystem enemyAISystem)
    {
        this.enemyAISystem = enemyAISystem;
        this.enemyController = enemyAISystem.GetComponent<EnemyController>();
    }

    public void OnEnter()
    {
        wanderTime = Random.Range(1f, 3f);
        pauseTime = Random.Range(3f, 5f);

        enemyController.StartWandering(wanderTime, pauseTime);
        enemyController.EnableSeparation();
    }

    public void OnUpdate()
    {
        if (enemyAISystem.Perception.IsPlayerInDetectionRange())
        {
            enemyAISystem.ChangeState(enemyAISystem.ChaseState);
        }
    }

    public void OnExit()
    {
        // Nada especial al salir
    }
}
