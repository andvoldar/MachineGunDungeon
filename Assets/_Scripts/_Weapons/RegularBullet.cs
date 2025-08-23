// RegularBullet.cs
using UnityEngine;
using FMODUnity;

public class RegularBullet : Bullet
{
    private Rigidbody2D rb;

    public override BulletDataSO bulletData
    {
        get => base.bulletData;
        set
        {
            base.bulletData = value;
            // Ajustamos el Rigidbody en cuanto recibimos bulletData
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
        // 1) Si el objeto es IHittable, le aplicamos daño y destruimos la bala


        // 2) Si colisiona contra un obstáculo “duro”
        if (collision.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            HitObstacle(collision);
            Destroy(gameObject);
            return;
        }

        // 3) Si colisiona contra un enemigo (aplicamos knockback además de daño)
        if (collision.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            HitEnemy(collision);
            Destroy(gameObject);
            return;
        }
    }

    private void HitObstacle(Collider2D collision)
    {
        // 1) Aplicar daño si puede recibirlo
        var hittable = collision.GetComponent<IHittable>();
        if (hittable != null)
            hittable.GetHit(bulletData.Damage, gameObject);

        // 2) Aplicar knockback si implementa IKnockbackable
        if (collision.TryGetComponent<IKnockbackable>(out var knockable) && bulletData.KnockbackPower > 0f)
        {
            // Dirección: desde la bala hacia el objeto
            Vector2 dir = ((Vector2)collision.transform.position - (Vector2)transform.position).normalized;
            knockable.ApplyKnockback(dir, bulletData.KnockbackPower, bulletData.KnockbackDuration);
        }

        // 3) Efecto visual en el obstáculo
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

    private void HitEnemy(Collider2D collision)
    {
        // 1) Daño
        if (collision.TryGetComponent<IHittable>(out var hittable))
            hittable.GetHit(bulletData.Damage, gameObject);

        // 2) Knockback — tomamos dirección y fuerza + duración del SO
        if (collision.TryGetComponent<IKnockbackable>(out var kb) && bulletData.KnockbackPower > 0f)
        {
            Vector2 dir = (collision.transform.position - transform.position).normalized;
            kb.ApplyKnockback(dir, bulletData.KnockbackPower, bulletData.KnockbackDuration);
        }

        // 3) Efecto visual
        Vector2 randomOffset = Random.insideUnitCircle * 0.5f;
        Instantiate(bulletData.ImpactEnemyPrefab,
                    collision.transform.position + (Vector3)randomOffset,
                    Quaternion.identity);
    }
}
