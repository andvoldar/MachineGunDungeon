using UnityEngine;
using FMODUnity;

public class MinotaurProjectile : Bullet
{
    private Rigidbody2D rb;

    public override BulletDataSO bulletData
    {
        get => base.bulletData;
        set
        {
            base.bulletData = value;
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.drag = bulletData.Friction;
        }
    }

    private void FixedUpdate()
    {
        if (rb != null && bulletData != null)
        {
            Vector2 delta = bulletData.BulletSpeed * Time.fixedDeltaTime * (Vector2)transform.right;
            rb.MovePosition(rb.position + delta);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Impacto con obstáculo
        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            HitObstacle(collision);
            Destroy(gameObject);
            return;
        }

        // Impacto con el jugador
        if (collision.CompareTag("Player"))
        {
            HitPlayer(collision);
            Destroy(gameObject);
        }
    }

    private void HitObstacle(Collider2D collision)
    {
        if (collision.TryGetComponent<IHittable>(out var hittable))
            hittable.GetHit(bulletData.Damage, gameObject);

        if (collision.TryGetComponent<IKnockbackable>(out var knockable) && bulletData.KnockbackPower > 0f)
        {
            Vector2 dir = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;
            knockable.ApplyKnockback(dir, bulletData.KnockbackPower, bulletData.KnockbackDuration);
        }

        RaycastHit2D hit = Physics2D.Raycast(transform.position,
                                             transform.right,
                                             1f,
                                             bulletData.bulletLayerMask);
        if (hit.collider != null)
        {
            Instantiate(bulletData.ImpactObstaclePrefab, hit.point, Quaternion.identity);
            RuntimeManager.PlayOneShot(bulletData.obstacleImpactSound, hit.point);
        }
    }

    private void HitPlayer(Collider2D collision)
    {
        if (collision.TryGetComponent<IHittable>(out var hittable))
            hittable.GetHit(bulletData.Damage, gameObject);

        if (collision.TryGetComponent<IKnockbackable>(out var kb) && bulletData.KnockbackPower > 0f)
        {
            Vector2 dir = (collision.transform.position - transform.position).normalized;
            kb.ApplyKnockback(dir, bulletData.KnockbackPower, bulletData.KnockbackDuration);
        }

        Instantiate(bulletData.ImpactEnemyPrefab,
                    collision.transform.position + (Vector3)(Random.insideUnitCircle * 0.5f),
                    Quaternion.identity);
    }
}
