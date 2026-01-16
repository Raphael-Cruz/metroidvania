using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Jump : MonoBehaviour
{
    [Header("Jump Settings")]
    [Tooltip("Initial upward force applied when jumping")]
    public float jumpForce = 14f;
    
    [Tooltip("Base gravity multiplier (default Unity gravity is -9.81)")]
    public float gravityScale = 4f;
    
    [Tooltip("Extra gravity multiplier when falling (makes falls snappier)")]
    public float fallGravityMultiplier = 2.5f;
    
    [Tooltip("Extra gravity when releasing jump button early (enables variable jump height)")]
    public float lowJumpMultiplier = 2f;
    
    [Tooltip("Maximum downward velocity to prevent infinite acceleration")]
    public float maxFallSpeed = 20f;

    [Header("Assist Settings")]
    [Tooltip("Time window after leaving ground where jump still works")]
    public float coyoteTime = 0.12f;
    
    [Tooltip("Time window to buffer jump input before landing")]
    public float jumpBufferTime = 0.12f;

    [Header("Input Settings")]
    [Tooltip("Key to press for jumping")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D theRB;
    private bool isGrounded;

    // Timers for assist mechanics (now handled in FixedUpdate for consistency)
    private float coyoteCounter;
    private float jumpBufferCounter;

    // Input state flags
    private bool jumpPressedThisFrame;
    private bool jumpHeldThisFrame;

    void Awake()
    {
        theRB = GetComponent<Rigidbody2D>();
        theRB.gravityScale = gravityScale;
        
        // Validate groundCheck reference
        if (groundCheck == null)
        {
            Debug.LogError($"[Jump] groundCheck is not assigned on {gameObject.name}! Ground detection will not work.", this);
        }
    }

    void Update()
    {
        // Capture input in Update (runs every frame for responsive input)
        // Store flags that will be read in FixedUpdate
        if (Input.GetKeyDown(jumpKey))
            jumpPressedThisFrame = true;
        
        jumpHeldThisFrame = Input.GetKey(jumpKey);
    }

    void FixedUpdate()
    {
        // GROUND CHECK
        CheckGround();

        // COYOTE TIME - allows jumping shortly after leaving ground
        if (isGrounded)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.fixedDeltaTime;

        // JUMP BUFFER - allows jump input slightly before landing
        if (jumpPressedThisFrame)
        {
            jumpBufferCounter = jumpBufferTime;
            jumpPressedThisFrame = false; // Clear flag after reading
        }
        else
        {
            jumpBufferCounter -= Time.fixedDeltaTime;
        }

        // EXECUTE JUMP - using AddForce for smoother physics interaction
        if (jumpBufferCounter > 0 && coyoteCounter > 0)
        {
            PerformJump();
            jumpBufferCounter = 0; // Clear buffer
            coyoteCounter = 0;     // Clear coyote time
        }

        // Apply enhanced jump physics
        ApplyBetterJumpPhysics();
        
        // Clamp fall speed to prevent infinite acceleration
        ClampFallSpeed();
    }

    void PerformJump()
    {
        // Cancel any existing vertical velocity before applying jump force
        // This ensures consistent jump height regardless of current velocity
        theRB.velocity = new Vector2(theRB.velocity.x, 0f);
        
        // Apply jump force using AddForce for more natural physics
        // Using Impulse mode for instant velocity change (similar to setting velocity but additive)
        theRB.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void ApplyBetterJumpPhysics()
    {
        // FALLING - apply extra gravity for snappier, more responsive falls
        if (theRB.velocity.y < 0)
        {
            theRB.velocity += Vector2.up * Physics2D.gravity.y *
                              (fallGravityMultiplier - 1f) * Time.fixedDeltaTime;
        }
        // RISING + RELEASED JUMP - enables variable jump height
        // Releasing jump button early cuts the jump short
        else if (theRB.velocity.y > 0 && !jumpHeldThisFrame)
        {
            theRB.velocity += Vector2.up * Physics2D.gravity.y *
                              (lowJumpMultiplier - 1f) * Time.fixedDeltaTime;
        }
    }

    void ClampFallSpeed()
    {
        // Prevent falling too fast (avoids collision detection issues and feels better)
        if (theRB.velocity.y < -maxFallSpeed)
        {
            theRB.velocity = new Vector2(theRB.velocity.x, -maxFallSpeed);
        }
    }

    void CheckGround()
    {
        // Safety check to prevent null reference errors
        if (groundCheck == null)
        {
            isGrounded = false;
            return;
        }

        isGrounded = Physics2D.OverlapCircle(
            groundCheck.position,
            groundCheckRadius,
            groundLayer
        );
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;
        
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }
}
