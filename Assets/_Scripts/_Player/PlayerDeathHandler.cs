using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;

public class PlayerDeathHandler : MonoBehaviour
{
    [field: SerializeField] public UnityEvent OnDeath { get; set; }

    [Header("Comportamientos a desactivar")]
    [SerializeField] private MonoBehaviour[] componentsToDisable;

    [Header("Sprites a ocultar")]
    [SerializeField] private SpriteRenderer[] spritesToHide;

    [Header("Colliders a desactivar")]
    [SerializeField] private Collider2D[] collidersToDisable;

    [Header("Weapon Visual")]
    [SerializeField] private GameObject weaponParent;

    [Header("Disolución Visual")]
    [SerializeField] private AgentRenderer agentRenderer;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.4f;

    private PlayerController playerController;
    private bool isDead = false;

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();
        if (agentRenderer == null)
            agentRenderer = GetComponentInChildren<AgentRenderer>();
    }

    public void HandleDeath()
    {
        if (isDead) return;
        isDead = true;

        OnDeath?.Invoke();
        playerController?.StopMovement();

        // Suavemente reducir velocidad si se mueve aún
        DOVirtual.DelayedCall(0.1f, () => {
            foreach (var comp in componentsToDisable)
                if (comp != null) comp.enabled = false;
        });

        // Desactivar colisiones
        foreach (var col in collidersToDisable)
            if (col != null) col.enabled = false;

        // Fade out sprites
        foreach (var sprite in spritesToHide)
        {
            if (sprite != null)
                sprite.DOFade(0, fadeDuration).SetEase(Ease.InQuad);
        }

        // Desactivar arma suavemente
        if (weaponParent != null)
        {
            weaponParent.transform.DOScale(Vector3.zero, fadeDuration).SetEase(Ease.InBack).SetLink(weaponParent, LinkBehaviour.KillOnDestroy).OnComplete(() =>
            {

        if (weaponParent != null)
            weaponParent.SetActive(false);

             });
        }

        // Ralentizar tiempo
        Time.timeScale = 0.1f;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        StartCoroutine(DeathSequence());
    }

    private IEnumerator DeathSequence()
    {
        yield return new WaitForSecondsRealtime(0.4f);

        if (agentRenderer != null)
            agentRenderer.PlayDissolveFlash();

        yield return new WaitForSecondsRealtime(1.6f);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;

        if (this != null) // Por si ya fue destruido antes
            Destroy(gameObject);
    }
}
