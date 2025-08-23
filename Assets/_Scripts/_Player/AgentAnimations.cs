using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AgentAnimations : MonoBehaviour
{
    protected Animator agentAnimator;


    private void Awake()
    {
        agentAnimator = GetComponent<Animator>();
    }

    public void AnimatePlayer(float velocity)
    {
        SetWalkAnimation(velocity > 0);
    }

    public void SetWalkAnimation(bool val)
    {
        agentAnimator.SetBool("Walk", val);
    }

    public void SetWalkSpeed(int val)
    {
        agentAnimator.SetFloat("WalkMultiplier", val);
    }

}
