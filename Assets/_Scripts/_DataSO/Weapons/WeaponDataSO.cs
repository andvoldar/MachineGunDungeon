using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;

[CreateAssetMenu(menuName = "Weapons/WeaponData")]

public class WeaponDataSO : ScriptableObject

{

    [field: SerializeField] public Sprite PickupSprite { get; private set; }
    [field: SerializeField] public GameObject WeaponPrefab { get; private set; }


    [field: SerializeField]
    public BulletDataSO BulletData { get; set; }

    [field: SerializeField]
    [field: Range(0, 10000)]
    public int AmmoCapacity { get; set; } = 10000;


    [field: SerializeField]
    public bool AutomaticFire { get; set; } = false;


    [field: SerializeField]
    [field: Range(0.1f, 2f)]
    public float WeaponDelay { get; set; } = .1f;


    [field: SerializeField]
    [field: Range(0, 100)]
    public float SpreadAngle { get; set; } = 5;

    [field: SerializeField]
    [field: Range(0, 100)]
    public float SpreadAngleRandomizer { get; set; } = 10;

    [SerializeField]
    private bool multiBulletShoot = false;


    [SerializeField]
    [Range(1, 10)]
    private int bulletCount = 1;

    internal int GetBulletCountToSpawn()
    {
        if (multiBulletShoot)
        {
            return bulletCount;
        }
        return 1;
    }

    [field: SerializeField]
    [Header("FMOD Settings")]
    public EventReference gunSoundFMOD;


}