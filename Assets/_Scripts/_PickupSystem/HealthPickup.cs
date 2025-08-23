using UnityEngine;

public class HealthPickup : MonoBehaviour
{
    [Header("Configuraci�n")]
    [SerializeField] private int healAmount = 25;
    [SerializeField] private bool destroyOnPickup = true;

    [SerializeField] private GameObject pickupVFX;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<PlayerHealthHandler>(out var health))
        {
            if (health.IsDead) return;

            int maxHealth = health.playerData.maxHealth;

            // Si ya est� al m�ximo, no curamos ni destruimos
            if (health.CurrentHealth >= maxHealth) return;

            health.Heal(healAmount);

                SoundManager.Instance.PlaySound(SoundType.PickupHealthSFX, transform.position);

            if (pickupVFX)
                Instantiate(pickupVFX, transform.position, Quaternion.identity);

            if (destroyOnPickup)
                Destroy(gameObject);
        }
    }
}
