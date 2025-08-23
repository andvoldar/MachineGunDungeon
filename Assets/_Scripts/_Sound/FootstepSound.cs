// Assets/Scripts/FootstepSound.cs
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class FootstepSound : MonoBehaviour
{
    public void PlayFootstepSound() =>SoundManager.Instance.PlaySound(SoundType.PlayerFootstep, transform.position);
    public void PlayEnemyFootstepSound() => SoundManager.Instance.PlaySound(SoundType.EnemyFootstep, transform.position);
}