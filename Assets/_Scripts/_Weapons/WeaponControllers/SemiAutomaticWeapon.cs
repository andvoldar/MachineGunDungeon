// SemiAutomaticWeaponController.cs
using UnityEngine;

public class SemiAutomaticWeaponController : WeaponController
{
    public override void HandleTriggerPressed()
    {
        FireCore();
    }

    public override void HandleTriggerReleased()
    {
        // Sin lógica adicional
    }
}
