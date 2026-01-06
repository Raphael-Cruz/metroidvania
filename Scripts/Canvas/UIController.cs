using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static UIController instance;

    [Header("Fade Settings")]
    public Image fadeScreen;
    public float fadeSpeed = 2f;
    private bool fadingToBlack, fadingFromBlack;

    [Header("Save Notification")]
    public CanvasGroup saveMessageGroup; // Drag your 'SaveNotification' object here
    public float textFadeSpeed = 2f;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // Hide save message on start
        if (saveMessageGroup != null) saveMessageGroup.alpha = 0;
    }

    void Update()
    {
        // Handle Screen Fading
        if (fadingToBlack)
        {
            fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, Mathf.MoveTowards(fadeScreen.color.a, 1f, fadeSpeed * Time.deltaTime));
            if (fadeScreen.color.a == 1f) fadingToBlack = false;
        }
        else if (fadingFromBlack)
        {
            fadeScreen.color = new Color(fadeScreen.color.r, fadeScreen.color.g, fadeScreen.color.b, Mathf.MoveTowards(fadeScreen.color.a, 0f, fadeSpeed * Time.deltaTime));
            if (fadeScreen.color.a == 0f) fadingFromBlack = false;
        }
    }

    // --- Save Notification Logic ---
    public void ShowSaveMessage()
    {
        if (saveMessageGroup != null)
        {
            StopCoroutine("SaveMessageSequence"); // Stop current if already playing
            StartCoroutine(SaveMessageSequence());
        }
    }

private IEnumerator SaveMessageSequence()
{
    Debug.Log("UI Sequence Started!");
    
    if (saveMessageGroup == null) {
        Debug.LogError("SaveMessageGroup is missing!");
        yield break;
    }

    // FORCE VISIBILITY CHECKS
    saveMessageGroup.gameObject.SetActive(true); // Ensure object is active
    saveMessageGroup.transform.SetAsLastSibling(); // Move to front of UI
    saveMessageGroup.alpha = 0f; // Start from zero

    // Fade In
    while (saveMessageGroup.alpha < 1)
    {
        saveMessageGroup.alpha += Time.deltaTime * textFadeSpeed;
        yield return null;
    }

    // Stay and Flicker
    float timer = 0;
    while (timer < 1.5f)
    {
        saveMessageGroup.alpha = Random.Range(0.8f, 1f); 
        timer += Time.deltaTime;
        yield return null;
    }
    saveMessageGroup.alpha = 1f;

    // Fade Out
    while (saveMessageGroup.alpha > 0)
    {
        saveMessageGroup.alpha -= Time.deltaTime * textFadeSpeed;
        yield return null;
    }
    
    saveMessageGroup.alpha = 0f;
}

    // --- Scene Management ---
    private void OnEnable() { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadeScreen == null)
        {
            var found = GameObject.Find("FadeScreen");
            if (found != null) fadeScreen = found.GetComponent<Image>();
        }
        
        // Find save group in new scene if it's not part of the persistent UI
        if (saveMessageGroup == null)
        {
            var foundGroup = GameObject.Find("SaveNotification")?.GetComponent<CanvasGroup>();
            if (foundGroup != null) saveMessageGroup = foundGroup;
        }
    }

    public void StartFadeToBlack() { fadingToBlack = true; fadingFromBlack = false; }
    public void StartFadeFromBlack() { fadingFromBlack = true; fadingToBlack = false; }
}