using UnityEngine;

public class EnemyWeaponRenderer : MonoBehaviour
{
    [SerializeField] private SpriteRenderer weaponSpriteRenderer;
    [SerializeField] private SpriteRenderer enemyBodyRenderer;

    public void FlipWeaponSprite(bool flip)
    {
        weaponSpriteRenderer.flipY = flip;
    }

    public void RenderWeaponBehind(bool behind)
    {
        if (enemyBodyRenderer == null || weaponSpriteRenderer == null) return;

        weaponSpriteRenderer.sortingOrder = behind
            ? enemyBodyRenderer.sortingOrder - 1
            : enemyBodyRenderer.sortingOrder + 1;
    }
}
