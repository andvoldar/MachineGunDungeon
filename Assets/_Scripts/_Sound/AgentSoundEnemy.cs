using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using UnityEngine.Events;

public class AgentSoundEnemy : MonoBehaviour
{




    [SerializeField] private float soundDelay = 0.4f;

    public void PlayHitSound() =>
        SoundManager.Instance.PlaySound(SoundType.EnemyHit, transform.position);

    public void PlayAttackVoiceSound() =>
        SoundManager.Instance.PlaySound(SoundType.EnemyAttackVoice, transform.position);

    public void PlayMeleeAttackSound() =>
        SoundManager.Instance.PlaySound(SoundType.EnemyMeleeAttack, transform.position);

    public void PlayDeathVoiceSound() =>
        SoundManager.Instance.PlaySound(SoundType.EnemyDeathVoice, transform.position);

    public void PlayGetHitVoiceSound() =>
        SoundManager.Instance.PlaySound(SoundType.EnemyHitVoice, transform.position);

    public void PlayDeathVFXSound() =>
        StartCoroutine(DelayedDeathVFX());

    private IEnumerator DelayedDeathVFX()
    {
        yield return new WaitForSeconds(soundDelay);
        SoundManager.Instance.PlaySound(SoundType.EnemyDeathVFX, transform.position);
    }
}
