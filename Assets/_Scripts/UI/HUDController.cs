using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class HUDController : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private HealthBarUI healthBarUI;
    [SerializeField] private StaminaBarUI staminaBarUI;

    [Header("Grenade Charge UI")]
    [SerializeField] private GrenadeChargeBarUI grenadeChargeBarUI;

    [Header("Grenade Count UI")]
    [SerializeField] private GameObject grenadeCountRoot;
    [SerializeField] private TMP_Text grenadeCountText;

    [Header("Ammo UI")]
    [SerializeField] private GameObject ammoUIRoot;
    [SerializeField] private Image ammoIcon;
    [SerializeField] private TMP_Text ammoCountText;

    private PlayerHealthHandler playerHealth;
    private PlayerStaminaHandler staminaHandler;
    private GrenadeHandler grenadeHandler;

    private Weapon currentWeapon;

    private Coroutine hideGrenadeCoroutine;
    [SerializeField] private float grenadeBarHideDelay = 0.2f;

    private void Start()
    {
        grenadeChargeBarUI?.Show(false);

        if (ammoUIRoot != null)
            ammoUIRoot.SetActive(false);
        else
            Debug.LogWarning("[HUDController] ammoUIRoot no asignado en el Inspector.");

        if (grenadeCountRoot != null)
            grenadeCountRoot.SetActive(false);

        StartCoroutine(InitAfterDelay());
    }

    private IEnumerator InitAfterDelay()
    {
        yield return new WaitForSeconds(0.1f);

        GameObject player = null;
        while (player == null)
        {
            player = GameObject.FindWithTag("Player");
            yield return null;
        }

        playerHealth = player.GetComponentInParent<PlayerHealthHandler>();
        staminaHandler = player.GetComponentInParent<PlayerStaminaHandler>();
        grenadeHandler = player.GetComponentInParent<GrenadeHandler>();

        if (grenadeChargeBarUI == null)
        {
            grenadeChargeBarUI = player.GetComponentInChildren<GrenadeChargeBarUI>();

            if (grenadeChargeBarUI == null)
            {
                Debug.LogWarning("[HUDController] No se encontró ningún GrenadeChargeBarUI en el Player.");
            }
            else
            {
                grenadeChargeBarUI.Show(false);
            }
        }

        // Salud
        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged.AddListener(UpdateHealthBar);
            UpdateHealthBar(playerHealth.CurrentHealth, playerHealth.playerData.maxHealth);
        }
        else Debug.LogWarning("[HUDController] No se encontró PlayerHealthHandler.");

        // Stamina
        if (staminaHandler != null)
        {
            staminaHandler.OnStaminaChanged.AddListener(UpdateStaminaBar);
            UpdateStaminaBar(staminaHandler.CurrentStamina, staminaHandler.MaxStamina);
        }
        else Debug.LogWarning("[HUDController] No se encontró PlayerStaminaHandler.");

        // HUD granada: cargo, contador, eventos
        if (grenadeHandler != null)
        {
            grenadeHandler.OnThrowChargeChanged += OnGrenadeChargeChanged;
            grenadeHandler.OnGrenadeCountChanged += UpdateGrenadeCountText;
            UpdateGrenadeCountText(grenadeHandler.CurrentGrenades, grenadeHandler.MaxGrenades);
        }
        else
        {
            Debug.LogWarning("[HUDController] No se encontró GrenadeHandler en hijos.");
        }

        // Armas
        var weaponHandler = player.GetComponentInChildren<WeaponHandler>();
        if (weaponHandler != null)
        {
            weaponHandler.OnWeaponChanged += HandleOnWeaponChanged;
        }
        else
        {
            Debug.LogWarning("[HUDController] No se encontró WeaponHandler en el Player para suscribir OnWeaponChanged.");
        }

        Debug.Log("[HUDController] Inicialización completada.");
    }

    private void UpdateHealthBar(int current, int max)
    {
        healthBarUI.SetHealth(current, max);
        healthBarUI.PlayDamageFeedback();
    }

    private void UpdateStaminaBar(float current, float max)
    {
        staminaBarUI.SetStamina(current, max);
    }

    private void UpdateGrenadeCountText(int current, int max)
    {
        if (grenadeCountText != null)
            grenadeCountText.text = $"{current} / {max}";

        if (grenadeCountRoot != null)
            grenadeCountRoot.SetActive(current > 0);
    }

    private void OnGrenadeChargeChanged(float current, float max, bool isCharging)
    {
        if (grenadeChargeBarUI == null) return;

        grenadeChargeBarUI.Show(true);
        grenadeChargeBarUI.SetCharge(current, max);

        if (!isCharging)
        {
            if (hideGrenadeCoroutine != null)
                StopCoroutine(hideGrenadeCoroutine);
            hideGrenadeCoroutine = StartCoroutine(HideGrenadeBarCoroutine());
        }
    }

    private IEnumerator HideGrenadeBarCoroutine()
    {
        yield return new WaitForSeconds(grenadeBarHideDelay);
        grenadeChargeBarUI.Show(false);
        hideGrenadeCoroutine = null;
    }

    private void HandleOnWeaponChanged(Weapon newWeapon)
    {
        UnsubscribeFromCurrentWeapon();

        currentWeapon = newWeapon;

        if (currentWeapon != null)
        {
            if (ammoUIRoot != null)
                ammoUIRoot.SetActive(true);

            UpdateAmmoCountText();

            currentWeapon.OnShoot.AddListener(HandleOnWeaponShoot);
            currentWeapon.OnEquippedChanged += HandleOnWeaponEquippedChanged;
        }
        else
        {
            if (ammoUIRoot != null)
                ammoUIRoot.SetActive(false);
        }
    }

    private void UnsubscribeFromCurrentWeapon()
    {
        if (currentWeapon == null) return;

        currentWeapon.OnShoot.RemoveListener(HandleOnWeaponShoot);
        currentWeapon.OnEquippedChanged -= HandleOnWeaponEquippedChanged;

        currentWeapon = null;
    }

    private void HandleOnWeaponShoot()
    {
        UpdateAmmoCountText();
    }

    private void HandleOnWeaponEquippedChanged(bool isEquipped)
    {
        if (!isEquipped)
        {
            if (ammoUIRoot != null)
                ammoUIRoot.SetActive(false);
        }
        else
        {
            if (ammoUIRoot != null)
            {
                ammoUIRoot.SetActive(true);
                UpdateAmmoCountText();
            }
        }
    }

    private void UpdateAmmoCountText()
    {
        if (currentWeapon == null || ammoCountText == null)
            return;

        int current = currentWeapon.Ammo;
        int max = currentWeapon.GetWeaponData()?.AmmoCapacity ?? 0;
        ammoCountText.text = $"{current} / {max}";
    }
}
