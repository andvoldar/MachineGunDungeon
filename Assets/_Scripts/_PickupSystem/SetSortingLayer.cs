using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class SetSortingLayer : MonoBehaviour
{
    [Tooltip("Nombre de la Sorting Layer (p.ej. \"Default\", \"UI\", \"Foreground\")")]
    public string sortingLayerName = "UI";

    [Tooltip("Orden en la capa (cuanto mayor, más encima)")]
    public int sortingOrder = 100;

    private void Awake()
    {
        var rend = GetComponent<Renderer>();
        rend.sortingLayerName = sortingLayerName;
        rend.sortingOrder = sortingOrder;
    }
}
