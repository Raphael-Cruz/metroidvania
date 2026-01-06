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
    }
void Start()
{
    if (EnergyBeamVisual.Instance != null)
    {
        EnergyBeamVisual.Instance.player = transform;
        EnergyBeamVisual.Instance.pull = this;
    }
}
    void Update()
    {
        if (!abilities.canHook)
            return;

        if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
            return;
        }

        if (!pulling && currentHook != null && Input.GetKeyDown(hookKey))
        {
            StartPull();
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

        Vector2 pullDir = (HookCenter - rb.position).normalized;

        // ONE-WAY pull (no backward force)
        float dot = Vector2.Dot(rb.velocity, pullDir);
        if (dot < 0f)
            rb.velocity -= pullDir * dot;

        rb.AddForce(pullDir * pullForce, ForceMode2D.Force);
    }

    void StartPull()
    {
        pulling = true;
        pullTimer = pullDuration;

       
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
