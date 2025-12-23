using UnityEngine;

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
   
    private bool isRising;
    private bool isHighJump;

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

    public bool canMove;


    private void Start()
    {
        
        if (theRB == null)
            theRB = GetComponent<Rigidbody2D>();

        if (anim == null)
            anim = visual.GetComponent<Animator>();

        abilities = GetComponent<PlayerAbilityTracker>();

        if (RespawnController.instance != null)
    {
        // Sets the respawn point to current position ONLY if it's currently (0,0,0)
        RespawnController.instance.SetRespawnPointIfEmpty(transform.position);
    }

        afterImageCounter = timeBetweenAfterImage;
        canMove = true;

        visual.localScale = new Vector3(1, 1, 1);
        facingDirection = 1;
    }

public void SetInitialDirection(float scaleX)
{
    // Ensure the internal integer matches the visual scale
    facingDirection = scaleX >= 0 ? 1 : -1;
    visual.localScale = new Vector3(facingDirection, 1, 1);
}
private void HandleGrounding()
{
    bool touchingGround = Physics2D.OverlapCircle(groundPoint.position, 0.03f, whatIsGround);
    // Only grounded if touching ground AND not actively moving upward (prevents jump-snapping)
    isOnGround = touchingGround && theRB.velocity.y <= 0.1f;
}
    private void Update()
    {
        if (!canMove)
        {
            
            return;
        }



        //Only treat as grounded if  not rising:
bool touchingGround = Physics2D.OverlapCircle(groundPoint.position, 0.03f, whatIsGround);
isOnGround = touchingGround && theRB.velocity.y <= 0.1f;


        // -----------------------
        // COYOTE TIME
        // -----------------------
       if (isOnGround)
            coyoteCounter = coyoteTime;
        else
            coyoteCounter -= Time.deltaTime;

        // -----------------------
        // JUMP BUFFER
        // -----------------------
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else if (jumpBufferCounter > 0)
            jumpBufferCounter -= Time.deltaTime;


        // -----------------------
        // DASH COOLDOWN
        // -----------------------
        if (dashRechargeCounter > 0)
            dashRechargeCounter -= Time.deltaTime;

        // -----------------------
        // DASH INPUT
        // -----------------------
        if (!isDashing && dashRechargeCounter <= 0 && Input.GetButtonDown("Fire2") && abilities.canDash)
        {
            isDashing = true;
            dashCounter = dashTime;
            ShowAfterImage();
        }

        // -----------------------
        // DASH PROCESSING
        // -----------------------
        if (isDashing)
        {
            theRB.velocity = new Vector2(facingDirection * dashSpeed, theRB.velocity.y);
            dashCounter -= Time.deltaTime;

            afterImageCounter -= Time.deltaTime;
            if (afterImageCounter <= 0)
                ShowAfterImage();

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

            shotPoint.localPosition = new Vector3(
                Mathf.Abs(shotPoint.localPosition.x) * facingDirection,
                shotPoint.localPosition.y,
                shotPoint.localPosition.z
            );
        }

        // -----------------------
        // SUPER JUMP
        // -----------------------
        if (Input.GetKeyDown(KeyCode.Mouse2) && coyoteCounter > 0 && abilities.canSuperJump)
        {
            DoSuperJump();
            canDoubleJump = true;
            return;
        }

// -----------------------
        // NORMAL + DOUBLE JUMP
        // -----------------------
        if (jumpBufferCounter > 0)
        {
            // Normal jump
            if (coyoteCounter > 0)
            {
                DoJump();
                canDoubleJump = true;
                jumpBufferCounter = 0;
            }
            // Double jump
            else if (canDoubleJump && abilities.canDoubleJump)
            {
                canDoubleJump = false;
                anim.SetTrigger("doubleJump");
                DoJump();
                jumpBufferCounter = 0;
            }
        }

        // -----------------------
        // VARIABLE JUMP HEIGHT
        // -----------------------
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

      // -----------------------
// ANIMATION PARAMETERS
// -----------------------
anim.SetBool("isOnGround", isOnGround);


// Rising = moving up
anim.SetBool("isRising", theRB.velocity.y > 1.5f);

// High Jump = player held jump long enough
anim.SetBool("isHighJump", theRB.velocity.y > jumpForce * 2.5f);

anim.SetFloat("speed", Mathf.Abs(theRB.velocity.x));

// -----------------------
// COYOTE FALL LOGIC
// -----------------------
if (isOnGround)
{
    coyoteFallCounter = coyoteFallTime;
}
else
{
    coyoteFallCounter -= Time.deltaTime;
}

// FALL only after timer expires
bool shouldFall = theRB.velocity.y < -0.1f && coyoteFallCounter <= 0f;
anim.SetBool("isFalling", shouldFall);

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
}