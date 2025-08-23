// MinotaurMeleeAttackState.cs
using UnityEngine;

public class MinotaurMeleeAttackState : MinotaurState
{
    private Transform player;
    private bool hasAttacked = false;
    private bool isExitingEarly = false;

    public MinotaurMeleeAttackState(MinotaurStateMachine stateMachine) : base(stateMachine) { }

    public override void OnEnter()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        controller.StopMovement();

        animator.ResetTrigger("MeleeAttack");
        animator.SetTrigger("MeleeAttack");

        hasAttacked = false;
        isExitingEarly = false;
    }

    public override void OnUpdate()
    {
        if (boss.IsDead() || player == null)
        {
            ForceExit();
            return;
        }

        float distance = Vector2.Distance(boss.transform.position, player.position);

        // Si el jugador se aleja del rango de ataque
        if (distance > controller.attackRange + 0.5f && !isExitingEarly)
        {
            ForceExit();
            return;
        }

        // Ejecutar daño si está en el frame adecuado
        if (!hasAttacked)
        {
            // Aquí opcionalmente puedes usar AnimatorStateInfo.normalizedTime >= X
            hasAttacked = true;

            if (distance <= controller.attackRange)
            {
                Debug.Log("Minotaur hits the player!");
                // Aquí aplicas daño real más adelante
            }
        }
    }

    public override void OnExit()
    {
        animator.ResetTrigger("MeleeAttack");
    }

    private void ForceExit()
    {
        isExitingEarly = true;
        stateMachine.ChangeState(new MinotaurChaseState(stateMachine));
    }
}
