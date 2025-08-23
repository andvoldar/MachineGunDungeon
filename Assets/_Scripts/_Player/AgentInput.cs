using UnityEngine;
using UnityEngine.Events;

public class AgentInput : MonoBehaviour, IAgentInput
{
    private Camera mainCamera;
    private bool fireButtonPressed = false;
    private bool altButtonPressed = false;
    [field: SerializeField] public UnityEvent<Vector2> OnMovementKeyPressed { get; set; }
    [field: SerializeField] public UnityEvent<Vector2> OnPointerPositionChange { get; set; }
    [field: SerializeField] public UnityEvent OnFireButtonPressed { get; set; }
    [field: SerializeField] public UnityEvent OnFireButtonReleased { get; set; }
    [field: SerializeField] public UnityEvent OnAltFireButtonPressed { get; set; }    // agregado
    [field: SerializeField] public UnityEvent OnAltFireButtonReleased { get; set; }   // ya existía
    [field: SerializeField] public UnityEvent OnDashPressed { get; set; }
    [field: SerializeField] public UnityEvent OnInteractPressed { get; set; }
    [field: SerializeField] public UnityEvent OnDropWeaponPressed { get; set; }
    [field: SerializeField] public UnityEvent OnSwapWeaponPressed { get; set; }



    public bool BlockFireInput { get; set; }
    public bool BlockSwapInput { get; set; }

    private void Awake()
    {
        mainCamera = Camera.main;
        InitializeEvents();
    }

    private void InitializeEvents()
    {
        OnFireButtonPressed ??= new UnityEvent();
        OnFireButtonReleased ??= new UnityEvent();
        OnAltFireButtonPressed ??= new UnityEvent();    // inicializado
        OnAltFireButtonReleased ??= new UnityEvent();
        OnMovementKeyPressed ??= new UnityEvent<Vector2>();
        OnPointerPositionChange ??= new UnityEvent<Vector2>();
        OnDashPressed ??= new UnityEvent();
        OnInteractPressed ??= new UnityEvent();
        OnDropWeaponPressed ??= new UnityEvent();
        OnSwapWeaponPressed ??= new UnityEvent();
    }

    private void Update()
    {
        GetMovementInput();
        GetPointerInput();
        GetFireInput();
        GetAltFireInput();      // invocar aquí
        GetDashInput();
        GetInteractInput();
        GetDropWeaponInput();
        GetSwapWeaponInput();
    }

    private void GetAltFireInput()
    {
        if (Input.GetMouseButtonDown(1))
        {
            altButtonPressed = true;
            OnAltFireButtonPressed?.Invoke();
        }
        else if (altButtonPressed && Input.GetMouseButtonUp(1))
        {
            altButtonPressed = false;
            OnAltFireButtonReleased?.Invoke();
        }
    }


    private void GetSwapWeaponInput()
    {
        if (BlockSwapInput) return;               // <-- ignora swap si está bloqueado
        if (Input.GetKeyDown(KeyCode.Tab))
            OnSwapWeaponPressed?.Invoke();
    }

    private void GetInteractInput()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            OnInteractPressed?.Invoke();
        }
    }

    private void GetDropWeaponInput()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            OnDropWeaponPressed?.Invoke();
        }
    }



    private void GetDashInput()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnDashPressed?.Invoke();
        }
    }



    private void GetFireInput()
    {
        if (BlockFireInput) return;              // <-- ignora fire si está bloqueado

        if (Input.GetAxisRaw("Fire1") > 0)
        {
            if (!fireButtonPressed)
            {
                fireButtonPressed = true;
                OnFireButtonPressed?.Invoke();
            }
        }
        else if (fireButtonPressed)
        {
            fireButtonPressed = false;
            OnFireButtonReleased?.Invoke();
        }
    }

    private void GetPointerInput()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = mainCamera.nearClipPlane;
        Vector2 worldMousePos = mainCamera.ScreenToWorldPoint(mousePos);
        OnPointerPositionChange?.Invoke(worldMousePos);
    }

    private void GetMovementInput()
    {
        OnMovementKeyPressed?.Invoke(new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")));
    }
}