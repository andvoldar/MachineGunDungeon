using UnityEngine;

public class MinotaurChaseState : MinotaurState
{
    private Transform player;
    private float attackRange;

    public MinotaurChaseState(MinotaurStateMachine stateMachine) : base(stateMachine) { }

    public override void OnEnter()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        attackRange = controller.attackRange;

        // Resetear triggers activos por seguridad
        animator.ResetTrigger("Dash");
        animator.ResetTrigger("RangedAttack");
        animator.ResetTrigger("MeleeAttack");

        // Forzar "Run" verdadero
        animator.SetBool("Run", true);
    }

    public override void OnUpdate()
    {
        if (boss.IsDead() || player == null)
            return;

        float distance = Vector2.Distance(boss.transform.position, player.position);
        controller.MoveTowards(player.position);

        // Si está en rango de ataque cuerpo a cuerpo
        if (distance <= attackRange)
        {
            stateMachine.ChangeState(new MinotaurMeleeAttackState(stateMachine));
            return;
        }

        // Si el jugador está lejos y puede atacar a distancia
        if (distance >= boss.rangedAttackDistanceThreshold && boss.CanDoRangedAttack())
        {
            stateMachine.ChangeState(new MinotaurRangedAttackState(stateMachine));
            return;
        }
    }

    public override void OnExit()
    {
        animator.SetBool("Run", false);
        controller.StopMovement();
    }
}
