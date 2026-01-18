using UnityEngine;

public class GhostSprite : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float activeTime = 0.5f;
    [SerializeField] private float startAlpha = 0.8f;
    
    private SpriteRenderer myRenderer;
    private SpriteRenderer targetRenderer;
    
    private float timeActive;
    
    private void Awake()
    {
        myRenderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// Activates the ghost at the target's position/visuals.
    /// </summary>
    public void Activate(SpriteRenderer target, Transform targetTransform)
    {
        if (target == null) return;

        targetRenderer = target;
        
        // Copy visuals
        myRenderer.sprite = targetRenderer.sprite;
        myRenderer.color = new Color(targetRenderer.color.r, targetRenderer.color.g, targetRenderer.color.b, startAlpha);
        myRenderer.flipX = targetRenderer.flipX;
        myRenderer.flipY = targetRenderer.flipY;
        myRenderer.sortingLayerID = targetRenderer.sortingLayerID;
        myRenderer.sortingOrder = targetRenderer.sortingOrder - 1; // Render behind the real object

        // Copy Transform
        transform.position = targetTransform.position;
        transform.rotation = targetTransform.rotation;
        transform.localScale = targetTransform.localScale;

        timeActive = activeTime;
        gameObject.SetActive(true);
    }

    private void Update()
    {
        if (timeActive > 0)
        {
            timeActive -= Time.deltaTime;

            if (timeActive <= 0)
            {
                gameObject.SetActive(false);
            }
            else
            {
                // Fade out
                float alpha = (timeActive / activeTime) * startAlpha;
                Color c = myRenderer.color;
                c.a = alpha;
                myRenderer.color = c;
            }
        }
    }
}
