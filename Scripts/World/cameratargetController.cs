using UnityEngine;

public class CameraTargetController : MonoBehaviour
{
    public Transform player;

    [Header("Horizontal Look")]
    public float lookAheadX = 3f;
    public float lookAheadSmoothTime = 0.15f;

    [Header("Vertical Look")]
    public float lookAheadUp = 1.5f;
    public float verticalSmoothUp = 0.35f;

    [Header("Wall Camera Limit")]
    [Tooltip("Limite máximo de X enquanto a parede não foi quebrada")]
    public float maxXBeforeBreak = -2.085706f;
    public bool wallBroken = false;

    Vector3 smoothVelocity;
    float currentVerticalOffset;
    float verticalVelocity;

void Awake()
{
    if (player == null)
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    BreakableTilemapWall.OnWallFullyBroken += UnlockCamera;

    // Força posição inicial dentro do limite
    if (!wallBroken && player != null)
    {
        Vector3 startPos = transform.position;
        startPos.x = Mathf.Min(player.position.x, maxXBeforeBreak);
        transform.position = startPos;
    }
}

    void OnDestroy()
    {
        // Sempre cancela a inscrição para evitar memory leak
        BreakableTilemapWall.OnWallFullyBroken -= UnlockCamera;
    }

    void LateUpdate()
{
    if (player == null)
        return;

    float inputX = Input.GetAxisRaw("Horizontal");
    float inputY = Input.GetAxisRaw("Vertical");

    float targetX = inputX * lookAheadX;

    if (inputY > 0)
    {
        currentVerticalOffset = Mathf.SmoothDamp(
            currentVerticalOffset,
            lookAheadUp,
            ref verticalVelocity,
            verticalSmoothUp
        );
    }
    else
    {
        currentVerticalOffset = 0f;
        verticalVelocity = 0f;
    }

    Vector3 targetPos = new Vector3(
        player.position.x + targetX,
        player.position.y + currentVerticalOffset,
        transform.position.z
    );

    if (!wallBroken)
    {
        targetPos.x = Mathf.Min(targetPos.x, maxXBeforeBreak);
    }

    transform.position = Vector3.SmoothDamp(
        transform.position,
        targetPos,
        ref smoothVelocity,
        lookAheadSmoothTime
    );

    // Trava hard o transform DEPOIS do SmoothDamp
    if (!wallBroken)
    {
        Vector3 clamped = transform.position;
        clamped.x = Mathf.Min(clamped.x, maxXBeforeBreak);
        transform.position = clamped;
    }
}

    public void UnlockCamera()
    {
        wallBroken = true;
    }
}