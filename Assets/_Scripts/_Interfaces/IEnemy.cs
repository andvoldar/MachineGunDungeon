using UnityEngine;
using UnityEngine.Events;

public interface IEnemy
{
    int Health { get; }
    UnityEvent OnDeath { get; set; }
    UnityEvent OnGetHit { get; set; }

}