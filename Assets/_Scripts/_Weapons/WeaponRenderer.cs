using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WeaponRenderer : MonoBehaviour
{
    [SerializeField]
    protected int playerSortingOrder = 0;
    protected SpriteRenderer weaponRenderer;
    private int flipValue = 1;

    private void Awake()
    {
        weaponRenderer = GetComponent<SpriteRenderer>();
    }

    public void FlipWeaponSprite(bool flipped)
    {
        weaponRenderer.flipY = flipped;
    }

    public void RenderWeaponBehindPlayer(bool behind)
    {
        if (behind)
            weaponRenderer.sortingOrder = playerSortingOrder - flipValue; // Renderizar detr�s del jugador
        else
            weaponRenderer.sortingOrder = playerSortingOrder + flipValue; // Renderizar al frente
    }
}
