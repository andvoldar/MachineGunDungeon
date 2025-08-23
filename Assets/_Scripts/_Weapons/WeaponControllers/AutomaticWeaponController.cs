
using System.Collections;
using UnityEngine;

public class AutomaticWeaponController : WeaponController
{
    private bool isHolding = false;

    public override void HandleTriggerPressed()
    {
        if (!isHolding)
        {
            isHolding = true;
            StartCoroutine(FireLoop());
        }
    }

    public override void HandleTriggerReleased()
    {
        isHolding = false;
        StopAllFire();
    }

    private IEnumerator FireLoop()
    {
        while (isHolding)
        {
            FireCore();
            yield return new WaitForSeconds(weapon.weaponData.WeaponDelay);
        }
    }

    public override void StopAllFire()
    {
        StopAllCoroutines();   // corta el bucle
    }


    public bool IsFacingLeft()
    {
        // Si el ángulo de rotación del arma en el eje Z es mayor a 90 grados, consideramos que el arma está mirando a la izquierda
        return transform.localEulerAngles.z > 90f && transform.localEulerAngles.z < 270f;
    }
}
