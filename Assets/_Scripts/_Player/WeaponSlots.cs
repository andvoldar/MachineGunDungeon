using UnityEngine;

public class WeaponSlots : MonoBehaviour
{
    private Transform weaponParent;
    private GameObject[] slots = new GameObject[2];
    private int currentIndex = 0;

    public GameObject CurrentWeaponGO => slots[currentIndex];
    public bool HasSecondary => slots[(currentIndex + 1) % 2] != null;
    public int SlotCount => (slots[0] != null ? 1 : 0) + (slots[1] != null ? 1 : 0);

    public void Initialize(Transform parent) => weaponParent = parent;

    public void EquipWeapon(WeaponDataSO data, WeaponState st = null)
    {
        if (slots[currentIndex] != null)
            Destroy(slots[currentIndex]);

        slots[currentIndex] = InstantiateWeapon(data, st);
    }

    public void AddWeaponToNextSlot(WeaponDataSO data, WeaponState st = null)
    {
        int nxt = (currentIndex + 1) % 2;
        if (slots[nxt] != null)
            Destroy(slots[nxt]);

        slots[nxt] = InstantiateWeapon(data, st);
        slots[nxt].SetActive(false);
        SwapWeapon();
    }

    public void SwapWeapon()
    {
        int other = (currentIndex + 1) % 2;
        if (slots[currentIndex] == null || slots[other] == null)
            return;

        slots[currentIndex].SetActive(false);
        currentIndex = other;
        slots[currentIndex].SetActive(true);
    }

    public void EquipSecondaryWeapon()
    {
        SwapWeapon();
    }

    public void DropCurrentWeapon()
    {
        var curGO = slots[currentIndex];
        if (curGO == null) return;

        // 1) Extraemos datos y estado
        WeaponDataSO data;
        WeaponState state;
        if (curGO.TryGetComponent<Weapon>(out var w))
        {
            data = w.GetWeaponData();
            state = w.GetCurrentState();
        }
        else if (curGO.TryGetComponent<MeleeWeapon>(out var mw))
        {
            data = mw.GetWeaponData();
            state = mw.GetCurrentState();
        }
        else
        {
            Debug.LogError($"No sé dropear {curGO.name}: ni Weapon ni MeleeWeapon");
            return;
        }

        // 2) Calcula posición de instanciación
        Vector2 dir = (Camera.main.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        Vector3 spawnPos = transform.position + (Vector3)dir * 0.5f;

        // 3) Instancia el pickup con rotación neutra
        var pickup = Instantiate(data.WeaponPrefab, spawnPos, Quaternion.identity);
        pickup.tag = "Pickup";
        pickup.layer = LayerMask.NameToLayer("Interactable");

        // 4) Transfiere el estado
        if (pickup.TryGetComponent<WeaponStateHolder>(out var holder))
            holder.SetState(state);
        else
            pickup.AddComponent<WeaponStateHolder>().SetState(state);

        // 5) Asigna datos al componente correspondiente
        if (pickup.TryGetComponent<Weapon>(out var w2))
        {
            w2.AssignWeaponData(data);
            w2.LoadState(state);
            w2.SetEquipped(false);
        }
        else if (pickup.TryGetComponent<MeleeWeapon>(out var mw2) && data is MeleeWeaponDataSO meleeData)
        {
            mw2.AssignWeaponData(meleeData);
            mw2.LoadState(state);
            mw2.SetEquipped(false);
        }

        // 6) Física y throw
        if (pickup.TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb.isKinematic = false;
            rb.velocity = Vector2.zero;
        }
        if (pickup.TryGetComponent<DroppedWeaponVisuals>(out var dv))
        {
            dv.Throw(dir);
        }

        // 7) Limpia slot actual y auto-equip secondary
        Destroy(curGO);
        slots[currentIndex] = null;

        // Si había otro arma en la otra ranura, equiparla automáticamente
        int other = (currentIndex + 1) % 2;
        if (slots[other] != null)
        {
            currentIndex = other;
            slots[currentIndex].SetActive(true);
        }
    }

    private GameObject InstantiateWeapon(WeaponDataSO data, WeaponState state)
    {
        var go = Instantiate(
            data.WeaponPrefab,
            weaponParent.position,
            weaponParent.rotation,
            weaponParent
        );
        go.transform.localPosition = Vector3.zero;
        go.tag = "Untagged";

        // Si es arma de fuego
        if (go.TryGetComponent<Weapon>(out var w))
        {
            w.AssignWeaponData(data);
            if (state != null)
                w.LoadState(state);
            else
            {
                var h = go.GetComponent<WeaponStateHolder>() ?? go.AddComponent<WeaponStateHolder>();
                h.InitializeState(w);
                w.LoadState(h.GetState());
            }
            w.SetEquipped(true);
        }
        // Si es arma melee
        else if (go.TryGetComponent<MeleeWeapon>(out var mw) && data is MeleeWeaponDataSO md)
        {
            mw.AssignWeaponData(md);
            if (state != null) mw.LoadState(state);
            else mw.LoadState(null);
            mw.SetEquipped(true);
        }
        else
        {
            Debug.LogError($"Prefab {data.name} no tiene ni Weapon ni MeleeWeapon!");
        }

        if (go.TryGetComponent<Rigidbody2D>(out var rb))
            rb.isKinematic = true;

        return go;
    }
}
