using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;


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
    [SerializeField] private GameObject AccessCard; 
    [SerializeField] private GameObject CrossHitBox; 
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
    
    // NEW: Animation timing overrides (set to 0 to use actual animation length)
    [Header("Animation Timing Overrides")]
    [SerializeField] private float swingAnimDuration = 0f; // 0 = auto-detect
    [SerializeField] private float lungeAnimDuration = 0f;
    [SerializeField] private float dashAnimDuration = 0f;
    [SerializeField] private float circleSkillCastDuration = 0.8f; // You had this hardcoded
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
    
    // Track the phase 2 coroutine so we don't kill it during death
    private Coroutine phase2Coroutine;
    #endregion

    #region ANIMATION PARAMETERS
    [Header("Animation Parameters - Phase 1")]
    [SerializeField] private string param_IsRunning = "isRunning";
    [SerializeField] private string param_IsIdle = "isIdle";
    [SerializeField] private string param_Dashing = "Dashing";
    [SerializeField] private string param_Swinging = "isSwinging";
    [SerializeField] private string param_Lunge = "Lunge";
    [SerializeField] private string param_Dead = "Dead";

    [Header("Animation Parameters - Phase 2")]
    [SerializeField] private string param_IsRunning_Cast = "isRunning_Cast";
    [SerializeField] private string param_IsIdle_Cast = "isIdle_Cast";
    [SerializeField] private string param_Dashing_Cast = "Dashing_Cast";
    [SerializeField] private string param_Swinging_Cast = "isSwinging_Cast";
    [SerializeField] private string param_Lunge_Cast = "Lunge_Cast";
    [SerializeField] private string param_Circle_Skill_Casting = "Circle_Skill_Casting";

    [Header("Animation Parameters - Shared")]
    [SerializeField] private string param_Cast = "isCastingMagicSword";
    [SerializeField] private string param_IsPhase2 = "isPhase2";

    // Cached animation IDs
    private int animID_IsRunning, animID_IsIdle, animID_Dashing, animID_Swinging, animID_Lunge, animID_Dead;
    private int animID_IsRunning_Cast, animID_IsIdle_Cast, animID_Dashing_Cast, animID_Swinging_Cast, animID_Lunge_Cast;
    private int animID_Circle_Skill_Casting, animID_Cast, animID_IsPhase2;
    #endregion

    #region STATE
    private enum ActionState { Idle, Walking, Dashing, Lunging, Swinging, Casting, Dead }
    private ActionState currentState = ActionState.Idle;
    private bool IsBusy => currentState != ActionState.Idle && currentState != ActionState.Walking;

    private float lastAttackTime;
    private bool isFacingRight = true;
    private Vector2 currentVelocity;
    
    // NEW: Track active coroutines to prevent conflicts
    private Coroutine activeActionCoroutine;
    #endregion

    #region INITIALIZATION
    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!anim) anim = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (!healthController) healthController = GetComponent<EnemyHealthController>();

        CacheAnimationIDs();
        DisableAllEffects();
    }

    void CacheAnimationIDs()
    {
        // Phase 1
        animID_IsRunning = Animator.StringToHash(param_IsRunning);
        animID_IsIdle = Animator.StringToHash(param_IsIdle);
        animID_Dashing = Animator.StringToHash(param_Dashing);
        animID_Swinging = Animator.StringToHash(param_Swinging);
        animID_Lunge = Animator.StringToHash(param_Lunge);
        animID_Dead = Animator.StringToHash(param_Dead);
        
        // Phase 2
        animID_IsRunning_Cast = Animator.StringToHash(param_IsRunning_Cast);
        animID_IsIdle_Cast = Animator.StringToHash(param_IsIdle_Cast);
        animID_Dashing_Cast = Animator.StringToHash(param_Dashing_Cast);
        animID_Swinging_Cast = Animator.StringToHash(param_Swinging_Cast);
        animID_Lunge_Cast = Animator.StringToHash(param_Lunge_Cast);
        animID_Circle_Skill_Casting = Animator.StringToHash(param_Circle_Skill_Casting);
        
        // Shared
        animID_Cast = Animator.StringToHash(param_Cast);
        animID_IsPhase2 = Animator.StringToHash(param_IsPhase2);
    }

    void DisableAllEffects()
    {
        if (swordHitbox360) swordHitbox360.SetActive(false);
        if (AccessCard) AccessCard.SetActive(false);
        if (CrossHitBox) CrossHitBox.SetActive(false);
        if (LightGlow) LightGlow.SetActive(false);
        if (Smoke) Smoke.SetActive(false);
    }

    void Start()
    {
        SetupHealth();
        FindPlayer();
        ResetAnimatorToIdle();
    }

    void SetupHealth()
    {
        if (healthController)
        {
            if (maxHealth > 0) 
            {
                healthController.totalhealth = maxHealth;
                if (debugMode) Debug.Log($"[CyberPig] Health set to max: {maxHealth}");
                
                if (BossHealthUI.instance != null)
                {
                   BossHealthUI.instance.SetBoss(healthController);
                   BossHealthUI.instance.RefreshUI();
                }
            }
            healthController.onDeathCallback = OnDeath;
            healthController.onDamageCallback = (_) => CheckPhaseTransitions();
        }

        int hp = healthController ? healthController.totalhealth : maxHealth;
        phase2Threshold = Mathf.RoundToInt(hp * 0.6f);
    }

    void FindPlayer()
    {
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

        // Update movement animation based on velocity
        UpdateMovementAnimation();
    }

    void UpdateMovementAnimation()
    {
        if (!anim || IsBusy) return;

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

        // Phase 2 circle skill check
        if (isPhase2 && TryPhase2CircleSkill())
        {
            return;
        }

        // Attack decision tree
        if (dist <= attackRange)
        {
            StartActionCoroutine(DoSwingAttack());
        }
        else if (dist <= lungeRange)
        {
            StartActionCoroutine(DoLunge());
        }
        else if (dist <= dashRange)
        {
            if (Random.value < 0.4f)
                StartActionCoroutine(DoDashSwingCombo());
            else
                StartActionCoroutine(DoDashLungeCombo());
        }
        else
        {
            WalkTowardPlayer();
        }
    }

    bool TryPhase2CircleSkill()
    {
        if (Time.time < lastCircleSkillTime + circleSkillCooldown)
            return false;

        if (Random.value < circleSkillChance)
        {
            StartActionCoroutine(CastCircleSkill());
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
    
    // NEW: Safely start action coroutines (prevents overlapping actions)
    void StartActionCoroutine(IEnumerator routine)
    {
        if (activeActionCoroutine != null)
        {
            StopCoroutine(activeActionCoroutine);
        }
        activeActionCoroutine = StartCoroutine(routine);
    }
    #endregion

    #region ACTIONS
    IEnumerator DoSwingAttack()
    {
        currentState = ActionState.Swinging;
        StopMovement();
        FacePlayer();

        TriggerAnimation(isPhase2 ? animID_Swinging_Cast : animID_Swinging);

        yield return WaitForAnimation(swingAnimDuration);
        yield return new WaitForSeconds(postActionPause);

        FinishAction();
    }

    IEnumerator DoLunge()
    {
        currentState = ActionState.Lunging;
        StopMovement();
        CloseCrossHitbox(); // Safety
        FacePlayer();
        
        Vector2 lungeDir = GetFacingDirection();
        TriggerAnimation(isPhase2 ? animID_Lunge_Cast : animID_Lunge);

        yield return null; // Wait one frame for animation to start

        float elapsed = 0f;
        while (elapsed < lungeDuration)
        {
            currentVelocity = lungeDir * lungeSpeed;
            elapsed += Time.deltaTime;
            yield return null;
        }

        StopMovement();
        yield return WaitForAnimation(lungeAnimDuration);
        yield return new WaitForSeconds(postActionPause);

        FinishAction();
    }

    IEnumerator DoDashLungeCombo()
    {
        FacePlayer();
        Vector2 dashDir = GetFacingDirection();

        // DASH PHASE
        currentState = ActionState.Dashing;
        StopMovement();
        CloseCrossHitbox();

        TriggerAnimation(isPhase2 ? animID_Dashing_Cast : animID_Dashing);

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            currentVelocity = dashDir * dashSpeed;
            if (ghostTrail) ghostTrail.TrySpawnGhost(spriteRenderer, transform);
            elapsed += Time.deltaTime;
            yield return null;
        }

        StopMovement();
        yield return new WaitForSeconds(0.05f);

        // FOLLOW-UP ATTACK
        float dist = Vector2.Distance(transform.position, player.position);
        
        if (dist <= attackRange)
        {
            FacePlayer();
            currentState = ActionState.Swinging;
            TriggerAnimation(isPhase2 ? animID_Swinging_Cast : animID_Swinging);
            yield return WaitForAnimation(swingAnimDuration);
        }
        else if (dist <= lungeRange)
        {
            FacePlayer();
            Vector2 lungeDir = GetFacingDirection();
            currentState = ActionState.Lunging;
            TriggerAnimation(isPhase2 ? animID_Lunge_Cast : animID_Lunge);
            
            yield return null;

            elapsed = 0f;
            while (elapsed < lungeDuration)
            {
                currentVelocity = lungeDir * lungeSpeed;
                elapsed += Time.deltaTime;
                yield return null;
            }

            StopMovement();
            yield return WaitForAnimation(lungeAnimDuration);
        }

        yield return new WaitForSeconds(postActionPause);
        FinishAction();
    }

    IEnumerator DoDashSwingCombo()
    {
        FacePlayer();
        Vector2 dashDir = GetFacingDirection();

        // DASH PHASE
        currentState = ActionState.Dashing;
        StopMovement();

        TriggerAnimation(isPhase2 ? animID_Dashing_Cast : animID_Dashing);

        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            currentVelocity = dashDir * dashSpeed;
            if (ghostTrail) ghostTrail.TrySpawnGhost(spriteRenderer, transform);
            elapsed += Time.deltaTime;
            yield return null;
        }

        StopMovement();
        yield return new WaitForSeconds(0.05f);

        // SWING IF IN RANGE
        float dist = Vector2.Distance(transform.position, player.position);
        if (dist <= attackRange + 1f)
        {
            FacePlayer();
            currentState = ActionState.Swinging;
            TriggerAnimation(isPhase2 ? animID_Swinging_Cast : animID_Swinging);
            yield return WaitForAnimation(swingAnimDuration);
        }

        yield return new WaitForSeconds(postActionPause);
        FinishAction();
    }

    IEnumerator CastCircleSkill()
    {
        currentState = ActionState.Casting;
        StopMovement();
        lastCircleSkillTime = Time.time;

        FacePlayer();
        TriggerAnimation(animID_Circle_Skill_Casting);
        
        yield return new WaitForSeconds(circleSkillCastDuration);
       
        FinishAction();
    }

    // Called by Animation Event
    public void TriggerSkillProjectile()
    {
        if (skillCaster != null)
        {
            skillCaster.CastCircleSkill();
        }
    }
    #endregion

    #region ANIMATION HELPERS
    void TriggerAnimation(int triggerID)
    {
        if (!anim) return;
        
        // Reset all triggers first to prevent conflicts
        ResetAllTriggers();
        
        // Set the new trigger
        anim.SetTrigger(triggerID);
        
        if (debugMode) Debug.Log($"[CyberPig] Triggered animation: {triggerID}");
    }

    void ResetAllTriggers()
    {
        if (!anim) return;
        
        // Phase 1 triggers
        anim.ResetTrigger(animID_Swinging);
        anim.ResetTrigger(animID_Lunge);
        anim.ResetTrigger(animID_Dashing);
        
        // Phase 2 triggers
        anim.ResetTrigger(animID_Swinging_Cast);
        anim.ResetTrigger(animID_Lunge_Cast);
        anim.ResetTrigger(animID_Dashing_Cast);
        anim.ResetTrigger(animID_Circle_Skill_Casting);
        
        // Shared
        anim.ResetTrigger(animID_Cast);
    }

    IEnumerator WaitForAnimation(float overrideDuration = 0f)
    {
        // If override duration is provided, use it
        if (overrideDuration > 0f)
        {
            yield return new WaitForSeconds(overrideDuration);
            yield break;
        }

        // Otherwise, wait for actual animation to complete
        yield return null; // Wait one frame for transition
        
        if (!anim) 
        {
            yield return new WaitForSeconds(0.5f);
            yield break;
        }

        // Wait for transition to finish
        while (anim.IsInTransition(0))
        {
            yield return null;
        }

        // Get the current animation state
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        float previousNormalizedTime = stateInfo.normalizedTime;

        // Wait until animation completes OR loops back
        while (stateInfo.normalizedTime < 1.0f)
        {
            stateInfo = anim.GetCurrentAnimatorStateInfo(0);
            
            // Safety: Break if we're back in idle/motion state
            if (stateInfo.IsTag("Motion")) 
            {
                if (debugMode) Debug.Log("[CyberPig] Animation returned to Motion state early");
                break;
            }

            // Loop detection: if time wraps around (current < previous)
            if (stateInfo.normalizedTime < previousNormalizedTime - 0.1f)
            {
                if (debugMode) Debug.LogWarning("[CyberPig] Animation loop detected, breaking wait");
                break;
            }

            previousNormalizedTime = stateInfo.normalizedTime;
            yield return null;
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
            phase2Coroutine = StartCoroutine(TriggerPhase2());
        }
    }
    
    IEnumerator TriggerPhase2()
    {
        BossBattleState.IsInTransition = true;

        try
        {
            if (debugMode) Debug.Log("[PHASE2] TRANSITION START");

            healthController.isInvulnerable = true;
            rb.simulated = false;
            StopMovement();

            currentState = ActionState.Casting;
            FacePlayer();

            ResetAllTriggers();

            // Flash and shake effects
            if (Screen_Flash_Shake.instance != null) 
            {
                Screen_Flash_Shake.instance.TriggerFlash(0.5f, 0.8f);
            }

            if (CameraShake.instance != null)
            {
                CameraShake.instance.Shake(0.5f, 0.3f);
            }
            
            isPhase2 = true;
            anim.SetTrigger(animID_Cast);

            yield return new WaitForSeconds(3.5f);

            anim.SetBool(animID_IsPhase2, true);

            // Phase 2 buffs
            walkSpeed *= 1.15f;
            attackCooldown *= 0.85f;
        }
        finally
        {
            rb.simulated = true;
            StopMovement();
            healthController.isInvulnerable = false;
            BossBattleState.IsInTransition = false;
            ResetAnimatorToIdle();
            currentState = ActionState.Idle;

            if (debugMode) Debug.Log("[PHASE2] TRANSITION COMPLETE");
        }
    }
    #endregion

    #region HITBOX CONTROL (Called by Animation Events)
    public void OpenSwordHitbox()
    {
        if (swordHitbox360) swordHitbox360.SetActive(true);
    }

    public void CloseSwordHitbox()
    {
        if (swordHitbox360) swordHitbox360.SetActive(false);
    }

    public void OpenCrossHitbox()
    {
        if (CrossHitBox) CrossHitBox.SetActive(true);   
    }

    public void CloseCrossHitbox()
    {
        if (CrossHitBox) CrossHitBox.SetActive(false);
    }

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
    #endregion

    #region DEATH
    void OnDeath(EnemyHealthController controller)
    {
        if (currentState == ActionState.Dead) return;

        StopAllCoroutines();
        phase2Coroutine = null;
        activeActionCoroutine = null;
        
        currentState = ActionState.Dead;
        StopMovement();
        rb.simulated = false;

        DisableAllEffects();

        // Drop access card
        if (AccessCard != null) 
        {
            GameObject cardDrop = Instantiate(AccessCard, transform.position, Quaternion.identity);
            cardDrop.SetActive(true);
            cardDrop.transform.SetParent(null); 
            cardDrop.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            
            if (debugMode) Debug.Log("[CyberPig] Access Card dropped");
        }

        anim.SetTrigger(animID_Dead);
        controller.PerformDefaultDeath();

        if (BossHealthUI.instance != null) 
            BossHealthUI.instance.gameObject.SetActive(false);

        // Play normal music after boss dies
        if (MusicManager.instance != null)
{
    Scene currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

    foreach (var item in MusicManager.instance.playlist)
    {
        if (item.sceneName == currentScene.name)
        {
            MusicManager.instance.PlayTrack(item.track);
            break;
        }
    }
}
    }
    #endregion

    #region UTILITIES
    void StopMovement()
    {
        currentVelocity = Vector2.zero;
        rb.velocity = Vector2.zero;
        StopMovementBools();
    }

    void StopMovementBools()
    {
        if (!anim) return;
        anim.SetBool(animID_IsRunning, false);
        anim.SetBool(animID_IsRunning_Cast, false);
    }

    void FinishAction()
    {
        lastAttackTime = Time.time;
        ResetAnimatorToIdle();
        currentState = ActionState.Idle;
        activeActionCoroutine = null;
    }

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
        
        // Notify skill caster to flip spawn point
        if (skillCaster != null)
        {
            skillCaster.FlipSpawnPoint(isFacingRight);
        }
    }

    Vector2 GetFacingDirection()
    {
        return isFacingRight ? Vector2.right : Vector2.left;
    }

    void ResetAnimatorToIdle()
    {
        if (!anim) return;

        CloseCrossHitbox();
        CloseSwordHitbox();
        StopMovementBools();
        ResetAllTriggers();

        if (isPhase2)
        {
            anim.SetBool(animID_IsIdle_Cast, true);
            anim.SetBool(animID_IsIdle, false);
        }
        else
        {
            anim.SetBool(animID_IsIdle, true);
            anim.SetBool(animID_IsIdle_Cast, false);
        }
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