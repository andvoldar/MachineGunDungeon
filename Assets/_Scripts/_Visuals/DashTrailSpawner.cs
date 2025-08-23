using UnityEngine;

public class DustTrailSpawner : MonoBehaviour
{
    [Header("Prefab de polvo")]
    [SerializeField] private GameObject dustPrefab;
    [Tooltip("Distancia detrás del jugador según dirección de dash")]
    [SerializeField] private float offsetDistance = 0.5f;

    private DashAbility dashAbility;
    private Transform playerRoot;

    private void Awake()
    {
        // Buscamos el DashAbility en los padres
        dashAbility = GetComponentInParent<DashAbility>();
        if (dashAbility == null)
        {
            Debug.LogWarning("DustTrailSpawner: no encontré DashAbility en los padres.");
            enabled = false;
            return;
        }
        // Suscribimos
        dashAbility.OnDashStarted.AddListener(SpawnDust);
        playerRoot = dashAbility.transform;
    }

    private void OnDestroy()
    {
        if (dashAbility != null)
            dashAbility.OnDashStarted.RemoveListener(SpawnDust);
    }

    private void SpawnDust()
    {
        if (dustPrefab == null) return;

        // Dirección de dash que acabamos de exponer
        Vector2 dir = dashAbility.LastDashDirection;
        if (dir == Vector2.zero) return;

        // Punto de spawn detrás del jugador
        Vector3 spawnPos = playerRoot.position - (Vector3)dir * offsetDistance;

        // Instanciaremos sin parent para evitar heredar escalas raras
        GameObject dust = Instantiate(dustPrefab, spawnPos, Quaternion.identity);

        // Rotamos de forma que el polvo "mire" en la dirección del dash
        // (o ajusta -dir si prefieres que apunte hacia atrás)
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        dust.transform.rotation = Quaternion.Euler(0, 0, angle);
    }
}
