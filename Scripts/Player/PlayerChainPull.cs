using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerAbilityTracker))]
public class PlayerChainPull : MonoBehaviour
{
    [Header("Pull Settings")]
    public float pullForce = 25f;
    public float pullDuration = 0.3f;
    public KeyCode hookKey = KeyCode.R;

    [Header("Cooldown")]
    public float hookCooldown = 0.4f;

    private Rigidbody2D rb;
    private PlayerAbilityTracker abilities;
    private HookPoint currentHook;
    private PlayerMovement movement;
    private Jump jumpScript;

    private float pullTimer;
    private float cooldownTimer;
    private bool pulling;

    // ===== PUBLIC READ-ONLY STATE (FOR VISUALS) =====
    public bool IsPulling => pulling;
    public bool HasHook => currentHook != null;

    public Vector2 HookCenter
    {
        get
        {
            if (currentHook == null) return rb.position;
            return currentHook.GetComponent<Collider2D>().bounds.center;
        }
    }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        abilities = GetComponent<PlayerAbilityTracker>();
        movement = GetComponent<PlayerMovement>();
        jumpScript = GetComponent<Jump>();
    }

    void Update()
    {
        if (!abilities.canHook)
            return;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
        else // Only check for input if cooldown is done (or if we are pulling)
        {
            // NEW: Hook Jump / Jump Cancel
            if (pulling && InputManager.instance.GetJumpDown()) // Use key from Jump script for consistency
            {
                StopPull();
                if(jumpScript) jumpScript.PerformJump();
                return;
            }

            if (InputManager.instance.GetHookDown())
            {
                if (!pulling && currentHook != null)
                {
                    StartPull();
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!pulling)
            return;

        pullTimer -= Time.fixedDeltaTime;
        if (pullTimer <= 0f)
        {
            StopPull();
            return;
        }

        Vector2 diff = HookCenter - rb.position;
        
        // Prevent NaN (Divide by Zero) if we are extremely close to the center
        if (diff.sqrMagnitude < 0.1f) 
        {
            StopPull();
            rb.velocity = Vector2.zero; 
            return;
        }

        Vector2 pullDir = diff.normalized;

        // ONE-WAY pull (no backward force)
        float dot = Vector2.Dot(rb.velocity, pullDir);
        if (dot < 0f)
            rb.velocity -= pullDir * dot;

        Vector2 force = pullDir * pullForce;
        if (!float.IsNaN(force.x) && !float.IsNaN(force.y))
        {
            rb.AddForce(force, ForceMode2D.Force);
        }
        else
        {
            Debug.LogError("[PlayerChainPull] Attempted to apply NaN Force! Aborting.");
            StopPull();
        }
    }

    void StartPull()
    {
        pulling = true;
        pullTimer = pullDuration;

        if (jumpScript != null)
        {
            jumpScript.ResetDoubleJump();
        }
       
        movement.canMove = false;
    }

    void StopPull()
    {
        pulling = false;
        cooldownTimer = hookCooldown;

      
        movement.canMove = true;
        movement.canDoubleJump = true;
    }

    // ===== HookPoint callbacks =====
    public void SetHook(HookPoint hook)
    {
        if (pulling || cooldownTimer > 0f)
            return;

        currentHook = hook;
    }

    public void ClearHook(HookPoint hook)
    {
        if (currentHook == hook)
            currentHook = null;
    }
}
