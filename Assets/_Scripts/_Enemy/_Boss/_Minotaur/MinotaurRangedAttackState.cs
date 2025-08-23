using UnityEngine;

public class MinotaurRangedAttackState : MinotaurState
{
    private Transform player;
    private float timer = 0f;
    private float attackDuration = 1.2f;
    private int rangedAttackCount = 0;
    private bool hasFired = false;
    public MinotaurRangedAttackState(MinotaurStateMachine stateMachine) : base(stateMachine) { }

    public override void OnEnter()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        if (player == null) return;

        timer = 0f;
        rangedAttackCount = 0;
        hasFired = false;

        controller.StopMovement();
        animator.SetTrigger("RangedAttack");

    }

    public override void OnUpdate()
    {
        if (boss.IsDead() || player == null) return;

        timer += Time.deltaTime;

        float distance = Vector2.Distance(boss.transform.position, player.position);

        // Dispara a mitad de la animación
        if (!hasFired && timer >= attackDuration * 0.5f)
        {
            boss.FireRangedProjectile(); // ← Se dispara UNA vez
            hasFired = true;
        }

        // Cuando termina la animación completa
        if (timer >= attackDuration)
        {
            timer = 0f;
            hasFired = false;

            rangedAttackCount++;
            boss.RegisterRangedAttack();

            if (!boss.CanDoRangedAttack())
            {
                if (distance > boss.dashDistanceThreshold)
                    stateMachine.ChangeState(new MinotaurDashState(stateMachine));
                else
                    stateMachine.ChangeState(new MinotaurChaseState(stateMachine));
                return;
            }

            animator.SetTrigger("RangedAttack"); // Lanza siguiente animación
        }
    }
    public override void OnExit()
    {
        controller.StopMovement();
        animator.ResetTrigger("RangedAttack");
    }

}
