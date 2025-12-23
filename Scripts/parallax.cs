using UnityEngine;

public class MenuParallax : MonoBehaviour

{
    [Header("Settings")]
    public float scrollSpeed; // Positive for left, Negative for right
    
    private float textureUnitSizeX;
    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
        
        // Get the width of the sprite
        Sprite sprite = GetComponent<SpriteRenderer>().sprite;
        Texture2D texture = sprite.texture;
        textureUnitSizeX = texture.width / sprite.pixelsPerUnit;
        
        // Match the scale if you resized it in the editor
        textureUnitSizeX *= transform.localScale.x;
    }

    void Update()
    {
        // Move the layer
        float newPos = Mathf.Repeat(Time.time * scrollSpeed, textureUnitSizeX);
        transform.position = startPos + Vector3.right * -newPos;
    }
}