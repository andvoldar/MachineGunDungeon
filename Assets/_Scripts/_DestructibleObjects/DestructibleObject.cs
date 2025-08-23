// DestructibleObject.cs
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class DestructibleObject : MonoBehaviour, IHittable, IKnockbackable
{
    [Header("Número de impactos para romper")]
    [SerializeField] private int hitsToBreak = 2;

    [Header("Umbral de daño para romper de un solo golpe")]
    [SerializeField] private int oneShotDamageThreshold = 5;

    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce = 5f;
    [SerializeField] private float knockbackDuration = 0.2f;

    [Header("Box Punch Rotation Settings")]
    [SerializeField] private float punchDuration = 0.2f;
    [SerializeField] private float punchAngle = 15f;
    [SerializeField] private Ease punchEase = Ease.OutQuad;

    [Header("Empuje continuo por parte del jugador")]
    [SerializeField] private float pushForceApplied = 3f; // fuerza de empuje sostenido
    private bool isBeingPushed = false;
    private Vector2 pushDirection;

    [Header("LayerMask de Obstáculos")]
    [SerializeField] private LayerMask obstacleLayerMask; // Asignar capa "Obstacle" aquí

    [Header("Prefabs de escombros (usa pool si quieres)")]
    [SerializeField] private GameObject[] debrisPrefabs = new GameObject[6];

    [Header("Debris Fly Settings")]
    [SerializeField] private float debrisFlyDuration = 0.5f;
    [SerializeField] private float debrisFlyStrength = 1f;
    [SerializeField] private float debrisJumpPower = 1f;

    [Header("Duración en segundos antes de destruir cada debris")]
    [SerializeField] private float debrisLifetime = 5f;

    [Header("Evento al recibir golpe")]
    [SerializeField] private UnityEvent onGetHit = new UnityEvent();
    public UnityEvent OnGetHit { get => onGetHit; set => onGetHit = value; }

    /// <summary>
    /// Cada DropEntry ahora lleva:
    /// - prefab: el GameObject a soltar
    /// - dropChance: probabilidad (0–100) de que este item sea soltado al romperse
    /// </summary>
    [System.Serializable]
    public struct DropEntry
    {
        public GameObject prefab;
        [Range(0f, 100f), Tooltip("Probabilidad (0–100) de que este ítem dropee al romperse.")]
        public float dropChance;
    }

    [Header("Drop Settings")]
    [Tooltip("Lista de items (ej: granadas, salud, etc.). Cada uno se chequeará por separado.")]
    [SerializeField] private DropEntry[] itemDrops;
    [Tooltip("Lista de armas. Cada entrada tiene su chance de soltar.")]
    [SerializeField] private DropEntry[] weaponDrops;

    private Collider2D col2D;
    private Rigidbody2D rb;
    private int hitCount = 0;
    private bool isBreaking = false;

    // Variables para controlar knockback manual y evitar atravesar obstáculos
    private bool isKnockback = false;
    private Vector2 knockbackVelocity = Vector2.zero;
    private float knockbackTimer = 0f;

    private void Awake()
    {
        col2D = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();

        // Aseguramos que el Rigidbody2D esté en modo Kinematic
        rb.bodyType = RigidbodyType2D.Kinematic;
        // Para detección continua y reducir el riesgo de atravesar obstáculos al moverse rápido
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    public void GetHit(int damage, GameObject damageDealer)
    {
        if (isBreaking) return;

        onGetHit.Invoke();
        Vector2 knockDir = ((Vector2)transform.position - (Vector2)damageDealer.transform.position).normalized;

        // Primer golpe: punch rotation si el daño es menor al umbral de one-shot
        if (hitCount == 0 && damage < oneShotDamageThreshold)
        {
            hitCount++;
            ApplyKnockback(knockDir, knockbackForce, knockbackDuration);
            transform.DOPunchRotation(new Vector3(0, 0, punchAngle), punchDuration, 1, 0f)
                     .SetEase(punchEase)
                     .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
            return;
        }

        // Condición de ruptura: daño alto (oneShot) o segundo golpe
        if (damage >= oneShotDamageThreshold || ++hitCount >= hitsToBreak)
        {
            StartBreak(knockDir);
        }
    }

    private void StartBreak(Vector2 knockDirection)
    {
        isBreaking = true;
        col2D.enabled = false; // deshabilitamos collider para que no reciba más empuje
        ApplyKnockback(knockDirection, knockbackForce, knockbackDuration);
        SoundManager.Instance.PlaySound(SoundType.BoxDestroyedSFX, transform.position);

        // 1) Instanciamos escombros (igual que antes, o mejor usa tu DebrisPool si lo tienes)
        SpawnDebrisLocally();

        // 2) Intentamos dropear items y armas (ya con probabilidad y tween “guay”)
        TryDropWithChance();

        // 3) Destruimos este objeto (desaparece la caja)
        Destroy(gameObject);
    }

    /// <summary>
    /// Instancia cada debris sin usar pool para simplificar: hace un DOJump y luego lo destruye tras debrisLifetime.
    /// </summary>
    private void SpawnDebrisLocally()
    {
        foreach (var prefab in debrisPrefabs)
        {
            if (prefab == null) continue;

            // 1) Instanciamos el fragmento en la posición del objeto
            GameObject piece = Instantiate(prefab, transform.position, Quaternion.identity);

            // 2) Limpiamos tweens anteriores si el prefab venía con alguno
            piece.transform.DOKill();

            // 3) Calculamos una dirección aleatoria y objetivo del salto
            Vector2 randomDir = Random.insideUnitCircle.normalized;
            Vector2 target = (Vector2)transform.position + randomDir * debrisFlyStrength;

            // 4) Animar salto con DOJump
            piece.transform.DOJump(
                    target,
                    debrisJumpPower,
                    1,                // un solo salto
                    debrisFlyDuration
                )
                .SetEase(Ease.OutQuad)
                .SetLink(piece, LinkBehaviour.KillOnDestroy);

            // 5) Animar rotación continua durante debrisFlyDuration
            piece.transform.DORotate(
                    new Vector3(
                        Random.Range(0f, 360f),
                        Random.Range(0f, 360f),
                        Random.Range(0f, 360f)
                    ),
                    debrisFlyDuration,
                    RotateMode.FastBeyond360
                )
                .SetEase(Ease.Linear)
                .SetLink(piece, LinkBehaviour.KillOnDestroy);

            // 6) Programar destrucción automática tras debrisLifetime segundos
            Destroy(piece, debrisLifetime);
        }
    }

    /// <summary>
    /// Recorre itemDrops y weaponDrops. Para cada entrada:
    /// - Se genera un número aleatorio de 0 a 100.
    /// - Si es menor que dropChance, se instancia el prefab.
    /// - Aplicamos un tween “fly out” con DOJump para que parezca que sale volando.
    /// </summary>
    private void TryDropWithChance()
    {
        // *** Helper local para instanciar y animar un drop ***
        void SpawnAndAnimateDrop(GameObject dropPrefab)
        {
            if (dropPrefab == null) return;

            // 1) Instanciamos el item en la posición del objeto
            GameObject item = Instantiate(dropPrefab, transform.position, Quaternion.identity);

            // 2) Limpiamos tweens si el prefab trae alguno
            item.transform.DOKill();

            // 3) Calculamos una dirección con sesgo hacia arriba (para que parezca que sale volando)
            //    Por ejemplo: un rango horizontal limitado y un impulso vertical.
            float horizontalOffset = Random.Range(-0.5f, 0.5f);
            float verticalOffset = Random.Range(0.8f, 1.2f); // empuje principal hacia arriba
            Vector2 direction = new Vector2(horizontalOffset, verticalOffset).normalized;

            // 4) Definimos distancia del “vuelo”
            float flyDistance = 1f;
            Vector2 targetPos = (Vector2)transform.position + direction * flyDistance;

            // 5) Aplicamos DOJump pero con 1 salto y altura moderada
            float jumpPower = 0.8f;
            float jumpDuration = 0.6f;

            item.transform.DOJump(
                    targetPos,
                    jumpPower,
                    1,
                    jumpDuration
                )
                .SetEase(Ease.OutQuad)
                .SetLink(item, LinkBehaviour.KillOnDestroy);

            // 6) Al finalizar el DOJump, dejamos el objeto estático en targetPos (sin destruirlo)
            //    para que el jugador pueda interactuar/recogerlo.
            //    Podemos añadirle un pequeño “bounce” al aterrizar:
            item.transform.DOPunchScale(new Vector3(0.2f, 0.2f, 0.2f), 0.2f, 1, 0f)
                .SetDelay(jumpDuration)
                .SetEase(Ease.OutBounce)
                .SetLink(item, LinkBehaviour.KillOnDestroy);
        }

        // — Recorremos cada itemDrops —
        foreach (var entry in itemDrops)
        {
            if (entry.prefab == null || entry.dropChance <= 0f)
                continue;

            float roll = Random.Range(0f, 100f);
            if (roll < entry.dropChance)
            {
                SpawnAndAnimateDrop(entry.prefab);
            }
        }

        // — Recorremos cada weaponDrops —
        foreach (var entry in weaponDrops)
        {
            if (entry.prefab == null || entry.dropChance <= 0f)
                continue;

            float roll = Random.Range(0f, 100f);
            if (roll < entry.dropChance)
            {
                SpawnAndAnimateDrop(entry.prefab);
            }
        }
    }

    public void ApplyKnockback(Vector2 direction, float force, float duration)
    {
        // Iniciamos knockback “manual” para comprobar obstáculos en FixedUpdate
        knockbackVelocity = direction.normalized * force;
        knockbackTimer = duration;
        isKnockback = true;
    }

    // Detecta contacto continuo: empuje normal y detección de dash en colisión
    private void OnCollisionStay2D(Collision2D col)
    {
        if (isBreaking) return;

        // 1) Si quien colisiona tiene DashAbility y está en dash → rompemos de un golpe
        if (col.gameObject.TryGetComponent<DashAbility>(out var dashComp) && dashComp.IsDashing())
        {
            Collider2D otherCol = col.collider;
            Physics2D.IgnoreCollision(col2D, otherCol);

            GetHit(oneShotDamageThreshold, col.gameObject);
            return;
        }

        // 2) Si no es dash, procesamos el empuje normal (push) solo si colisiona Player/Enemy
        if (col.gameObject.CompareTag("Player") || col.gameObject.CompareTag("Enemy"))
        {
            pushDirection = ((Vector2)transform.position - (Vector2)col.transform.position).normalized;
            isBeingPushed = true;
        }
    }

    private void OnCollisionExit2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player") || col.gameObject.CompareTag("Enemy"))
        {
            isBeingPushed = false;
        }
    }

    private void FixedUpdate()
    {
        // 1) Manejo de knockback
        if (isKnockback && knockbackTimer > 0f && !isBreaking)
        {
            float deltaTime = Time.fixedDeltaTime;
            Vector2 movementThisFrame = knockbackVelocity * deltaTime;

            if (movementThisFrame.sqrMagnitude > Mathf.Epsilon)
            {
                Vector2 dir = movementThisFrame.normalized;
                float dist = movementThisFrame.magnitude;

                // Comprobamos solo contra “Obstacles”, ignorando otros DestructibleObject
                RaycastHit2D[] hits = new RaycastHit2D[5];
                ContactFilter2D filter = new ContactFilter2D();
                filter.ClearLayerMask();
                filter.SetLayerMask(obstacleLayerMask);
                filter.useLayerMask = true;

                int hitCountTotal = rb.Cast(dir, filter, hits, dist);
                bool hitValidObstacle = false;

                // Revisamos cada hit: si NO es otro DestructibleObject, contamos como obstáculo real
                for (int i = 0; i < hitCountTotal; i++)
                {
                    if (hits[i].collider == null) continue;
                    if (hits[i].collider.GetComponent<DestructibleObject>() == null)
                    {
                        hitValidObstacle = true;
                        break;
                    }
                }

                if (!hitValidObstacle)
                {
                    rb.MovePosition(rb.position + movementThisFrame);
                }
                else
                {
                    knockbackTimer = 0f;
                    isKnockback = false;
                }
            }

            knockbackTimer -= deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockback = false;
            }
            return;
        }

        // 2) Manejo de empuje continuo por jugador/enemigo
        if (isBeingPushed && !isBreaking)
        {
            float delta = pushForceApplied * Time.fixedDeltaTime;
            Vector2 movimiento = pushDirection * delta;

            if (movimiento.sqrMagnitude > Mathf.Epsilon)
            {
                Vector2 dir = movimiento.normalized;
                float dist = movimiento.magnitude;

                // Comprobamos solo contra “Obstacles”, ignorando otros DestructibleObject
                RaycastHit2D[] hits = new RaycastHit2D[5];
                ContactFilter2D filter = new ContactFilter2D();
                filter.ClearLayerMask();
                filter.SetLayerMask(obstacleLayerMask);
                filter.useLayerMask = true;

                int hitCountTotal = rb.Cast(dir, filter, hits, dist);
                bool hitValidObstacle = false;

                for (int i = 0; i < hitCountTotal; i++)
                {
                    if (hits[i].collider == null) continue;
                    if (hits[i].collider.GetComponent<DestructibleObject>() == null)
                    {
                        hitValidObstacle = true;
                        break;
                    }
                }

                if (!hitValidObstacle)
                {
                    rb.MovePosition(rb.position + movimiento);
                }
                else
                {
                    isBeingPushed = false;
                }
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (isBreaking) return;

        if (collision.gameObject.TryGetComponent<DashAbility>(out var dashComp) && dashComp.IsDashing())
        {
            Collider2D otherCol = collision.collider;
            Physics2D.IgnoreCollision(col2D, otherCol);

            GetHit(oneShotDamageThreshold, collision.gameObject);
        }
    }
}
