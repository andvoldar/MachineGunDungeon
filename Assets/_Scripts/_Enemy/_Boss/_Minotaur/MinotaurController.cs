using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MinotaurController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float attackRange = 2f;
    private Rigidbody2D rb;
    private Vector2 moveDir = Vector2.zero;
    private bool canMove = true;

    [Header("Facing")]
    public bool isFacingRight = true;
    public bool IsFacingRight => isFacingRight;

    [Tooltip("Umbral para decidir flip horizontal. Evita jitter cuando dir.x es muy pequeño.")]
    [SerializeField] private float facingFlipDeadzone = 0.12f;

    private Transform player;
    private MinotaurWeaponController weaponController;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player")?.transform;
        weaponController = GetComponent<MinotaurWeaponController>();
    }

    private void FixedUpdate()
    {
        if (canMove)
            rb.velocity = moveDir * moveSpeed;
    }

    private void Update()
    {
        if (player != null && weaponController != null)
        {
            // El propio WeaponController ignora el aim si el combate está desactivado.
            weaponController.AimWeapon(player.position);
        }
    }

    public void MoveTowards(Vector2 target)
    {
        if (!canMove) return;

        Vector2 dir = (target - (Vector2)transform.position);
        float mag = dir.magnitude;
        dir = (mag > 0.0001f) ? dir / mag : Vector2.zero;
        moveDir = dir;

        // Flip con histéresis (evita tembleque)
        if (dir.x > facingFlipDeadzone && !isFacingRight)
            Flip();
        else if (dir.x < -facingFlipDeadzone && isFacingRight)
            Flip();
    }

    public void StopMovement()
    {
        moveDir = Vector2.zero;
        rb.velocity = Vector2.zero;
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void SetDashVelocity(Vector2 dashVelocity)
    {
        rb.velocity = dashVelocity;
    }
}
