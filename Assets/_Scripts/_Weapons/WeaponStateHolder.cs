using UnityEngine;

public class WeaponStateHolder : MonoBehaviour
{
    public WeaponState StoredState;

    // Guardar el estado
    public void SetState(WeaponState state)
    {
        StoredState = state;
    }

    // Obtener el estado
    public WeaponState GetState()
    {
        return StoredState;
    }

    // Inicializar el estado con el número máximo de balas (cuando es la primera recogida)
    public void InitializeState(Weapon weapon)
    {
        if (StoredState == null)
        {
            StoredState = new WeaponState(weapon.Ammo, weapon.weaponData.AmmoCapacity);
        }
    }
}
