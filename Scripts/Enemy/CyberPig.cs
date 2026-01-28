using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CyberPig : MonoBehaviour
{
    #region REFERENCES
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Animator anim;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private EnemyHealthController healthController;
    [SerializeField] private BossSkillCaster skillCaster;
    [SerializeField] private GhostTrail ghostTrail;
    [SerializeField] private GameObject swordHitbox360; 
    [SerializeField] private GameObject LightGlow; 
    [SerializeField] private GameObject Smoke; 
    #endregion

    #region RANGES
    [Header("Ranges")]
    [SerializeField] private float attackRange = 3.5f;   // Swing attack
    [SerializeField] private float lungeRange = 6.0f;    // Lunge attack
    [SerializeField] private float dashRange = 10.5f;    // Dash + Lunge combo
    [SerializeField] private float aggroRange = 15f;     // Start chasing
    #endregion

    #region SPEEDS & DURATIONS
    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float lungeSpeed = 12f;

    [Header("Durations")]
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float lungeDuration = 0.2f;
    [SerializeField] private float postActionPause = 0.5f; // Deliberate pause between actions
    [SerializeField] private float attackCooldown = 1.2f;
    #endregion

    #region HEALTH & PHASES
    [Header("Health System")]
    [SerializeField] private int maxHealth = 50;

    [Header("Phase 2")]
    [SerializeField] private float circleSkillChance = 0.25f; // 25% chance to use circle skill in phase 2
    [SerializeField] private float circleSkillCooldown = 4f;   // Minimum time between circle skills

    private int phase2Threshold;
    private bool triggeredPhase2;
    private bool isPhase2 = false;
    private float lastCircleSkillTime = -999f;
    #endregion

    #region ANIMATION PARAMETERS
    [Header("Animation Parameters")]
    [SerializeField] private string param_IsRunning = "isRunning";
    [SerializeField] private string param_IsIdle = "isIdle";
    [SerializeField] private string param_Dashing = "dashing";
    [SerializeField] private string param_Swinging = "isSwinging";
    [SerializeField] private string param_Lunge = "Lunge";
    [SerializeField] private string param_Cast = "isCastingMagicSword";
    [SerializeField] private string param_FireMode = "hasFireSword";
    [SerializeField] private string param_Dead = "Dead";
   
    [Header("Door Lever")]
    [SerializeField] private DoorLever CyberDoor;

    private int animID_IsRunning;
    private int animID_IsIdle;
    private int animID_Dashing;
    private int animID_Swinging;
    private int animID_Lunge;
    private int animID_Cast;
    private int animID_FireMode;
    private int animID_Dead;
    #endregion

    #region STATE
    private enum ActionState { Idle, Walking, Dashing, Lunging, Swinging, Casting, Dead }
    private ActionState currentState = ActionState.Idle;

    private bool IsBusy => currentState != ActionState.Idle && currentState != ActionState.Walking;

    private float lastAttackTime;
    private bool isFacingRight = true;
    private Vector2 currentVelocity;

    private DoorLever door;

    #endregion

    #region INITIALIZATION
    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!anim) anim = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (!healthController) healthController = GetComponent<EnemyHealthController>();

        // Cache animation hashes
        animID_IsRunning = Animator.StringToHash(param_IsRunning);
        animID_IsIdle = Animator.StringToHash(param_IsIdle);
        animID_Dashing = Animator.StringToHash(param_Dashing);
        animID_Swinging = Animator.StringToHash(param_Swinging);
        animID_Lunge = Animator.StringToHash(param_Lunge);
        animID_Cast = Animator.StringToHash(param_Cast);
        animID_FireMode = Animator.StringToHash(param_FireMode);
        animID_Dead = Animator.StringToHash(param_Dead);

        // Ensure 360 sword is inactive at start
        if (swordHitbox360) swordHitbox360.SetActive(false);
         if (LightGlow ) LightGlow .SetActive(false);
          if (Smoke ) Smoke .SetActive(false);
    }

    bool HasAnimatorParameter(string paramName, AnimatorControllerParameterType type)
    {
        if (!anim) return false;
        foreach (var p in anim.parameters)
        {
            if (p.name == paramName && p.type == type) return true;
        }
        return false;
    }

    void Start()
    {
        // Setup health
        if (healthController)
        {
            if (maxHealth > 0) healthController.totalhealth = maxHealth;
            healthController.onDeathCallback = OnDeath;
            healthController.onDamageCallback = (_) => CheckPhaseTransitions();
        }

        int hp = healthController ? healthController.totalhealth : maxHealth;
        phase2Threshold = Mathf.RoundToInt(hp * 0.6f);
       

        // Find player if not assigned
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }
    #endregion

    #region UPDATE LOOPS
    void Update()
    {
        if (currentState == ActionState.Dead || !player) return;

        if (!IsBusy)
        {
            DecideNextAction();
        }

        UpdateFacing();
    }

    void FixedUpdate()
    {
        if (currentState == ActionState.Dead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = new Vector2(currentVelocity.x, rb.velocity.y);

        // Update running/idle animation based on movement
        if (anim)
        {
            bool isMoving = Mathf.Abs(currentVelocity.x) > 0.1f;
            anim.SetBool(animID_IsRunning, isMoving);
            anim.SetBool(animID_IsIdle, !isMoving);
        }
    }
    #endregion

    #region AI DECISION
    void DecideNextAction()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        // Outside aggro range - idle
        if (dist > aggroRange)
        {
            currentVelocity = Vector2.zero;
            return;
        }

        // On cooldown - walk closer if needed
        if (Time.time < lastAttackTime + attackCooldown)
        {
            if (dist > attackRange + 0.5f)
                WalkTowardPlayer();
            else
                currentVelocity = Vector2.zero;
            return;
        }

        // PHASE 2: Randomly use circle skill
        if (isPhase2 && TryPhase2CircleSkill())
        {
            return; // Circle skill initiated
        }

        // Standard combat logic with improved range prioritization
        // Always prioritize swing if player is very close
        if (dist <= attackRange)
        {
            StartCoroutine(DoSwingAttack());
        }
        else if (dist <= lungeRange)
        {
            // 50% chance to lunge directly, 50% to do nothing (allows swing if player moves closer)
            StartCoroutine(DoLunge());
        }
        else if (dist <= dashRange)
        {
            // Randomly choose between dash+swing or dash+lunge
            if (Random.value < 0.4f)
                StartCoroutine(DoDashSwingCombo());
            else
                StartCoroutine(DoDashLungeCombo());
        }
        else
        {
            WalkTowardPlayer();
        }
    }

    bool TryPhase2CircleSkill()
    {
        // Check if on cooldown
        if (Time.time < lastCircleSkillTime + circleSkillCooldown)
            return false;

        // Random chance to use circle skill
        if (Random.value < circleSkillChance)
        {
            StartCoroutine(CastCircleSkill());
            return true;
        }

        return false;
    }

    void WalkTowardPlayer()
    {
        currentState = ActionState.Walking;
        float dir = player.position.x > transform.position.x ? 1f : -1f;
        currentVelocity = new Vector2(dir * walkSpeed, 0f);
    }
    #endregion

    #region ACTIONS
    IEnumerator DoSwingAttack()
    {
        currentState = ActionState.Swinging;
        currentVelocity = Vector2.zero;
        lastAttackTime = Time.time;

        // Face player before attacking (direction locks here)
        FacePlayer();

        // Trigger animation
        anim.SetTrigger(animID_Swinging);

        // Wait for animation to complete
        yield return WaitForCurrentAnimation();

       

        // Post-action pause
        yield return new WaitForSeconds(postActionPause);
 
        currentState = ActionState.Idle;

    }
        //animation event first "damage" frame
        public void OpenSwordHitbox() {
            if (swordHitbox360) swordHitbox360.SetActive(true);
             
        }
        //animation event last "damage" frame
        public void CloseSwordHitbox() {
            if (swordHitbox360) swordHitbox360.SetActive(false);
              
        }


    IEnumerator DoLunge()
    {
        currentState = ActionState.Lunging;
        currentVelocity = Vector2.zero;
        lastAttackTime = Time.time;

        // Face player before lunging (direction locks here)
        FacePlayer();
        Vector2 lungeDir = GetFacingDirection();

        // Trigger animation
        anim.SetTrigger(animID_Lunge);

        // Wait a frame for state transition
        yield return null;

        // Lunge movement (uses locked direction)
        float elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            currentVelocity = lungeDir * lungeSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentVelocity = Vector2.zero;

        // Wait for rest of animation
        yield return WaitForCurrentAnimation();

        // Post-action pause
        yield return new WaitForSeconds(postActionPause);

        currentState = ActionState.Idle;
    }

    IEnumerator DoDashLungeCombo()
    {
        // Set attack time at START to prevent immediate follow-up
        lastAttackTime = Time.time;

        // Face player before dashing (direction locks here)
        FacePlayer();
        Vector2 dashDir = GetFacingDirection();

        // DASH PHASE
        currentState = ActionState.Dashing;
        currentVelocity = Vector2.zero;

        anim.SetTrigger(animID_Dashing);

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            currentVelocity = dashDir * dashSpeed;
            
            // Ghost trail effect
            if (ghostTrail) ghostTrail.TrySpawnGhost(spriteRenderer, transform);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentVelocity = Vector2.zero;

        // Brief pause between dash and next action
        yield return new WaitForSeconds(0.05f);

        // Re-check distance - prioritize swing if player is very close
        float dist = Vector2.Distance(transform.position, player.position);
        
        if (dist <= attackRange)
        {
            // Player is in swing range - swing instead of lunge
            FacePlayer();
            currentState = ActionState.Swinging;
            lastAttackTime = Time.time;

            anim.SetTrigger(animID_Swinging);
            yield return WaitForCurrentAnimation();
        }
        else if (dist <= lungeRange)
        {
            // Player is in lunge range - lunge
            FacePlayer();
            Vector2 lungeDir = GetFacingDirection();

            currentState = ActionState.Lunging;
            lastAttackTime = Time.time;

            anim.SetTrigger(animID_Lunge);
            yield return null;

            elapsed = 0f;
            while (elapsed < lungeDuration)
            {
                currentVelocity = lungeDir * lungeSpeed;
                elapsed += Time.deltaTime;
                yield return null;
            }

            currentVelocity = Vector2.zero;
            yield return WaitForCurrentAnimation();
        }

        // Post-action pause
        yield return new WaitForSeconds(postActionPause);

        currentState = ActionState.Idle;
    }

    IEnumerator DoDashSwingCombo()
    {
        // Set attack time at START to prevent immediate follow-up
        lastAttackTime = Time.time;

        // Face player before dashing
        FacePlayer();
        Vector2 dashDir = GetFacingDirection();

        // DASH PHASE
        currentState = ActionState.Dashing;
        currentVelocity = Vector2.zero;

        anim.SetTrigger(animID_Dashing);

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            currentVelocity = dashDir * dashSpeed;
            
            // Ghost trail effect
            if (ghostTrail) ghostTrail.TrySpawnGhost(spriteRenderer, transform);
            
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentVelocity = Vector2.zero;

        // Brief pause
        yield return new WaitForSeconds(0.05f);

        // Check if we're close enough to swing
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange + 1f) // Slightly more lenient range for combo
        {
            // SWING PHASE
            FacePlayer();
            currentState = ActionState.Swinging;
            lastAttackTime = Time.time;

            anim.SetTrigger(animID_Swinging);
            yield return WaitForCurrentAnimation();
        }

        // Post-action pause
        yield return new WaitForSeconds(postActionPause);

        currentState = ActionState.Idle;
    }

    IEnumerator WaitForCurrentAnimation()
    {
        // Wait for animator to transition into the new state
        yield return new WaitForSeconds(0.5f);
        
        // Get the current animation length
        float length = 1.5f; // Minimum fallback
        if (anim)
        {
            AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            length = Mathf.Max(stateInfo.length, 0.5f); // At least 0.5s to prevent cutting
        }
        
        yield return new WaitForSeconds(length);
    }
    #endregion

    #region PHASES
    void CheckPhaseTransitions()
    {
        if (!healthController) return;
        int hp = healthController.totalhealth;

        if (!triggeredPhase2 && hp <= phase2Threshold)
        {
            triggeredPhase2 = true;
            StartCoroutine(TriggerPhase2());
        }
    }

    IEnumerator TriggerPhase2()
    {
        StopCurrentAction();
        currentState = ActionState.Casting;
        currentVelocity = Vector2.zero;

        // Trigger CAST animation (should play animation events for Light/Smoke)
        anim.SetTrigger(animID_Cast);
        
        // Wait for CAST animation to complete BEFORE enabling phase 2
        yield return WaitForCurrentAnimation();

        // NOW enable phase 2 (circle skill can be used randomly in combat)
        isPhase2 = true;

        // Buff stats
        walkSpeed *= 1.15f;
        attackCooldown *= 0.85f;

        currentState = ActionState.Idle;
    }

    IEnumerator CastCircleSkill()
    {
        currentState = ActionState.Casting;
        currentVelocity = Vector2.zero;
        lastCircleSkillTime = Time.time;
        lastAttackTime = Time.time; // Prevent immediate follow-up

        // Face player before casting
        FacePlayer();

        // Trigger CAST animation
        anim.SetTrigger(animID_Cast);

        // Wait for animation to complete
        yield return WaitForCurrentAnimation();

        // Now actually cast the skill through the skill caster
        if (skillCaster)
        {
            // Temporarily enable caster for one shot
            skillCaster.SetCanCast(true);
            
            // Wait a frame to let it cast
            yield return new WaitForSeconds(0.1f);
            
            // Disable it again (we control when to cast via TryPhase2CircleSkill)
            skillCaster.SetCanCast(false);
        }

        // Post-action pause
        yield return new WaitForSeconds(postActionPause);

        currentState = ActionState.Idle;
    }
    
     public void ActivateLight() {
            if (LightGlow) LightGlow.SetActive(true);
            
        }

        public void DeactivateLight() {
            if (LightGlow) LightGlow.SetActive(false);
            
        }

        public void ActivateSmoke() {
            if (Smoke) Smoke.SetActive(true);
        }

        public void DeactivateSmoke() {
            if (Smoke) Smoke.SetActive(false);
        }
        

  

    void StopCurrentAction()
    {
        StopAllCoroutines();
        if (swordHitbox360) swordHitbox360.SetActive(false);
        if (LightGlow) LightGlow.SetActive(false);
        if (Smoke) Smoke.SetActive(false);
    }
    #endregion

    #region DEATH
    void OnDeath(EnemyHealthController controller)
    {
        if (currentState == ActionState.Dead) return;

        StopCurrentAction();
        currentState = ActionState.Dead;
        currentVelocity = Vector2.zero;
        rb.velocity = Vector2.zero;
        rb.simulated = false;

        anim.SetTrigger(animID_Dead);
        controller.PerformDefaultDeath();

    if (CyberDoor != null)
    {
        CyberDoor.OpenDoor();
    }
    else
    {
        Debug.LogWarning("CyberPig died but no DoorLever was assigned in the Inspector!");
    }
    }
    #endregion

    #region UTILITIES
    void UpdateFacing()
    {
        if (IsBusy || !player) return;
        FacePlayer();
    }

    /// <summary>
    /// Immediately face the player. Call this before starting an action.
    /// </summary>
    void FacePlayer()
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

        // Flip entire transform (affects all children - hitboxes, VFX, etc.)
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;

        // Also notify skill caster if it needs special handling
        if (skillCaster) skillCaster.FlipSpawnPoint(isFacingRight);
    }

   
    Vector2 GetFacingDirection()
    {
        return isFacingRight ? Vector2.right : Vector2.left;
    }
    #endregion

    #region GIZMOS
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, lungeRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, dashRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aggroRange);
    }
    #endregion
}