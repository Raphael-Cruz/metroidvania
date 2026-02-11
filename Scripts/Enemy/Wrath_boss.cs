using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wrath_boss : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private EnemyHealthController healthController;
     [SerializeField] private FireballRainSpawner fireballRainSpawner;
    
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashDuration = 0.5f;
    
    [Header("Combat Settings")]
    [SerializeField] private float scytheRange = 2f;
    [SerializeField] private float meleeAttackRange = 2.5f;
    [SerializeField] private float spellCastRange = 8f;
    [SerializeField] private float optimalSpellRange = 5f; // Sweet spot for casting
    
    [Header("Cooldowns")]
    [SerializeField] private float scytheCooldown = 1.5f;
    [SerializeField] private float dashCooldown = 3f;
    [SerializeField] private float spellCooldown = 5f;
    [SerializeField] private float minTimeBetweenSpells = 3f;
    
    [Header("Decision Making")]
    [SerializeField] private float decisionInterval = 0.5f;
    [SerializeField] private float aggressionLevel = 0.6f; // 0-1, higher = more aggressive melee

    [Header ("Skills")]
     
    [SerializeField] private GameObject fireRain; 
    [SerializeField] private GameObject fireballPrefab; 
    [SerializeField] private GameObject sunkidama;   


 
    [Header("Health System")]
    [SerializeField] private int maxHealth = 120;
    public int MaxHealth => maxHealth;

  [Header("Animation Parameters")]
    [SerializeField] private string param_IsRunning = "isRunning";
    [SerializeField] private string param_FireRain = "fireRain";
    [SerializeField] private string param_Sunkidama = "sunkidama";

    // State tracking
    private bool isCasting = false;
    private bool isDashing = false;
    private bool isAttacking = false;
    private bool isAnySpellActive = false;
 

    // Physics
    private Vector2 currentVelocity;
    private bool isFacingRight = true; // Assuming default sprite faces right
    
    // Cooldown timers
    private float scytheTimer = 0f;
    private float dashTimer = 0f;
    private float spellTimer = 0f;
    private float lastSpellCastTime = 0f;
    
    // Decision making
    private float decisionTimer = 0f;
    private BossAction currentAction = BossAction.Idle;
    private string currentState;

    // Cached animation IDs
     private int animID_IsRunning;
     private int animID_Fire;
     private int animID_Sunkidama;



    
    private enum BossAction
    {
        Idle,
        Chase,
        Retreat,
        Dash,
        ScytheAttack,
        CastFire,
        CastFireProjectile,
        CastSunkidama
    }
    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody2D>();
        if (!animator) animator = GetComponent<Animator>();
        if (!spriteRenderer) spriteRenderer = GetComponent<SpriteRenderer>();
        if (!healthController) healthController = GetComponent<EnemyHealthController>();

         animID_IsRunning = Animator.StringToHash(param_IsRunning);
         animID_Fire = Animator.StringToHash(param_FireRain);
         animID_Sunkidama = Animator.StringToHash(param_Sunkidama);

  
    
    }
    void Start()
    {
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        // Initialize facing direction based on sprite flip if needed, or assume default
        // If spriteRenderer.flipX is true initially, it means we are facing left
        if (spriteRenderer != null && spriteRenderer.flipX) 
            isFacingRight = false;
    }
    
    void Update()
    {
        if (player == null || isCasting || isDashing || isAttacking) 
        {
            // If busy, zero out velocity unless dashing handles it (dashing might use velocity)
            if (!isDashing) currentVelocity = Vector2.zero;
            return;
        }
        
        UpdateCooldowns();
        
        decisionTimer -= Time.deltaTime;
        
        if (decisionTimer <= 0f)
        {
            DecideNextAction();
            decisionTimer = decisionInterval;
        }
        
        ExecuteCurrentAction();
        
        // Face player if not busy
        if (!isCasting && !isDashing && !isAttacking)
        {
            FacePlayer();
        }
    }

    void FixedUpdate()
    {
        if (isDashing) return; // Dashing handles its own movement or velocity

        // Apply velocity
        if (rb != null)
        {
            rb.velocity = new Vector2(currentVelocity.x, rb.velocity.y);
        }

        // Handle Animation
        if (animator != null)
        {
            // Use currentVelocity (intent) instead of rb.velocity (physics result) to prevent jitter/early stops
            bool isMoving = Mathf.Abs(currentVelocity.x) > 0.1f;
            animator.SetBool(animID_IsRunning, isMoving);
        }
    }
    
    void UpdateCooldowns()
    {
        scytheTimer -= Time.deltaTime;
        dashTimer -= Time.deltaTime;
        spellTimer -= Time.deltaTime;
    }
    
    void DecideNextAction()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Priority 1: Scythe attack if in range and ready
        if (distanceToPlayer <= scytheRange && scytheTimer <= 0f)
        {
            currentAction = BossAction.ScytheAttack;
            return;
        }
        
        // Priority 2: Spell casting decision
        if (CanCastSpell() && ShouldCastSpell(distanceToPlayer))
        {
            currentAction = ChooseRandomSpell();
            return;
        }
        
        // Priority 3: Dash towards player if they're mid-range
        if (distanceToPlayer > meleeAttackRange && distanceToPlayer < spellCastRange && dashTimer <= 0f)
        {
            if (Random.value > 0.5f) // 50% chance to dash
            {
                currentAction = BossAction.Dash;
                return;
            }
        }
        
        // Priority 4: Chase or retreat based on optimal range
        if (distanceToPlayer > meleeAttackRange)
        {
            currentAction = BossAction.Chase;
        }
        else if (distanceToPlayer < scytheRange * 0.5f)
        {
            // Too close, back up slightly
            currentAction = BossAction.Retreat;
        }
        else
        {
            currentAction = BossAction.Idle;
        }
    }
    
    bool CanCastSpell()
    {
        return spellTimer <= 0f && 
               !isAnySpellActive && 
               Time.time - lastSpellCastTime >= minTimeBetweenSpells;
    }
    
    bool ShouldCastSpell(float distance)
    {
        // More likely to cast at optimal range
        if (distance >= meleeAttackRange && distance <= spellCastRange)
        {
            // Aggression affects spell vs melee choice
            float spellChance = 1f - aggressionLevel;
            
            // Bonus chance if at optimal range
            if (distance >= optimalSpellRange - 1f && distance <= optimalSpellRange + 1f)
            {
                spellChance += 0.3f;
            }
            
            return Random.value < spellChance;
        }
        
        return false;
    }
    
    BossAction ChooseRandomSpell()
    {
        float roll = Random.value;
        
        if (roll < 0.33f)
            return BossAction.CastFire;
        else if (roll < 0.66f)
            return BossAction.CastFireProjectile;
        else
            return BossAction.CastSunkidama;
    }
    
    void ExecuteCurrentAction()
    {
        if (isCasting || isDashing || isAttacking)
    return;

        switch (currentAction)
        {
            case BossAction.Chase:
                MoveTowardsPlayer();
                break;
                
            case BossAction.Retreat:
                MoveAwayFromPlayer();
                break;
                
            case BossAction.Dash:
                StartCoroutine(DashTowardsPlayer());
                break;
                
            case BossAction.ScytheAttack:
                StartCoroutine(PerformScytheAttack());
                break;
                
            case BossAction.CastFire:
              isCasting = true; 
              
            StartCoroutine(CastFireRain());
                   
                break;
                
            case BossAction.CastFireProjectile:
              isCasting = true; 
                StartCoroutine(CastFireProjectile());
                break;
                
            case BossAction.CastSunkidama:
                isCasting = true; 
                StartCoroutine(CastSunkidama());
                break;
                
            case BossAction.Idle:
                currentVelocity = Vector2.zero;
                // Animation handled by FixedUpdate (velocity check)
                break;
        }
    }
    
    void MoveTowardsPlayer()
    {
        float dir = player.position.x > transform.position.x ? 1f : -1f;
        currentVelocity = new Vector2(dir * moveSpeed, 0f);
    }
    
    void MoveAwayFromPlayer()
    {
        float dir = player.position.x > transform.position.x ? -1f : 1f; // Opposition direction
        currentVelocity = new Vector2(dir * moveSpeed * 0.5f, 0f);
    }
    
    IEnumerator DashTowardsPlayer()
    {
        isDashing = true;
        dashTimer = dashCooldown;
        
        // Stop normal movement
        currentVelocity = Vector2.zero;
        if (rb) rb.velocity = Vector2.zero;

        FacePlayer(); // Ensure facing correct way before dash
        
        Vector2 dashDirection = (player.position - transform.position).normalized;
        // Lock Y for ground dash if needed, or allow diagonal. Assuming ground boss:
        dashDirection.y = 0; 
        dashDirection.Normalize();

        SetAnimationState("Dash");
        
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
             if (rb) rb.velocity = dashDirection * dashSpeed;
            elapsed += Time.deltaTime;
            yield return new WaitForFixedUpdate(); // Use FixedUpdate for physics sync
        }
        
        if (rb) rb.velocity = Vector2.zero;
        isDashing = false;
        currentAction = BossAction.Idle;
    }
    
    IEnumerator PerformScytheAttack()
    {
        isAttacking = true;
        scytheTimer = scytheCooldown;
        currentVelocity = Vector2.zero; // Stop moving
        if (rb) rb.velocity = Vector2.zero;
        
        FacePlayer();
        SetAnimationState("ScytheSwing");
        
        // Wait for animation (adjust timing as needed)
        yield return new WaitForSeconds(0.6f);
        
        isAttacking = false;
        currentAction = BossAction.Idle;
    }
    

IEnumerator CastFireRain()
{
    isCasting = true;
    isAnySpellActive = true;

    spellTimer = spellCooldown;
    lastSpellCastTime = Time.time;

    currentVelocity = Vector2.zero;
    if (rb) rb.velocity = Vector2.zero;

    FacePlayer();


    animator.SetBool("IsChannelingFireRain", true);

    // Wait until casting animation finishes
    yield return null;

    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    while (stateInfo.normalizedTime < 1f)
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return null;
    }

    fireballRainSpawner.StartFireballRain();

   


    while (fireballRainSpawner.IsRaining)   
    {
        yield return null;
    }

  
    animator.SetBool("IsChannelingFireRain", false);

    isCasting = false;
    isAnySpellActive = false;
    currentAction = BossAction.Idle;
}

IEnumerator CastFireProjectile()
{
    isCasting = true;
    isAnySpellActive = true;

    spellTimer = spellCooldown;
    lastSpellCastTime = Time.time;

    currentVelocity = Vector2.zero;
    if (rb) rb.velocity = Vector2.zero;

    FacePlayer();

    animator.SetBool("IsChannelingFireProjectile", true);

    yield return null;

    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    while (stateInfo.normalizedTime < 1f)
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return null;
    }

    Vector2 playerLastPosition = player.position;

    GameObject fireball = Instantiate(
        fireballPrefab,
        transform.position + Vector3.up * 2f,
        Quaternion.identity
    );

    FireBall fireballScript = fireball.GetComponent<FireBall>();

    if (fireballScript != null)
        fireballScript.Init(playerLastPosition);
    else
        Debug.LogError("FireBall script missing on prefab!");

    while (fireball != null)
        yield return null;

    animator.SetBool("IsChannelingFireProjectile", false);

    isCasting = false;
    isAnySpellActive = false;
    currentAction = BossAction.Idle;
}


IEnumerator CastSunkidama()
{
    isCasting = true;
    isAnySpellActive = true;

    spellTimer = spellCooldown;
    lastSpellCastTime = Time.time;

    currentVelocity = Vector2.zero;
    if (rb) rb.velocity = Vector2.zero;

    FacePlayer();

    
    animator.SetTrigger("CastSunkidama");

    // Wait for casting animation to finish
    yield return null;

    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    while (stateInfo.normalizedTime < 1f)
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return null;
    }

    
    Vector2 playerLastPosition = player.position;

    GameObject sun = Instantiate(
        sunkidama,
        transform.position + Vector3.up * 2f,
        Quaternion.identity
    );

    Sunkidama sunScript = sun.GetComponent<Sunkidama>();
    sun.SetActive(true);
    sunScript.Init(playerLastPosition);

    
    animator.SetBool("IsChannelingSunkidama", true);

    
    while (sun != null)
    {
        yield return null;
    }

 
    animator.SetBool("IsChannelingSunkidama", false);

    isCasting = false;
    isAnySpellActive = false;
    currentAction = BossAction.Idle;
}



    
    void ActivateSpell(string skillName)
    {
        // Call your spell logic here based on skillName
        switch (skillName)
        {
            case "FireRain":
                // Trigger fire skill
              
                fireballRainSpawner.StartFireballRain();
                
                break;
            case "FireProjectile":
                // Trigger fire projectile
                Debug.Log("Fire Projectile activated!");
                
                break;
            case "Sunkidama":
            

            if (player == null)
            {
                
                return;
            }

            if (sunkidama == null)
            {
               
                return;
            }

            Vector2 playerLastPosition = player.position;

            GameObject sun = Instantiate(
                sunkidama,
                transform.position + Vector3.up * 2f,
                Quaternion.identity
            );

            Sunkidama sunScript = sun.GetComponent<Sunkidama>();

            if (sunScript == null)
            {
                
                return;
            }

            sun.SetActive(true);
            sunScript.Init(playerLastPosition);
            break;
                }
    }
    
    void FacePlayer()
    {
        if (player == null) return;
        float direction = player.position.x - transform.position.x;
        
        if (direction > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (direction < 0 && isFacingRight)
        {
            Flip();
        }
    }
    
    void Flip()
    {
        isFacingRight = !isFacingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
    
    void SetAnimationState(string stateName)
    {
        // Prevent restarting the animation if it's already playing
        if (currentState == stateName) return;

        if (animator != null)
        {
            // Adjust these to match your animation controller
            animator.Play(stateName);
            currentState = stateName;
        }
    }
}
