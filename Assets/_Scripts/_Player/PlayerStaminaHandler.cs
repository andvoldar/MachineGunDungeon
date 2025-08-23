// PlayerStaminaHandler.cs
using UnityEngine;
using UnityEngine.Events;

public class PlayerStaminaHandler : MonoBehaviour
{
    [Header("Configuración (desde PlayerDataSO)")]
    [SerializeField] private PlayerDataSO playerData;

    public float CurrentStamina { get; private set; }
    public float MaxStamina { get { return playerData.maxStamina; } }  // <-- Exponer máximo

    public UnityEvent<float, float> OnStaminaChanged;

    private bool isRegenAllowed = true;

    private void Awake()
    {
        CurrentStamina = playerData.maxStamina;
    }

    private void Update()
    {
        if (isRegenAllowed && CurrentStamina < playerData.maxStamina)
        {
            CurrentStamina = Mathf.Clamp(CurrentStamina + playerData.staminaRegenRate * Time.deltaTime, 0f, playerData.maxStamina);
            OnStaminaChanged?.Invoke(CurrentStamina, playerData.maxStamina);
        }
    }

    public bool TryConsumeStamina(float amount)
    {
        if (CurrentStamina >= amount)
        {
            CurrentStamina -= amount;
            OnStaminaChanged?.Invoke(CurrentStamina, playerData.maxStamina);
            return true;
        }
        return false;
    }

    public void SetRegenEnabled(bool value)
    {
        isRegenAllowed = value;
    }
}