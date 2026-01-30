using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;   


public class Screen_Flash_Shake : MonoBehaviour
{
    public static Screen_Flash_Shake instance;
    private Image flashImage;

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        flashImage = GetComponent<Image>();
        Debug.Log($"[FLASH_DEBUG] Screen_Flash_Shake initialized on {gameObject.name}");
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
            Debug.Log("[FLASH_DEBUG] Screen_Flash_Shake instance cleared");
        }
    }

    public void TriggerFlash(float duration, float maxAlpha)
    {
        Debug.Log($"[FLASH_DEBUG] TriggerFlash called. Duration: {duration}, MaxAlpha: {maxAlpha}");
        
        if (flashImage == null)
        {
            flashImage = GetComponent<Image>();
            if (flashImage == null)
            {
                Debug.LogError("[FLASH_DEBUG] FlashImage component is MISSING!");
                return;
            }
        }
        
        // Ensure alpha starts at 0
        SetAlpha(0);
        gameObject.SetActive(true); // Ensure object is active
        
        StopAllCoroutines();
        StartCoroutine(FlashRoutine(duration, maxAlpha));
    }

    private IEnumerator FlashRoutine(float duration, float maxAlpha)
    {
        float halfDuration = duration / 2;

        // Fade In
        float elapsed = 0;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Use unscaled so it works during freezes
            SetAlpha(Mathf.Lerp(0, maxAlpha, elapsed / halfDuration));
            yield return null;
        }

        // Fade Out
        elapsed = 0;
        while (elapsed < halfDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(maxAlpha, 0, elapsed / halfDuration));
            yield return null;
        }

        SetAlpha(0);
    }

    private void SetAlpha(float alpha)
    {
        Color c = flashImage.color;
        c.a = alpha;
        flashImage.color = c;
    }
}


public class CameraShake : MonoBehaviour
{
    public static CameraShake instance;
    private Vector3 originalPos;

    void Awake()
    {
        instance = this;
    }

    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines(); // Reset if already shaking
        StartCoroutine(ShakeRoutine(duration, magnitude));
    }

    private IEnumerator ShakeRoutine(float duration, float magnitude)
    {
        originalPos = transform.localPosition;
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.unscaledDeltaTime; // Works even if game is paused
            yield return null;
        }

        transform.localPosition = originalPos;
    }
}