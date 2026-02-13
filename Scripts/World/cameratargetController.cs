using UnityEngine;

public class cameratargetController : MonoBehaviour
{
    public Transform player;

    [Header("Horizontal Look")]
    public float lookAheadX = 3f;
    public float lookAheadSmoothTime = 0.15f;

    [Header("Vertical Look")]
    public float lookAheadUp = 1.5f;
    public float verticalSmoothUp = 0.35f;

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
    }

    void LateUpdate()
    {
        if (player == null)
            return; // safety

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

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref smoothVelocity,
            lookAheadSmoothTime
        );
    }
}
