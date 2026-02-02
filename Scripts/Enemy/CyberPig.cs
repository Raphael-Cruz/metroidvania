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
     [SerializeField] private GameObject BossHealthPanel;   
 
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    #endregion

    #region RANGES
    [Header("Ranges")]
    [SerializeField] private float attackRange = 3.5f;
    [SerializeField] private float lungeRange = 6.0f;
    [SerializeField] private float dashRange = 10.5f;
    [SerializeField] private float aggroRange = 15f;
    #endregion

    #region SPEEDS & DURATIONS
    [Header("Speeds")]
    [SerializeField] private float walkSpeed = 3f;
    [SerializeField] private float dashSpeed = 14f;
    [SerializeField] private float lungeSpeed = 12f;

    [Header("Durations")]
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float lungeDuration = 0.2f;
    [SerializeField] private float postActionPause = 0.5f;
    [SerializeField] private float attackCooldown = 1.2f;
    #endregion

    #region HEALTH & PHASES
    [Header("Health System")]
    [SerializeField] private int maxHealth = 120;
    public int MaxHealth => maxHealth;

    [Header("Phase 2")]
    [SerializeField] private float circleSkillChance = 0.25f;
    [SerializeField] private float circleSkillCooldown = 4f;

    private int phase2Threshold;
    private bool triggeredPhase2;
    private bool isPhase2 = false;
 
    private float lastCircleSkillTime = -999f;
    private GameObject currentCircleSkillObject; // Tracks active skill to prevent double casting
    
    // Track the phase 2 coroutine so we don't kill it
    private Coroutine phase2Coroutine;
    #endregion

    #region ANIMATION PARAMETERS
    [Header("Animation Parameters - Phase 1")]
    [SerializeField] private string param_IsRunning = "isRunning";
    [SerializeField] private string param_IsRunning_Cast = "isRunning_Cast";
    
    [SerializeField] private string param_IsIdle = "isIdle";
    [SerializeField] private string param_IsIdle_Cast = "isIdle_Cast";
    
    [SerializeField] private string param_Dashing = "Dashing";
    [SerializeField] private string param_Dashing_Cast = "Dashing_Cast";
    
    [SerializeField] private string param_Swinging = "isSwinging";
    [SerializeField] private string param_Swinging_Cast = "isSwinging_Cast";

    [SerializeField] private string param_Lunge = "Lunge";
    [SerializeField] private string param_Lunge_Cast = "Lunge_Cast";

    [SerializeField] private string param_Dead = "Dead";
    
    [SerializeField] private string param_Circle_Skill_Casting = "Circle_Skill_Casting";

    [Header("Animation Parameters - Shared")]
    [SerializeField] private string param_Cast = "isCastingMagicSword";
    [SerializeField] private string param_IsPhase2 = "isPhase2";
    
    [Header("Door Lever")]
    [SerializeField] private DoorLever CyberDoor2;
        [Header("Door Lever")]
    [SerializeField] private DoorLever CyberDoor1;

    // Cached animation IDs - Phase 1
    private int animID_IsRunning;
    private int animID_IsIdle;
    private int animID_Dashing;
    private int animID_Swinging;
    private int animID_Lunge;
    private int animID_Dead;
    
    // Cached animation IDs - Phase 2
    private int animID_IsRunning_Cast;
    private int animID_IsIdle_Cast;
    private int animID_Dashing_Cast;
    private int animID_Swinging_Cast;
    private int animID_Lunge_Cast;
    private int animID_Circle_Skill_Casting;
    
    // Cached animation IDs - Shared
    private int animID_Cast;
    private int animID_IsPhase2;
    
    // Track which _Cast parameters exist in the Animator
    private bool hasIdleCast, hasRunningCast, hasDashingCast, hasSwingingCast, hasLungeCast;
    #endregion

    #region STATE
    private enum ActionState { Idle, Walking, Dashing, Lunging, Swinging, Casting, Dead }
    private ActionState currentState = ActionState.Idle;

    private bool IsBusy => currentState != ActionState.Idle && currentState != ActionState.Walking;

    private float lastAttackTime;
    private bool isFacingRight = true;
    private Vector2 currentVelocity;



    #endregion

    #region INITIALIZATION
    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!anim) anim = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (!healthController) healthController = GetComponent<EnemyHealthController>();

        // Cache animation hashes - Phase 1
        animID_IsRunning = Animator.StringToHash(param_IsRunning);
        animID_IsIdle = Animator.StringToHash(param_IsIdle);
        animID_Dashing = Animator.StringToHash(param_Dashing);
        animID_Swinging = Animator.StringToHash(param_Swinging);
        animID_Lunge = Animator.StringToHash(param_Lunge);
        animID_Dead = Animator.StringToHash(param_Dead);
        
        // Cache animation hashes - Phase 2
        animID_IsRunning_Cast = Animator.StringToHash(param_IsRunning_Cast);
        animID_IsIdle_Cast = Animator.StringToHash(param_IsIdle_Cast);
        animID_Dashing_Cast = Animator.StringToHash(param_Dashing_Cast);
        animID_Swinging_Cast = Animator.StringToHash(param_Swinging_Cast);
        animID_Lunge_Cast = Animator.StringToHash(param_Lunge_Cast);
        animID_Circle_Skill_Casting = Animator.StringToHash(param_Circle_Skill_Casting);
        
        // Cache animation hashes - Shared
        animID_Cast = Animator.StringToHash(param_Cast);
        animID_IsPhase2 = Animator.StringToHash(param_IsPhase2);
       

        // Ensure effects are inactive at start
        if (swordHitbox360) swordHitbox360.SetActive(false);
        if (LightGlow) LightGlow.SetActive(false);
        if (Smoke) Smoke.SetActive(false);
    }

    bool HasParameter(string paramName, AnimatorControllerParameterType type)
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
        
        // Check which _Cast parameters exist
        hasIdleCast = HasParameter(param_IsIdle_Cast, AnimatorControllerParameterType.Bool);
        hasRunningCast = HasParameter(param_IsRunning_Cast, AnimatorControllerParameterType.Bool);
        hasDashingCast = HasParameter(param_Dashing_Cast, AnimatorControllerParameterType.Trigger);
        hasSwingingCast = HasParameter(param_Swinging_Cast, AnimatorControllerParameterType.Trigger);
        hasLungeCast = HasParameter(param_Lunge_Cast, AnimatorControllerParameterType.Trigger);
        
       
    }
    #endregion

    #region UPDATE LOOPS
    void Update()
    {
        if (BossBattleState.IsInTransition)
{
    rb.velocity = Vector2.zero;
    return;
}
        if (currentState == ActionState.Dead || !player) return;

        if (!IsBusy)
        {
            DecideNextAction();
        }

        UpdateFacing();
    }

    void FixedUpdate()
    {
       if (BossBattleState.IsInTransition)
{
    rb.velocity = Vector2.zero;
    return;
}
        if (currentState == ActionState.Dead)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        rb.velocity = new Vector2(currentVelocity.x, rb.velocity.y);

        // Update running/idle animation based on movement
        if (anim && !IsBusy)
        {
            bool isMoving = Mathf.Abs(currentVelocity.x) > 0.1f;
            
            if (isPhase2)
            {
                anim.SetBool(animID_IsRunning_Cast, isMoving);
                anim.SetBool(animID_IsIdle_Cast, !isMoving);
            }
            else
            {
                anim.SetBool(animID_IsRunning, isMoving);
                anim.SetBool(animID_IsIdle, !isMoving);
            }
        }
    }
    #endregion

    #region AI DECISION
    void DecideNextAction()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        if (dist > aggroRange)
        {
            currentVelocity = Vector2.zero;
            return;
        }

        if (Time.time < lastAttackTime + attackCooldown)
        {
            if (dist > attackRange + 0.5f)
                WalkTowardPlayer();
            else
                currentVelocity = Vector2.zero;
            return;
        }

        if (isPhase2 && TryPhase2CircleSkill())
        {
            return;
        }

        if (dist <= attackRange)
        {
            StartCoroutine(DoSwingAttack());
        }
        else if (dist <= lungeRange)
        {
            StartCoroutine(DoLunge());
        }
        else if (dist <= dashRange)
        {
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
        // Don't cast if one is already active (wait for it to be destroyed)
        if (currentCircleSkillObject != null) return false;
        
        if (Time.time < lastCircleSkillTime + circleSkillCooldown)
            return false;

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
        StopMovementBools();
        currentVelocity = Vector2.zero;
        lastAttackTime = Time.time;

        FacePlayer();

        if (isPhase2)
            anim.SetTrigger(animID_Swinging_Cast);
        else
            anim.SetTrigger(animID_Swinging);

        yield return WaitForCurrentAnimation();
        yield return new WaitForSeconds(postActionPause);

        currentState = ActionState.Idle;
    }

    public void OpenSwordHitbox()
    {
        if (swordHitbox360) swordHitbox360.SetActive(true);
    }

    public void CloseSwordHitbox()
    {
        if (swordHitbox360) swordHitbox360.SetActive(false);
    }

    IEnumerator DoLunge()
    {
        currentState = ActionState.Lunging;
        StopMovementBools();
        currentVelocity = Vector2.zero;
        lastAttackTime = Time.time;

        FacePlayer();
        Vector2 lungeDir = GetFacingDirection();

        if (isPhase2)
            anim.SetTrigger(animID_Lunge_Cast);
        else
            anim.SetTrigger(animID_Lunge);

        yield return null;

        float elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            currentVelocity = lungeDir * lungeSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentVelocity = Vector2.zero;
        yield return WaitForCurrentAnimation();
        yield return new WaitForSeconds(postActionPause);

        currentState = ActionState.Idle;
    }

    IEnumerator DoDashLungeCombo()
    {
        lastAttackTime = Time.time;
        FacePlayer();
        Vector2 dashDir = GetFacingDirection();

        currentState = ActionState.Dashing;
        StopMovementBools();
        currentVelocity = Vector2.zero;

        if (isPhase2)
            anim.SetTrigger(animID_Dashing_Cast);
        else
            anim.SetTrigger(animID_Dashing);

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            currentVelocity = dashDir * dashSpeed;
            if (ghostTrail) ghostTrail.TrySpawnGhost(spriteRenderer, transform);
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.05f);

        float dist = Vector2.Distance(transform.position, player.position);
        
        if (dist <= attackRange)
        {
            FacePlayer();
            currentState = ActionState.Swinging;

            if (isPhase2)
                anim.SetTrigger(animID_Swinging_Cast);
            else
                anim.SetTrigger(animID_Swinging);
                
            yield return WaitForCurrentAnimation();
        }
        else if (dist <= lungeRange)
        {
            FacePlayer();
            Vector2 lungeDir = GetFacingDirection();
            currentState = ActionState.Lunging;

            if (isPhase2)
                anim.SetTrigger(animID_Lunge_Cast);
            else
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

        ResetAnimatorToIdle();
        yield return new WaitForSeconds(postActionPause);

        currentState = ActionState.Idle;
    }

    IEnumerator DoDashSwingCombo()
    {
        lastAttackTime = Time.time;
        FacePlayer();
        Vector2 dashDir = GetFacingDirection();

        currentState = ActionState.Dashing;
        StopMovementBools();
        currentVelocity = Vector2.zero;

        if (isPhase2)
            anim.SetTrigger(animID_Dashing_Cast);
        else
            anim.SetTrigger(animID_Dashing);

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            currentVelocity = dashDir * dashSpeed;
            if (ghostTrail) ghostTrail.TrySpawnGhost(spriteRenderer, transform);
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentVelocity = Vector2.zero;
        yield return new WaitForSeconds(0.05f);

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange + 1f)
        {
            FacePlayer();
            currentState = ActionState.Swinging;

            if (isPhase2)
                anim.SetTrigger(animID_Swinging_Cast);
            else
                anim.SetTrigger(animID_Swinging);
                
            yield return WaitForCurrentAnimation();
        }

        ResetAnimatorToIdle();
        yield return new WaitForSeconds(postActionPause);

        currentState = ActionState.Idle;
    }

    IEnumerator WaitForCurrentAnimation()
    {
        // Wait one frame to ensure the Animator has picked up the trigger and started transitioning
        yield return null;
        
        if (anim)
        {
            // If currently transitioning (e.g. from Idle to Attack), wait for the transition to finish
            while (anim.IsInTransition(0))
            {
                yield return null;
            }

            // Now wait for the actual animation to finish playing (normalizedTime >= 1.0)
            while (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
            {
                // Safety break if state is Idle/Running (means we somehow exited early)
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Idle") || 
                    anim.GetCurrentAnimatorStateInfo(0).IsName("Run") ||
                    anim.GetCurrentAnimatorStateInfo(0).IsTag("Motion")) 
                {
                    break; 
                }
                
                yield return null;
            }
        }
        else
        {
            // Fallback if no animator
            yield return new WaitForSeconds(0.5f);
        }
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
            // Store the coroutine reference so we don't kill it
            phase2Coroutine = StartCoroutine(TriggerPhase2());
        }
    }
    
IEnumerator TriggerPhase2()
{
    BossBattleState.IsInTransition = true;

    try
    {
        if (debugMode) Debug.Log("[PHASE2] FORCED TRANSITION START");

        healthController.isInvulnerable = true;

        rb.velocity = Vector2.zero;
        rb.simulated = false;

        currentState = ActionState.Casting;
        FacePlayer();

        anim.ResetTrigger(animID_Swinging);
        anim.ResetTrigger(animID_Lunge);
        anim.ResetTrigger(animID_Dashing);

        if (Screen_Flash_Shake.instance != null) 
        {
            Debug.Log("[FLASH_DEBUG] Calling TriggerFlash from CyberPig");
            Screen_Flash_Shake.instance.TriggerFlash(0.5f, 0.8f);
        }
        else
        {
            Debug.LogError("[FLASH_DEBUG] Screen_Flash_Shake.instance is NULL in CyberPig TriggerPhase2!");
        }

    if (CameraShake.instance != null)
        CameraShake.instance.Shake(0.5f, 0.3f);
        
         isPhase2 = true;
        anim.SetTrigger(animID_Cast);

        yield return new WaitForSeconds(3.5f);

      
        anim.SetBool(animID_IsPhase2, true);

        walkSpeed *= 1.15f;
        attackCooldown *= 0.85f;
    }
    finally
{
    rb.simulated = true;
    rb.velocity = Vector2.zero;

    healthController.isInvulnerable = false;
    BossBattleState.IsInTransition = false;

    ResetAnimatorToIdle(); 

    currentState = ActionState.Idle;

    if (debugMode) Debug.Log("[PHASE2] TRANSITION COMPLETE");
}
}

  IEnumerator CastCircleSkill()
{
    currentState = ActionState.Casting;
    currentVelocity = Vector2.zero;
    lastCircleSkillTime = Time.time;
    lastAttackTime = Time.time;

    FacePlayer();
    anim.SetTrigger(animID_Circle_Skill_Casting); // Triggers the "Circle_Skill_Casting" animation

    
    // wait for the animation length to return to Idle state
    yield return new WaitForSeconds(0.8f);
   
    currentState = ActionState.Idle;
}

// THIS METHOD IS CALLED BY THE ANIMATION EVENT
public void TriggerSkillProjectile()
{
    if (skillCaster != null)
    {
        skillCaster.CastCircleSkill();
    }
}
    
    // Animation events
    public void ActivateLight()
    {
        if (LightGlow) LightGlow.SetActive(true);
    }

    public void DeactivateLight()
    {
        if (LightGlow) LightGlow.SetActive(false);
    }

    public void ActivateSmoke()
    {
        if (Smoke) Smoke.SetActive(true);
    }

    public void DeactivateSmoke()
    {
        if (Smoke) Smoke.SetActive(false);
    }

  void StopCurrentAction()
{
    StopAllCoroutines();
    phase2Coroutine = null;

    if (swordHitbox360) swordHitbox360.SetActive(false);
    if (LightGlow) LightGlow.SetActive(false);
    if (Smoke) Smoke.SetActive(false);

    currentVelocity = Vector2.zero;
    currentState = ActionState.Idle;
}
    #endregion

    #region DEATH
    void OnDeath(EnemyHealthController controller)
    {
        if (currentState == ActionState.Dead) return;

        StopAllCoroutines(); // Now we CAN stop everything
        phase2Coroutine = null;
        
        currentState = ActionState.Dead;
        currentVelocity = Vector2.zero;
        rb.velocity = Vector2.zero;
        rb.simulated = false;

        if (swordHitbox360) swordHitbox360.SetActive(false);
        if (LightGlow) LightGlow.SetActive(false);
        if (Smoke) Smoke.SetActive(false);

        anim.SetTrigger(animID_Dead);
        controller.PerformDefaultDeath();

        if (CyberDoor1 != null)
        {
            CyberDoor1.OpenDoor();
        }
        if (CyberDoor2 != null)
        {
            CyberDoor2.OpenDoor();
        }

        if (BossHealthPanel) BossHealthPanel.SetActive(false);
    }
    #endregion

    #region UTILITIES
    void UpdateFacing()
    {
        if (IsBusy || !player) return;
        FacePlayer();
    }

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

        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    Vector2 GetFacingDirection()
    {
        return isFacingRight ? Vector2.right : Vector2.left;
    }

    void ResetAnimatorToIdle()
    {
        if (!anim) return;
        
        // Ensure "Run" is off
        StopMovementBools();

        if (isPhase2)
        {
            // Phase 2: Use _Cast versions
            anim.SetBool(animID_IsIdle_Cast, true);
            anim.SetBool(animID_IsRunning_Cast, false);
            
            // Turn off Phase 1 bools
            anim.SetBool(animID_IsIdle, false);
            anim.SetBool(animID_IsRunning, false);
            
            // Reset Phase 2 triggers
            anim.ResetTrigger(animID_Swinging_Cast);
            anim.ResetTrigger(animID_Lunge_Cast);
            anim.ResetTrigger(animID_Dashing_Cast);
            
            if (debugMode) Debug.Log("[RESET] Reset to Phase 2 Idle (isIdle_Cast)");
        }
        else
        {
            // Phase 1: Use normal versions
            anim.SetBool(animID_IsIdle, true);
            anim.SetBool(animID_IsRunning, false);
            
            // Reset Phase 1 triggers
            anim.ResetTrigger(animID_Swinging);
            anim.ResetTrigger(animID_Lunge);
            anim.ResetTrigger(animID_Dashing);
            
            if (debugMode) Debug.Log("[RESET] Reset to Phase 1 Idle (isIdle)");
        }
        
        // Always reset shared triggers
        anim.ResetTrigger(animID_Cast);
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

    void StopMovementBools()
    {
        if (!anim) return;
        // Turn off all running bools immediately
        anim.SetBool(animID_IsRunning, false);
        anim.SetBool(animID_IsRunning_Cast, false);
        // Ensure Idle is true (so it doesn't get stuck in a weird state, state machine will exit Idle to Attack via trigger)
        // Actually, usually we want to let the Attack state handle it.
        // But if we force Running OFF, the transition condition "Running -> Exit" or "Any State -> Attack" should work.
    }
}