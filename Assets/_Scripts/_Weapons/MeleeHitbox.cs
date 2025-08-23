// Assets/Scripts/Weapons/MeleeHitbox.cs
using UnityEngine;
using FMODUnity;

[RequireComponent(typeof(Collider2D))]
public class MeleeHitbox : MonoBehaviour
{
    private Collider2D _collider;
    private MeleeWeapon _weaponDataComp;
    private MeleeWeaponDataSO _data;

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _collider.enabled = false;
    }

    // Esperamos a que todos los Awakes de los padres se ejecuten
    // Se inicializa al habilitar la hitbox
    private bool _initialized = false;
    private void OnEnable()
    {
        if (!_initialized)
        {
            _weaponDataComp = GetComponentInParent<MeleeWeapon>();

            if (_weaponDataComp)
            {
                _data = _weaponDataComp.weaponData;
                _initialized = true;
            }
        }
        else
        {
            _data = _weaponDataComp.weaponData;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_collider.enabled) return;

        // Verificar si el objeto tiene la etiqueta DestructibleObject


        // Verificar si es un enemigo u otro objeto que se puede golpear
        if (other.TryGetComponent<IHittable>(out var hittable))
        {
            // Aplicar daño
            hittable.GetHit((int)_data.Damage, _weaponDataComp.gameObject);

            // Knockback
            if (other.TryGetComponent<IKnockbackable>(out var kb))
            {
                Vector2 dir = (other.transform.position - transform.position).normalized;
                kb.ApplyKnockback(dir, _data.KnockbackForce, _data.KnockbackDuration);
            }


            if (other.CompareTag("DestructibleObject"))
            {
                // Reproducir sonido específico para objetos destructibles
                SoundManager.Instance.PlaySound(SoundType.HitDestructibleSFX, transform.position);
                return; // Opcional: salir para que no continúe con el resto del código si no aplica
            }
            else
            // Sonido de impacto para enemigos
            RuntimeManager.PlayOneShot(_data.HitSound, transform.position);
        }
    }


    /// <summary>
    /// Habilita o deshabilita la ventana de daño.
    /// </summary>
    public void SetEnabled(bool on)
{
    if (_collider != null)
        _collider.enabled = on;
}
}
