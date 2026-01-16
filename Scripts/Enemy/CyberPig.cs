using System.Collections;
using UnityEngine;

/// <summary>
/// CyberPig Boss - Advanced AI with Hollow Knight-inspired movement
/// Features: Phase-based combat, smooth acceleration movement, intelligent attack patterns
/// </summary>
public class CyberPig : MonoBehaviour
{
    #region HEALTH & PHASE SYSTEM
    [Header("Health System")]
    [SerializeField] private EnemyHealthController healthController;
    
    // Health configuration
    private int maxHealth = 50;
    private int phase2HealthThreshold; // 2/3 HP (33 HP)
    private int phase3HealthThreshold; // 1/3 HP (17 HP)
    
    private BossPhase currentPhase = BossPhase.Phase1_Normal;
    private bool hasTriggeredMagicSwordCast = false;
    private bool hasTriggeredFireSword = false;
    private bool isDead = false;
    
    public enum BossPhase
    {
        Phase1_Normal,
        Phase2_MagicSwordCast,
        Phase3_FireSword
    }
    #endregion

    #region MOVEMENT SETTINGS
    [Header("Movement - Hollow Knight Style")]
    [Tooltip("Maximum movement speed")]
    [SerializeField] private float maxMoveSpeed = 5f;
    
    [Tooltip("How quickly boss accelerates (higher = snappier)")]
    [SerializeField] private float acceleration = 12f;
    
    [Tooltip("How quickly boss decelerates when stopping")]
    [SerializeField] private float deceleration = 18f;
    
    [Tooltip("Range at which boss starts chasing player")]
    [SerializeField] private float aggroRange = 8f;
    
    [Tooltip("Preferred combat distance from player")]
    [SerializeField] private float combatDistance = 3f;
    
    [Tooltip("Distance tolerance for combat positioning")]
    [SerializeField] private float combatDistanceTolerance = 1f;
    
    // Current velocity for smooth movement
    private Vector2 currentVelocity;
    private bool isFacingRight = true;
    #endregion

    #region ATTACK SETTINGS
    [Header("Combat Behavior")]
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private float attackRange = 2.5f;
    [SerializeField] private float lungeSpeed = 8f;
    [SerializeField] private float lungeDuration = 0.4f;
    
    private float lastAttackTime;
    private bool isAttacking = false;
    private bool isLunging = false;
    #endregion

    #region ANIMATION & EFFECTS
    [Header("Animation & Visuals")]
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    // Animation parameter names (customize to your animator)
    private readonly string ANIM_IDLE = "isIdle";
    private readonly string ANIM_CHASE = "isChasing";
    private readonly string ANIM_ATTACK = "isAttacking";
    private readonly string ANIM_MAGIC_CAST = "isCastingMagicSword";
    private readonly string ANIM_FIRE_SWORD = "hasFireSword";
    private readonly string ANIM_HURT = "isHurt";
    private readonly string ANIM_DEAD = "Dead";
    #endregion

    #region REFERENCES
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D rb;
    
    private BossState currentState = BossState.Idle;
    
    private enum BossState
    {
        Idle,
        Chasing,
        Positioning,
        Attacking,
        Lunging,
        Casting,
        Hurt
    }
    #endregion

    #region INITIALIZATION
    void Awake()
    {
        // Auto-find components if not assigned
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (anim == null) anim = GetComponent<Animator>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (healthController == null) healthController = GetComponent<EnemyHealthController>();
        
        // Configure Rigidbody2D for smooth movement
        if (rb != null)
        {
            rb.gravityScale = 0; // Top-down or platformer with custom gravity
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    void Start()
    {
        // Setup health controller
        if (healthController != null)
        {
            healthController.totalhealth = maxHealth;
            healthController.isPermanentEnemy = true;
            healthController.enemyID = "CyberPig_Boss"; // Unique ID for save system
            
            // Register custom death handler to play animation before destruction
            healthController.onDeathCallback = OnHealthControllerDeath;
            
            Debug.Log($"[CyberPig] Health controller configured - HP: {maxHealth}");
        }
        else
        {
            Debug.LogError("[CyberPig] EnemyHealthController not found! Boss requires health component.", this);
        }
        
        // Calculate phase thresholds
        phase2HealthThreshold = Mathf.RoundToInt(maxHealth * (2f / 3f)); // 33 HP
        phase3HealthThreshold = Mathf.RoundToInt(maxHealth * (1f / 3f)); // 17 HP
        
        // Find player
        if (player == null)
        {
            GameObject playerGO = GameObject.FindWithTag("Player");
            if (playerGO != null)
            {
                player = playerGO.transform;
            }
            else
            {
                Debug.LogError("[CyberPig] Cannot find Player! Make sure Player has 'Player' tag.", this);
            }
        }
        
        Debug.Log($"[CyberPig] Boss initialized - Phase 2 at {phase2HealthThreshold} HP, Phase 3 at {phase3HealthThreshold} HP");
    }
    #endregion

    #region UPDATE LOOP
    void Update()
    {
        if (player == null || !player.gameObject.activeSelf || isDead)
            return;

        // Update state machine
        UpdateBossAI();
        
        // Update animations
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (player == null || isAttacking)
            return;

        // Apply smooth movement with acceleration/deceleration
        ApplySmoothMovement();
        
        // Handle flipping to face player
        UpdateFacing();
    }
    #endregion

    #region AI STATE MACHINE
    void UpdateBossAI()
    {
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Check for phase transitions
        CheckPhaseTransitions();
        
        // State machine logic
        switch (currentState)
        {
            case BossState.Idle:
                HandleIdleState(distToPlayer);
                break;
                
            case BossState.Chasing:
                HandleChasingState(distToPlayer);
                break;
                
            case BossState.Positioning:
                HandlePositioningState(distToPlayer);
                break;
                
            case BossState.Attacking:
                HandleAttackingState();
                break;
                
            case BossState.Lunging:
                // Handled in coroutine
                break;
                
            case BossState.Casting:
                // Handled in coroutine
                break;
        }
    }

    void HandleIdleState(float distToPlayer)
    {
        // Decelerate to stop
        currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, deceleration * Time.deltaTime);
        
        // Transition to chasing if player enters aggro range
        if (distToPlayer < aggroRange)
        {
            ChangeState(BossState.Chasing);
        }
    }

    void HandleChasingState(float distToPlayer)
    {
        // Calculate desired velocity towards player
        Vector2 directionToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
        Vector2 targetVelocity = directionToPlayer * maxMoveSpeed;
        
        // Smooth acceleration towards target velocity
        currentVelocity = Vector2.MoveTowards(
            currentVelocity,
            targetVelocity,
            acceleration * Time.deltaTime
        );
        
        // Transition to positioning when within combat distance
        if (distToPlayer <= combatDistance + combatDistanceTolerance)
        {
            ChangeState(BossState.Positioning);
        }
    }

    void HandlePositioningState(float distToPlayer)
    {
        // Maintain combat distance with slight adjustments
        if (distToPlayer > combatDistance + combatDistanceTolerance)
        {
            // Too far, move closer
            Vector2 directionToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
            Vector2 targetVelocity = directionToPlayer * (maxMoveSpeed * 0.6f);
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else if (distToPlayer < combatDistance - combatDistanceTolerance)
        {
            // Too close, back away
            Vector2 directionAwayFromPlayer = ((Vector2)transform.position - (Vector2)player.position).normalized;
            Vector2 targetVelocity = directionAwayFromPlayer * (maxMoveSpeed * 0.4f);
            currentVelocity = Vector2.MoveTowards(currentVelocity, targetVelocity, acceleration * Time.deltaTime);
        }
        else
        {
            // Perfect distance, decelerate
            currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, deceleration * Time.deltaTime);
        }
        
        // Attempt attack if in range and cooldown ready
        if (distToPlayer <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            StartCoroutine(PerformAttack());
        }
    }

    void HandleAttackingState()
    {
        // Attack logic handled in coroutine
        // Slow down during attack
        currentVelocity = Vector2.Lerp(currentVelocity, Vector2.zero, deceleration * 2f * Time.deltaTime);
    }
    #endregion

    #region PHASE SYSTEM
    void CheckPhaseTransitions()
    {
        if (healthController == null)
            return;
        
        int currentHealth = healthController.totalhealth;
        
        // Phase 2: Magic Sword Cast (at 2/3 HP remaining)
        if (!hasTriggeredMagicSwordCast && currentHealth <= phase2HealthThreshold && currentHealth > 0)
        {
            hasTriggeredMagicSwordCast = true;
             hasTriggeredFireSword = true;
            StartCoroutine(TriggerMagicSwordCast());
                StartCoroutine(TriggerFireSword());
        }
        
        // Phase 3: Fire Sword Active (at 1/3 HP remaining)
        if (!hasTriggeredFireSword && currentHealth <= phase3HealthThreshold && currentHealth > 0)
        {
            hasTriggeredFireSword = true;
            StartCoroutine(TriggerFireSword());
             
        }

    
    }

    IEnumerator TriggerMagicSwordCast()
    {
        Debug.Log("[CyberPig] Phase 2 - Casting Magic Sword!");
        ChangeState(BossState.Casting);
        currentPhase = BossPhase.Phase2_MagicSwordCast;
        
        // Stop movement
        currentVelocity = Vector2.zero;
        
        // Play magic cast animation
        if (anim != null)
            anim.SetTrigger(ANIM_MAGIC_CAST);
        
        // Cast duration (adjust to your animation length)
        yield return new WaitForSeconds(2.5f);
        
        // Power up effects - increase stats for phase 2
        maxMoveSpeed *= 1.2f;
        acceleration *= 1.15f;
        attackCooldown *= 0.85f;
        
        Debug.Log("[CyberPig] Magic Sword Cast Complete! Boss powered up.");
        ChangeState(BossState.Chasing);
    }

    IEnumerator TriggerFireSword()
    {
        Debug.Log("[CyberPig] Phase 3 - Fire Sword Activated!");
        ChangeState(BossState.Casting);
        currentPhase = BossPhase.Phase3_FireSword;
        
        // Stop movement
        currentVelocity = Vector2.zero;
        
        // Play fire sword animation
        if (anim != null)
        {
            anim.SetTrigger(ANIM_MAGIC_CAST); // Cast animation
            yield return new WaitForSeconds(1.5f);
            anim.SetBool(ANIM_FIRE_SWORD, true); // Activate fire sword
        }
        else
        {
            yield return new WaitForSeconds(1.5f);
        }
        
        // Final phase power up - boss becomes extremely aggressive
        maxMoveSpeed *= 1.35f;
        acceleration *= 1.25f;
        attackCooldown *= 0.7f;
        combatDistance *= 0.8f; // Gets closer
        
        Debug.Log("[CyberPig] Fire Sword Active! Final phase!");
        ChangeState(BossState.Chasing);
    }
    #endregion

    #region COMBAT
    IEnumerator PerformAttack()
    {
        ChangeState(BossState.Attacking);
        isAttacking = true;
        lastAttackTime = Time.time;
        
        // Trigger attack animation
        if (anim != null)
            anim.SetTrigger(ANIM_ATTACK);
        
        // Lunge towards player for dynamic attack
        yield return StartCoroutine(PerformLunge());
        
        // Attack duration (adjust to animation)
        yield return new WaitForSeconds(0.6f);
        
        isAttacking = false;
        ChangeState(BossState.Positioning);
    }

    IEnumerator PerformLunge()
    {
        isLunging = true;
        ChangeState(BossState.Lunging);
        
        // Calculate lunge direction
        Vector2 lungeDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        
        float elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            // Apply lunge velocity with falloff
            float lungeStrength = Mathf.Lerp(lungeSpeed, maxMoveSpeed, elapsed / lungeDuration);
            currentVelocity = lungeDirection * lungeStrength;
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        isLunging = false;
    }
    
    public void OnBossDamaged()
    {
        // Play hurt animation
        if (anim != null)
            anim.SetTrigger(ANIM_HURT);
    }
    
    // Called by EnemyHealthController when health reaches 0
    private void OnHealthControllerDeath(EnemyHealthController controller)
    {
        if (!isDead)
        {
            StartCoroutine(HandleDeathAnimation());
        }
    }
    
    IEnumerator HandleDeathAnimation()
    {
        isDead = true;
        
        Debug.Log("[CyberPig] Boss died! Playing death animation...");
        
        // Stop all movement and attacks
        currentVelocity = Vector2.zero;
        isAttacking = false;
        isLunging = false;
        
        if (rb != null)
            rb.velocity = Vector2.zero;
        
        // Trigger death animation
        if (anim != null)
        {
            anim.SetTrigger(ANIM_DEAD);
            
            // Wait a frame to let the animator transition to the death state
            yield return null;
            
            // Get the animation clip length
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            float deathAnimLength = stateInfo.length;
            
            // Wait for the death animation to complete
            Debug.Log($"[CyberPig] Waiting {deathAnimLength} seconds for death animation...");
            yield return new WaitForSeconds(2.5f);
        }
        else
        {
            // If no animator, wait a default time
            yield return new WaitForSeconds(1.5f);
        }
        
        Debug.Log("[CyberPig] Death animation complete. Calling default death handler.");
        
        // Now call the default death behavior (save game, spawn effects, destroy)
        if (healthController != null)
        {
            healthController.PerformDefaultDeath();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    //MOVEMENT & PHYSICS
    void ApplySmoothMovement()
    {
        if (rb != null)
        {
            rb.velocity = currentVelocity;
        }
        else
        {
            transform.position += (Vector3)currentVelocity * Time.fixedDeltaTime;
        }
    }

    void UpdateFacing()
    {
        if (player == null)
            return;
        
        float horizontalDiff = player.position.x - transform.position.x;
        
        // Only flip if there's significant horizontal difference
        if (Mathf.Abs(horizontalDiff) > 0.5f)
        {
            bool shouldFaceRight = horizontalDiff > 0;
            
            if (shouldFaceRight != isFacingRight)
            {
                Flip();
            }
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = !isFacingRight;
        }
        else
        {
            // Fallback: flip via scale
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
    #endregion

    #region ANIMATION
    void UpdateAnimations()
    {
        if (anim == null)
            return;
        
        
        anim.SetBool(ANIM_IDLE, currentState == BossState.Idle);
        anim.SetBool(ANIM_CHASE, currentState == BossState.Chasing || currentState == BossState.Positioning);
        
        // Speed parameter for movement animations
        float speed = currentVelocity.magnitude;
        if (anim.parameters.Length > 0) // Check if parameter exists
        {
            foreach (var param in anim.parameters)
            {
                if (param.name == "moveSpeed")
                {
                    anim.SetFloat("moveSpeed", speed);
                    break;
                }
            }
        }
    }
    #endregion

    #region STATE MANAGEMENT
    void ChangeState(BossState newState)
    {
        if (currentState == newState)
            return;
        
        currentState = newState;
        // Debug.Log($"[CyberPig] State changed to: {newState}");
    }
    #endregion

    #region DEBUG
    void OnDrawGizmosSelected()
    {
        // Draw aggro range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        
        // Draw combat distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, combatDistance);
        
        // Draw attack range
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        // Draw current velocity
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)currentVelocity);
    }
    #endregion
}
