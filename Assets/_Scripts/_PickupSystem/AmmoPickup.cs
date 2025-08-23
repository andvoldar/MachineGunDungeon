// AmmoPickup.cs
using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int ammoAmount = 30;                // Cantidad de balas que da este pickup
    [SerializeField] private bool destroyOnPickup = true;
    [SerializeField] private GameObject pickupVFX;                // VFX opcional al recoger

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // 1) Intentamos obtener el WeaponController de arma equipada
        //    Como cada arma equipada lleva un componente 'Weapon' en algún hijo del jugador,
        //    usamos GetComponentInChildren<Weapon>() para encontrarla.
        if (!other.TryGetComponent(out WeaponHandler weaponHandler)) return;

        // Sacamos el Weapon (componente) de entre los hijos: será el arma actualmente equipada
        Weapon currentWeapon = weaponHandler.GetComponentInChildren<Weapon>();
        if (currentWeapon == null) return;

        // 2) Si el arma ya está al máximo de munición, no hacemos nada
        int maxCap = currentWeapon.weaponData.AmmoCapacity;  // AmmoCapacity definido en WeaponDataSO :contentReference[oaicite:0]{index=0}
        if (currentWeapon.Ammo >= maxCap) return;

        // 3) Calculamos la munición resultante (sin pasarnos del máximo)
        currentWeapon.Ammo = Mathf.Min(currentWeapon.Ammo + ammoAmount, maxCap);

        // 4) Actualizamos el WeaponStateHolder, si existe, para mantener el estado de cara a dropear/guardar
        WeaponStateHolder stateHolder = currentWeapon.GetComponent<WeaponStateHolder>();
        if (stateHolder != null)
        {
            // Creamos un nuevo estado con la munición actualizada
            WeaponState updatedState = new WeaponState(currentWeapon.Ammo, maxCap);
            stateHolder.SetState(updatedState);
        }

        // 5) Reproducir sonido o VFX de recogida
        SoundManager.Instance.PlaySound(SoundType.PickupAmmoSFX, transform.position);

        if (pickupVFX != null)
            Instantiate(pickupVFX, transform.position, Quaternion.identity);

        // 6) Destruir el pickup si corresponde
        if (destroyOnPickup)
            Destroy(gameObject);
    }
}
