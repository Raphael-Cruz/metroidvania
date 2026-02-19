using System.Collections;
using UnityEngine;

public class YellowHarpy : MonoBehaviour
{
    [Header("Detection")]
    public float chaseRange = 10f;
    public float attackRange = 6f;

    [Header("Movement")]
    public float moveSpeed = 4f;

    [Header("Energy Ball")]
    public GameObject energyBallPrefab;
    public Transform firePoint;
    public float shootCooldown = 3f;

    [Header("References")]
    public Transform player;
    public Animator anim;

    private enum State { Idle, Chasing, Attacking, Cooldown }
    private State currentState = State.Idle;

    private float cooldownTimer = 0f;
    private bool attackCoroutineRunning = false;

    void Start()
    {
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO != null)
            player = playerGO.transform;
        else
            Debug.LogError("YellowHarpy: Player not found. Make sure Player tag is set.");
    }

    void Update()
    {
        if (player == null || !player.gameObject.activeSelf) return;

        float dist = Vector2.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Idle:
                HandleIdle(dist);
                break;

            case State.Chasing:
                HandleChase(dist);
                break;

            case State.Attacking:
                // Handled by coroutine
                break;

            case State.Cooldown:
                HandleCooldown(dist);
                break;
        }

        FlipSprite();
        LockZ();
    }

    // ── State Handlers ──────────────────────────────────────────

    private void HandleIdle(float dist)
    {
        anim.SetBool("isChasing", false);
        anim.SetBool("isAttacking", false);

        if (dist <= chaseRange)
            SetState(State.Chasing);
    }

    private void HandleChase(float dist)
    {
        anim.SetBool("isChasing", true);
        anim.SetBool("isAttacking", false);

        if (dist > chaseRange)
        {
            SetState(State.Idle);
            return;
        }

        if (dist <= attackRange && !attackCoroutineRunning)
        {
            StartCoroutine(AttackRoutine());
            return;
        }

        // Move toward player
        Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
        transform.position += (Vector3)(dir * moveSpeed * Time.deltaTime);
    }

    private void HandleCooldown(float dist)
    {
        anim.SetBool("isChasing", false);
        anim.SetBool("isAttacking", false);

        cooldownTimer -= Time.deltaTime;

        if (cooldownTimer <= 0f)
        {
            if (dist <= chaseRange)
                SetState(State.Chasing);
            else
                SetState(State.Idle);
        }
    }

    // ── Attack Routine ───────────────────────────────────────────

    private IEnumerator AttackRoutine()
    {
        attackCoroutineRunning = true;

        // Stop and play attack animation
        SetState(State.Attacking);
        anim.SetBool("isChasing", false);
        anim.SetBool("isAttacking", true);

        // Brief wind-up before shooting
        yield return new WaitForSeconds(0.5f);

       YellowHarpyEnergyBall yellowHarpyEnergyBall = Instantiate(energyBallPrefab, firePoint.position, Quaternion.identity).GetComponent<YellowHarpyEnergyBall>();
       yellowHarpyEnergyBall.Init(player.position);


        // Back to idle for 1 second
        anim.SetBool("isAttacking", false);
        SetState(State.Cooldown);
        
        // Wait for shoot cooldown
        yield return new WaitForSeconds(shootCooldown);
        cooldownTimer = 1f;

        attackCoroutineRunning = false;
    }



    // ── Helpers ──────────────────────────────────────────────────

    private void SetState(State newState)
    {
        currentState = newState;
    }

    private void FlipSprite()
    {
        if (player == null) return;

        bool facingLeft = player.position.x < transform.position.x;
        Vector3 scale = transform.localScale;
        scale.x = facingLeft ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    private void LockZ()
    {
        Vector3 pos = transform.position;
        pos.z = 0f;
        transform.position = pos;
        transform.rotation = Quaternion.identity;
    }

    // ── Gizmos ───────────────────────────────────────────────────

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, chaseRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, attackRange);
        Gizmos.color = new Color(1f, 0.5f, 0f, 1f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}