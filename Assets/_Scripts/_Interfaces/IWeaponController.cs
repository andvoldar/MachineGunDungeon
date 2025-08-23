// IWeaponController.cs
using UnityEngine;

public interface IWeaponController
{
    /// <summary>Apunta el arma hacia la posición del puntero.</summary>
    void AimWeapon(Vector2 pointerPosition);

    /// <summary>Se llama cuando el usuario pulsa Fire1.</summary>
    void HandleTriggerPressed();

    /// <summary>Se llama cuando el usuario suelta Fire1.</summary>
    void HandleTriggerReleased();



    void HandleAltPressed();
    void HandleAltReleased();


    /// <summary>Detiene TODO feedback / animaciones activas.</summary>
    /// 
    void StopAllFire();

    /// <summary>Resetea cooldowns, corrutinas, etc.</summary>
    void FullReset();
}
