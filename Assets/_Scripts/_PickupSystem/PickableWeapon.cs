using UnityEngine;

public class PickableWeapon : MonoBehaviour, IInteractable
{
    [SerializeField] private WeaponDataSO weaponData;
    [SerializeField] private Transform visualTransform;

    private WeaponState weaponState;

    private void Start()
    {
        if (visualTransform != null)
            visualTransform.localPosition += new Vector3(0, Mathf.Sin(Time.time) * 0.1f, 0);
    }

    public void SetWeaponState(WeaponState state)
    {
        weaponState = state;
    }

    public WeaponState GetWeaponState()
    {
        return weaponState;
    }

    public void SetWeaponData(WeaponDataSO data, WeaponState state = null)
    {
        weaponData = data;
        weaponState = state != null ? state : new WeaponState(weaponData.AmmoCapacity, weaponData.AmmoCapacity);  // Asignar un estado por defecto si es null
    }

    public WeaponDataSO GetWeaponData() => weaponData;

    public void Interact(GameObject interactor)
    {
        var weaponHandler = interactor.GetComponent<WeaponHandler>();
        if (weaponHandler != null && weaponData != null)
        {
            if (weaponState == null)
            {
                var holder = GetComponent<WeaponStateHolder>();
                if (holder != null)
                    weaponState = holder.GetState();
            }

            weaponHandler.PickUpWeapon(weaponData, weaponState); // <- CORRECTO: usamos PickUpWeapon del nuevo sistema
            Destroy(gameObject); // El objeto dropeado se destruye después de transferir el arma
        }
    }


}
