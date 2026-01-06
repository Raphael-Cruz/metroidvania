using UnityEngine;

public class FloatingButtonAnimator : MonoBehaviour
{
    [Header("Animation Settings")]
    public float pulseSpeed = 4f;
    public float pulseAmount = 0.08f;

    public float fadeSpeed = 6f;

    private Vector3 baseScale;
    private CanvasGroup canvasGroup;
    private bool visible = false;

    private void Awake()
    {
        baseScale = transform.localScale;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = 0f;
    }

    private void Update()
    {
        // Fade
        canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, visible ? 1f : 0f, fadeSpeed * Time.deltaTime);

        if (visible)
        {
            // Pulse
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = baseScale * scale;
        }
    }
public void Show()
{
    Debug.Log("SHOW CALLED");
    visible = true;
}
    public void Hide() => visible = false;
}
