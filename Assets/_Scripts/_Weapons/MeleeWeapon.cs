using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class MeleeWeapon : MonoBehaviour
{
    [SerializeField] public GameObject hitboxPivot;
    [SerializeField] public Collider2D hitbox;
    [SerializeField] public MeleeWeaponDataSO weaponData;

    [field: SerializeField] public UnityEvent OnSwing { get; private set; }
    [field: SerializeField] public UnityEvent OnHit { get; private set; }

    private WeaponState currentState;
    private float lastAttackTime = -999f;

    public bool CanAttack => Time.time >= lastAttackTime + weaponData.Cooldown;

    public void StartSwing()
    {
        lastAttackTime = Time.time;
        OnSwing?.Invoke();
    }

    public MeleeWeaponDataSO GetWeaponData() => weaponData;

    private void Start()
    {
        if (weaponData != null && currentState == null)
            LoadState(null);

        if (TryGetComponent(out WeaponStateHolder h) && h.StoredState == null)
            h.SetState(GetCurrentState());
    }

    public void AssignWeaponData(MeleeWeaponDataSO data) => weaponData = data;

    public void LoadState(WeaponState state)
    {
        currentState = state ?? new WeaponState(0, 0);
    }

    public WeaponState GetCurrentState() => new WeaponState(0, 0);

    public void SetEquipped(bool equipped)
    {
        if (TryGetComponent(out DroppedWeaponVisuals v))
            v.SetCanPickup(!equipped);

        if (TryGetComponent(out ChargedSpinBladeAbility spinAbility))
            spinAbility.enabled = equipped;
    }
}
