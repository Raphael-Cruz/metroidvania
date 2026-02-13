using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Jump : MonoBehaviour
{
    [Header("Jump Settings")]
    public float jumpForce = 14f;
    public float gravityScale = 4f;
    public float fallGravityMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float maxFallSpeed = 20f;

    [Header("Assist Settings")]
    public float doubleJumpMultiplier = 0.8f;
    public float coyoteTime = 0.12f;
    public float jumpBufferTime = 0.12f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;
    public LayerMask playerPlatformLayer;

    private PlayerAbilityTracker abilities;
    private Rigidbody2D rb;

    private bool isGrounded;
    private bool canDoubleJump;

    // Timers
    private float coyoteCounter;
    private float jumpBufferCounter;

    // Input flags
    private bool jumpPressedThisFrame;
    private bool jumpHeldThisFrame;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        abilities = GetComponent<PlayerAbilityTracker>();

        rb.gravityScale = gravityScale;

        if (groundCheck == null)
        {
            Debug.LogError($"[Jump] GroundCheck not assigned on {gameObject.name}", this);
        }
    }

    void Update()
    {
        if (InputManager.instance == null)
            return;

        if (InputManager.instance.GetJumpDown())
            jumpPressedThisFrame = true;

        jumpHeldThisFrame = InputManager.instance.GetJump();
    }

    void FixedUpdate()
    {
        CheckGround();

        // --- COYOTE TIME ---
        if (isGrounded)
        {
            coyoteCounter = coyoteTime;
            canDoubleJump = true;
        }
        else
        {
            coyoteCounter -= Time.fixedDeltaTime;
        }

        // --- JUMP BUFFER ---
        if (jumpPressedThisFrame)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpPressedThisFrame = false;
        }
        else
        {
            jumpBufferCounter -= Time.fixedDeltaTime;
        }

        // --- JUMP EXECUTION ---
        if (jumpBufferCounter > 0)
        {
            // Normal jump
            if (coyoteCounter > 0)
            {
                PerformJump();
                jumpBufferCounter = 0;
                coyoteCounter = 0;
            }
            // Double jump
            else if (canDoubleJump && abilities != null && abilities.canDoubleJump)
            {
                PerformJump(doubleJumpMultiplier);
                canDoubleJump = false;
                jumpBufferCounter = 0;

                var anim = GetComponentInChildren<Animator>();
                if (anim) anim.SetTrigger("doubleJump");
            }
        }

        ApplyBetterJumpPhysics();
        ClampFallSpeed();
    }

    public void PerformJump(float multiplier = 1f)
    {
        // Reset vertical velocity for consistent jump height
        rb.velocity = new Vector2(rb.velocity.x, 0f);

        rb.AddForce(Vector2.up * jumpForce * multiplier, ForceMode2D.Impulse);
    }

    void ApplyBetterJumpPhysics()
    {
        if (float.IsNaN(rb.velocity.y))
            return;

        // Falling
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y *
                           (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
        // Rising + released jump (variable height)
        else if (rb.velocity.y > 0 && !jumpHeldThisFrame)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y *
                           (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    void ClampFallSpeed()
    {
        if (rb.velocity.y < -maxFallSpeed)
        {
            rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
        }
    }

    void CheckGround()
    {
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer | playerPlatformLayer
        );
    }
public void ResetDoubleJump()
{
    canDoubleJump = true;
}
    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
