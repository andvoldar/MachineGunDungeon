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
            weaponController.AimWeapon(player.position);
        }
    }


    public void MoveTowards(Vector2 target)
    {
        if (!canMove) return;

        Vector2 dir = (target - (Vector2)transform.position).normalized;
        moveDir = dir;

        if (dir.x > 0 && !isFacingRight)
            Flip();
        else if (dir.x < 0 && isFacingRight)
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
