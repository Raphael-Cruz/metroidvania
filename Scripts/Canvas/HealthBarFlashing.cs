using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class HealthBarFlashing : MonoBehaviour

{

    [Header("References")]
    public Image bar;   // Your image (can be entire bar or overlay)

    [Header("Flash Settings")]
    public Color flashColor = Color.white;
    public float flashDuration = 0.15f;

    [Header("Low Health Flash")]
    public Color lowHealthColor = Color.red;
    public float lowHealthSpeed = 4f;
    
    private Color originalColor;
    private bool isLowHealth = false;
    private float flashTimer = 0f;

    void Start()
    {
        if (bar == null) bar = GetComponent<Image>();
        originalColor = bar.color;
    }

    void Update()
    {
        // Temporary damage flash
        if (flashTimer > 0)
        {
            flashTimer -= Time.deltaTime;
            float t = flashTimer / flashDuration;
            bar.color = Color.Lerp(originalColor, flashColor, t);
        }
        else if (!isLowHealth)
        {
            bar.color = originalColor;
        }

        // Continuous low HP flash
        if (isLowHealth)
        {
            float ping = (Mathf.Sin(Time.time * lowHealthSpeed) + 1f) / 2f;
            bar.color = Color.Lerp(originalColor, lowHealthColor, ping);
        }
    }

    // Call this when the player takes damage
    public void TriggerHitFlash()
    {
        flashTimer = flashDuration;
    }

    // Call this whenever health changes
 public void SetLowHealthState(bool active)
{
    isLowHealth = active;

    if (!active)
    {
        // stop pulsing and reset color
        bar.color = originalColor;
    }
}
}
