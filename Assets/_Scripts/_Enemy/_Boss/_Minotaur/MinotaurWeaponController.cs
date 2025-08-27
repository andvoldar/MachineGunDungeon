using UnityEngine;

public class MinotaurWeaponController : MonoBehaviour
{
    [Header("Weapon System")]
    [SerializeField] private Transform weaponParent;
    [SerializeField] private Transform attackSpawnPoint;
    [SerializeField] private GameObject attackVFXPrefab;
    [SerializeField] private float meleeHitRange = 1.8f;

    private Transform player;
    private MinotaurController controller;
    private MinotaurBoss boss;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player")?.transform;
        controller = GetComponent<MinotaurController>();
        boss = GetComponent<MinotaurBoss>();
    }

    public void AimWeapon(Vector3 targetPosition)
    {
        // Si no hay combate activo, NO apuntamos.
        if (boss != null && !boss.IsCombatEnabled()) return;
        if (player == null || weaponParent == null || controller == null) return;

        Vector2 direction = (player.position - weaponParent.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        weaponParent.rotation = Quaternion.Euler(0f, 0f, angle);

        Vector3 scale = weaponParent.localScale;
        scale.x = controller.IsFacingRight ? 1f : -1f;
        weaponParent.localScale = scale;
    }

    // Llamado por AnimationEvent
    public void SpawnAttackVFX()
    {
        // Si no hay combate activo, NO generamos daño/VFX.
        if (boss != null && !boss.IsCombatEnabled()) return;
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
