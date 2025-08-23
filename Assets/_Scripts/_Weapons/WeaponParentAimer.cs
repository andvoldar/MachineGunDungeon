using UnityEngine;

[RequireComponent(typeof(Transform))]
public class WeaponParentAimer : MonoBehaviour
{
    [Tooltip("Distancia local X cuando apunta a la derecha")]
    [SerializeField] private Vector2 rightOffset = new Vector2(0.5f, 0f);
    [Tooltip("Distancia local X cuando apunta a la izquierda")]
    [SerializeField] private Vector2 leftOffset = new Vector2(-0.5f, 0f);
    [Tooltip("Qué tan rápido interpola el offset")]
    [SerializeField] private float offsetSmoothSpeed = 10f;

    private Camera _cam;
    private Vector2 _targetOffset;

    private void Awake()
    {
        _cam = Camera.main;
    }

    private void Update()
    {
        // 1) Rotación hacia el ratón (como antes)
        Vector3 mouseWorld = _cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;
        Vector2 dir = (mouseWorld - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        // 2) Decidir si apunta a la izquierda o a la derecha
        bool facingLeft = angle > 90f || angle < -90f;
        _targetOffset = facingLeft ? leftOffset : rightOffset;

        // 3) Interpolar localPosition para dar el efecto de orbit
        Vector2 current = transform.localPosition;
        Vector2 next = Vector2.Lerp(current, _targetOffset, Time.deltaTime * offsetSmoothSpeed);
        transform.localPosition = new Vector3(next.x, next.y, transform.localPosition.z);
    }
}