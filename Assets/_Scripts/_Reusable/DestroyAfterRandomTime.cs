using UnityEngine;

public class DestroyAfterRandomTime : MonoBehaviour
{
    private void OnEnable()
    {
        float delay = Random.Range(5f, 7f);
        Invoke(nameof(DestroySelf), delay);
    }

    private void DestroySelf()
    {
        Destroy(gameObject);
    }

    private void OnDisable()
    {
        CancelInvoke(); // Por si se regresa al pool antes de tiempo
    }
}
