// IWeaponAbility.cs
public interface IWeaponAbility
{
    void OnAbilityPressed();    // comienza carga
    void OnAbilityReleased();   // soltar antes de completar
    void CancelAbility();       // fuerza cancelar
}
