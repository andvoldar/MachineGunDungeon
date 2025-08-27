using UnityEngine;

public class MinotaurWanderState : MinotaurState
{
    private Rect bounds;
    private Vector2 currentTarget;
    private float targetReachThreshold = 0.25f;
    private float pickNewTargetInterval = 3.5f;
    private float minTargetDistance = 1.2f;   // evita targets pegados al boss
    private float timer;
    private float moveSpeedBackup;

    public MinotaurWanderState(MinotaurStateMachine stateMachine, Rect bounds) : base(stateMachine)
    {
        this.bounds = bounds;
    }

    public override void OnEnter()
    {
        // Limpia triggers y fuerza locomoción normal
        animator.ResetTrigger("MeleeAttack");
        animator.ResetTrigger("RangedAttack");
        animator.ResetTrigger("Dash");
        animator.SetBool("Idle", false);
        animator.SetBool("Run", true);

        moveSpeedBackup = controller.moveSpeed;
        controller.moveSpeed = Mathf.Max(1.8f, moveSpeedBackup * 0.65f);

        PickNewTarget();
        timer = 0f;
    }

    public override void OnUpdate()
    {
        if (boss.IsDead()) return;

        Vector2 pos = boss.transform.position;
        float dist = Vector2.Distance(pos, currentTarget);

        controller.MoveTowards(currentTarget);

        if (dist <= targetReachThreshold || timer >= pickNewTargetInterval)
        {
            PickNewTarget();
            timer = 0f;
        }

        timer += Time.deltaTime;
    }

    public override void OnExit()
    {
        animator.SetBool("Run", false);
        animator.SetBool("Idle", false);
        controller.moveSpeed = moveSpeedBackup;
        controller.StopMovement();
    }

    private void PickNewTarget()
    {
        // Busca un punto que no quede “encima” del boss para evitar flip-flop
        for (int i = 0; i < 8; i++)
        {
            float x = Random.Range(bounds.xMin, bounds.xMax);
            float y = Random.Range(bounds.yMin, bounds.yMax);
            Vector2 candidate = new Vector2(x, y);
            if (Vector2.Distance(candidate, boss.transform.position) >= minTargetDistance)
            {
                currentTarget = candidate;
                return;
            }
        }
        // Fallback corregido (boss.position convertido a Vector2)
        currentTarget = (Vector2)boss.transform.position + Random.insideUnitCircle.normalized * minTargetDistance;
    }
}
