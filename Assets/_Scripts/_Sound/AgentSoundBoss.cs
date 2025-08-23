using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using UnityEngine.Events;

public class AgentSoundBoss : MonoBehaviour
{




    [SerializeField] private float soundDelay = 0.4f;

    public void PlayHitSound() =>
        SoundManager.Instance.PlaySound(SoundType.EnemyHit, transform.position);

    public void PlayAttackVoiceSound() =>
        SoundManager.Instance.PlaySound(SoundType.MinotaurAttackSFX, transform.position);

    public void PlayMeleeAttackSound() =>
        SoundManager.Instance.PlaySound(SoundType.SwordAttackSFX, transform.position);

    public void PlayDistanceAttackSound() =>
        SoundManager.Instance.PlaySound(SoundType.MinotaurDistanceAttackSFX, transform.position);

    public void PlayDeathVoiceSound() =>
        SoundManager.Instance.PlaySound(SoundType.MinotaurDeathSFX, transform.position);

    public void PlayGetHitVoiceSound() =>
        SoundManager.Instance.PlaySound(SoundType.MinotaurGetHitSFX, transform.position);

    public void PlayDashSound() =>
    SoundManager.Instance.PlaySound(SoundType.PlayerDash, transform.position);

    public void PlayDeathVFXSound() =>
        StartCoroutine(DelayedDeathVFX());

    private IEnumerator DelayedDeathVFX()
    {
        yield return new WaitForSeconds(soundDelay);
        SoundManager.Instance.PlaySound(SoundType.EnemyDeathVFX, transform.position);
    }
}
