using UnityEngine;

public class MinotaurWeaponController : MonoBehaviour
{
    [Header("Weapon System")]
    [SerializeField] private Transform weaponParent;
    [SerializeField] private Transform attackSpawnPoint;
    [SerializeField] private GameObject attackVFXPrefab;
    [SerializeField] private float meleeHitRange = 1.8f; // Ajusta desde Inspector

    private Transform player;

    private MinotaurController controller;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        controller = GetComponent<MinotaurController>();
    }

    public void AimWeapon(Vector3 targetPosition)
    {
        if (player == null || weaponParent == null || controller == null) return;

        // 1. Calcular rotación
        Vector2 direction = (player.position - weaponParent.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        weaponParent.rotation = Quaternion.Euler(0f, 0f, angle);

        // 2. Aplicar flip horizontal al WeaponParent localScale
        Vector3 scale = weaponParent.localScale;
        scale.x = controller.IsFacingRight ? 1f : -1f;
        weaponParent.localScale = scale;
    }

    // Este método lo llama la animación (AnimationEvent)
    public void SpawnAttackVFX()
    {
        if (attackVFXPrefab == null || attackSpawnPoint == null || player == null) return;

        float distanceToPlayer = Vector2.Distance(attackSpawnPoint.position, player.position);

        if (distanceToPlayer <= meleeHitRange)
        {
            Vector2 dir = (player.position - attackSpawnPoint.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, 0f, angle);

            Instantiate(attackVFXPrefab, attackSpawnPoint.position, rotation);
        }
    }

}
