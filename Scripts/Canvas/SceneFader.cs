using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 1.0f;

    public static SceneFader instance { get; private set; }

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
        fadeImage.raycastTarget = true;
        StartCoroutine(FadeIn());
    }

    public IEnumerator FadeIn()
    {
        float alpha = 1.0f;
        while (alpha > 0)
        {
            alpha -= Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        fadeImage.raycastTarget = false;
    }

    // This is the new function for switching scenes
    public IEnumerator FadeOut(string sceneName)
    {
        fadeImage.raycastTarget = true;
        float alpha = 0f;
        while (alpha < 1)
        {
            alpha += Time.deltaTime * fadeSpeed;
            fadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

 public IEnumerator FadeToBlack(float duration)
    {
        float t = 0;
        Color c = fadeImage.color;

        while (t < duration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0, 1, t / duration);
            fadeImage.color = c;
            yield return null;
        }
    }

}

