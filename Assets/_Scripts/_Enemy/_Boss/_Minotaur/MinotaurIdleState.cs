// MinotaurIdleState.cs
using UnityEngine;

public class MinotaurIdleState : MinotaurState
{
    private float idleDuration = 1.5f; // Tiempo que permanece en idle antes de actuar
    private float timer = 0f;
    private Transform player;

    public MinotaurIdleState(MinotaurStateMachine stateMachine) : base(stateMachine) { }

    public override void OnEnter()
    {
        timer = 0f;
        player = GameObject.FindWithTag("Player")?.transform;
        controller.StopMovement();
        animator.SetBool("Idle", true);
    }

    public override void OnUpdate()
    {
        if (boss.IsDead()) return;

        // Buscar player si aún no se detectó
        if (player == null)
            player = GameObject.FindWithTag("Player")?.transform;

        timer += Time.deltaTime;

        if (player != null && timer >= idleDuration)
        {
            stateMachine.ChangeState(new MinotaurChaseState(stateMachine));
        }
    }


    public override void OnExit()
    {
        animator.SetBool("Idle", false);
    }
}
