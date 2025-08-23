using UnityEngine;

public interface IKnockbackable
{
    void ApplyKnockback(Vector2 knockbackDirection, float force, float duration);
}
