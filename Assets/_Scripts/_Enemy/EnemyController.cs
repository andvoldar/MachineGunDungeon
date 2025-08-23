// EnemyController.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyController : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector2 moveDirection = Vector2.zero;
    private bool canMove = true;
    private float moveSpeed;
    private EnemyAvatar enemyAvatar;
    private EnemyAISystem enemyAISystem;
    private AgentSoundEnemy agentSoundEnemies;
    private Enemy enemy;
    private KnockbackController knockbackController;

    [Header("Separación entre enemigos")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float separationRadius = 1.5f;
    [SerializeField] private float separationForce = 1.5f;
    private bool useSeparation = true;

    [Header("Wander Settings")]
    [SerializeField] private float wanderRange = 3f;
    [SerializeField] private float wanderTime = 3f;
    [SerializeField] private float pauseTime = 1f;
    [SerializeField] private float raycastDistance = 1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemy = GetComponent<Enemy>();
        enemyAvatar = GetComponentInChildren<EnemyAvatar>();
        enemyAISystem = GetComponent<EnemyAISystem>();
        knockbackController = GetComponent<KnockbackController>();
        agentSoundEnemies = GetComponentInChildren<AgentSoundEnemy>();
    }

    private void Start()
    {
        moveSpeed = enemy.EnemyData.MoveSpeed;
        StartWandering(wanderTime, pauseTime);
    }

    private void FixedUpdate()
    {
        // Si está en knockback, no movemos
        if (knockbackController != null && knockbackController.IsKnockedBack)
            return;

        if (!canMove) return;

        Vector2 separation = useSeparation ? GetSeparationForce() : Vector2.zero;
        Vector2 finalDirection = (moveDirection + separation).normalized;

        if (enemyAvatar != null && finalDirection != Vector2.zero)
        {
            enemyAvatar.LookAtMovement(finalDirection);
            if (finalDirection.x != 0f)
            {
                bool facingRight = finalDirection.x > 0f;
                enemy.SetFacingDirection(facingRight);
            }
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position, finalDirection, raycastDistance, LayerMask.GetMask("Obstacle"));
        if (hit.collider != null)
        {
            moveDirection = -moveDirection;
        }

        rb.velocity = finalDirection * moveSpeed;
    }

    public void SetMoveSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
    }

    public void StopMovement()
    {
        rb.velocity = Vector2.zero;
        moveDirection = Vector2.zero;
    }

    public void Wander(Vector2 targetPosition)
    {
        moveDirection = (targetPosition - (Vector2)transform.position).normalized;
    }

    public void StartWandering(float wanderTime, float pauseTime)
    {
        StartCoroutine(WanderCoroutine(wanderTime, pauseTime));
    }

    private IEnumerator WanderCoroutine(float wanderTime, float pauseTime)
    {
        while (true)
        {
            Vector2 wanderTarget = (Vector2)transform.position + Random.insideUnitCircle * wanderRange;
            wanderTarget = new Vector2(
                Mathf.Clamp(wanderTarget.x, -10f, 10f),
                Mathf.Clamp(wanderTarget.y, -10f, 10f));
            Wander(wanderTarget);
            yield return new WaitForSeconds(wanderTime);
            StopMovement();
            yield return new WaitForSeconds(pauseTime);
        }
    }

    public void MoveTo(Vector2 targetPosition)
    {
        moveDirection = (targetPosition - (Vector2)transform.position).normalized;
    }

    private Vector2 GetSeparationForce()
    {
        Vector2 force = Vector2.zero;
        int count = 0;
        Collider2D[] nearbyEnemies = Physics2D.OverlapCircleAll(transform.position, separationRadius, enemyLayer);
        foreach (var other in nearbyEnemies)
        {
            if (other.gameObject == gameObject) continue;
            Vector2 away = (Vector2)(transform.position - other.transform.position);
            float distance = away.magnitude;
            if (distance > 0.01f)
            {
                force += away.normalized / distance;
                count++;
            }
        }
        if (count > 0)
            force = (force / count) * separationForce;
        return force;
    }

    public void DisableSeparation()
    {
        useSeparation = false;
    }

    public void EnableSeparation()
    {
        useSeparation = true;
    }

    public void PlayAttackEffectEnemyAvatar()
    {
        if (enemyAvatar != null)
            enemyAvatar.PlayAttackEffectEnemyAvatar();
    }

    public void PlayAttackVoiceSound()
    {
        agentSoundEnemies.PlayAttackVoiceSound();
    }

    internal void PlayMeleeAttackSound()
    {
        agentSoundEnemies.PlayMeleeAttackSound();
    }

    public void ResetVisualsEnemyAvatar()
    {
        if (enemyAvatar != null)
            enemyAvatar.ResetVisualsEnemyAvatar();
    }
}
