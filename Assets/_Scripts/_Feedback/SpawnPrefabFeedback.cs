// Assets/Scripts/Feedbacks/SpawnPrefabFeedback.cs
using UnityEngine;

[AddComponentMenu("Feedbacks/Spawn Prefab Feedback")]
public class SpawnPrefabFeedback : Feedback
{
    [Header("Prefabs Settings")]
    [Tooltip("Lista de prefabs posibles a instanciar")]
    [SerializeField] private GameObject[] prefabs = null;
    [Tooltip("Número de instancias por feedback")]
    [SerializeField] private int spawnCount = 5;

    [Header("Spawn Area")]
    [Tooltip("Radio máximo desde el FeedbackPlayer donde caerán las instancias")]
    [SerializeField] private float spawnRadius = 1f;

    [Header("Physics")]
    [Tooltip("Impulso mínimo aplicado si tiene Rigidbody2D")]
    [SerializeField] private float forceMin = 1f;
    [Tooltip("Impulso máximo aplicado si tiene Rigidbody2D")]
    [SerializeField] private float forceMax = 3f;

    [Header("Cleanup")]
    [Tooltip("Tiempo en segundos antes de destruir cada instancia")]
    [SerializeField] private float lifeTime = 2f;

    // Llamada por FeedbackPlayer sin parámetros  
    public override void CreateFeedback()
    {
        for (int i = 0; i < spawnCount; i++)
        {
            Vector2 dir = Random.insideUnitCircle.normalized;
            CreateFeedback(dir);
        }
    }

    // Llamada si tú mismo decides pasar una dirección concreta  
    public override void CreateFeedback(Vector2 direction)
    {
        if (prefabs.Length == 0) return;

        // Elige un prefab al azar
        var prefab = prefabs[Random.Range(0, prefabs.Length)];
        if (prefab == null) return;

        // Calcula posición de spawn aleatoria en el radio
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + (Vector3)offset;
        var go = Instantiate(prefab, spawnPos, Quaternion.identity);

        // Aplica fuerza en la dirección dada (o la propia offset si prefieres)
        if (go.TryGetComponent<Rigidbody2D>(out var rb))
        {
            float force = Random.Range(forceMin, forceMax);
            rb.AddForce(direction * force, ForceMode2D.Impulse);
        }

        // Auto-destrucción
        Destroy(go, lifeTime);
    }

    public override void CompletePreviousFeedback()
    {
        // No hay corutinas ni tweens que matar aquí
        // Si quisiéramos, podríamos trackear instancias y limpiar, pero no es necesario
    }
}
