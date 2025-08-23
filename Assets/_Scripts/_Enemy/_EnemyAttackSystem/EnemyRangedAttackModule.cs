using UnityEngine;

public class EnemyRangedAttackModule : EnemyAttackModule
{
    public EnemyRangedAttackModule(EnemyAISystem system) : base(system) { }

    protected override void PrepareAttack()
    {
        PerformAttack();
    }

    protected override void PerformAttack()
    {
        if (playerTransform == null || enemyAISystem.enemyIsDead) return;

        enemyController.PlayAttackVoiceSound();
        // Aqu� podr�as instanciar un proyectil en vez de hacer da�o directo
        enemyAISystem.WeaponController?.SpawnAttackVFX();

        EndAttack();
    }
}
