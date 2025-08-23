using UnityEngine;
using System;

[RequireComponent(typeof(AgentInput))]
public class GrenadeHandler : MonoBehaviour
{
    public event Action<float, float, bool> OnThrowChargeChanged;
    public event Action<int, int> OnGrenadeCountChanged;

    [SerializeField] private Transform grenadeSpawnPoint;
    [SerializeField] private KeyCode throwKey = KeyCode.LeftShift;

    private GrenadeDataSO equippedGrenade;
    private int grenadeCount;
    private const int maxGrenadeCount = 3; // puedes hacer esto variable si lo necesitas

    private bool isChargingThrow;
    private float throwCharge;
    private AgentInput input;
    private AgentSoundPlayer audioPlayer;

    public int CurrentGrenades => grenadeCount;
    public int MaxGrenades => maxGrenadeCount;

    private void Awake()
    {
        input = GetComponent<AgentInput>();
        audioPlayer = GetComponentInChildren<AgentSoundPlayer>();
    }

    private void Update()
    {
        if (equippedGrenade == null || grenadeCount <= 0)
            return;

        if (Input.GetKeyDown(throwKey))
            StartChargingThrow();

        if (isChargingThrow)
            UpdateChargingThrow();

        if (Input.GetKeyUp(throwKey) && isChargingThrow)
            ReleaseThrow();
    }

    private void StartChargingThrow()
    {
        audioPlayer.PlayPrepareThrowSFX();
        isChargingThrow = true;
        throwCharge = 0f;
        OnThrowChargeChanged?.Invoke(0f, 1f, true);
    }

    private void UpdateChargingThrow()
    {
        throwCharge = Mathf.Clamp01(throwCharge + Time.deltaTime);
        OnThrowChargeChanged?.Invoke(throwCharge, 1f, true);
    }

    private void ReleaseThrow()
    {
        isChargingThrow = false;
        OnThrowChargeChanged?.Invoke(throwCharge, 1f, false);

        float force = Mathf.Lerp(equippedGrenade.LaunchForceMin, equippedGrenade.LaunchForceMax, throwCharge);
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (mousePos - (Vector2)transform.position).normalized;

        GameObject grenadeGO = Instantiate(equippedGrenade.GrenadePrefab, grenadeSpawnPoint.position, Quaternion.identity);
        if (grenadeGO.TryGetComponent<Grenade>(out var grenade))
        {
            grenade.Initialize(equippedGrenade);
            grenade.Launch(dir, force);
            audioPlayer.PlayThrowSFX();
        }

        grenadeCount--;
        OnGrenadeCountChanged?.Invoke(grenadeCount, maxGrenadeCount);

        if (grenadeCount <= 0)
            equippedGrenade = null;
    }

    public void PickUpGrenade(GrenadeDataSO grenade)
    {
        if (equippedGrenade == null)
            equippedGrenade = grenade;

        grenadeCount = Mathf.Clamp(grenadeCount + 1, 0, maxGrenadeCount);
        OnGrenadeCountChanged?.Invoke(grenadeCount, maxGrenadeCount);

        audioPlayer.PlayPickupItemSFX();
    }
}
