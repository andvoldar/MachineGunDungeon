// MinotaurStateMachine.cs
public class MinotaurStateMachine
{
    public MinotaurState CurrentState { get; private set; }
    private readonly MinotaurBoss boss;

    public MinotaurStateMachine(MinotaurBoss boss)
    {
        this.boss = boss;
    }

    public void FixedUpdate()
    {
        CurrentState?.OnFixedUpdate();
    }

    public void ChangeState(MinotaurState newState)
    {
        CurrentState?.OnExit();
        CurrentState = newState;
        CurrentState?.OnEnter();
    }

    public MinotaurBoss GetBoss() => boss;
}
