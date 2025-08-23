using UnityEngine;
using DG.Tweening;
using FMODUnity;

public class BulletShellFeedback : MonoBehaviour
{
    [Header("Bullet Shell Settings")]
    [SerializeField] private GameObject bulletShellPrefab;
    [SerializeField] private float flyDuration = 0.3f;
    [SerializeField] private float flyStrength = 1f;


    [Header("FMOD Settings")]
    [SerializeField] private EventReference bulletShellSound;

    public void SpawnBulletShell()
    {
        GameObject shell = Instantiate(bulletShellPrefab, transform.position, Quaternion.identity);
        MoveShell(shell);
    }

    private void MoveShell(GameObject shell)
    {
        if (shell == null) return;

        shell.transform.DOComplete(true);
        shell.transform.position = transform.position;
        shell.transform.rotation = Quaternion.identity;

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        if (randomDir.y > 0) randomDir.y *= -1;
        Vector2 target = (Vector2)transform.position + randomDir * flyStrength;

        // Guardamos la posición desde ahora por seguridad
        Vector3 soundPos = transform.position;

        // Movimiento
        shell.transform.DOMove(target, flyDuration)
            .SetEase(Ease.OutQuad)
            .SetLink(shell, LinkBehaviour.KillOnDestroy)
            .OnComplete(() =>
            {
                if (!bulletShellSound.IsNull)
                    SoundManager.Instance.PlaySound(SoundType.BulletShell, soundPos); ; // usamos posición segura
        });

        // Rotación
        shell.transform.DORotate(new Vector3(0, 0, Random.Range(0f, 360f)), flyDuration)
            .SetEase(Ease.Linear)
            .SetLink(shell, LinkBehaviour.KillOnDestroy);
    }

}
