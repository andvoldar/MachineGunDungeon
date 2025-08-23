using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Enemy : MonoBehaviour, IHittable, IEnemy, IKnockbackable
{
    [field: SerializeField] public EnemyDataSO EnemyData { get; set; }
    [field: SerializeField] public int Health { get; private set; }

    public bool IsFacingRight { get; private set; } = true;

    private bool isDead = false;
    [field: SerializeField] public UnityEvent OnMeleeAttack { get; set; }
    [field: SerializeField] public UnityEvent OnGetHit { get; set; }
    [field: SerializeField] public UnityEvent OnDeath { get; set; }

    private Rigidbody2D rb;
    private EnemyController enemyController;
    private EnemyAISystem enemyAISystem;
    private EnemyAvatar enemyAvatar;
    private KnockbackController knockbackController;

    [SerializeField] private float deathDelay = .1f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyController = GetComponent<EnemyController>();
        enemyAISystem = GetComponent<EnemyAISystem>();
        enemyAvatar = GetComponentInChildren<EnemyAvatar>();
        knockbackController = GetComponent<KnockbackController>();
    }

    private void Start()
    {
        Health = EnemyData.MaxHealth;
    }

    public void GetHit(int damage, GameObject damageDealer)
    {
        if (isDead) return;


        Health -= damage;
        Health = Mathf.Max(Health, 0);

        OnGetHit?.Invoke();
        enemyAISystem.EnterFuriousState();

        if (Health <= 0)
        {
            EnemyIsDead();
        }
    }


    public void TriggerSpawnVFX()
    {
        var spawnFeedback = GetComponentInChildren<EnemySpawnVFXFeedback>();
        if (spawnFeedback != null)
        {
            spawnFeedback.CreateFeedback();
        }
    }

    public void ApplyKnockback(Vector2 knockbackDirection, float force, float duration)
    {
        knockbackController.ApplyKnockback(knockbackDirection, force, duration);
    }


    private void EnemyIsDead()
    {
        OnDeath?.Invoke();
        enemyAvatar.PlayDeathVisuals();
        isDead = true;
        enemyController.StopMovement();
        enemyAISystem.EnemyIsDead(true);

        
        


        DissolveFeedback dissolve = GetComponentInChildren<DissolveFeedback>();
        if (dissolve != null)
        {
            dissolve.OnDissolveComplete = () => Destroy(gameObject);
        }
        else
        {
            StartCoroutine(WaitForDeath());
        }

        
    }

    private IEnumerator WaitForDeath()
    {
        yield return new WaitForSeconds(deathDelay);
        Destroy(gameObject);
    }

    public void SetFacingDirection(bool isFacingRight)
    {
        IsFacingRight = isFacingRight;
    }
}
