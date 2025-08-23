using UnityEngine;

public class PickableGrenade : MonoBehaviour
{
    [SerializeField] private GrenadeDataSO grenadeData;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.TryGetComponent<GrenadeHandler>(out var handler))
        {
            handler.PickUpGrenade(grenadeData);
            Destroy(gameObject);
        }
    }
}
