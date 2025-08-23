using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(AgentInput))]
public class PlayerController : MonoBehaviour
{
    private AgentInput agentInput;
    private Rigidbody2D rb;
    private AgentRenderer agentRenderer;
    private AgentAnimations agentAnimations;
    private AgentSoundPlayer agentSoundPlayer;
    [SerializeField] private PlayerDataSO playerData;
    private GhostTrailSpawner ghostTrailSpawner;
    private PlayerInvulnerabilityHandler invulnerabilityHandler;
    private WeaponHandler weaponHandler;
    public bool playerIsMoving { get; private set; }

    private DashAbility dashAbility;
    public PlayerStaminaHandler staminaHandler;

    private Vector2 inputDirection;
    private float currentSpeed;

    // Post-processing
    private Volume dashVolume;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;

    [Header("Post-Processing Effects Intensity")]
    [SerializeField] private float chromaticAberrationIntensity = 1f;
    [SerializeField] private float lensDistortionIntensity = -0.5f;

    private void Awake()
    {
        agentInput = GetComponent<AgentInput>();
        rb = GetComponent<Rigidbody2D>();
        agentRenderer = GetComponentInChildren<AgentRenderer>();
        agentAnimations = GetComponentInChildren<AgentAnimations>();
        ghostTrailSpawner = GetComponentInChildren<GhostTrailSpawner>();
        invulnerabilityHandler = GetComponent<PlayerInvulnerabilityHandler>();
        agentSoundPlayer = GetComponentInChildren<AgentSoundPlayer>();
        weaponHandler = GetComponent<WeaponHandler>();
        staminaHandler = GetComponent<PlayerStaminaHandler>();

        dashAbility = GetComponent<DashAbility>();
        dashAbility.Initialize(playerData);
        dashAbility.OnDashStarted.AddListener(OnDashStart);
        dashAbility.OnDashEnded.AddListener(OnDashEnd);

        // Post-processing setup
        var globalVolume = Camera.main.GetComponentInChildren<Volume>();
        if (globalVolume != null)
        {
            dashVolume = globalVolume;
            dashVolume.profile.TryGet(out chromaticAberration);
            dashVolume.profile.TryGet(out lensDistortion);
        }
    }

    private void OnEnable()
    {
        agentInput.OnMovementKeyPressed.AddListener(HandleMovementInput);
        agentInput.OnDashPressed.AddListener(HandleDash);
        agentRenderer?.OnMovingBackwards.AddListener(HandleDirectionMultiplier);
    }

    private void OnDisable()
    {
        agentInput.OnMovementKeyPressed.RemoveListener(HandleMovementInput);
        agentInput.OnDashPressed.RemoveListener(HandleDash);
        agentRenderer?.OnMovingBackwards.RemoveListener(HandleDirectionMultiplier);
    }

    private void HandleDirectionMultiplier(int dirMul)
    {
        agentAnimations?.SetWalkSpeed(dirMul);
    }

    private void HandleMovementInput(Vector2 dir)
    {
        inputDirection = dir.normalized;
        agentRenderer?.CheckIfMovingBackwards(inputDirection);
    }

    private void HandleDash()
    {
        if (dashAbility.TryExecuteDash(inputDirection))
        {
            agentSoundPlayer.PlayDashEffectSound();
            agentSoundPlayer.PlaySlowMotionSound();
            ghostTrailSpawner?.StartSpawning();
            invulnerabilityHandler?.ActivateForDuration(playerData.dashDuration);
            TimeManager.Instance?.DoSlowMotion(playerData.dashDuration);
            ApplyPostProcessingEffects(true);
        }
        else
        {
            Debug.Log("[Player] Dash not executed: stamina or cooldown");
        }
    }

    private void OnDashStart()
    {
        // Aquí podrías añadir animaciones específicas de dash
    }

    private void OnDashEnd()
    {
        ghostTrailSpawner?.StopSpawning();
        ApplyPostProcessingEffects(false);
    }

    private void ApplyPostProcessingEffects(bool enable)
    {
        if (chromaticAberration != null)
            DOTween.To(() => chromaticAberration.intensity.value, x => chromaticAberration.intensity.value = x,
                       enable ? chromaticAberrationIntensity : 0f, 0.2f)
                   .SetEase(Ease.OutSine)
                   .SetLink(gameObject, LinkBehaviour.KillOnDestroy);

        if (lensDistortion != null)
            DOTween.To(() => lensDistortion.intensity.value, x => lensDistortion.intensity.value = x,
                       enable ? lensDistortionIntensity : 0f, 0.2f)
                   .SetEase(Ease.OutSine)
                   .SetLink(gameObject, LinkBehaviour.KillOnDestroy);
    }

    private void Update()
    {
        agentAnimations?.AnimatePlayer(rb.velocity.magnitude);
    }

    private void FixedUpdate()
    {
        // Si dash está activo, no pisamos la velocidad
        if (dashAbility != null && dashAbility.IsDashing())
            return;

        // Movimiento base
        if (inputDirection != Vector2.zero)
            currentSpeed += playerData.acceleration * Time.fixedDeltaTime;
        else
            currentSpeed -= playerData.deacceleration * Time.fixedDeltaTime;

        currentSpeed = Mathf.Clamp(currentSpeed, 0f, playerData.maxSpeed);
        rb.velocity = inputDirection * currentSpeed;
    }

    public void StopMovement()
    {
        rb.velocity = Vector2.zero;
        currentSpeed = 0f;
        rb.isKinematic = true;
    }
}
