// Assets/_Scripts/Rooms/EnergyBarrier.cs
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class EnergyBarrier : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Renderer rend;
    [SerializeField] private Collider2D col;

    private void Awake()
    {
        if (rend == null) rend = GetComponentInChildren<Renderer>();
        if (col == null) col = GetComponent<Collider2D>();
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

    /// <summary>
    /// Hace visible la barrera y, tras appearDuration, habilita el collider.
    /// Puedes pasar un VFX opcional que se instancia como hijo.
    /// </summary>
    public IEnumerator Appear(float appearDuration, bool enableColliderAtEnd, GameObject spawnVFXPrefab = null)
    {
        if (rend != null) rend.enabled = true;
        if (col != null) col.enabled = false;

        if (spawnVFXPrefab != null)
        {
            Instantiate(spawnVFXPrefab, transform.position, Quaternion.identity, transform);
        }

        if (appearDuration > 0f)
            yield return new WaitForSeconds(appearDuration);

        if (enableColliderAtEnd && col != null)
            col.enabled = true;
    }
}
