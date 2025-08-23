[System.Serializable]
public class WeaponState
{
    public int ammo;
    public int maxAmmo;

    // Constructor para inicializar los valores de ammo y maxAmmo
    public WeaponState(int ammo, int maxAmmo)
    {
        this.ammo = ammo;
        this.maxAmmo = maxAmmo;
    }
}