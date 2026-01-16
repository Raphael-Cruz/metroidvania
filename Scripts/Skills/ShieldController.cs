using UnityEngine;
using System.Collections;

public class ShieldController : MonoBehaviour
{
    public static ShieldController instance;

    [Header("Settings")]
    public float shieldDuration = 4.0f;
    public float castAnimationTime = 0.5f;

    [Header("References")]
    public GameObject shieldObject;

    private Collider2D shieldCollider;
    private bool isShieldActive;
    private Coroutine shieldRoutine;

    private void Awake()
    {
        instance = this;
      

        if (shieldObject != null)
        {
            shieldCollider = shieldObject.GetComponent<Collider2D>();
            shieldObject.SetActive(false);
        }
    }



 public IEnumerator ShieldRoutine()
{
    if (isShieldActive)
        yield break;

    isShieldActive = true;

    PlayerMovement.instance.canMove = false;
    PlayerMovement.instance.theRB.velocity = Vector2.zero;
    PlayerMovement.instance.anim.SetTrigger("shieldUse");

    yield return new WaitForSeconds(castAnimationTime);

    if (this == null || shieldObject == null)
        yield break;

    PlayerMovement.instance.canMove = true;
    HealthManager.instance.isInvulnerable = true;
    shieldObject.SetActive(true);

    yield return new WaitForSeconds(shieldDuration);

    if (shieldObject != null)
        shieldObject.SetActive(false);

    if (HealthManager.instance != null)
        HealthManager.instance.isInvulnerable = false;

    isShieldActive = false;
}


    private void OnDestroy()
    {
        CleanupShield();
    }

    private void OnDisable()
    {
        CleanupShield();
    }

    private void CleanupShield()
    {
        if (shieldRoutine != null)
            StopCoroutine(shieldRoutine);

        if (shieldObject != null)
            shieldObject.SetActive(false);

        if (HealthManager.instance != null)
            HealthManager.instance.isInvulnerable = false;

        isShieldActive = false;
    }
}
