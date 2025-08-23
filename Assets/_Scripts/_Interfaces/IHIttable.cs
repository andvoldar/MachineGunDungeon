using UnityEngine;
using UnityEngine.Events;

public interface IHittable
{
    UnityEvent OnGetHit { get; set; }
    // Se agrega la firma que incluye los parámetros para knockback.
    void GetHit(int damage, GameObject damageDealer);
}
    