
using UnityEngine;

public class LaserScaleDown : MonoBehaviour
{
    public Transform target;       // LaserPoint
    public float followSpeed = 15f;

    [Header("Scale Animation")]
    public float peakScale = 1.5f;
    public float growTime = 0.1f;
    public float shrinkTime = 0.2f;

    private Vector3 originalScale;

    void Start()
    {
        originalScale = transform.localScale;
        transform.localScale = Vector3.zero;  // Start invisible
        StartCoroutine(ScaleAnimation());
    }

    void Update()
    {
        if (target == null) return;

        // Smooth follow
        transform.position = Vector3.Lerp(
            transform.position,
            target.position,
            followSpeed * Time.deltaTime
        );
    }

    private System.Collections.IEnumerator ScaleAnimation()
    {
        // ----- GROW -----
        float t = 0;
        while (t < growTime)
        {
            t += Time.deltaTime;
            float p = t / growTime;

            transform.localScale = Vector3.Lerp(Vector3.zero, originalScale * peakScale, p);
            yield return null;
        }

        // ----- SHRINK -----
        t = 0;
        while (t < shrinkTime)
        {
            t += Time.deltaTime;
            float p = t / shrinkTime;

            transform.localScale = Vector3.Lerp(originalScale * peakScale, originalScale, p);
            yield return null;
        }
    }
}