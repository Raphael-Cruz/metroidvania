using UnityEngine;

public class ParallaxEffect : MonoBehaviour
{
    [Header("Target Settings")]
    public Transform cameraTransform;
    
    [Header("Parallax Strength")]
    [Tooltip("0 = moves with camera, 1 = stays static, 0.5 = half speed")]
    public Vector2 parallaxEffectMultiplier;

    private Vector3 lastCameraPosition;
    private float textureUnitSizeX;

    void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        lastCameraPosition = cameraTransform.position;

        // Optional: For seamless looping backgrounds
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
    }

    void LateUpdate()
    {
        Vector3 deltaMovement = cameraTransform.position - lastCameraPosition;
        
        // Move the layer by the delta multiplied by the effect strength
        transform.position += new Vector3(deltaMovement.x * parallaxEffectMultiplier.x, 
                                          deltaMovement.y * parallaxEffectMultiplier.y, 0);
        
        lastCameraPosition = cameraTransform.position;

        // Infinite Scrolling Logic (Optional)
        if (Mathf.Abs(cameraTransform.position.x - transform.position.x) >= textureUnitSizeX)
        {
            float offsetPositionX = (cameraTransform.position.x - transform.position.x) % textureUnitSizeX;
            transform.position = new Vector3(cameraTransform.position.x + offsetPositionX, transform.position.y);
        }
    }
}