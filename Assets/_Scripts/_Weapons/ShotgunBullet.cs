using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

public class ShotgunBullet : Bullet, IKnockbackable
{
    protected Rigidbody2D rb;

    public override BulletDataSO bulletData
    {
        get => base.bulletData;
        set
        {
            base.bulletData = value;
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.drag = bulletData.Friction;
            }
        }
    }

    private void FixedUpdate()
    {
        if (rb != null && bulletData != null)
        {
            rb.MovePosition(transform.position + bulletData.BulletSpeed * transform.right * Time.fixedDeltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            HitObstacle(collision);
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            HitEnemy(collision);
            Destroy(gameObject);
        }
    }

    void HitObstacle(Collider2D collision)
    {
        // 1) Daño (si es IHittable)
        if (collision.TryGetComponent<IHittable>(out var hittable))
            hittable.GetHit(bulletData.Damage, gameObject);

        // 2) Knockback (si es IKnockbackable)
        if (collision.TryGetComponent<IKnockbackable>(out var kb) && bulletData.KnockbackPower > 0f)
        {
            Vector2 dir = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;
            kb.ApplyKnockback(dir, bulletData.KnockbackPower, bulletData.KnockbackDuration);
        }

        // 3) Efecto visual
        RaycastHit2D hit = Physics2D.Raycast(transform.position,
                                             transform.right,
                                             1f,
                                             bulletData.bulletLayerMask);
        if (hit.collider != null)
        {
            Instantiate(bulletData.ImpactObstaclePrefab, hit.point, Quaternion.identity);
            PlayObstacleHitSound();
        }
    }


    void HitEnemy(Collider2D collision)
    {
        // Daño
        var hittable = collision.GetComponent<IHittable>();
        if (hittable != null && bulletData.Damage > 0)
        {
            hittable.GetHit(bulletData.Damage, gameObject);
        }

        // Knockback
        var knockable = collision.GetComponent<IKnockbackable>();
        if (knockable != null && bulletData.KnockbackPower > 0)
        {
            Vector2 direction = (collision.transform.position - transform.position).normalized;
            knockable.ApplyKnockback(direction, bulletData.KnockbackPower, bulletData.KnockbackDuration);
        }

        Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
        Instantiate(bulletData.ImpactEnemyPrefab, collision.transform.position + (Vector3)randomOffset, Quaternion.identity);
    }


    public void ApplyKnockback(Vector2 knockbackDirection, float force, float duration)
    {
        throw new System.NotImplementedException();
    }

    public void PlayObstacleHitSound()
    {
        RuntimeManager.PlayOneShot(bulletData.obstacleImpactSound, transform.position);
    }
}
