using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    public Image fadeImage;
    public float fadeSpeed = 1.0f;

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
}