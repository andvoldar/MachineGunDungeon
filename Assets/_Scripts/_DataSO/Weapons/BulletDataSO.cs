using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
[CreateAssetMenu(menuName = ("Weapons/BulletData"))]

public class BulletDataSO : ScriptableObject
{
    [field: SerializeField]
    public GameObject bulletPrefab { get; set; }


    [field: SerializeField]
    [field: Range(1,100)]
    public float BulletSpeed { get; internal set; } = 1;


    
    [field: SerializeField]
    [field: Range(1, 10)]
    public int Damage { get; set; }


    [field: SerializeField]
    [field: Range(0, 100)]
    public float Friction { get; internal set; } = 0;


    [field: SerializeField]
    public bool Bounce { get; set; } = false;

    [field: SerializeField]
    public bool GoThroughHittable { get; set; } = false;


    [field: SerializeField]
    public bool IsRaycast { get; set; } = false;


    [field: SerializeField]
    public GameObject ImpactObstaclePrefab { get; set; }

    [field: SerializeField]
    public GameObject ImpactEnemyPrefab { get; set; }


    [field: SerializeField, Range(1, 500000)]
    public int KnockbackPower { get; set; } = 50;

    [field: SerializeField, Range(0.01f, 1f)]
    public float KnockbackDuration { get; set; } = 0.1f;


    [field: SerializeField]
    public EventReference obstacleImpactSound;


    [field: SerializeField]
    public LayerMask bulletLayerMask { get; set; }

}
