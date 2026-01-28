using UnityEngine;
using System.Collections;


public class PlayerMovement : MonoBehaviour
{
    public float FacingDirectionX => visual.localScale.x;

    [Header("References")]
    public Transform visual;
    public Rigidbody2D theRB;
    public Animator anim;
    private PlayerAbilityTracker abilities;

    [Header("Movement")]
    public float speed = 4.0f;
    private int facingDirection = 1;

    [Header("Particles")]
    public GameObject laserBeam;
    public Transform laserPoint;

    [Header("Jump")]
    public float jumpForce = 20f;
    public Transform groundPoint;
    public LayerMask whatIsGround;
    private bool isOnGround;
    public bool IsOnGround => isOnGround;

    public bool canDoubleJump;
    private float groundedTimer = 0f;
    public float groundedTimeRequired = 0.1f;
    
    [Header("Super Jump")]
    public float superJumpForce = 40f;

    [Header("Coyote Time")]
    public float coyoteTime = 0.15f;
    private float coyoteCounter;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.15f;
    private float jumpBufferCounter;

    [Header("Coyote Fall")]
    public float coyoteFallTime = 0.1f;
    private float coyoteFallCounter;

    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashTime = 0.25f;
    public float waitAfterDashing = 0.35f;
    private float dashCounter;
    private float dashRechargeCounter;
    private bool isDashing;

    [Header("Afterimage")]
    public SpriteRenderer theSR;
    public SpriteRenderer afterImage;
    public float afterImageLifeTime = 0.3f;
    public float timeBetweenAfterImage = 0.05f;
    public Color afterImageColor;
    private float afterImageCounter;

    [Header("Shooting")]
    public bulletController shotToFire;
    public Transform shotPoint;

    [Header("Shooting the Missile")]
    public MissileController shotMissile;
    public Transform missileShotPoint;
    private bool isMissileLocking;
    public SkillHUDManager hud;
    public bool canMissile;

    public bool canMove;

[Header("Edge Detection")]
public Transform wallCheck;
public Transform ledgeCheck;
public LayerMask whatIsEdge; 
public float checkDistance = 0.2f;
public Vector2 hangingOffset; 

private bool isOnEdge;
private bool isHanging;

public static PlayerMovement instance;

 private void Awake()
    {
        instance = this;
    }
    

    private void Start()
    {
        if (theRB == null)
            theRB = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = visual.GetComponent<Animator>();

        abilities = GetComponent<PlayerAbilityTracker>();

if (hud == null) hud = FindFirstObjectByType<SkillHUDManager>();

if (abilities != null)
    {
        WorldState.ApplyAbilitiesToPlayer(abilities);
    }

        if (RespawnController.instance != null)
        {
            RespawnController.instance.SetRespawnPointIfEmpty(transform.position);
        }

        if (GetComponent<PlayerAbilityTracker>() != null)
    {
        WorldState.ApplyAbilitiesToPlayer(GetComponent<PlayerAbilityTracker>());
    }

        afterImageCounter = timeBetweenAfterImage;
        canMove = true;

        visual.localScale = new Vector3(1, 1, 1);
        facingDirection = 1;
    }

    public void SetInitialDirection(float scaleX)
    {
        facingDirection = scaleX >= 0 ? 1 : -1;
        visual.localScale = new Vector3(facingDirection, 1, 1);
    }

private void HandleGrounding()
{
    // Check if the circle actually overlaps ground
    bool touchingGround = Physics2D.OverlapCircle(groundPoint.position, 0.2f, whatIsGround);
    
    // We only want to be "grounded" if we are NOT moving upwards. 
    // BUT, we add a tiny buffer (-0.1) so small micro-fluctuations don't break it.
    bool notRising = theRB.velocity.y <= 0.1f; 
    
    isOnGround = touchingGround && notRising;

    if (isOnGround)
    {
        coyoteCounter = coyoteTime;
        coyoteFallCounter = coyoteFallTime;
        
        // Reset the grounded timer
        groundedTimer += Time.deltaTime;
        if (groundedTimer >= groundedTimeRequired && abilities.canDoubleJump)
        {
            canDoubleJump = true;
        }
    }
    else
    {
        coyoteCounter -= Time.deltaTime;
        coyoteFallCounter -= Time.deltaTime;
        groundedTimer = 0f; 
    }
}





private void OnDrawGizmos()
{
    if (wallCheck == null || ledgeCheck == null) return;

    Gizmos.color = Color.red;
    float dir = transform.localScale.x; // Accounts for player facing left/right
    
    Gizmos.DrawRay(wallCheck.position, Vector2.right * dir * checkDistance);
    Gizmos.DrawRay(ledgeCheck.position, Vector2.right * dir * checkDistance);
}

    private void Update()
    {
        if (!InGameMenuController.isGamePaused) 
    {
        CheckSanity(); // Global NaN Guard
        HandleGrounding();
       

       
        if (!canMove) return;

        // -----------------------
        // DASH INPUT & PROCESSING
        // -----------------------
        if (!isDashing && dashRechargeCounter <= 0 && Input.GetKeyDown(KeyCode.LeftControl) && abilities.canDash)
        {
            isDashing = true;
            dashCounter = dashTime;
            ShowAfterImage();
        }

        if (isDashing)
        {
            theRB.velocity = new Vector2(facingDirection * dashSpeed, theRB.velocity.y);
            dashCounter -= Time.deltaTime;

            afterImageCounter -= Time.deltaTime;
            if (afterImageCounter <= 0) ShowAfterImage();

            if (dashCounter <= 0)
            {
                isDashing = false;
                dashRechargeCounter = waitAfterDashing;
            }
            return;
        }

        // -----------------------
        // MOVEMENT
        // -----------------------
        float moveInput = canMove ? Input.GetAxisRaw("Horizontal") : 0f;
        theRB.velocity = new Vector2(moveInput * speed, theRB.velocity.y);

        if (moveInput != 0)
        {
            facingDirection = moveInput > 0 ? 1 : -1;
            visual.localScale = new Vector3(facingDirection, 1, 1);
           
        }


        // -----------------------
        // SHOOTING
        // -----------------------
        if (Input.GetButtonDown("Fire1"))
        {
            bulletController newBullet = Instantiate(shotToFire, shotPoint.position, shotPoint.rotation);
            newBullet.moveDir = new Vector2(facingDirection, 0);
            anim.SetTrigger("shotFired");
        }

//----------------------
// SKILLS
//----------------------


    if (Input.GetButtonDown("Fire2") && isOnGround && abilities != null )
    {
    
if (abilities.canMissile || abilities.canShield)
{
    if (hud == null)
    {
        hud = FindFirstObjectByType<SkillHUDManager>();
        return;
    }

    if (hud.UseCurrentSkill())
    {
        if (abilities.canMissile && !isMissileLocking && shotMissile )
        {
            StartCoroutine(ShootMissileRoutine());
        }
        else if (abilities.canShield)
        {
           StartCoroutine(ShieldController.instance.ShieldRoutine());
        }
    }
}
    
}


        UpdateAnimator();
    }
    }






//------------------------------------

    private IEnumerator ShootMissileRoutine()
{
    anim.SetBool("isFalling", false);
    anim.SetBool("isRising", false);
    anim.SetBool("isHighJump", false);
    anim.SetTrigger("missileFired");

    FreezePlayer(0.6f);

    isMissileLocking = true; 
    float angle = facingDirection > 0 ? 0f : 180f;
    Quaternion spawnRotation = Quaternion.Euler(0, 0, angle);

    // Spawn the missile
    MissileController newMissile = Instantiate(shotMissile, missileShotPoint.position, spawnRotation);
    
    // Set the initial direction for the missile controller
    newMissile.moveDir = new Vector2(facingDirection, 0);

    Collider2D playerCol = visual.GetComponent<Collider2D>(); 
    if(playerCol != null) Physics2D.IgnoreCollision(newMissile.GetComponent<Collider2D>(), playerCol);

    yield return null; 
}

    private void ShowAfterImage()
    {
        SpriteRenderer image = Instantiate(afterImage, transform.position, transform.rotation);
        image.sprite = theSR.sprite;
        image.transform.localScale = visual.localScale;
        image.color = afterImageColor;
        Destroy(image.gameObject, afterImageLifeTime);
        afterImageCounter = timeBetweenAfterImage;
    }

    public void FreezePlayer(float duration)
    {
        if (isMissileLocking) return;
        StartCoroutine(FreezeRoutine(duration));
    }

    private IEnumerator FreezeRoutine(float duration)
    {
        isMissileLocking = true;
        canMove = false;
        theRB.velocity = Vector2.zero;
        theRB.constraints = RigidbodyConstraints2D.FreezeAll;

        yield return new WaitForSeconds(duration);

        theRB.constraints = RigidbodyConstraints2D.FreezeRotation;
        canMove = true;
        isMissileLocking = false;
    }

private void UpdateAnimator()
{
    // If we are frozen (like shooting a missile), let that animation finish
    if (isMissileLocking) return;

    // BASIC STATES
    anim.SetBool("isOnGround", isOnGround);
    anim.SetFloat("speed", Mathf.Abs(theRB.velocity.x));

    // VERTICAL MOVEMENT
    bool isHighJump = theRB.velocity.y > jumpForce * 1.1f; // Adjusted threshold
    bool isRising = theRB.velocity.y > 0.1f && !isOnGround;
    
    // FALL LOGIC
    // We only fall if we aren't grounded and velocity is downward
    bool isFalling = theRB.velocity.y < -0.1f && !isOnGround;

    // Apply to animator
    anim.SetBool("isHighJump", isHighJump);
    anim.SetBool("isRising", isRising && !isHighJump);
    anim.SetBool("isFalling", isFalling);
}

    // SAFETY GUARD: Prevents NaN (Not a Number) from crashing the Camera/Game
    void CheckSanity()
    {
        if (theRB != null && (float.IsNaN(theRB.velocity.x) || float.IsNaN(theRB.velocity.y)))
        {
            Debug.LogError($"[PlayerMovement] NaN detected in Velocity! Resetting. Prev Velocity: {theRB.velocity}");
            theRB.velocity = Vector2.zero;
        }

        if (float.IsNaN(transform.position.x) || float.IsNaN(transform.position.y) || float.IsNaN(transform.position.z))
        {
            Debug.LogError($"[PlayerMovement] NaN detected in Position! Resetting to safe point. Prev Pos: {transform.position}");
            // Attempt to recover validity - this is a last resort
            if (RespawnController.instance != null)
                transform.position = RespawnController.instance.GetRespawnPoint();
            else
                transform.position = Vector3.zero;
        }
    }
}














