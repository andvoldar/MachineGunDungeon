using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(PlayerStaminaHandler))]
[RequireComponent(typeof(Rigidbody2D))]
public class DashAbility : MonoBehaviour
{
    private Rigidbody2D rb;
    private PlayerStaminaHandler stamina;
    private PlayerDataSO data;
    private bool isDashing = false;
    private float dashTimer;
    public Vector2 LastDashDirection { get; private set; }
    public bool CanDash { get; private set; } = true;
    private float cooldownTimer;

    private Vector2 dashDirection;
    private float currentSpeed;

    public UnityEvent OnDashStarted;
    public UnityEvent OnDashEnded;

    public void Initialize(PlayerDataSO playerData)
    {
        data = playerData;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        stamina = GetComponent<PlayerStaminaHandler>();
        // Asegurarnos de que la colisión use Continuous para mejor detección (opcional)
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                EndDash();
        }

        if (!CanDash)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
                CanDash = true;
        }
    }

    public bool TryExecuteDash(Vector2 inputDir)
    {
        if (!CanDash || isDashing || inputDir == Vector2.zero)
            return false;
        if (!stamina.TryConsumeStamina(data.dashStaminaCost))
            return false;

        Vector2 dir = inputDir.normalized;
        LastDashDirection = dir;
        dashDirection = dir;
        StartDash();
        return true;
    }

    private void StartDash()
    {
        isDashing = true;
        CanDash = false;

        dashTimer = data.dashDuration;
        cooldownTimer = data.dashCooldown;

        OnDashStarted?.Invoke();

        // Rampa rápida en 0.1s hasta llegar a dashSpeed (por efeito visual), pero luego mantenemos la velocidad en FixedUpdate
        DOTween.Kill("DashSpeed");
        DOTween.To(() => currentSpeed, x => currentSpeed = x, data.dashSpeed, 0.1f)
            .SetEase(Ease.OutSine)
            .SetId("DashSpeed")
            .OnUpdate(() =>
            {
                rb.velocity = LastDashDirection * currentSpeed;
            });
    }

    private void EndDash()
    {
        isDashing = false;
        OnDashEnded?.Invoke();

        // Matamos cualquier tween de dash y volvemos a velocidad normal en rampa
        DOTween.Kill("DashSpeed");
        DOTween.To(() => currentSpeed, x => currentSpeed = x, data.maxSpeed, 0.2f)
            .SetEase(Ease.OutSine)
            .OnUpdate(() =>
            {
                rb.velocity = dashDirection * currentSpeed;
            });
    }

    public bool IsDashing() => isDashing;

    // ------------------------------------------------------------
    // AÑADIMOS este FixedUpdate para REAPLICAR la velocidad de dash
    // durante cada frame de física, y así evitar que colisiones
    // reduzcan o quiten su velocidad.
    // ------------------------------------------------------------
    private void FixedUpdate()
    {
        if (isDashing)
        {
            // Forzamos la velocidad en cada paso de física; así el dash
            // nunca se interrumpe por un choque con un objeto dinámico.
            rb.velocity = LastDashDirection * data.dashSpeed;
        }
    }
}
