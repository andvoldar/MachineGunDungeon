using System.Collections;
using UnityEngine;

public class MinotaurDashState : MinotaurState
{
    private float dashSpeed = 10f;
    private float dashDuration = 0.4f;
    private bool dashFinished = false;
    public MinotaurDashState(MinotaurStateMachine stateMachine) : base(stateMachine) { }

    public override void OnEnter()
    {
        controller.StopMovement();
        animator.SetTrigger("Dash");
        dashFinished = false;
        boss.StartCoroutine(DashRoutine());
    }

    private IEnumerator DashRoutine()
    {
        Transform player = GameObject.FindWithTag("Player")?.transform;
        if (player == null) yield break;

        Vector2 direction = (player.position - boss.transform.position).normalized;

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            elapsed += Time.fixedDeltaTime;
            boss.GetComponent<Rigidbody2D>().MovePosition(boss.transform.position + (Vector3)(direction * dashSpeed * Time.fixedDeltaTime));
            yield return new WaitForFixedUpdate();
        }

        dashFinished = true;
    }

    public override void OnUpdate()
    {
        if (dashFinished)
        {
            stateMachine.ChangeState(new MinotaurChaseState(stateMachine));
        }
    }


    public override void OnFixedUpdate() { }
    public override void OnExit()
    {
        animator.ResetTrigger("Dash");
    }
}
