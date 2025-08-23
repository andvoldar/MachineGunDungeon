using UnityEngine;

public class EnemyWeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform weaponParent;
    private EnemyWeaponRenderer enemyWeaponRenderer;

    private Enemy enemy;
    private EnemyPerception perception;
    private EnemyAvatar avatar;

    [Header("Attack")]
    [SerializeField] private GameObject attackPrefab;
    [SerializeField] private Transform attackSpawnPoint;

    private float desiredAngle;

    private void Awake()
    {
        enemy = GetComponentInParent<Enemy>();
        enemyWeaponRenderer = GetComponentInChildren<EnemyWeaponRenderer>();
        avatar = GetComponentInChildren<EnemyAvatar>(); // asumiendo que es hijo

        var aiSystem = GetComponentInParent<EnemyAISystem>();
        if (aiSystem != null)
        {
            perception = aiSystem.Perception;
        }
    }

    private void Update()
    {

        if (avatar != null)
        {
            Vector3 scale = weaponParent.localScale;
            scale.x = avatar.IsFacingLeft ? -1f : 1f;
            weaponParent.localScale = scale;
        }

        if (perception == null)
        {
            // Intenta obtener la percepción en caso de que no se haya asignado
            var aiSystem = GetComponentInParent<EnemyAISystem>();
            if (aiSystem != null)
            {
                perception = aiSystem.Perception;
            }
        }

        // Si ya hay un target, giramos hacia él constantemente
        if (perception != null && perception.HasTarget())
        {
            AimWeapon(perception.CurrentTarget.position);
        }
    }


    private void AimWeapon(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - (Vector2)enemy.transform.position;
        desiredAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        weaponParent.rotation = Quaternion.Euler(0f, 0f, desiredAngle);


        AdjustWeaponRendering();
    }

    private void AdjustWeaponRendering()
    {
        if (enemyWeaponRenderer != null)
        {
            bool shouldFlip = desiredAngle > 90 || desiredAngle < -90;
            bool renderBehind = desiredAngle < 180 && desiredAngle > 0;

            enemyWeaponRenderer.FlipWeaponSprite(shouldFlip);
            enemyWeaponRenderer.RenderWeaponBehind(renderBehind);
        }
    }


    public void SpawnAttackVFX()
    {
        if (enemy == null || enemy.EnemyData == null) return;

        GameObject vfxPrefab = enemy.EnemyData.MeleeAttackVFXPrefab;

        if (vfxPrefab != null && attackSpawnPoint != null)
        {
            Instantiate(vfxPrefab, attackSpawnPoint.position, weaponParent.rotation, weaponParent);
        }
    }


}
