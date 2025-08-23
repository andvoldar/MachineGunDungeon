// MinotaurDeathState.cs
public class MinotaurDeathState : MinotaurState
{
    public MinotaurDeathState(MinotaurStateMachine stateMachine) : base(stateMachine) { }

    public override void OnEnter()
    {
        controller.StopMovement();
        animator.SetTrigger("Die");
    }

    public override void OnUpdate() { }

    public override void OnExit() { }
}
