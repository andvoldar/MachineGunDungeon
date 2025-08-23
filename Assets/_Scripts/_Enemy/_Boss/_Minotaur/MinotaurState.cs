// MinotaurState.cs
using UnityEngine;

public abstract class MinotaurState
{
    protected MinotaurStateMachine stateMachine;
    protected MinotaurBoss boss;
    protected MinotaurController controller;
    protected Animator animator;

    public MinotaurState(MinotaurStateMachine stateMachine)
    {
        this.stateMachine = stateMachine;
        this.boss = stateMachine.GetBoss();
        this.controller = boss.GetController();
        this.animator = boss.GetAnimator();
    }

    public virtual void OnFixedUpdate() { }

    public virtual void OnEnter() { }
    public virtual void OnUpdate() { }
    public virtual void OnExit() { }
}
