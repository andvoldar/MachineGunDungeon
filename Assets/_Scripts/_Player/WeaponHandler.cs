using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// Gestiona la lógica de recoger, intercambiar (swap) y dropear armas.
/// Dispara OnWeaponChanged(Weapon) cada vez que la arma actual cambia.
/// </summary>
public class WeaponHandler : MonoBehaviour
{
    [SerializeField] private Transform weaponParent;
    [SerializeField] private float swapDelay = 0.15f;

    private WeaponSlots slots;
    private AgentInput input;
    private IWeaponController currentCtrl;
    private AgentSoundPlayer audioPlayer;
    private Vector2 lastPointer;
    private bool wantFire;

    private SwapController swaper;

    /// <summary>
    /// Se dispara cada vez que cambia la “Current Weapon”.
    /// Pasa la instancia de Weapon recién equipada (o null si no hay arma).
    /// </summary>
    public Action<Weapon> OnWeaponChanged;

    private void Awake()
    {
        slots = gameObject.AddComponent<WeaponSlots>();
        slots.Initialize(weaponParent);

        input = GetComponent<AgentInput>();
        audioPlayer = GetComponentInChildren<AgentSoundPlayer>();

        input.OnPointerPositionChange.AddListener(OnPointerMoved);
        input.OnFireButtonPressed.AddListener(OnFirePressed);
        input.OnFireButtonReleased.AddListener(OnFireReleased);
        input.OnSwapWeaponPressed.AddListener(() => swaper.Request(ActionType.Swap));
        input.OnDropWeaponPressed.AddListener(() => swaper.Request(ActionType.Drop));
        input.OnAltFireButtonPressed.AddListener(OnAltPressed);
        input.OnAltFireButtonReleased.AddListener(OnAltReleased);

        swaper = new SwapController(this);
        HookUpWeapon();
    }

    private void Update() => swaper.Tick(Time.deltaTime);

    private void OnPointerMoved(Vector2 p)
    {
        lastPointer = p;
        currentCtrl?.AimWeapon(p);
    }

    private void OnFirePressed()
    {
        wantFire = true;
        currentCtrl?.HandleTriggerPressed();
    }

    private void OnFireReleased()
    {
        wantFire = false;
        currentCtrl?.HandleTriggerReleased();
    }

    private void OnAltPressed() => currentCtrl?.HandleAltPressed();
    private void OnAltReleased() => currentCtrl?.HandleAltReleased();

    /// <summary>
    /// Al recoger un arma (pick up), si ya hay 2, dropea la actual antes
    /// y suena Drop + Pickup.
    /// </summary>
    public void PickUpWeapon(WeaponDataSO data, WeaponState state)
    {
        if (slots.SlotCount == 2)
        {
            // Si era láser, suprime el LaserStop
            if (currentCtrl is ChargingLaserController laser)
                laser.SuppressNextReleaseSound();

            audioPlayer.PlayDropWeaponSFX();
            slots.DropCurrentWeapon();
        }

        if (slots.CurrentWeaponGO == null)
            slots.EquipWeapon(data, state);
        else
            slots.AddWeaponToNextSlot(data, state);

        HookUpWeapon();
        audioPlayer.PlayPickupWeaponSFX();

        if (wantFire)
            currentCtrl?.HandleTriggerPressed();
    }

    /// <summary>
    /// Configura currentCtrl apuntando al IWeaponController del arma equipada (o null).
    /// Luego dispara OnWeaponChanged(newWeapon).
    /// </summary>
    private void HookUpWeapon()
    {
        var go = slots.CurrentWeaponGO?.gameObject;
        currentCtrl = go?.GetComponent<IWeaponController>();

        if (currentCtrl != null)
        {
            currentCtrl.FullReset();
            currentCtrl.AimWeapon(lastPointer);
        }

        Weapon newWeapon = go?.GetComponent<Weapon>();
        OnWeaponChanged?.Invoke(newWeapon);
    }

    private void OnDestroy()
    {
        input.OnPointerPositionChange.RemoveListener(OnPointerMoved);
        input.OnFireButtonPressed.RemoveListener(OnFirePressed);
        input.OnFireButtonReleased.RemoveListener(OnFireReleased);
    }

    private enum ActionType { None, Swap, Drop }

    private class SwapController
    {
        private readonly WeaponHandler h;
        private ActionType pending = ActionType.None;
        private float timer;
        private bool busy;

        public SwapController(WeaponHandler handler) => h = handler;

        public void Request(ActionType act)
        {
            if (busy) return;
            if (act == ActionType.Swap && h.slots.SlotCount < 2) return;
            if (act == ActionType.Drop && h.slots.SlotCount == 0) return;

            busy = true;
            pending = act;
            timer = h.swapDelay;

            if (h.currentCtrl is ChargingLaserController laser)
                laser.SuppressNextReleaseSound();

            h.currentCtrl?.HandleTriggerReleased();
            if (act == ActionType.Drop)
                h.audioPlayer.PlayDropWeaponSFX();
        }

        public void Tick(float dt)
        {
            if (!busy) return;
            if ((timer -= dt) > 0) return;

            switch (pending)
            {
                case ActionType.Swap:
                    h.slots.SwapWeapon();
                    break;
                case ActionType.Drop:
                    h.slots.DropCurrentWeapon();
                    if (h.slots.HasSecondary)
                        h.slots.EquipSecondaryWeapon();
                    break;
            }

            h.HookUpWeapon();

            if (h.wantFire)
                h.currentCtrl?.HandleTriggerPressed();

            if (pending == ActionType.Swap)
                h.audioPlayer.PlayPickupWeaponSFX();

            busy = false;
            pending = ActionType.None;
        }
    }
}
