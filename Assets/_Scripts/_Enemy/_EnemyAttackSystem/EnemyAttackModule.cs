using UnityEngine;

public abstract class EnemyAttackModule
{
    protected EnemyAISystem enemyAISystem;
    protected EnemyController enemyController;
    protected Transform playerTransform;

    protected float attackCooldown;
    protected float attackRange;
    protected int attackDamage;
    protected float lastAttackTime;
    protected bool isPreparingAttack;

    public EnemyAttackModule(EnemyAISystem enemyAISystem)
    {
        this.enemyAISystem = enemyAISystem;
        this.enemyController = enemyAISystem.GetComponent<EnemyController>();
        this.attackCooldown = enemyAISystem.EnemyData.AttackCooldown;
        this.attackRange = enemyAISystem.EnemyData.AttackRange;
        this.attackDamage = enemyAISystem.EnemyData.AttackDamage;
        this.lastAttackTime = -attackCooldown;
    }

    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    public bool IsAttackCooldownReady()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    public void StartPreparingAttack()
    {
        if (isPreparingAttack || !IsAttackCooldownReady() || enemyAISystem.enemyIsDead) return;

        isPreparingAttack = true;
        lastAttackTime = Time.time;

        enemyController.ResetVisualsEnemyAvatar();
        enemyController.PlayAttackEffectEnemyAvatar();

        PrepareAttack();
    }

    protected abstract void PrepareAttack();
    protected abstract void PerformAttack();

    protected void EndAttack()
    {
        isPreparingAttack = false;
    }
}
