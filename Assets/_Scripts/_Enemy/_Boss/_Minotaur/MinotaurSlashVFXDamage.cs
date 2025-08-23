using UnityEngine;

public class MinotaurSlashVFXDamage : MonoBehaviour
{
    public int damage = 25;
    public float lifetime = 0.3f;

    void Start()
    {
        // si el ángulo de rotación indica que estamos mirando a la izquierda
        if (transform.eulerAngles.z > 90f && transform.eulerAngles.z < 270f)
        {
            Vector3 scale = transform.localScale;
            scale.y *= -1f;
            transform.localScale = scale;
        }

        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            IHittable hittable = collision.GetComponent<IHittable>();
            if (hittable != null)
            {
                hittable.GetHit(damage, gameObject);
            }
        }
    }
}
