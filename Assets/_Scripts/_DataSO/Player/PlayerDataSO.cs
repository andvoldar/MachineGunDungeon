using UnityEngine;

[CreateAssetMenu(menuName = "Player/PlayerData")]
public class PlayerDataSO : ScriptableObject
{
    [Header("Health")]
    public int maxHealth = 100;

    [Header("Stamina")]
    public float maxStamina = 100f;
    public float staminaRegenRate = 10f;
    public float dashStaminaCost = 50f; 


    [Range(1, 10)]
    public float maxSpeed = 5;

    [Range(0.1f, 100)]
    public float acceleration = 50, deacceleration = 50;

    [Header("Dash")]
    public float dashForce = 15f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float dashTimeFreeze = 0.1f;



}
