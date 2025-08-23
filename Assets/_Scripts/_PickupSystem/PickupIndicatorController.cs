using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PlayerInteractor))]
public class PickupIndicatorController : MonoBehaviour
{
    [Header("Objeto indicador (child) que se activa/desactiva")]
    [SerializeField] private GameObject indicator;

    [Tooltip("Cada cuánto tiempo en segundos comprobamos cercanía")]
    [SerializeField] private float checkInterval = 0.1f;

    private PlayerInteractor interactor;

    private void Awake()
    {
        interactor = GetComponent<PlayerInteractor>();
        if (interactor == null)
        {
            Debug.LogError("[PickupIndicatorController] Falta PlayerInteractor en este GameObject.");
            enabled = false;
            return;
        }

        if (indicator == null)
        {
            Debug.LogError("[PickupIndicatorController] No has asignado el indicador.");
            enabled = false;
            return;
        }

        // Arrancamos oculto sin desactivar este script
        indicator.SetActive(false);
    }

    private void OnEnable()
    {
        InvokeRepeating(nameof(CheckForPickup), 0f, checkInterval);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(CheckForPickup));
    }

    private void CheckForPickup()
    {
        // Buscamos objetos tag "Pickup" en la máscara y rango del interactor
        var hits = Physics2D.OverlapCircleAll(
            interactor.transform.position,
            interactor.InteractRange,
            interactor.InteractableMask
        );

        bool found = false;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].CompareTag("Pickup"))
            {
                found = true;
                break;
            }
        }

        // Solo activamos/desactivamos el indicador, el script sigue vivo
        indicator.SetActive(found);
    }

    private void OnDrawGizmosSelected()
    {
        if (interactor != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(interactor.transform.position, interactor.InteractRange);
        }
    }
}
