using UnityEngine;

public class EnemyMeleeAttackModule : EnemyAttackModule
{
    public EnemyMeleeAttackModule(EnemyAISystem system) : base(system) { }

    protected override void PrepareAttack()
    {
        if (enemyAISystem.enemyIsDead) return;
        PerformAttack();
    }

    protected override void PerformAttack()
    {
        if (playerTransform == null || enemyAISystem.enemyIsDead) return;

        enemyController.PlayAttackVoiceSound();
        enemyController.PlayMeleeAttackSound();
        enemyAISystem.WeaponController?.SpawnAttackVFX();

        float distance = Vector2.Distance(enemyAISystem.transform.position, playerTransform.position);
        if (distance <= attackRange)
        {
            var hittable = playerTransform.GetComponent<IHittable>();
            hittable?.GetHit(attackDamage, enemyAISystem.gameObject);
        }

        EndAttack();
    }
}
