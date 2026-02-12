using UnityEngine;

public class Credits : MonoBehaviour
{
    [Header("Skip Settings")]
    public float holdToSkipTime = 5f;
    private float skipHoldTimer = 0f;

    [Header("References")]
    public RectTransform firstText;
    public RectTransform endingImage;
    public RectTransform creditsText;

    [Header("First Text Roll")]
    public float firstTextScrollSpeed = 60f;
    public float firstTextEndY = 500f;

    [Header("Image Roll")]
    public float imageScrollSpeed = 80f;
    public float imageEndY = 600f;
    public float pauseAfterImage = 2f;

    [Header("Credits Roll")]
    public float creditsScrollSpeed = 60f;
    public float creditsEndY = 1400f;

    private bool firstTextDone = false;
    private bool imageDone = false;
    private bool creditsDone = false;
    private bool allowHoldToQuit = false;

    void Start()
    {
        if (!firstText || !endingImage || !creditsText)
        {
            Debug.LogError("Credits: Missing references!");
            enabled = false;
        }
    }

    void Update()
    {
        if (allowHoldToQuit)
            HandleHoldToQuit();

        if (!firstTextDone)
        {
            RollFirstText();
            return;
        }

        if (!imageDone)
        {
            RollImage();
            return;
        }

        if (!creditsDone)
        {
            RollCredits();
        }
    }

    // ---------------- FIRST TEXT ----------------
    void RollFirstText()
    {
        firstText.anchoredPosition += Vector2.up * firstTextScrollSpeed * Time.deltaTime;

        if (firstText.anchoredPosition.y >= firstTextEndY)
        {
            firstTextDone = true;
        }
    }

    // ---------------- IMAGE ----------------
    void RollImage()
    {
        endingImage.anchoredPosition += Vector2.up * imageScrollSpeed * Time.deltaTime;

        if (endingImage.anchoredPosition.y >= imageEndY)
        {
            imageDone = true;
            Invoke(nameof(EnableHoldToQuit), pauseAfterImage);
        }
    }

    void EnableHoldToQuit()
    {
        allowHoldToQuit = true;
    }

    // ---------------- CREDITS ----------------
    void RollCredits()
    {
        creditsText.anchoredPosition += Vector2.up * creditsScrollSpeed * Time.deltaTime;

        if (creditsText.anchoredPosition.y >= creditsEndY)
        {
            creditsDone = true;
        }
    }

    // ---------------- HOLD TO QUIT ----------------
    void HandleHoldToQuit()
    {
        bool holdingInput =
            Input.anyKey ||
            Input.GetMouseButton(0) ||
            Input.GetMouseButton(1);

        if (holdingInput)
        {
            skipHoldTimer += Time.deltaTime;

            if (skipHoldTimer >= holdToSkipTime)
            {
                ExitGame();
            }
        }
        else
        {
            skipHoldTimer = 0f;
        }
    }

    void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
