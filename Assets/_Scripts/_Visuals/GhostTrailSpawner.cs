using UnityEngine;

public class GhostTrailSpawner : MonoBehaviour
{
    [SerializeField] private GameObject ghostTrailPrefab;
    [SerializeField] private float trailInterval = 0.05f;
    [SerializeField] private float trailLifetime = 0.5f;

    private SpriteRenderer spriteRenderer;
    private float timer;
    private bool isSpawning;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (!isSpawning) return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            SpawnGhost();
            timer = trailInterval;
        }
    }

    private void SpawnGhost()
    {
        GameObject ghost = Instantiate(ghostTrailPrefab, transform.position, Quaternion.identity);
        GhostTrail ghostScript = ghost.GetComponent<GhostTrail>();
        ghostScript.Init(
            spriteRenderer.sprite,
            transform.position,
            transform.localScale,
            spriteRenderer.flipX,
            spriteRenderer.material // Mantiene el material (dissolve, etc.)
        );
        Destroy(ghost, trailLifetime);
    }

    public void StartSpawning()
    {
        isSpawning = true;
        timer = 0;
    }

    public void StopSpawning()
    {
        isSpawning = false;
    }
}
