using UnityEngine;

public class UIFollower : MonoBehaviour
{
    public Transform target;   // The stone object
    public Vector3 offset;     // UI offset in screen pixels
    private Camera cam;

    private void Start()
    {
        cam = Camera.main;
    }

  private void Update()
{
    if (target == null) return;

    // Convert world → screen
    Vector3 screenPos = cam.WorldToScreenPoint(target.position);

    // Convert screen → canvas local space
    RectTransformUtility.ScreenPointToLocalPointInRectangle(
        transform.parent as RectTransform,
        screenPos,
        cam,
        out Vector2 uiPos
    );

    // Apply position inside the canvas
    (transform as RectTransform).anchoredPosition = uiPos + (Vector2)offset;
}
}
