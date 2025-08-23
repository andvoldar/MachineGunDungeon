using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EnergyBarrier : MonoBehaviour
{
    private Renderer rend;
    private Collider2D col;

    private void Awake()
    {
        rend = GetComponentInChildren<Renderer>();
        col = GetComponent<Collider2D>();
    }

    public void EnableBarrier()
    {
        if (rend != null) rend.enabled = true;
        if (col != null) col.enabled = true;
    }

    public void DisableBarrier()
    {
        if (rend != null) rend.enabled = false;
        if (col != null) col.enabled = false;
    }
}
