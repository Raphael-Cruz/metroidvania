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
public LayerMask whatIsEdge; // Set this to your 'Ledge' layer
public float checkDistance = 0.2f;
public Vector2 hangingOffset; // Manual adjustment to align hands to pixels

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
        bool touchingGround = Physics2D.OverlapCircle(groundPoint.position, 0.1f, whatIsGround);
        // Grounded only if touching ground and not moving up quickly
        isOnGround = touchingGround && theRB.velocity.y <= 0.1f;

        if (isOnGround)
        {
            coyoteCounter = coyoteTime;
            coyoteFallCounter = coyoteFallTime;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
            coyoteFallCounter -= Time.deltaTime;
        }
    }

private void HandleEdgeHanging()
{
    // Only look for ledges if we are in the air 
    if (isOnGround) 
    {
        if (isHanging) 
        ExitHanging(); // Safety check
        return;
    }

    // Detection Rays
    float dir = visual.localScale.x; // Use your 'visual' reference for direction
    Vector2 moveDir = new Vector2(dir, 0);

    bool wallInFront = Physics2D.Raycast(wallCheck.position, moveDir, checkDistance, whatIsEdge);
    bool airAbove = !Physics2D.Raycast(ledgeCheck.position, moveDir, checkDistance, whatIsEdge);

    // Trigger Hanging
    if (wallInFront && airAbove && !isHanging)
    {
        StartHanging();
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
        HandleGrounding();
        HandleEdgeHanging(); // <-- Add this here

        if (isHanging)
        {
            HandleHangingInput();
            return; // Block normal movement while hanging
        }

        if (!canMove) return;

        // -----------------------
        // JUMP BUFFER & DASH COOLDOWN
        // -----------------------
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.deltaTime;

        if (dashRechargeCounter > 0)
            dashRechargeCounter -= Time.deltaTime;

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
        // JUMP LOGIC
        // -----------------------
        if (Input.GetKeyDown(KeyCode.Mouse2) && coyoteCounter > 0 && abilities.canSuperJump)
        {
            DoSuperJump();
            canDoubleJump = true;
        }
        else if (jumpBufferCounter > 0)
        {
            if (coyoteCounter > 0)
            {
                DoJump();
                canDoubleJump = true;
            }
            else if (canDoubleJump && abilities.canDoubleJump)
            {
                canDoubleJump = false;
                anim.SetTrigger("doubleJump");
                DoJump();
            }
        }

        if (Input.GetButtonUp("Jump") && theRB.velocity.y > 0f)
        {
            theRB.velocity = new Vector2(theRB.velocity.x, theRB.velocity.y * 0.5f);
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

    if (Input.GetButtonDown("Fire2") && !isMissileLocking && shotMissile && isOnGround && abilities != null && abilities.canMissile)
{
    // Check with the HUD if we have a missile to fire
    if (hud != null)
    {
        if (hud.UseCurrentSkill()) 
        {
            StartCoroutine(ShootMissileRoutine());
        }
    }
    else 
    {
        // Try to find it if it's missing
        hud = FindFirstObjectByType<SkillHUDManager>();
    }
}


        UpdateAnimator();
    }
    }

//-----------------
//HANGIN
//-----------------
private void StartHanging()
{
/*/    isHanging = true;
    canMove = false; // Disable normal WASD
    theRB.velocity = Vector2.zero;
    theRB.gravityScale = 0f; // Gravity goes away
    theRB.constraints = RigidbodyConstraints2D.FreezeAll; // Total freeze
    
    anim.SetBool("isHanging", true);*/
}


private void HandleHangingInput()
{
    if (Input.GetButtonDown("Jump"))
    {
        ExitHanging();
        DoJump(); // Re-use your existing Jump logic
    }
    
    // Drop down if player presses 'Down' or 'S'
    if (Input.GetAxisRaw("Vertical") < -0.1f)
    {
        ExitHanging();
    }
}

private void ExitHanging()
{
    isHanging = false;
    canMove = true; 
    theRB.gravityScale = 1f; // IMPORTANT: Gravity must return to 1 (or your default)
    theRB.constraints = RigidbodyConstraints2D.FreezeRotation; // Unfreeze position
    
    anim.SetBool("isHanging", false);
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

    private void DoJump()
    {
        theRB.velocity = new Vector2(theRB.velocity.x, jumpForce);
        coyoteCounter = 0;
        jumpBufferCounter = 0;
    }

    private void DoSuperJump()
    {
        if (laserBeam != null && laserPoint != null)
            Instantiate(laserBeam, laserPoint.position, laserPoint.rotation, laserPoint);

        theRB.velocity = new Vector2(theRB.velocity.x, superJumpForce);
        coyoteCounter = 0;
        jumpBufferCounter = 0;
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
   
    if (!canMove) return; 

    anim.SetBool("isOnGround", isOnGround);
    anim.SetBool("isRising", theRB.velocity.y > 1.5f);
    anim.SetBool("isHighJump", theRB.velocity.y > jumpForce * 1.5f);
    anim.SetFloat("speed", Mathf.Abs(theRB.velocity.x));

    bool shouldFall = !isMissileLocking && theRB.velocity.y < -0.1f && coyoteFallCounter <= 0f;
    anim.SetBool("isFalling", shouldFall);
}
}