using UnityEngine;
using UnityEngine.Rendering.Universal; // Para Light2D
using DG.Tweening;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class Grenade : MonoBehaviour
{
    [Header("Comportamiento")]
    [SerializeField] private float torqueOnLaunch = 300f;

    [Header("Luz de parpadeo")]
    [SerializeField] private Light2D blinkLight;
    [SerializeField] private float blinkIntensityMin = 1f;
    [SerializeField] private float blinkIntensityMax = 4f;
    [SerializeField] private float blinkSpeedBase = 1f;

    [Header("Ignorar colisiones iniciales")]
    [Tooltip("Nombre de la capa de muros para ignorar al lanzar")]
    [SerializeField] private string wallLayerName = "Wall";
    [Tooltip("Segundos que la granada ignora la colisión con los muros")]
    [SerializeField] private float ignoreWallDuration = 0.5f;

    private GrenadeDataSO grenadeData;
    private float fuseTimer;
    private bool exploded = false;
    private bool isArmed = false;

    private Rigidbody2D rb;
    private Light2D grenadeLight;
    private Collider2D grenadeCollider;
    private DroppedWeaponVisuals droppedVisuals;
    private FeedbackPlayer feedbackPlayer;

    private int wallLayer;
    private int grenadeLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        grenadeCollider = GetComponent<Collider2D>();
        grenadeLight = GetComponentInChildren<Light2D>();
        droppedVisuals = GetComponent<DroppedWeaponVisuals>();
        feedbackPlayer = GetComponent<FeedbackPlayer>();

        // Configuración top-down 2D
        rb.gravityScale = 0f;
        rb.drag = 0.5f;
        rb.angularDrag = 0.5f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.sharedMaterial = new PhysicsMaterial2D("GrenadeBounce")
        {
            bounciness = 0.8f,
            friction = 0.2f
        };

        rb.isKinematic = true;

        if (grenadeLight != null)
            grenadeLight.enabled = true;

        if (blinkLight != null)
            blinkLight.enabled = false;

        // Caches de capas
        wallLayer = LayerMask.NameToLayer(wallLayerName);
        grenadeLayer = gameObject.layer;
    }

    public void Initialize(GrenadeDataSO data)
    {
        grenadeData = data;
        fuseTimer = data.FuseTime;
        isArmed = true;

        if (droppedVisuals != null)
            droppedVisuals.enabled = false;

        if (grenadeLight != null)
            grenadeLight.enabled = true;
    }

    private void Update()
    {
        if (!isArmed || exploded) return;

        fuseTimer -= Time.deltaTime;

        // Parpadeo dinámico de la luz
        if (blinkLight != null && blinkLight.enabled)
        {
            float speed = blinkSpeedBase + (1f / Mathf.Max(fuseTimer, 0.1f));
            float intensity = Mathf.Lerp(blinkIntensityMin, blinkIntensityMax, Mathf.PingPong(Time.time * speed, 1f));
            blinkLight.intensity = intensity;
        }

        if (fuseTimer <= 0f)
        {
            Explode();
        }
    }

    /// <summary>
    /// Lanza la granada en la dirección dada con la fuerza dada.
    /// Ignora colisiones con la capa de muros durante 0.5 segundos.
    /// </summary>
    public void Launch(Vector2 direction, float force)
    {
        if (!isArmed) return;

        StartCoroutine(LaunchRoutine(direction, force));
    }

    private IEnumerator LaunchRoutine(Vector2 direction, float force)
    {
        // 1) Ignorar colisiones entre granada y capa de muros
        if (wallLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(grenadeLayer, wallLayer, true);
        }

        // 2) Activar física
        rb.isKinematic = false;

        if (grenadeLight != null)
            grenadeLight.enabled = false;

        if (blinkLight != null)
            blinkLight.enabled = true;

        if (droppedVisuals != null)
            droppedVisuals.enabled = false;

        rb.velocity = Vector2.zero;
        rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
        rb.AddTorque(torqueOnLaunch, ForceMode2D.Impulse);

        // 3) Esperar ignoreWallDuration segundos
        yield return new WaitForSeconds(ignoreWallDuration);

        // 4) Restaurar colisiones con los muros
        if (wallLayer >= 0)
        {
            Physics2D.IgnoreLayerCollision(grenadeLayer, wallLayer, false);
        }
    }

    private void Explode()
    {
        if (!isArmed || exploded) return;
        exploded = true;

        if (blinkLight != null)
            blinkLight.enabled = false;

        feedbackPlayer.PlayFeedback();

        if (grenadeData != null && grenadeData.ExplosionPrefab != null)
        {
            Instantiate(grenadeData.ExplosionPrefab, transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning($"Missing ExplosionPrefab in {grenadeData?.name ?? "GrenadeData (null)"}");
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, grenadeData.ExplosionRadius);
        foreach (var hit in hits)
        {
            if (hit.TryGetComponent(out IHittable damageable))
            {
                damageable.GetHit((int)grenadeData.ExplosionDamage, gameObject);
            }

            if (hit.TryGetComponent(out IKnockbackable knockbackTarget))
            {
                Vector2 dir = (hit.transform.position - transform.position).normalized;
                knockbackTarget.ApplyKnockback(dir, grenadeData.KnockbackForce, 0.3f);
            }
        }

        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (grenadeData != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, grenadeData.ExplosionRadius);
        }
    }
}
