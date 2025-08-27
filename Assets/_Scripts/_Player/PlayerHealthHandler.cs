using UnityEngine;
using UnityEngine.Events;

public class PlayerHealthHandler : MonoBehaviour, IHittable
{
    [SerializeField] public PlayerDataSO playerData;
    public int CurrentHealth { get; private set; }

    public UnityEvent<int, int> OnHealthChanged;

    public bool IsDead { get; private set; } = false;
    [field:SerializeField] public UnityEvent OnGetHit { get; set; }

    [SerializeField] private PlayerDeathHandler deathHandler;

    private AgentRenderer agentRenderer;
    public bool IsInvulnerable { get; private set; } = false;


    private void Awake()
    {
        CurrentHealth = playerData.maxHealth;
        agentRenderer = GetComponentInChildren<AgentRenderer>();
    }

    public void GetHit(int damage, GameObject damageDealer)
    {
        if (IsDead || IsInvulnerable) return;

        Debug.Log($"[PlayerHealthHandler] Recibiendo daño: {damage}");

        CurrentHealth -= damage;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, playerData.maxHealth);

        Debug.Log($"[PlayerHealthHandler] Nueva vida: {CurrentHealth} / {playerData.maxHealth}");

        OnHealthChanged?.Invoke(CurrentHealth, playerData.maxHealth);
        Debug.Log("[PlayerHealthHandler] Evento OnHealthChanged INVOCADO");

        OnGetHit?.Invoke();
        agentRenderer?.PlayGetHitPlayerVisuals();

        if (CurrentHealth <= 0)
        {
            IsDead = true;
            deathHandler?.HandleDeath();

            // Notificar al boss que el jugador murió
            var boss = FindObjectOfType<MinotaurBoss>();
            if (boss != null) boss.NotifyPlayerDied();
        }
    }

    public void SetInvulnerable(bool value)
    {
        IsInvulnerable = value;
    }

    public void Heal(int amount)
    {
        if (IsDead) return;

        int previousHealth = CurrentHealth;
        CurrentHealth += amount;
        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, playerData.maxHealth);

        OnHealthChanged?.Invoke(CurrentHealth, playerData.maxHealth);

        if (agentRenderer != null && CurrentHealth > previousHealth)
        {
            agentRenderer.PlayHealVisualFeedback(); // ✅ Aquí se dispara el feedback visual
        }

        Debug.Log($"[PlayerHealthHandler] Curado: +{amount}. Nueva vida: {CurrentHealth} / {playerData.maxHealth}");
    }




}
