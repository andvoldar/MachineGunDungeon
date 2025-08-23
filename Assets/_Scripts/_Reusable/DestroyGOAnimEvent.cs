
using UnityEngine;

public class DestroyGOAnimEvent : MonoBehaviour
{
    [SerializeField] private float lifetime = 0.2f;

    public void CallDestroyGO()
    {
        Destroy(gameObject,lifetime);
    }
}