// EnemyIdleState.cs
using UnityEngine;

public class EnemyIdleState : IEnemyState
{
    private EnemyAISystem enemyAISystem;
    private float idleTime;
    private float timer;

    public EnemyIdleState(EnemyAISystem enemyAISystem)
    {
        this.enemyAISystem = enemyAISystem;
    }

    public void OnEnter()
    {
        idleTime = Random.Range(1f, 3f);
        timer = 0f;
    }

    public void OnUpdate()
    {
        if (enemyAISystem.Perception.IsPlayerInDetectionRange())
        {
            enemyAISystem.ChangeState(enemyAISystem.ChaseState);
            return;
        }

        timer += Time.deltaTime;
        if (timer >= idleTime)
        {
            enemyAISystem.ChangeState(enemyAISystem.WanderState);
        }
    }

    public void OnExit()
    {
        // Sin acciones adicionales al salir
    }
}
