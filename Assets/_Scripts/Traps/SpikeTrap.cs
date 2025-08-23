using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Animator))]
public class SpikeTrap : MonoBehaviour
{
    [Tooltip("Daño por segundo mientras los pinchos están arriba")]
    public float damagePerSecond = 20f;

    [Header("Rango de tiempo expuesto (en segundos)")]
    [SerializeField] private float minStayTime = 1.5f;
    [SerializeField] private float maxStayTime = 2.2f;

    [Header("Rango de tiempo escondido (en segundos)")]
    [SerializeField] private float minHideTime = 1.5f;
    [SerializeField] private float maxHideTime = 2.2f;

    [Header("Nombres de clips en el Animator")]
    [SerializeField] private string showAnimName = "SpikesShowing";
    [SerializeField] private string hideAnimName = "SpikesHiding";

    private Collider2D dmgCollider;
    private Animator animator;
    private bool canDamage;
    private float damageAccumulator;

    void Awake()
    {
        dmgCollider = GetComponent<Collider2D>();
        animator = GetComponent<Animator>();
        dmgCollider.isTrigger = true;
        dmgCollider.enabled = false;
    }

    void Start()
    {
        StartTrap();
    }

    public void StartTrap()
    {
        canDamage = false;
        damageAccumulator = 0f;
        dmgCollider.enabled = false;
        animator.Play(showAnimName, 0, 0f);
    }

    // ► Animation Event al final de SpikesShowing
    public void ActivateDamage()
    {
        canDamage = true;
        dmgCollider.enabled = true;
        damageAccumulator = 0f;

        StopAllCoroutines();
        StartCoroutine(DisableDamageAndHide());
    }

    private IEnumerator DisableDamageAndHide()
    {
        // esperamos un tiempo aleatorio antes de bajar
        float stay = Random.Range(minStayTime, maxStayTime);
        yield return new WaitForSeconds(stay);

        canDamage = false;
        dmgCollider.enabled = false;

        animator.Play(hideAnimName, 0, 0f);
    }

    // ► Animation Event al final de SpikesHiding
    public void OnHideAnimationEnd()
    {
        StopAllCoroutines();
        StartCoroutine(ShowAgainAfterDelay());
    }

    private IEnumerator ShowAgainAfterDelay()
    {
        // esperamos un tiempo aleatorio antes de subir
        float hide = Random.Range(minHideTime, maxHideTime);
        yield return new WaitForSeconds(hide);

        animator.Play(showAnimName, 0, 0f);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        if (!canDamage) return;

        var hittable = other.GetComponent<IHittable>()
                       ?? other.GetComponentInParent<IHittable>();
        if (hittable == null) return;

        damageAccumulator += damagePerSecond * Time.deltaTime;
        if (damageAccumulator >= 1f)
        {
            int dmg = Mathf.FloorToInt(damageAccumulator);
            damageAccumulator -= dmg;

            hittable.GetHit(dmg, gameObject);
        }
    }
}
