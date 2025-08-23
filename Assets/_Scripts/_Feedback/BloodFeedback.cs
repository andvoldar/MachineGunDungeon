// Assets/Scripts/Feedbacks/BloodFeedback.cs
using UnityEngine;
using DG.Tweening;

[AddComponentMenu("Feedbacks/BloodFeedback")]
public class BloodFeedback : Feedback
{
    [Header("Prefabs de gotas de sangre")]
    [Tooltip("Lista de prefabs posibles a instanciar")]
    [SerializeField] private GameObject[] bloodPrefabs;

    [Header("Rango de cantidad")]
    [Tooltip("Mínimo de gotas a instanciar")]
    [SerializeField, Min(1)] private int minSpawnCount = 3;
    [Tooltip("Máximo de gotas a instanciar")]
    [SerializeField, Min(1)] private int maxSpawnCount = 8;

    [Header("Vuelo y caída")]
    [Tooltip("Duración mínima del vuelo antes de quedarse en tierra")]
    [SerializeField, Min(0f)] private float minFlyDuration = 0.3f;
    [Tooltip("Duración máxima del vuelo antes de quedarse en tierra")]
    [SerializeField, Min(0f)] private float maxFlyDuration = 0.7f;
    [Tooltip("Distancia mínima que vuelan las gotas")]
    [SerializeField, Min(0f)] private float minFlyStrength = 0.5f;
    [Tooltip("Distancia máxima que vuelan las gotas")]
    [SerializeField, Min(0f)] private float maxFlyStrength = 2.0f;

    [Header("Limpieza")]
    [Tooltip("Tiempo mínimo antes de destruir la gota ya en tierra")]
    [SerializeField, Min(0f)] private float minLifeTime = 1f;
    [Tooltip("Tiempo máximo antes de destruir la gota ya en tierra")]
    [SerializeField, Min(0f)] private float maxLifeTime = 3f;

    public override void CreateFeedback()
    {
        if (bloodPrefabs == null || bloodPrefabs.Length == 0) return;

        // Elige cantidad aleatoria
        int spawnCount = Random.Range(minSpawnCount, maxSpawnCount + 1);
        Vector3 origin = transform.position;

        for (int i = 0; i < spawnCount; i++)
        {
            // Prefab aleatorio
            var prefab = bloodPrefabs[Random.Range(0, bloodPrefabs.Length)];
            if (prefab == null) continue;

            // Instancia en origen
            var go = Instantiate(prefab, origin, Quaternion.identity);

            // Parametriza tween y destrucción individual
            float flyDuration = Random.Range(minFlyDuration, maxFlyDuration);
            float flyStrength = Random.Range(minFlyStrength, maxFlyStrength);
            float lifeTime = Random.Range(minLifeTime, maxLifeTime);

            MoveBlood(go, origin, flyDuration, flyStrength);
            Destroy(go, lifeTime);
        }
    }

    private void MoveBlood(GameObject go, Vector3 origin, float flyDuration, float flyStrength)
    {
        var t = go.transform;
        t.DOComplete(true);
        t.position = origin;
        t.rotation = Quaternion.identity;

        // Dirección aleatoria
        Vector2 dir = Random.insideUnitCircle.normalized;
        Vector2 target = (Vector2)origin + dir * flyStrength;

        // Movimiento
        t.DOMove(target, flyDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(go, LinkBehaviour.KillOnDestroy);

        // Rotación aleatoria
        t.DORotate(new Vector3(0, 0, Random.Range(0f, 360f)), flyDuration)
            .SetEase(Ease.Linear)
            .SetLink(go, LinkBehaviour.KillOnDestroy);
    }

    public override void CompletePreviousFeedback()
    {
        // Nada que limpiar
    }
}
