using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Animator), typeof(MinotaurController), typeof(Rigidbody2D))]
public class MinotaurBoss : MonoBehaviour, IHittable
{
    [Header("Stats")]
    public int maxHealth = 500;
    private int currentHealth;
    private bool isDead = false;

    [Header("Events")]
    public UnityEvent OnHit;
    public UnityEvent OnDeath;

    [Header("Ranged Attack")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public BulletDataSO minotaurBulletData;
    

    [Header("Ranged Attack Limit")]
    public int maxRangedAttacks = 5;
    public float rangedAttackCooldown = 5f;


    [Header("Behavior Distances")]
    public float dashDistanceThreshold = 5f;
    public float walkDistanceThreshold = 3f;
    public float rangedAttackDistanceThreshold = 6f;


    private int rangedAttacksDone = 0;
    private float rangedAttackCooldownTimer = 0f;

    private Animator animator;
    private MinotaurController controller;
    private MinotaurStateMachine stateMachine;
    private Rigidbody2D rb;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;
    private Color originalColor;
    private Coroutine flashCoroutine;


    public UnityEvent OnGetHitEvent = new UnityEvent();
    UnityEvent IHittable.OnGetHit
    {
        get => OnGetHitEvent;
        set => OnGetHitEvent = value;
    }
    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<MinotaurController>();
        rb = GetComponent<Rigidbody2D>();
        stateMachine = new MinotaurStateMachine(this);

        originalColor = spriteRenderer.color; // ✅ Guardamos el color real
    }

    private void Start()
    {
        currentHealth = maxHealth;
        stateMachine.ChangeState(new MinotaurIdleState(stateMachine));
    }

    private void Update()
    {
        if (!isDead)
        {
            stateMachine.CurrentState?.OnUpdate();

            if (rangedAttackCooldownTimer > 0f)
            {
                rangedAttackCooldownTimer -= Time.deltaTime;
                if (rangedAttackCooldownTimer <= 0f)
                {
                    rangedAttackCooldownTimer = 0f;
                    rangedAttacksDone = 0;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        stateMachine?.FixedUpdate();
    }

    public void OnGetHit()
    {
        // Opcional: podrías poner efectos visuales aquí
    }

    public void GetHit(int damage, GameObject dealer)
    {
        if (isDead) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        OnHit?.Invoke();
        PlayDamageFlash();

        if (currentHealth <= 0)
            Die();
    }

    private void PlayDamageFlash()
    {
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        spriteRenderer.color = flashColor;
        yield return new WaitForSeconds(flashDuration);
        spriteRenderer.color = originalColor;
    }


    public void FireRangedProjectile()
    {
        if (projectilePrefab == null || firePoint == null || isDead) return;

        // Dirección hacia la que apunta el WeaponParent (eje X)
        Vector2 direction = firePoint.right.normalized;

        // Instanciamos el proyectil
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        proj.transform.right = direction;

        // Aplicamos datos
        MinotaurProjectile bullet = proj.GetComponent<MinotaurProjectile>();
        if (bullet != null)
            bullet.bulletData = minotaurBulletData;

        RegisterRangedAttack(); // Para limitar disparos
    }

    public void OnAttackAnimationEnd()
    {
        stateMachine.ChangeState(new MinotaurChaseState(stateMachine));
    }

    private void Die()
    {
        isDead = true;
        animator.SetTrigger("Die");
        OnDeath?.Invoke();
        stateMachine.ChangeState(null);
        controller.StopMovement();
    }
    public bool CanDoRangedAttack() => rangedAttackCooldownTimer <= 0f && rangedAttacksDone < maxRangedAttacks;

    public void RegisterRangedAttack()
    {
        rangedAttacksDone++;
        if (rangedAttacksDone >= maxRangedAttacks)
            rangedAttackCooldownTimer = rangedAttackCooldown;
    }

    public void StartDashTowardPlayer(float dashSpeed, float dashDuration)
    {
        Transform player = GameObject.FindWithTag("Player")?.transform;
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        StartCoroutine(DashRoutine(direction, dashSpeed, dashDuration));
    }

    private IEnumerator DashRoutine(Vector2 direction, float speed, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        stateMachine.ChangeState(new MinotaurChaseState(stateMachine));
    }

    public Animator GetAnimator() => animator;
    public MinotaurController GetController() => controller;
    public bool IsDead() => isDead;
    public GameObject GetProjectilePrefab() => projectilePrefab;
    public Transform GetFirePoint() => firePoint;
}
