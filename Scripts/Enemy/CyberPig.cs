using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CyberPig : MonoBehaviour
{
    #region HEALTH & PHASE SYSTEM
    [Header("Health System")]
    [SerializeField] private EnemyHealthController healthController;
    [Tooltip("If set > 0, this will overwrite the EnemyHealthController's health on start.")]
    [SerializeField] private int maxHealth = 50;

    private int phase2HealthThreshold;
    private int phase3HealthThreshold;

    private bool hasTriggeredMagicSwordCast;
    private bool hasTriggeredFireSword;
    private bool isDead;

    public enum BossPhase
    {
        Phase1_Normal,
        Phase2_MagicSwordCast,
        Phase3_FireSword
    }

    private BossPhase currentPhase = BossPhase.Phase1_Normal;
    #endregion

    #region REFERENCES
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private BossSkillCaster skillCaster; // Cached reference
    #endregion

    #region BOSS MOVEMENT
    [Header("Boss Movement")]
    [SerializeField] private float aggroRange = 9f;
    [SerializeField] private float combatDistance = 3f; // Ideal distance to hang out
    [SerializeField] private float decisionDelay = 0.35f;

    [Header("Speeds")]
    [SerializeField] private float runSpeed = 4f;       // Normal chase speed
    [SerializeField] private float burstSpeed = 8f;      // Gap closer
    [SerializeField] private float backstepSpeed = 6f;   // Retreat
    [SerializeField] private float lungeSpeed = 10f;     // Attack lunge

    [Header("FX")]
    [SerializeField] private GhostTrail ghostTrail;         // Reference to the after-image system

    [Header("Durations")]
    [SerializeField] private float burstDuration = 0.4f;
    [SerializeField] private float backstepDuration = 0.3f;
    [SerializeField] private float lungeDuration = 0.3f;

    private Vector2 currentVelocity;
    private bool isFacingRight = true;
    private bool isDeciding;
    #endregion

    #region COMBAT
    [Header("Combat")]
    [SerializeField] private float minAttackCooldown = 1.5f;
    [SerializeField] private float maxAttackCooldown = 2.5f;
    [SerializeField] private float attackRange = 5.5f;
    [SerializeField] private float dashAttackRange = 10.5f; // Distance to trigger a dash-in attack

    [Header("Animation Timings")]
    [SerializeField] private float attackWindupTime = 0.15f; // Time before lunge starts
    [SerializeField] private float attackRecoverTime = 0.5f; // Time after lunge
    [SerializeField] private float phase2CastDuration = 2.0f;
    [SerializeField] private float phase3CastDuration = 1.5f;
    [SerializeField] private float deathAnimationDuration = 2.5f;

    private float lastAttackTime;
    private bool isAttacking;
    #endregion

    #region ANIMATION PARAMETERS
    [Header("Animation Parameter Names")]
    [SerializeField] private string param_Idle = "isIdle";
    [SerializeField] private string param_Chase = "isChasing";
    [SerializeField] private string param_Attack = "isAttacking";
    [SerializeField] private string param_Cast = "isCastingMagicSword";
    [SerializeField] private string param_FireMode = "hasFireSword";
    [SerializeField] private string param_Hurt = "isHurt";
    [SerializeField] private string param_Dead = "Dead";
    [SerializeField] private string param_MoveSpeed = "moveSpeed";
    [SerializeField] private string param_Dashing = "dashing";

    // Hashes
    private int animID_Idle;
    private int animID_Chase;
    private int animID_Attack;
    private int animID_Cast;
    private int animID_FireMode;
    private int animID_Hurt;
    private int animID_Dead;
    private int animID_MoveSpeed;
    private int animID_Dashing;

    // Availability Flags
    private bool hasParam_MoveSpeed;
    #endregion

    #region STATE
    private enum BossState
    {
        Idle,
        Thinking,
        Moving,
        Attacking,
        Casting,
        Dead
    }

    private BossState currentState = BossState.Idle;
    #endregion

    #region INITIALIZATION
    void Awake()
    {
        // Cache references
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!anim) anim = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (!healthController) healthController = GetComponent<EnemyHealthController>();
        if (!skillCaster) skillCaster = GetComponent<BossSkillCaster>();

        // Physics setup
        rb.gravityScale = 3f;
        rb.freezeRotation = true;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Initialize Animation IDs and Check Existence
        InitializeAnimatorParameters();
    }

    void InitializeAnimatorParameters()
    {
        if (!anim) return;

        
        
        animID_Idle = Animator.StringToHash(param_Idle);
        animID_Chase = Animator.StringToHash(param_Chase);
        animID_Attack = Animator.StringToHash(param_Attack);
        animID_Cast = Animator.StringToHash(param_Cast);
        animID_FireMode = Animator.StringToHash(param_FireMode);
        animID_Hurt = Animator.StringToHash(param_Hurt);
        animID_Dead = Animator.StringToHash(param_Dead);
        animID_Dashing = Animator.StringToHash(param_Dashing);
        
        // Strict check for Float parameter to avoid the crash loop
        animID_MoveSpeed = Animator.StringToHash(param_MoveSpeed);
        hasParam_MoveSpeed = HasParameter(param_MoveSpeed, AnimatorControllerParameterType.Float);

        if (!hasParam_MoveSpeed)
        {
            Debug.LogWarning($"[CyberPig] Animator is missing Float parameter '{param_MoveSpeed}'. movement speed animation will not play.");
        }
    }

    private bool HasParameter(string name, AnimatorControllerParameterType type)
    {
        if (!anim) return false;
        foreach (var param in anim.parameters)
        {
            if (param.name == name && param.type == type) return true;
        }
        return false;
    }

    void Start()
    {
        // Setup Health
        if (healthController != null)
        {
            if (maxHealth > 0)
            {
                healthController.totalhealth = maxHealth;
            }
            healthController.enemyID = "CyberPig_Boss";
            healthController.isPermanentEnemy = true;
            
            //**Important for death animation
            healthController.onDeathCallback = OnBossDeath;
            
            //**Important for phase transitions
            healthController.onDamageCallback = (dmg) => OnBossDamaged();
        }

        // Calculate Thresholds
        // Make sure we use the actual totalhealth if maxHealth was 0
        int hpBase = healthController != null ? healthController.totalhealth : maxHealth;
        phase2HealthThreshold = Mathf.RoundToInt(hpBase * 0.60f); // 60%
        
        phase3HealthThreshold = Mathf.RoundToInt(hpBase * 0.30f); // 30%

        Debug.Log($"[CyberPig] Inited. MaxHP: {hpBase}, P2 Threshold: {phase2HealthThreshold}, P3 Threshold: {phase3HealthThreshold}");

        // Find Player
        if (!player)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }
    #endregion

    #region UPDATE
    void Update()
    {
        if (isDead || !player) return;

        // Phase transitions are now handled in OnBossDamaged for efficiency,
        // BUT we keep a safety check if needed, or rely solely on events. 
        // For reliability in this specific edit, I will trust the event hook mechanism.
        
        UpdateAI();
        UpdateAnimations();
    }

    void FixedUpdate()
    {
        if (isDead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // Apply velocity from logic but PRESERVE GRAVITY (y-velocity)
        // If currentVelocity.y is 0, we don't want to force him to float.
        rb.velocity = new Vector2(currentVelocity.x, rb.velocity.y);
        
        // Face player unless attacking (prevents spinning during attack windup)
        if (!isAttacking && currentState != BossState.Casting)
        {
            UpdateFacing();
        }
    }
    #endregion

    #region AI CORE (IMPROVED)
    void UpdateAI()
    {
        // Block AI updates if we are busy
        if (isDeciding || isAttacking || currentState == BossState.Casting) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // De-aggro logic
        if (dist > aggroRange)
        {
            currentVelocity = Vector2.zero;
            currentState = BossState.Idle;
            return;
        }

        // If not busy, make a decision
        StartCoroutine(DecisionRoutine(dist));
    }

    IEnumerator DecisionRoutine(float dist)
    {
        isDeciding = true;
        currentState = BossState.Thinking;
        currentVelocity = Vector2.zero; // Brief pause to "think" looks natural

        // Allow decision time to vary slightly for organic feel
        float wait = decisionDelay * Random.Range(0.8f, 1.2f);
        yield return new WaitForSeconds(wait);

        // Decision Tree (Priority Selector)

        // Too Close? Backstep (Kiting)
        // If we are uncomfortably close, retreat to get better spacing
        if (dist < combatDistance * 0.5f) 
        {
            yield return StartCoroutine(Backstep());
        }
        // Can Attack?
        else if (CanAttack())
        {
            if (dist <= attackRange)
            {
                // Standard melee attack
                yield return StartCoroutine(PerformAttack(false));
            }
            else if (dist <= dashAttackRange)
            {
                // Gap closer attack (Dash then Strike)
                yield return StartCoroutine(PerformAttack(true));
            }
            else
            {
                // Cooldown ready, but too far. Move closer.
                yield return StartCoroutine(ChasePlayer(0.5f));
            }
        }
        // Just positioning
        else 
        {
            if (dist > combatDistance + 1.5f)
            {
                // Too far, close gap
                yield return StartCoroutine(ChasePlayer(Random.Range(0.3f, 0.6f)));
            }
            else
            {
                // In sweet spot, idle/strafe or small tactical repostion
                // For now, just wait a bit (stare down)
                yield return new WaitForSeconds(0.2f);
            }
        }

        isDeciding = false;
    }

    private bool CanAttack()
    {
        return Time.time >= lastAttackTime + GetCurrentAttackCooldown();
    }

    private float GetCurrentAttackCooldown()
    {
        // Randomize cooldown
        return Random.Range(minAttackCooldown, maxAttackCooldown);
    }
    #endregion

    #region MOVEMENTS & ACTIONS
    IEnumerator ChasePlayer(float duration)
    {
        currentState = BossState.Moving;
        float t = 0f;

        while (t < duration && !isAttacking) // Stop if interrupted
        {
            if (!player) break;
            
            Vector2 dir = (player.position - transform.position).normalized;
            currentVelocity = dir * runSpeed;
            
            t += Time.deltaTime;
            yield return null;
        }
        currentVelocity = Vector2.zero;
    }

    IEnumerator Backstep()
    {
        currentState = BossState.Moving;
        // Move away from player
        Vector2 dir = (transform.position - player.position).normalized;
        float t = 0f;

        while (t < backstepDuration)
        {
            currentVelocity = dir * backstepSpeed;
            t += Time.deltaTime;
            yield return null;
        }
        currentVelocity = Vector2.zero;
    }

    IEnumerator PerformAttack(bool isDashAttack)
    {
   
        currentState = BossState.Attacking;
        isAttacking = true;
        lastAttackTime = Time.time;
        currentVelocity = Vector2.zero;

        // Dash in first
        if (isDashAttack)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            float t = 0f;
            while (t < burstDuration)
            {
                // High speed burst
                currentVelocity = dir * burstSpeed;
                t += Time.deltaTime;
                anim?.SetTrigger(animID_Dashing);
                
                // Trigger After-Image
                if (ghostTrail != null) ghostTrail.TrySpawnGhost(spriteRenderer, transform);

                yield return null;
            }
            currentVelocity = Vector2.zero;
            // Small pause after burst before strike
            yield return new WaitForSeconds(0.1f);
        }

        // Ensure we are facing the player before attacking
        // (Player might have moved behind us during the dash)
        UpdateFacing();

        // Trigger Animation
        anim?.SetTrigger(animID_Attack);

        // Wait a frame for the animator to transition into the Attack state
        yield return null; 
        yield return null;

        // Get the length of the current animation (assuming it transitioned correctly)
        float animLength = 1.0f; // Default safety
        if (anim)
        {
            AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(0);
            // Verify we are actually in an attack state if possible, otherwise just take the length
            animLength = info.length;
        }

        float startTime = Time.time;

        // Windup (Wait before the damage/lunge part)
        // Reverted the 2.5f hardcode to using the variable (or 0.5f minimum if 0)
        float windup = attackWindupTime > 0 ? attackWindupTime : 0.5f;
        yield return new WaitForSeconds(windup);

        // Lunge forward during the strike
        yield return StartCoroutine(Lunge());

        // Now wait for the REST of the animation to finish
        // We calculate how much time has passed since start
        float elapsed = Time.time - startTime;
        float remaining = animLength - elapsed;

        if (remaining > 0)
        {
            yield return new WaitForSeconds(remaining);
        }
        else
        {
            // If our physics took longer than the animation, add a small recovery anyway
            yield return new WaitForSeconds(attackRecoverTime);
        }

        isAttacking = false;
        currentState = BossState.Idle;
    }

    IEnumerator Lunge()
    {
        if (!player) yield break;

        Vector2 dir = (player.position - transform.position).normalized;
        float t = 0f;

        while (t < lungeDuration)
        {
            currentVelocity = dir * lungeSpeed;
            t += Time.deltaTime;
            yield return null;
        }
        currentVelocity = Vector2.zero;
    }
    #endregion

    #region PHASES & EVENTS
    /// <summary>
    /// Called by EnemyHealthController via UnityEvent or direct call
    /// </summary>
    public void OnBossDamaged()
    {
        if (isDead) return;

        anim?.SetTrigger(animID_Hurt);
            

        
        Debug.Log($"[CyberPig] Taken Damage. Current HP: {healthController.totalhealth}");
        CheckPhaseTransitions();
    }

    void CheckPhaseTransitions()
    {
        int hp = healthController.totalhealth;
        Debug.Log($"[CyberPig] Checking Phase. HP: {hp}, P2Flag: {hasTriggeredMagicSwordCast}, P3Flag: {hasTriggeredFireSword}");

        // Phase 2 Transition
        if (!hasTriggeredMagicSwordCast && hp <= phase2HealthThreshold && hp > phase3HealthThreshold)
        {
            Debug.Log("[CyberPig] Activating Phase 2!");
            hasTriggeredMagicSwordCast = true;
            InterruptActions();
            StartCoroutine(StartPhase2());
        }

        // Phase 3 Transition
        if (!hasTriggeredFireSword && hp <= phase3HealthThreshold && hp > 0)
        {
            Debug.Log("[CyberPig] Activating Phase 3!");
            hasTriggeredFireSword = true;
            InterruptActions();
            StartCoroutine(StartPhase3());
        }
    }

    IEnumerator StartPhase2()
    {
       
        currentState = BossState.Casting;

        anim?.SetTrigger(animID_Cast);
        yield return new WaitForSeconds(phase2CastDuration);

        // Apply Phase 2 Buffs
        decisionDelay *= 0.85f; // Think faster
        runSpeed *= 1.1f;
        maxAttackCooldown *= 0.85f; 

        // Enable skill caster
        if (skillCaster) skillCaster.SetCanCast(true);
        
        currentPhase = BossPhase.Phase2_MagicSwordCast;
        currentState = BossState.Idle;
    }

    IEnumerator StartPhase3()
    {
      
        currentState = BossState.Casting;

        anim?.SetTrigger(animID_Cast);
        yield return new WaitForSeconds(phase3CastDuration);
        
        // Fire Mode
        anim?.SetBool(animID_FireMode, true);

        // Apply Phase 3 Buffs (Berserk)
        decisionDelay *= 0.7f;
        burstSpeed *= 1.25f;
        minAttackCooldown *= 0.6f;
        maxAttackCooldown *= 0.6f;
        lungeSpeed *= 1.3f;

        currentPhase = BossPhase.Phase3_FireSword;
        currentState = BossState.Idle;
    }

    void InterruptActions()
    {
        StopAllCoroutines();
        // Be careful not to stop the routine calling this one if it's a coroutine... 
        // But here we are calling from CheckPhaseTransitions -> void.
        // If CheckPhaseTransitions was valid, we want to override existing behavior.
        
        isDeciding = false;
        isAttacking = false;
        currentVelocity = Vector2.zero;
    }

    void OnBossDeath(EnemyHealthController controller)
    {
        if (!isDead)
        {
            InterruptActions();
            StartCoroutine(DeathRoutine());
        }
    }

    IEnumerator DeathRoutine()
    {
        isDead = true;
        currentState = BossState.Dead;
        // InterruptActions(); // Moved to OnBossDeath
        
        rb.velocity = Vector2.zero;
        rb.simulated = false; // Disable physics to prevent pushing corpse

        anim?.SetTrigger(animID_Dead);
        yield return new WaitForSeconds(deathAnimationDuration);

        // Finally destroy
        healthController.PerformDefaultDeath();
    }
    #endregion

    #region VISUALS
    void UpdateFacing()
    {
        if (!player) return;

        bool shouldFaceRight = player.position.x > transform.position.x;
        if (shouldFaceRight != isFacingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;
        
        if (skillCaster)
            skillCaster.FlipSpawnPoint(isFacingRight);
    }

    void UpdateAnimations()
    {
        if (!anim) return;

        anim.SetBool(animID_Idle, currentState == BossState.Idle || currentState == BossState.Thinking);
        anim.SetBool(animID_Chase, currentState == BossState.Moving);
        
        if (hasParam_MoveSpeed)
        {
            anim.SetFloat(animID_MoveSpeed, currentVelocity.magnitude);
        }
        
        // Note: Attack, Hurt, Cast, Dead are Triggers handled in logic
    }
    #endregion

    #region GIZMOS
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        
        Gizmos.color = new Color(1, 0.5f, 0); // Orange
        Gizmos.DrawWireSphere(transform.position, dashAttackRange);
    }
    #endregion
}
