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
    
    [Header("Spawn Points")]
    [SerializeField] private Transform fireballSpawnPoint;
    [SerializeField] private Transform sunkidamaSpawnPoint;
    
    [Header("Movement Settings")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float screenWidth = 30f; // Adjust to match your game's screen width


    [Header("Combat Settings")]
    
    [Header("Cooldowns")]
    [SerializeField] private float dashCooldown = 3f;
    [SerializeField] private float spellCooldown = 5f;
    [SerializeField] private float minTimeBetweenSpells = 3f;
    
    [Header("Decision Making")]
    [SerializeField] private float decisionInterval = 0.5f;
  

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
    
    // Phase 2 system
    private bool isPhase2 = false;
    private const float PHASE2_HP_THRESHOLD = 0.6f; // 60% HP
    private int currentHealth;
 

    // Physics
    private Vector2 currentVelocity;
    private bool isFacingRight = true; // Assuming default sprite faces right
    
    // Cooldown timers
 
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
        Dash,
   
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
     
        if (spriteRenderer != null && spriteRenderer.flipX) 
            isFacingRight = false;
            
        // Initialize health for Phase 2 tracking
        if (healthController != null)
            currentHealth = healthController.totalhealth;
        else
            currentHealth = maxHealth;
            
        // Make boss immovable by physics (collisions, shots, etc.)
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }
    }
    
    void Update()
    {
       
        if (!isPhase2 && healthController != null)
        {
            currentHealth = healthController.totalhealth;
            float hpPercentage = (float)currentHealth / maxHealth;
            
            if (hpPercentage <= PHASE2_HP_THRESHOLD)
            {
                EnterPhase2();
            }
        }
        
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
        if (isDashing) return; 

        // Apply velocity
        if (rb != null)
        {
            rb.velocity = new Vector2(currentVelocity.x, rb.velocity.y);
        }

        if (animator != null)
        {
           
            bool isMoving = Mathf.Abs(currentVelocity.x) > 0.1f;
            animator.SetBool(animID_IsRunning, isMoving);
        }
    }
    
    void LateUpdate()
    {
       
        if (fireballSpawnPoint != null)
        {
            // Keep the spawn point at the same X position as boss, maintain its local Y offset
            // Local position will naturally flip when parent (boss) flips
        }
    }
    
    void UpdateCooldowns()
    {
        // Cooldowns run twice as fast in Phase 2 (half the cooldown time)
        float cooldownMultiplier = isPhase2 ? 2f : 1f;
        
       
        dashTimer -= Time.deltaTime * cooldownMultiplier;
        spellTimer -= Time.deltaTime * cooldownMultiplier;
    }
    
    void EnterPhase2()
    {
        isPhase2 = true;
        dashSpeed = 35f; // Increase dash speed for Phase 2
        
        // Update FireballRain parameters for Phase 2: Speed 30, Count 45, Rate 0.3
        if (fireballRainSpawner != null)
        {
            fireballRainSpawner.SetPhase2Parameters(22f, 45, 0.3f);
        }
        
        Debug.Log("Boss entered Phase 2!");
    }
    
    void DecideNextAction()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
    
        
       
        if (CanCastSpell())
        {
            currentAction = ChooseRandomSpell();
            return;
        }
        
    
        
        // Check for Dash (if spell is on cooldown or not chosen)
        if (dashTimer <= 0f)
        {
            currentAction = BossAction.Dash;
            return;
        }

        // Default: Idle (no more Chase/Retreat movement)
        currentAction = BossAction.Idle;
    }
    
    bool CanCastSpell()
    {
        return spellTimer <= 0f && 
               !isAnySpellActive && 
               Time.time - lastSpellCastTime >= minTimeBetweenSpells;
    }
    
    BossAction ChooseRandomSpell()
    {
        float roll = Random.value;
        
        // More varied percentages: FireRain 33%, Fireball 33%, Sunkidama 34%
        if (roll < 0.33f)
            return BossAction.CastFire;
        else if (roll < 0.66f) // 0.33 + 0.33 = 0.66
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
            case BossAction.Dash:
                StartCoroutine(DashTowardsPlayer());
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
    

    
    IEnumerator DashTowardsPlayer()
    {
        isDashing = true;
        dashTimer = dashCooldown;
        
        // Stop normal movement
        currentVelocity = Vector2.zero;
        if (rb) rb.velocity = Vector2.zero;

        FacePlayer(); // Ensure facing correct way before dash
        
        // Determine dash direction (left or right based on player position)
        float direction = player.position.x > transform.position.x ? 1f : -1f;
        Vector2 dashDirection = new Vector2(direction, 0f);

        SetAnimationState("Dash");
        
        // Make rigidbody kinematic to prevent collisions from affecting dash
        bool wasKinematic = rb.isKinematic;
        if (rb) rb.isKinematic = true;
        
        // Store starting position to calculate distance traveled
        float startX = transform.position.x;
        
        // Dash across the full screen width - nothing can stop this movement
        while (Mathf.Abs(transform.position.x - startX) < screenWidth)
        {
            // Directly move transform since we're kinematic
            transform.position += (Vector3)(dashDirection * dashSpeed * Time.fixedDeltaTime);
            yield return new WaitForFixedUpdate();
        }
        
        // Restore rigidbody to original state
        if (rb) 
        {
            rb.isKinematic = wasKinematic;
            rb.velocity = Vector2.zero;
        }
        currentVelocity = Vector2.zero;
        
        // Face player after dash
        FacePlayer();
        
        
        isDashing = false;
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

    // Flip sprite for FireRain animation (animation sprite is inverted)

    Flip();

    animator.SetBool("IsChannelingFireRain", true);

    // Wait until casting animation finishes
    yield return null;

    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    while (stateInfo.normalizedTime < 1f)
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return null;
    }

    // Start the fireball rain AFTER animation completes
    fireballRainSpawner.StartFireballRain();

    // Keep animation active while raining
    while (fireballRainSpawner.IsRaining)   
    {
        yield return null;
    }

    // Turn off animation AFTER raining is done
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
    
    // Flip sprite for Fireball animation (animation sprite is inverted)
    Flip();
    
    animator.SetBool("isChannelingFireBall", true);

    // Wait until casting animation finishes
    yield return null;

    AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
    while (stateInfo.normalizedTime < 1f)
    {
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        yield return null;
    }

    // Phase 2: Cast fireball 3 times, Phase 1: Cast once
    int fireballCount = isPhase2 ? 3 : 1;
    
    for (int i = 0; i < fireballCount; i++)
    {
        // Capture player position at the moment of spawning for accurate targeting
        Vector2 playerTargetPosition = player.position;

        // Get spawn position - use spawn point if assigned, otherwise default to boss position
        Vector3 spawnPosition = fireballSpawnPoint != null 
            ? fireballSpawnPoint.position 
            : transform.position + Vector3.up * 2f;

        // Spawn fireball AFTER animation completes
        GameObject fireball = Instantiate(
            fireballPrefab,
            spawnPosition,
            Quaternion.identity
        );

        FireBall fireballScript = fireball.GetComponent<FireBall>();

        if (fireballScript != null)
        {
            fireballScript.Init(playerTargetPosition);
            
            // Phase 2: Fireball Speed 35
            if (isPhase2)
            {
                fireballScript.SetSpeed(35f);
            }
        }
        else
            Debug.LogError("FireBall script missing on prefab!");

        // Keep animation active until fireball is destroyed
        while (fireball != null)
            yield return null;
            
        // Small delay between fireballs if casting multiple
        if (i < fireballCount - 1)
            yield return new WaitForSeconds(0.1f);
    }

    // Turn off animation AFTER all fireballs are destroyed
    animator.SetBool("isChannelingFireBall", false);
    
    // Flip back
    Flip();

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

    // Phase 2: Cast sunkidama 2 times, Phase 1: Cast once
    int sunkidamaCount = isPhase2 ? 2 : 1;
    
    for (int i = 0; i < sunkidamaCount; i++)
    {
        Vector2 playerLastPosition = player.position;

        // Get spawn position - use spawn point if assigned, otherwise default to boss position
        Vector3 spawnPosition = sunkidamaSpawnPoint != null 
            ? sunkidamaSpawnPoint.position 
            : transform.position + Vector3.up * 2f;

        GameObject sun = Instantiate(
            sunkidama,
            spawnPosition,
            Quaternion.identity
        );

        Sunkidama sunScript = sun.GetComponent<Sunkidama>();
        sun.SetActive(true);
        sunScript.Init(playerLastPosition);
        
        // Phase 2: Sunkidama Speed 12
        if (isPhase2)
        {
            sunScript.SetSpeed(12f);
        }

        // Activate channeling animation to stay active while sun exists
        animator.SetBool("IsChannelingSunkidama", true);

        // Keep animation active until sunkidama is destroyed
        while (sun != null)
        {
            yield return null;
        }
        
        // Turn off animation after this sunkidama is destroyed
        animator.SetBool("IsChannelingSunkidama", false);
 
    }


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
