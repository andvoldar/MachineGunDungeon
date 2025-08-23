using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AgentEnemyAnimations : MonoBehaviour
{
    protected Animator agentAnimator;
    protected float velocity;
    protected Rigidbody2D rb;

    private void Awake()
    {
        agentAnimator = GetComponent<Animator>();

        // Buscar el Rigidbody2D en el Parent
        rb = GetComponentInParent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("No se encontró un Rigidbody2D en el parent de " + gameObject.name);

        }
    }

    private void Update()
    {
        if (rb != null)
        {
            velocity = rb.velocity.magnitude; // Obtener la magnitud de la velocidad
        }
        AnimateEnemy(velocity);
    }

    public void AnimateEnemy(float velocity)
    {
        SetWalkAnimation(velocity > 0.1f); // Pequeño umbral para evitar fluctuaciones
    }

    public void SetWalkAnimation(bool isWalking)
    {
        agentAnimator.SetBool("Walk", isWalking);
    }
}
