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

    [Header("Dormant / Activation")]
    [SerializeField] private bool startActive = false;
    private bool isDormant = true;

    [Header("Combat State")]
    [Tooltip("Si es false, el minotauro no persigue, no ataca ni apunta.")]
    [SerializeField] private bool combatEnabled = false;

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

    [Header("Audio")]
    public SoundType minotaurScreamSFX = SoundType.MinotaurScreamSFX;

    [Header("Wander Bounds")]
    [SerializeField] private Rect wanderBounds = new Rect(-100, -100, 200, 200);

    private int rangedAttacksDone = 0;
    private float rangedAttackCooldownTimer = 0f;

    private Animator animator;
    private MinotaurController controller;
    private MinotaurStateMachine stateMachine;
    private Rigidbody2D rb;
    private MinotaurWeaponController weaponController;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.1f;
    private Color originalColor;
    private Coroutine flashCoroutine;

    public UnityEvent OnGetHitEvent = new UnityEvent();
    UnityEvent IHittable.OnGetHit { get => OnGetHitEvent; set => OnGetHitEvent = value; }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        controller = GetComponent<MinotaurController>();
        rb = GetComponent<Rigidbody2D>();
        weaponController = GetComponent<MinotaurWeaponController>();
        stateMachine = new MinotaurStateMachine(this);

        originalColor = spriteRenderer != null ? spriteRenderer.color : Color.white;

        isDormant = !startActive;
        combatEnabled = startActive && !isDormant;
        SetComponentsEnabled(!isDormant);
    }

    private void Start()
    {
        currentHealth = maxHealth;
        stateMachine.ChangeState(new MinotaurIdleState(stateMachine));
    }

    private void Update()
    {
        if (isDead || isDormant) return;

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

    private void FixedUpdate()
    {
        if (isDead || isDormant) return;
        stateMachine?.FixedUpdate();
    }

    // ===== Activation API =====
    public void ActivateBoss(bool playScream = true)
    {
        if (!isDormant) return;

        isDormant = false;
        combatEnabled = true;
        SetComponentsEnabled(true);

        if (playScream) PlayScreamSFX();

        ResetAnimatorCombatTriggers();
        animator.SetBool("Idle", true);
        animator.SetBool("Run", false);
        stateMachine.ChangeState(new MinotaurIdleState(stateMachine));
    }

    private void SetComponentsEnabled(bool enabled)
    {
        if (controller != null) controller.enabled = enabled;
        if (weaponController != null) weaponController.enabled = enabled;

        if (!enabled)
        {
            controller?.StopMovement();
            rb.velocity = Vector2.zero;
        }
    }

    // ===== Wander API =====
    public void SetWanderBounds(Rect bounds) => wanderBounds = bounds;

    public void NotifyPlayerDied()
    {
        if (isDead) return;
        PlayScreamSFX();
        EnterWanderMode();
    }

    private void EnterWanderMode()
    {
        combatEnabled = false;          // apaga combate
        StopAllCoroutines();
        controller.StopMovement();
        rb.velocity = Vector2.zero;

        // Limpieza de animación: sólo “Run” activo para caminar normal, Idle off.
        ResetAnimatorCombatTriggers();
        animator.SetBool("Idle", false);
        animator.SetBool("Run", true);

        // Cambia a Wander
        stateMachine.ChangeState(new MinotaurWanderState(stateMachine, wanderBounds));
    }

    private void ResetAnimatorCombatTriggers()
    {
        if (animator == null) return;
        animator.ResetTrigger("MeleeAttack");
        animator.ResetTrigger("RangedAttack");
        animator.ResetTrigger("Dash");
        animator.ResetTrigger("Die");
    }

    // ===== IHittable =====
    public void OnGetHit() { }

    public void GetHit(int damage, GameObject dealer)
    {
        if (isDead || isDormant) return;

        currentHealth -= damage;
        currentHealth = Mathf.Max(0, currentHealth);
        OnHit?.Invoke();
        PlayDamageFlash();

        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        isDead = true;
        ResetAnimatorCombatTriggers();
        animator.SetBool("Idle", false);
        animator.SetBool("Run", false);
        animator.SetTrigger("Die");
        OnDeath?.Invoke();
        stateMachine.ChangeState(null);
        controller.StopMovement();
    }

    // ===== Ranged =====
    public void FireRangedProjectile()
    {
        if (projectilePrefab == null || firePoint == null || isDead || isDormant || !combatEnabled) return;

        Vector2 direction = firePoint.right.normalized;
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        proj.transform.right = direction;

        var bullet = proj.GetComponent<MinotaurProjectile>();
        if (bullet != null) bullet.bulletData = minotaurBulletData;

        RegisterRangedAttack();
    }

    public bool CanDoRangedAttack() => combatEnabled && rangedAttackCooldownTimer <= 0f && rangedAttacksDone < maxRangedAttacks;

    public void RegisterRangedAttack()
    {
        rangedAttacksDone++;
        if (rangedAttacksDone >= maxRangedAttacks)
            rangedAttackCooldownTimer = rangedAttackCooldown;
    }

    // ===== Dash helper =====
    public void StartDashTowardPlayer(float dashSpeed, float dashDuration)
    {
        if (isDormant || !combatEnabled) return;

        Transform player = GameObject.FindWithTag("Player")?.transform;
        if (player == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        StartCoroutine(DashRoutine(direction, dashSpeed, dashDuration));
    }

    private IEnumerator DashRoutine(Vector2 direction, float speed, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && !isDormant && !isDead && combatEnabled)
        {
            elapsed += Time.fixedDeltaTime;
            rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }

        if (combatEnabled)
            stateMachine.ChangeState(new MinotaurChaseState(stateMachine));
    }

    // ===== VFX / SFX =====
    private void PlayDamageFlash()
    {
        if (spriteRenderer == null) return;
        if (flashCoroutine != null) StopCoroutine(flashCoroutine);
        flashCoroutine = StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(flashDuration);
            spriteRenderer.color = originalColor;
        }
    }

    private void PlayScreamSFX()
    {
        if (SoundManager.Instance != null)
            SoundManager.Instance.PlaySound(minotaurScreamSFX, transform.position);
    }

    public void OnAttackAnimationEnd()
    {
        if (isDormant || isDead || !combatEnabled) return;
        animator.SetBool("Idle", false);
        animator.SetBool("Run", true);
        stateMachine.ChangeState(new MinotaurChaseState(stateMachine));
    }

    public Animator GetAnimator() => animator;
    public MinotaurController GetController() => controller;
    public bool IsDead() => isDead;
    public bool IsCombatEnabled() => combatEnabled;
    public GameObject GetProjectilePrefab() => projectilePrefab;
    public Transform GetFirePoint() => firePoint;
}
