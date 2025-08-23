// PlayerInteractor.cs
using UnityEngine;

public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] private float interactRange = 1.5f;
    [SerializeField] private LayerMask interactableMask;

    public float InteractRange => interactRange;
    public LayerMask InteractableMask => interactableMask;

    public void TryInteract()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, interactRange, interactableMask);
        foreach (var hit in hits)
        {
            // Ahora solo respondemos a tag "Interactable"
            if (!hit.CompareTag("Pickup"))
                continue;

            if (hit.TryGetComponent<IInteractable>(out var interactable))
            {
                interactable.Interact(gameObject);
                break;
            }
        }
    }
}
