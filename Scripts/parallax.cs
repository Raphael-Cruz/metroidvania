using UnityEngine;

public class ParallaxScroller : MonoBehaviour
{
    public float speed = 0.5f;

    private Transform leftPart;
    private Transform rightPart;
    private float width;

    void Start()
    {
        // Children must be exactly 2 sprites
        leftPart = transform.GetChild(0);
        rightPart = transform.GetChild(1);

        // Measure sprite width
        SpriteRenderer sr = leftPart.GetComponent<SpriteRenderer>();
        width = sr.bounds.size.x;
    }

    void Update()
    {
        // Move entire parent to the left
        transform.position += Vector3.left * speed * Time.deltaTime;

        // If left sprite is fully off-screen, shift both
        if (transform.position.x <= -width)
        {
            transform.position += new Vector3(width, 0, 0);
        }
    }
}