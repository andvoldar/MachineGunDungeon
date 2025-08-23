using UnityEngine;
using FMODUnity;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]

public class AgentSoundPlayer : MonoBehaviour
{




    private void PlayFootstep() =>
        SoundManager.Instance.PlaySound(SoundType.PlayerFootstep, transform.position);

    public void PlayHitSound()
    {
        SoundManager.Instance.PlaySound(SoundType.PlayerHitVoice, transform.position);
        SoundManager.Instance.PlaySound(SoundType.PlayerHitSFX, transform.position);
    }

    public void PlayDeathSound() =>
        SoundManager.Instance.PlaySound(SoundType.PlayerDeath, transform.position);

    public void PlayDashEffectSound() =>
        SoundManager.Instance.PlaySound(SoundType.PlayerDash, transform.position);

    public void PlaySlowMotionSound() =>
        SoundManager.Instance.PlaySound(SoundType.PlayerSlowMotion, transform.position);

    public void PlayPickupWeaponSFX() => SoundManager.Instance.PlaySound(SoundType.PickupWeaponSFX, transform.position);

    public void PlayDropWeaponSFX() => SoundManager.Instance.PlaySound(SoundType.DropWeaponSFX, transform.position);

    public void PlayPickupItemSFX() => SoundManager.Instance.PlaySound(SoundType.PickupItemSFX, transform.position);

    public void PlayPrepareThrowSFX() => SoundManager.Instance.PlaySound(SoundType.GrenadePrepareThrow, transform.position);

    public void PlayThrowSFX() => SoundManager.Instance.PlaySound(SoundType.GrenadeThrow, transform.position);

}
