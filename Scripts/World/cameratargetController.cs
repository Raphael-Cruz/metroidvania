using UnityEngine;

public class cameratargetController : MonoBehaviour
{
    public Transform player;

    [Header("Horizontal Look")]
    public float lookAheadX = 3f;
    public float lookAheadSmoothTime = 0.15f;

    [Header("Vertical Look")]
    public float lookAheadUp = 1.5f;
    public float verticalSmoothUp = 0.35f; // only used when going up

    Vector3 smoothVelocity;
    float currentVerticalOffset;
    float verticalVelocity;

    void LateUpdate()
    {
        float inputX = Input.GetAxisRaw("Horizontal");
        float inputY = Input.GetAxisRaw("Vertical");

        // ───── Horizontal (instant target change)
        float targetX = inputX * lookAheadX;

        // ───── Vertical
        if (inputY > 0)
        {
            // Smooth ONLY when moving up
            currentVerticalOffset = Mathf.SmoothDamp(
                currentVerticalOffset,
                lookAheadUp,
                ref verticalVelocity,
                verticalSmoothUp
            );
        }
        else
        {
            // Snap down instantly (like horizontal)
            currentVerticalOffset = 0f;
            verticalVelocity = 0f;
        }

        Vector3 targetPos = new Vector3(
            player.position.x + targetX,
            player.position.y + currentVerticalOffset,
            transform.position.z
        );

        // Final smoothing (same for all directions)
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPos,
            ref smoothVelocity,
            lookAheadSmoothTime
        );
    }
}
